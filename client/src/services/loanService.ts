import { apiRequest } from "./apiClient";
import type { ApiResult, CreateLoanRequest, Loan, LoanStatus, RawLoan, UpdateLoanRequest } from "../types/api";

const statusByNumber: Record<number, LoanStatus> = {
  1: "Pending",
  2: "Approved",
  3: "Rejected"
};

const normalizeLoanStatus = (status: RawLoan["status"]): LoanStatus => {
  if (typeof status === "number") {
    return statusByNumber[status] ?? "Pending";
  }

  return status;
};

const normalizeLoan = (loan: RawLoan): Loan => ({
  ...loan,
  status: normalizeLoanStatus(loan.status)
});

const normalizeLoanResult = (result: ApiResult<RawLoan>): ApiResult<Loan> => ({
  ...result,
  entity: result.entity ? normalizeLoan(result.entity) : null
});

const normalizeLoanListResult = (result: ApiResult<RawLoan[]>): ApiResult<Loan[]> => ({
  ...result,
  entity: result.entity?.map(normalizeLoan) ?? null
});

export const loanService = {
  list: async () => normalizeLoanListResult(await apiRequest<RawLoan[]>("/api/loans")),
  get: async (id: string) => normalizeLoanResult(await apiRequest<RawLoan>(`/api/loans/${id}`)),
  create: (request: CreateLoanRequest) =>
    apiRequest<RawLoan>("/api/loans", {
      method: "POST",
      body: request
    }).then(normalizeLoanResult),
  update: (id: string, request: UpdateLoanRequest) =>
    apiRequest<RawLoan>(`/api/loans/${id}`, {
      method: "PUT",
      body: request
    }).then(normalizeLoanResult),
  approve: (id: string) =>
    apiRequest<RawLoan>(`/api/loans/${id}/approve`, {
      method: "POST"
    }).then(normalizeLoanResult),
  reject: (id: string, reason: string) =>
    apiRequest<RawLoan>(`/api/loans/${id}/reject`, {
      method: "POST",
      body: { reason }
    }).then(normalizeLoanResult),
  remove: (id: string) =>
    apiRequest<object>(`/api/loans/${id}`, {
      method: "DELETE"
    })
};
