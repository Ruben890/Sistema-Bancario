import { LogOut, ShieldCheck, WalletCards } from "lucide-react";
import { NavLink, Outlet, useNavigate } from "react-router-dom";
import { BrandMark } from "../assets/brand";
import { resetAuthCache } from "../hooks/authSessionCache";
import { useAuth } from "../hooks/useAuth";
import { authService } from "../services/authService";
import type { UserRole } from "../types/api";

const roleLabels: Record<UserRole, string> = {
  Admin: "Administrador",
  Customer: "Cliente"
};

const getRoleLabel = (role: UserRole | undefined) => role ? roleLabels[role] : "";

export const AppShell = () => {
  const { user } = useAuth();
  const navigate = useNavigate();

  const logout = async () => {
    await authService.logout().catch(() => undefined);
    resetAuthCache();
    navigate("/login", { replace: true });
  };

  return (
    <div className="app-shell">
      <aside className="sidebar">
        <div className="brand-row">
          <BrandMark />
          <div>
            <strong>Sistema Bancario</strong>
            <span>{getRoleLabel(user?.role)}</span>
          </div>
        </div>

        <nav className="nav-list">
          <NavLink to="/loans">
            <WalletCards size={18} />
            Prestamos
          </NavLink>
          {user?.role === "Admin" && (
            <NavLink to="/admin">
              <ShieldCheck size={18} />
              Revision admin
            </NavLink>
          )}
        </nav>

        <button className="ghost-button logout-button" type="button" onClick={logout}>
          <LogOut size={18} />
          Salir
        </button>
      </aside>

      <section className="content-shell">
        <header className="topbar">
          <div>
            <span>Sesion activa</span>
            <strong>{user?.name}</strong>
          </div>
          <span className="role-pill">{getRoleLabel(user?.role)}</span>
        </header>
        <Outlet />
      </section>
    </div>
  );
};
