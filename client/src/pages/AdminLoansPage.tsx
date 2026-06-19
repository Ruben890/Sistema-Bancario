import { useEffect, useMemo, useState } from "react";
import { ClipboardList, FileX2, ShieldCheck } from "lucide-react";
import { StatCard } from "../components/StatCard";
import { StatusBadge } from "../components/StatusBadge";
import { Toast } from "../components/Toast";
import { ApiError } from "../services/apiClient";
import { loanService } from "../services/loanService";
import { userService } from "../services/userService";
import type { Loan, User } from "../types/api";

export const AdminLoansPage = () => {
  const [loans, setLoans] = useState<Loan[]>([]);
  const [users, setUsers] = useState<User[]>([]);
  const [error, setError] = useState<string | null>(null);
  const [message, setMessage] = useState<string | null>(null);
  const [rejectingLoanId, setRejectingLoanId] = useState<string | null>(null);
  const [rejectionReason, setRejectionReason] = useState("Riesgo crediticio no aceptado");
  const [isLoading, setIsLoading] = useState(true);

  const summary = useMemo(() => ({
    total: loans.length,
    pending: loans.filter((loan) => loan.status === "Pending").length,
    rejected: loans.filter((loan) => loan.status === "Rejected").length
  }), [loans]);

  const usersById = useMemo(() => {
    return new Map(users.map((user) => [user.id, user]));
  }, [users]);

  const loadLoans = async () => {
    setIsLoading(true);
    setError(null);
    try {
      const [loansResult, usersResult] = await Promise.all([
        loanService.list(),
        userService.list()
      ]);

      setLoans(loansResult.entity ?? []);
      setUsers(usersResult.entity ?? []);
    } catch (err) {
      setError(err instanceof ApiError ? err.message : "No se pudieron cargar los prestamos.");
    } finally {
      setIsLoading(false);
    }
  };

  useEffect(() => {
    loadLoans();
  }, []);

  const updateLoan = (loan: Loan) => {
    setLoans((current) => current.map((item) => item.id === loan.id ? loan : item));
  };

  const approve = async (loanId: string) => {
    setError(null);
    setMessage(null);
    try {
      const result = await loanService.approve(loanId);
      updateLoan(result.entity!);
      setMessage(result.message ?? "Prestamo aprobado.");
    } catch (err) {
      setError(err instanceof ApiError ? err.message : "No se pudo aprobar el prestamo.");
    }
  };

  const reject = async (loanId: string) => {
    setError(null);
    setMessage(null);

    if (!rejectionReason.trim()) {
      setError("El motivo de rechazo es requerido.");
      return;
    }

    try {
      const result = await loanService.reject(loanId, rejectionReason);
      updateLoan(result.entity!);
      setRejectingLoanId(null);
      setMessage(result.message ?? "Prestamo rechazado.");
    } catch (err) {
      setError(err instanceof ApiError ? err.message : "No se pudo rechazar el prestamo.");
    }
  };

  return (
    <main className="page-stack">
      <section className="page-heading">
        <div>
          <span>Revision administrativa</span>
          <h1>Aprobar o rechazar prestamos</h1>
        </div>
        <button className="ghost-button" type="button" onClick={loadLoans}>Actualizar</button>
      </section>

      <section className="stats-grid">
        <StatCard label="Solicitudes" value={summary.total} icon={ClipboardList} />
        <StatCard label="Pendientes" value={summary.pending} icon={ShieldCheck} />
        <StatCard label="Rechazados" value={summary.rejected} icon={FileX2} />
      </section>

      <section className="panel">
        <Toast message={message} />
        <Toast message={error} tone="error" />

        {isLoading ? (
          <p className="empty-state">Cargando solicitudes...</p>
        ) : loans.length === 0 ? (
          <p className="empty-state">No hay prestamos registrados.</p>
        ) : (
          <div className="admin-table">
            {loans.map((loan) => (
              <article className="admin-loan-row" key={loan.id}>
                <div className="loan-main">
                  <strong>RD$ {loan.amount.toLocaleString()}</strong>
                  <span>{loan.termInMonths} meses - {loan.purpose}</span>
                  <small>
                    Usuario: {usersById.get(loan.userId)?.name ?? "Usuario no encontrado"}
                  </small>
                  <small>
                    Email: {usersById.get(loan.userId)?.email ?? "No disponible"}
                  </small>
                </div>

                <StatusBadge status={loan.status} />

                {loan.status === "Pending" ? (
                  <div className="admin-actions">
                    <button className="approve-button" type="button" onClick={() => approve(loan.id)}>
                      Aprobar
                    </button>
                    <button className="reject-button" type="button" onClick={() => setRejectingLoanId(loan.id)}>
                      Rechazar
                    </button>
                  </div>
                ) : (
                  <span className="reviewed-text">Revisado</span>
                )}

                {rejectingLoanId === loan.id && (
                  <div className="reject-box">
                    <textarea
                      value={rejectionReason}
                      onChange={(event) => setRejectionReason(event.target.value)}
                      rows={3}
                    />
                    <div>
                      <button className="reject-button" type="button" onClick={() => reject(loan.id)}>
                        Confirmar rechazo
                      </button>
                      <button className="ghost-button" type="button" onClick={() => setRejectingLoanId(null)}>
                        Cancelar
                      </button>
                    </div>
                  </div>
                )}
              </article>
            ))}
          </div>
        )}
      </section>
    </main>
  );
};
