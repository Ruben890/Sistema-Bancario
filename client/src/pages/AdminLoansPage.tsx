import { useEffect, useMemo, useState } from "react";
import { ClipboardList, FileX2, ShieldCheck } from "lucide-react";
import { StatCard } from "../components/StatCard";
import { StatusBadge } from "../components/StatusBadge";
import { Toast } from "../components/Toast";
import { useAuth } from "../hooks/useAuth";
import { ApiError } from "../services/apiClient";
import { loanService } from "../services/loanService";
import type { Loan } from "../types/api";

export const AdminLoansPage = () => {
  const { user } = useAuth();
  const [loans, setLoans] = useState<Loan[]>([]);
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

  const loadLoans = async () => {
    setIsLoading(true);
    setError(null);
    try {
      const loansResult = await loanService.list();
      setLoans(loansResult.entity ?? []);
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
            {loans.map((loan) => {
              const isOwnLoan = loan.userId === user?.userId;

              return (
                <article className="admin-loan-row" key={loan.id}>
                  <div className="loan-main">
                    <strong>RD$ {loan.amount.toLocaleString()}</strong>
                    <span>{loan.termInMonths} meses - {loan.purpose}</span>
                    <small>
                      Usuario: {loan.user?.name ?? "Usuario no encontrado"}
                    </small>
                    <small>
                      Email: {loan.user?.email ?? "No disponible"}
                    </small>
                  </div>

                  <StatusBadge status={loan.status} />

                  {loan.status === "Pending" && isOwnLoan ? (
                    <span className="self-review-note">Espera validacion de otro admin</span>
                  ) : loan.status === "Pending" ? (
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
              );
            })}
          </div>
        )}
      </section>
    </main>
  );
};
