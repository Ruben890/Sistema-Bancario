import { Link } from "react-router-dom";

export const NotFoundPage = () => (
  <main className="center-page">
    <section className="panel error-panel">
      <span>404</span>
      <h1>Ruta no encontrada</h1>
      <p>La pantalla solicitada no existe en el cliente.</p>
      <Link className="primary-button link-button" to="/loans">Ir al inicio</Link>
    </section>
  </main>
);
