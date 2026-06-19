import type { ApiResult } from "../types/api";

const API_BASE_URL = import.meta.env.VITE_API_BASE_URL ?? "https://localhost:7205";

export class ApiError extends Error {
  constructor(
    message: string,
    public readonly status: number,
    public readonly code?: string | null
  ) {
    super(message);
  }
}

type RequestOptions = Omit<RequestInit, "body"> & {
  body?: unknown;
};

export const apiRequest = async <TEntity>(
  path: string,
  options: RequestOptions = {}
): Promise<ApiResult<TEntity>> => {
  const response = await fetch(`${API_BASE_URL}${path}`, {
    ...options,
    credentials: "include",
    headers: {
      "Content-Type": "application/json",
      ...(options.headers ?? {})
    },
    body: options.body === undefined ? undefined : JSON.stringify(options.body)
  });

  const result = (await response.json().catch(() => null)) as ApiResult<TEntity> | null;

  if (!response.ok) {
    if (response.status >= 500 && window.location.pathname !== "/error/500") {
      window.location.assign("/error/500");
    }

    throw new ApiError(
      result?.message ?? "Request failed.",
      response.status,
      result?.code
    );
  }

  if (!result) {
    throw new ApiError("Invalid API response.", response.status);
  }

  return result;
};
