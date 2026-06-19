import { apiRequest } from "./apiClient";
import type { User } from "../types/api";

export const userService = {
  list: () => apiRequest<User[]>("/api/users")
};
