import type { AuthUser } from "../types/api";

export interface AuthState {
  isAuthenticated: boolean | null;
  user: AuthUser | null;
  isLoading: boolean;
}

const AUTH_CACHE_TTL_MS = 10_000;

let authCache: { state: AuthState; updatedAt: number } | null = null;
let authRequest: Promise<AuthState> | null = null;

export const getCachedAuthState = () => {
  if (!authCache) return null;

  if (Date.now() - authCache.updatedAt > AUTH_CACHE_TTL_MS) {
    authCache = null;
    return null;
  }

  return authCache.state;
};

export const setCachedAuthState = (state: AuthState) => {
  authCache = { state, updatedAt: Date.now() };
};

export const getAuthRequest = () => authRequest;

export const setAuthRequest = (request: Promise<AuthState> | null) => {
  authRequest = request;
};

export const resetAuthCache = () => {
  authCache = null;
  authRequest = null;
};
