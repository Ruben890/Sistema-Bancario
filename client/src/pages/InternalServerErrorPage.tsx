import { RefreshCcw } from "lucide-react";
import { Link } from "react-router-dom";

export const InternalServerErrorPage = () => (
  <main className="center-page">
    <section className="panel error-panel">
      <span>500</span>
      <h1>Error interno</h1>
      <p>La API no pudo completar la solicitud. Intenta nuevamente o revisa el estado del backend.</p>
      <div className="error-actions">
        <button className="ghost-button" type="button" onClick={() => window.location.reload()}>
          <RefreshCcw size={18} />
          Reintentar
        </button>
        <Link className="primary-button link-button" to="/loans">Ir al inicio</Link>
      </div>
    </section>
  </main>
);
