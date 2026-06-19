export type UserRole = "Customer" | "Admin";
export type LoanStatus = "Pending" | "Approved" | "Rejected";
export type RawLoanStatus = LoanStatus | 1 | 2 | 3;

export interface ApiResult<TEntity> {
  entity: TEntity | null;
  code: string | null;
  message: string | null;
  statusCode: number;
}

export interface AuthUser {
  userId: string;
  name: string;
  email: string;
  role: UserRole;
  token?: string;
}

export interface User {
  id: string;
  name: string;
  email: string;
  role: UserRole;
  isActive: boolean;
}

export interface LoanUser {
  id: string;
  name: string;
  email: string;
  role: UserRole;
}

export interface Loan {
  id: string;
  userId: string;
  user: LoanUser | null;
  amount: number;
  termInMonths: number;
  purpose: string;
  status: LoanStatus;
  reviewedByUserId: string | null;
  reviewedAt: string | null;
  rejectionReason: string | null;
}

export interface RawLoan extends Omit<Loan, "status"> {
  status: RawLoanStatus;
}

export interface CreateLoanRequest {
  userId: string;
  amount: number;
  termInMonths: number;
  purpose: string;
}

export interface UpdateLoanRequest {
  amount: number;
  termInMonths: number;
  purpose: string;
}
