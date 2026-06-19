import type { LoanStatus } from "../types/api";

const labels: Record<LoanStatus, string> = {
  Pending: "Pendiente",
  Approved: "Aprobado",
  Rejected: "Rechazado"
};

const validStatuses: LoanStatus[] = ["Pending", "Approved", "Rejected"];

type StatusBadgeProps = {
  status: unknown;
};

const isLoanStatus = (value: unknown): value is LoanStatus => {
  return typeof value === "string" && validStatuses.includes(value as LoanStatus);
};

export const StatusBadge = ({ status }: StatusBadgeProps) => {
  const safeStatus: LoanStatus | null = isLoanStatus(status) ? status : null;

  const cssStatus = safeStatus ? safeStatus.toLowerCase() : "unknown";
  const label = safeStatus ? labels[safeStatus] : "Desconocido";

  return (
    <span className={`status-badge status-${cssStatus}`}>
      {label}
    </span>
  );
};