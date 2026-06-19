import { Navigate, Outlet } from "react-router-dom";
import { RoutePending } from "../components/RoutePending";
import { useAuth } from "../hooks/useAuth";
import type { UserRole } from "../types/api";

interface ProtectedRouteProps {
  roles?: UserRole[];
}

export const ProtectedRoute = ({ roles }: ProtectedRouteProps) => {
  const { isAuthenticated, isLoading, user } = useAuth();

  if (isLoading) return <RoutePending />;

  if (!isAuthenticated) {
    return <Navigate to="/login" replace />;
  }

  const hasRole = roles ? Boolean(user?.role && roles.includes(user.role)) : true;
  if (!hasRole) {
    return <Navigate to="/forbidden" replace />;
  }

  return <Outlet />;
};
