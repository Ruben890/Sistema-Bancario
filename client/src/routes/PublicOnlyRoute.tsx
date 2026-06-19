import { Navigate, Outlet } from "react-router-dom";
import { RoutePending } from "../components/RoutePending";
import { useAuth } from "../hooks/useAuth";

export const PublicOnlyRoute = () => {
  const { isAuthenticated, isLoading, user } = useAuth();

  if (isLoading) return <RoutePending />;

  if (isAuthenticated) {
    return <Navigate to={user?.role === "Admin" ? "/admin" : "/loans"} replace />;
  }

  return <Outlet />;
};
