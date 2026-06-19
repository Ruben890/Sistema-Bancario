import { useEffect, useState } from "react";
import { authService } from "../services/authService";
import {
  type AuthState,
  getAuthRequest,
  getCachedAuthState,
  setAuthRequest,
  setCachedAuthState
} from "./authSessionCache";

const unauthenticatedState: AuthState = {
  isAuthenticated: false,
  user: null,
  isLoading: false
};

const resolveAuthState = async (): Promise<AuthState> => {
  const cached = getCachedAuthState();
  if (cached) return cached;

  const currentRequest = getAuthRequest();
  if (currentRequest) return currentRequest;

  const request = authService
    .me()
    .then((result) => ({
      isAuthenticated: true,
      user: result.entity,
      isLoading: false
    }))
    .catch(() => unauthenticatedState)
    .then((state) => {
      setCachedAuthState(state);
      setAuthRequest(null);
      return state;
    });

  setAuthRequest(request);
  return request;
};

export const useAuth = () => {
  const cached = getCachedAuthState();
  const [authState, setAuthState] = useState<AuthState>({
    isAuthenticated: cached?.isAuthenticated ?? null,
    user: cached?.user ?? null,
    isLoading: !cached
  });

  useEffect(() => {
    let mounted = true;

    resolveAuthState().then((state) => {
      if (mounted) setAuthState(state);
    });

    return () => {
      mounted = false;
    };
  }, []);

  return authState;
};
