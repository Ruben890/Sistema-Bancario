import { apiRequest } from "./apiClient";
import type { AuthUser } from "../types/api";

export interface LoginRequest {
  email: string;
  password: string;
}

export interface RegisterRequest {
  name: string;
  email: string;
  password: string;
}

export const authService = {
  login: (request: LoginRequest) =>
    apiRequest<object>("/api/auth/login", {
      method: "POST",
      body: request
    }),

  register: (request: RegisterRequest) =>
    apiRequest<object>("/api/auth/register", {
      method: "POST",
      body: request
    }),

  me: () => apiRequest<AuthUser>("/api/auth/me"),

  logout: () =>
    apiRequest<object>("/api/auth/logout", {
      method: "POST"
    })
};
