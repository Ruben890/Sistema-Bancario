import { Link } from "react-router-dom";

export const ForbiddenPage = () => (
  <main className="center-page">
    <section className="panel error-panel">
      <span>403</span>
      <h1>Acceso restringido</h1>
      <p>Tu usuario no tiene permiso para abrir esta seccion.</p>
      <Link className="primary-button link-button" to="/loans">Volver a prestamos</Link>
    </section>
  </main>
);
