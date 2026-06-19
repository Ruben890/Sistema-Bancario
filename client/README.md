# Cliente - Sistema Bancario

Cliente React + TypeScript + Vite para la prueba tecnica de gestion de prestamos.

## Estructura

- `src/assets`: piezas visuales reutilizables.
- `src/components`: componentes compartidos de UI.
- `src/hooks`: estado de autenticacion y cache corta de sesion.
- `src/pages`: pantallas de login, registro, prestamos y administracion.
- `src/routes`: rutas publicas, protegidas y autorizacion por rol.
- `src/services`: cliente HTTP y servicios de API.
- `src/types`: tipos compartidos del contrato con backend.
- `src/styles`: estilos globales.

## Seguridad y Cookies

El backend escribe el JWT en una cookie `HttpOnly`. Por eso el cliente no guarda tokens en `localStorage` ni en memoria.

Todas las peticiones usan:

```ts
credentials: "include"
```

El servidor de Vite corre en HTTPS para que el navegador acepte el flujo de cookies en desarrollo.

## Variables de Entorno

Copia el ejemplo si necesitas cambiar la URL de la API:

```bash
cp .env.example .env
```

Valor por defecto:

```text
VITE_API_BASE_URL=https://localhost:7205
```

## Instalar Dependencias

Desde esta carpeta:

```bash
pnpm install
```

## Ejecutar en Desarrollo

```bash
pnpm dev
```

URL:

```text
https://localhost:5173
```

El navegador puede pedir aceptar el certificado local generado por Vite.

## Build

```bash
pnpm build
```

## Cuentas Demo

El backend crea datos iniciales:

```text
Admin:
email: admin@test.com
password: 123456

Usuario:
email: usuario@test.com
password: 123456
```

Tambien se crea el admin principal configurado en `appsettings.json`:

```text
email: admin@bank.local
password: admin123
```

## Rutas

- `/login`: inicio de sesion.
- `/register`: registro de cliente.
- `/loans`: solicitudes y estado de prestamos.
- `/admin`: revision administrativa, solo rol `Admin`.
- `/forbidden`: acceso denegado.
