# Sistema de Gestion de Prestamos Bancarios

Solucion completa para la prueba tecnica de .NET Core y Front-end. El sistema simula la gestion de prestamos bancarios: los clientes solicitan prestamos, consultan el estado de sus solicitudes y los administradores aprueban o rechazan cada solicitud.

## Alcance de la Prueba

La entrega cubre los requerimientos practicos del PDF:

- API Web en .NET con CRUD de usuarios y prestamos.
- Arquitectura limpia con separacion entre dominio, aplicacion, infraestructura, persistencia y API.
- PostgreSQL como base de datos relacional.
- Autenticacion con JWT usando cookies `HttpOnly`.
- Autorizacion por rol para que solo administradores aprueben o rechacen prestamos.
- Cache en memoria para consultas repetitivas del estado de prestamos.
- Transacciones durante aprobaciones y rechazos.
- Manejo centralizado de errores con respuestas HTTP claras.
- Tests unitarios para reglas criticas.
- Cliente React + TypeScript + Vite con rutas protegidas, validaciones y HTTPS en desarrollo.

## Estructura General

```text
.
├── Domain/                    # Reglas de negocio puras
├── Application/               # Casos de uso, contratos y Result<T>
├── Persistence/               # EF Core, PostgreSQL, repositorios y UnitOfWork
├── Infrastructure/            # JWT, cookies, cache y hashing
├── Sistema_bancario_backend/  # Web API, controllers, filtros y errores
├── Tests/                     # Pruebas unitarias
├── client/                    # React + TypeScript + Vite
├── docs/                      # Documentacion tecnica
├── scripts/                   # SQL para PostgreSQL
└── docker-compose.yml         # Contenedor PostgreSQL
```

## Arquitectura

El backend sigue una arquitectura limpia:

- `Domain` contiene entidades como `User` y `Loan`, enums y reglas de negocio. Esta capa no depende de frameworks.
- `Application` contiene servicios de caso de uso, contratos e interfaces. Los services devuelven `Result<T>` para errores esperados de aplicacion.
- `Persistence` implementa los repositorios con EF Core y PostgreSQL.
- `Infrastructure` implementa JWT, cookies, cache, blacklist de tokens y hashing de password.
- `Sistema_bancario_backend` expone HTTP y traduce los resultados de aplicacion en respuestas API.

El frontend sigue una estructura separada por responsabilidad:

- `pages`: pantallas.
- `components`: UI reutilizable.
- `routes`: rutas publicas/protegidas y autorizacion por rol.
- `hooks`: estado de autenticacion.
- `services`: llamadas HTTP con `credentials: "include"`.
- `types`: contratos TypeScript.

## Seguridad

La autenticacion usa JWT en cookie:

- El cliente no guarda token en `localStorage`.
- El backend escribe el JWT en una cookie `HttpOnly`.
- `JwtBearer` lee la cookie en `OnMessageReceived`.
- `OnTokenValidated` valida `userId + jti` contra una whitelist/blacklist en memoria.
- `logout` invalida el `jti` del token actual.
- El cliente corre con HTTPS en Vite para que el navegador envie cookies correctamente en desarrollo.

## Datos Demo

El backend ejecuta un inicializador al arrancar:

```text
Sistema_bancario_backend/Data/DatabaseBootstrapper.cs
```

Cuentas disponibles:

```text
Admin principal:
email: admin@bank.local
password: admin123

Admin demo:
email: admin@test.com
password: 123456

Usuario demo:
email: usuario@test.com
password: 123456
```

Tambien se crean usuarios adicionales y prestamos en estados `Pending`, `Approved` y `Rejected`.

## Ejecucion Rapida

Levantar PostgreSQL:

```bash
docker compose up -d
```

Ejecutar backend:

```bash
dotnet restore Sistema_bancario_backend.sln
dotnet run --project Sistema_bancario_backend/Sistema_bancario_backend.csproj
```

Ejecutar cliente:

```bash
cd client
pnpm install
pnpm dev
```

URLs:

```text
Backend HTTPS: https://localhost:7205
Cliente HTTPS: https://localhost:5173
OpenAPI JSON:  https://localhost:7205/openapi/v1.json
```

## Tests y Build

Backend:

```bash
dotnet build Sistema_bancario_backend.sln
dotnet test Tests/Tests.csproj
```

Frontend:

```bash
cd client
pnpm build
```

## Documentacion Tecnica

- [Backend](docs/backend.md)
- [Frontend](docs/frontend.md)
- [Ejecucion y entorno](docs/runbook.md)

El README principal resume el proyecto y la forma de correrlo. Los detalles tecnicos estan separados en `docs/` para mantener la entrega clara.
