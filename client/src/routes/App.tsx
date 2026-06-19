import { Navigate, Route, Routes } from "react-router-dom";
import { AppShell } from "../components/AppShell";
import { AdminLoansPage } from "../pages/AdminLoansPage";
import { ForbiddenPage } from "../pages/ForbiddenPage";
import { InternalServerErrorPage } from "../pages/InternalServerErrorPage";
import { LoansPage } from "../pages/LoansPage";
import { LoginPage } from "../pages/LoginPage";
import { NotFoundPage } from "../pages/NotFoundPage";
import { RegisterPage } from "../pages/RegisterPage";
import { ProtectedRoute } from "./ProtectedRoute";
import { PublicOnlyRoute } from "./PublicOnlyRoute";

export const App = () => (
  <Routes>
    <Route element={<PublicOnlyRoute />}>
      <Route path="/login" element={<LoginPage />} />
      <Route path="/register" element={<RegisterPage />} />
    </Route>

    <Route element={<ProtectedRoute />}>
      <Route element={<AppShell />}>
        <Route path="/" element={<Navigate to="/loans" replace />} />
        <Route path="/loans" element={<LoansPage />} />
      </Route>
    </Route>

    <Route element={<ProtectedRoute roles={["Admin"]} />}>
      <Route element={<AppShell />}>
        <Route path="/admin" element={<AdminLoansPage />} />
      </Route>
    </Route>

    <Route path="/forbidden" element={<ForbiddenPage />} />
    <Route path="/error/500" element={<InternalServerErrorPage />} />
    <Route path="/error/404" element={<NotFoundPage />} />
    <Route path="*" element={<NotFoundPage />} />
  </Routes>
);
