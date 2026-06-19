# Frontend

## Tecnologia

- React
- TypeScript
- Vite
- React Router
- lucide-react
- CSS modular por estructura global
- pnpm

## Estructura

```text
client/
├── src/assets
├── src/components
├── src/hooks
├── src/pages
├── src/routes
├── src/services
├── src/styles
└── src/types
```

## Responsabilidades

### assets

Contiene piezas visuales reutilizables. Actualmente incluye `BrandMark`.

### components

Componentes compartidos:

- `AppShell`
- `RoutePending`
- `StatCard`
- `StatusBadge`
- `Toast`

### hooks

Maneja estado de autenticacion:

- `useAuth`
- `authSessionCache`

El cache de sesion evita llamadas repetitivas a `/api/auth/me` durante navegacion corta.

### routes

Define middlewares de rutas:

- `ProtectedRoute`: valida sesion.
- `PublicOnlyRoute`: evita que usuarios autenticados vuelvan a login/register.
- Autorizacion por rol con `roles={["Admin"]}`.

Este concepto fue tomado del patron de Fligo: validar auth, mostrar pending mientras carga y redirigir segun estado/rol.

### services

Cliente HTTP:

- `apiClient`
- `authService`
- `loanService`
- `userService`

Todas las llamadas usan:

```ts
credentials: "include"
```

Esto permite enviar la cookie JWT `HttpOnly` al backend.

### pages

Pantallas:

- `LoginPage`
- `RegisterPage`
- `LoansPage`
- `AdminLoansPage`
- `ForbiddenPage`
- `NotFoundPage`
- `InternalServerErrorPage`

## HTTPS en Desarrollo

`vite.config.ts` usa:

```ts
plugins: [react(), basicSsl()]
```

Y configura:

```ts
server: {
  https: {},
  port: 5173,
  strictPort: true
}
```

Esto permite correr el cliente en:

```text
https://localhost:5173
```

## Autenticacion del Cliente

El cliente no recibe ni guarda el token.

Despues de login/register:

1. El backend escribe la cookie.
2. El cliente llama `/api/auth/me`.
3. Se guarda temporalmente el usuario autenticado en cache de memoria.
4. Las rutas protegidas usan ese estado para permitir o redirigir.

## Pantallas

### Login

Permite iniciar sesion con email/password.

### Registro

Crea un usuario cliente y entra automaticamente porque el backend escribe la cookie.

### Prestamos

Disponible para usuarios autenticados:

- Crear solicitud.
- Ver estado.
- Ver monto, plazo y motivo de rechazo.

### Admin

Solo rol `Admin`:

- Ver todas las solicitudes.
- Ver nombre y email del usuario asociado a cada prestamo.
- Aprobar prestamos pendientes.
- Rechazar prestamos pendientes con motivo.

El panel admin usa `userService` para consultar `/api/users` y mapear cada prestamo por `userId`. Asi muestra informacion legible del usuario en lugar de UUID.

## Errores

El cliente tiene pantallas dedicadas:

- `/error/404`: ruta no encontrada.
- `/error/500`: error interno de API.

`apiClient` redirige automaticamente a `/error/500` cuando una respuesta HTTP viene con status `500` o superior.

## Estados de Prestamo

El backend envia `LoanStatus` como string. El cliente tambien normaliza valores numericos antiguos (`1`, `2`, `3`) para mantener compatibilidad con respuestas previas o cacheadas.

## Validaciones

El cliente valida:

- Campos requeridos.
- Password minimo.
- Monto entre `1` y `5,000,000`.
- Plazo entre `1` y `120`.
- Motivo requerido al rechazar.

Las validaciones del backend siguen siendo la fuente de verdad.
