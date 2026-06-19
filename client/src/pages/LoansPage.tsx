import { FormEvent, useEffect, useMemo, useState } from "react";
import { BadgeDollarSign, Clock3, FileCheck2 } from "lucide-react";
import { StatCard } from "../components/StatCard";
import { StatusBadge } from "../components/StatusBadge";
import { Toast } from "../components/Toast";
import { useAuth } from "../hooks/useAuth";
import { ApiError } from "../services/apiClient";
import { loanService } from "../services/loanService";
import type { Loan } from "../types/api";

export const LoansPage = () => {
  const { user } = useAuth();
  const [loans, setLoans] = useState<Loan[]>([]);
  const [amount, setAmount] = useState("50000");
  const [termInMonths, setTermInMonths] = useState("24");
  const [purpose, setPurpose] = useState("Capital de trabajo");
  const [message, setMessage] = useState<string | null>(null);
  const [error, setError] = useState<string | null>(null);
  const [isLoading, setIsLoading] = useState(true);
  const [isSubmitting, setIsSubmitting] = useState(false);

  const summary = useMemo(() => {
    const pending = loans.filter((loan) => loan.status === "Pending").length;
    const approved = loans.filter((loan) => loan.status === "Approved").length;
    const totalApproved = loans
      .filter((loan) => loan.status === "Approved")
      .reduce((total, loan) => total + loan.amount, 0);

    return { pending, approved, totalApproved };
  }, [loans]);

  const loadLoans = async () => {
    setIsLoading(true);
    try {
      const result = await loanService.list();
      setLoans(result.entity ?? []);
    } catch (err) {
      setError(err instanceof ApiError ? err.message : "No se pudieron cargar los prestamos.");
    } finally {
      setIsLoading(false);
    }
  };

  useEffect(() => {
    loadLoans();
  }, []);

  const submitLoan = async (event: FormEvent) => {
    event.preventDefault();
    setMessage(null);
    setError(null);

    const parsedAmount = Number(amount);
    const parsedTerm = Number(termInMonths);

    if (!user?.userId) {
      setError("Sesion invalida.");
      return;
    }

    if (parsedAmount <= 0 || parsedAmount > 5_000_000) {
      setError("El monto debe estar entre 1 y 5,000,000.");
      return;
    }

    if (parsedTerm < 1 || parsedTerm > 120) {
      setError("El plazo debe estar entre 1 y 120 meses.");
      return;
    }

    if (!purpose.trim()) {
      setError("El proposito es requerido.");
      return;
    }

    setIsSubmitting(true);
    try {
      const result = await loanService.create({
        userId: user.userId,
        amount: parsedAmount,
        termInMonths: parsedTerm,
        purpose
      });
      setLoans((current) => [result.entity!, ...current]);
      setMessage(result.message ?? "Solicitud creada.");
    } catch (err) {
      setError(err instanceof ApiError ? err.message : "No se pudo solicitar el prestamo.");
    } finally {
      setIsSubmitting(false);
    }
  };

  return (
    <main className="page-stack">
      <section className="page-heading">
        <div>
          <span>Prestamos</span>
          <h1>Solicitudes y estado</h1>
        </div>
      </section>

      <section className="stats-grid">
        <StatCard label="Pendientes" value={summary.pending} icon={Clock3} />
        <StatCard label="Aprobados" value={summary.approved} icon={FileCheck2} />
        <StatCard label="Monto aprobado" value={`RD$ ${summary.totalApproved.toLocaleString()}`} icon={BadgeDollarSign} />
      </section>

      <section className="workspace-grid">
        <form className="panel form-panel" onSubmit={submitLoan}>
          <h2>Nueva solicitud</h2>
          <Toast message={message} />
          <Toast message={error} tone="error" />

          <label>
            Monto
            <input type="number" value={amount} onChange={(event) => setAmount(event.target.value)} min={1} max={5000000} />
          </label>

          <label>
            Plazo en meses
            <input type="number" value={termInMonths} onChange={(event) => setTermInMonths(event.target.value)} min={1} max={120} />
          </label>

          <label>
            Proposito
            <textarea value={purpose} onChange={(event) => setPurpose(event.target.value)} rows={4} />
          </label>

          <button className="primary-button" type="submit" disabled={isSubmitting}>
            {isSubmitting ? "Enviando..." : "Enviar solicitud"}
          </button>
        </form>

        <section className="panel">
          <div className="panel-heading">
            <h2>Prestamos solicitados</h2>
            <button className="ghost-button" type="button" onClick={loadLoans}>Actualizar</button>
          </div>

          {isLoading ? (
            <p className="empty-state">Cargando prestamos...</p>
          ) : loans.length === 0 ? (
            <p className="empty-state">Aun no tienes solicitudes.</p>
          ) : (
            <div className="loan-list">
              {loans.map((loan) => (
                <article className="loan-row" key={loan.id}>
                  <div className="loan-row-main">
                    <div className="loan-row-title">
                      <strong>RD$ {loan.amount.toLocaleString()}</strong>
                      <StatusBadge status={loan.status} />
                    </div>

                    <span>{loan.termInMonths} meses - {loan.purpose}</span>

                    <div className="loan-meta">
                      <small>Solicitado por: {loan.user?.name ?? "Usuario no encontrado"}</small>
                      <small>Email: {loan.user?.email ?? "No disponible"}</small>
                    </div>

                    {loan.rejectionReason && (
                      <small className="loan-rejection">Motivo: {loan.rejectionReason}</small>
                    )}
                  </div>
                </article>
              ))}
            </div>
          )}
        </section>
      </section>
    </main>
  );
};
