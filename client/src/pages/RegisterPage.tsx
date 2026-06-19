import { FormEvent, useState } from "react";
import { Link, useNavigate } from "react-router-dom";
import { BrandMark } from "../assets/brand";
import { Toast } from "../components/Toast";
import { resetAuthCache } from "../hooks/authSessionCache";
import { ApiError } from "../services/apiClient";
import { authService } from "../services/authService";

export const RegisterPage = () => {
  const navigate = useNavigate();
  const [name, setName] = useState("");
  const [email, setEmail] = useState("");
  const [password, setPassword] = useState("");
  const [error, setError] = useState<string | null>(null);
  const [isSubmitting, setIsSubmitting] = useState(false);

  const submit = async (event: FormEvent) => {
    event.preventDefault();
    setError(null);

    if (name.trim().length < 2) {
      setError("El nombre debe tener al menos 2 caracteres.");
      return;
    }

    if (password.length < 6) {
      setError("El password debe tener al menos 6 caracteres.");
      return;
    }

    setIsSubmitting(true);
    try {
      await authService.register({ name, email, password });
      resetAuthCache();
      navigate("/loans", { replace: true });
    } catch (err) {
      setError(err instanceof ApiError ? err.message : "No se pudo crear la cuenta.");
    } finally {
      setIsSubmitting(false);
    }
  };

  return (
    <main className="auth-page">
      <section className="auth-panel compact-auth">
        <div className="brand-row">
          <BrandMark />
          <div>
            <strong>Sistema Bancario</strong>
            <span>Registro de cliente</span>
          </div>
        </div>

        <form className="form-card" onSubmit={submit}>
          <Toast message={error} tone="error" />

          <label>
            Nombre
            <input value={name} onChange={(event) => setName(event.target.value)} required />
          </label>

          <label>
            Email
            <input
              type="email"
              value={email}
              onChange={(event) => setEmail(event.target.value)}
              required
            />
          </label>

          <label>
            Password
            <input
              type="password"
              value={password}
              onChange={(event) => setPassword(event.target.value)}
              minLength={6}
              required
            />
          </label>

          <button className="primary-button" type="submit" disabled={isSubmitting}>
            {isSubmitting ? "Creando..." : "Crear cuenta"}
          </button>
        </form>

        <p className="auth-switch">
          Ya tienes cuenta? <Link to="/login">Iniciar sesion</Link>
        </p>
      </section>
    </main>
  );
};
