import { FormEvent, useState } from "react";
import { LockKeyhole, Mail } from "lucide-react";
import { Link, useNavigate } from "react-router-dom";
import { BrandMark } from "../assets/brand";
import { Toast } from "../components/Toast";
import { resetAuthCache } from "../hooks/authSessionCache";
import { ApiError } from "../services/apiClient";
import { authService } from "../services/authService";

export const LoginPage = () => {
  const navigate = useNavigate();
  const [email, setEmail] = useState("admin@bank.local");
  const [password, setPassword] = useState("admin123");
  const [error, setError] = useState<string | null>(null);
  const [isSubmitting, setIsSubmitting] = useState(false);

  const submit = async (event: FormEvent) => {
    event.preventDefault();
    setError(null);

    if (!email.trim() || !password.trim()) {
      setError("Email y password son requeridos.");
      return;
    }

    setIsSubmitting(true);
    try {
      await authService.login({ email, password });
      resetAuthCache();
      const session = await authService.me();
      navigate(session.entity?.role === "Admin" ? "/admin" : "/loans", { replace: true });
    } catch (err) {
      setError(err instanceof ApiError ? err.message : "No se pudo iniciar sesion.");
    } finally {
      setIsSubmitting(false);
    }
  };

  return (
    <main className="auth-page">
      <section className="auth-panel">
        <div className="brand-row">
          <BrandMark />
          <div>
            <strong>Sistema Bancario</strong>
            <span>Gestion de prestamos</span>
          </div>
        </div>

        <div className="auth-copy">
          <h1>Acceso seguro</h1>
          <p>Administra solicitudes, revisa estados y protege la sesion</p>
        </div>

        <form className="form-card" onSubmit={submit}>
          <Toast message={error} tone="error" />

          <label>
            Email
            <div className="input-icon">
              <Mail size={18} />
              <input
                type="email"
                value={email}
                onChange={(event) => setEmail(event.target.value)}
                autoComplete="email"
                required
              />
            </div>
          </label>

          <label>
            Password
            <div className="input-icon">
              <LockKeyhole size={18} />
              <input
                type="password"
                value={password}
                onChange={(event) => setPassword(event.target.value)}
                autoComplete="current-password"
                required
              />
            </div>
          </label>

          <button className="primary-button" type="submit" disabled={isSubmitting}>
            {isSubmitting ? "Validando..." : "Entrar"}
          </button>
        </form>

        <p className="auth-switch">
          No tienes cuenta? <Link to="/register">Crear cuenta cliente</Link>
        </p>
      </section>
    </main>
  );
};
