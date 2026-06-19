import { LoaderCircle } from "lucide-react";

export const RoutePending = () => (
  <main className="route-pending">
    <LoaderCircle className="spin" size={28} />
    <span>Validando sesion</span>
  </main>
);
