# Ejecucion y Entorno

## Requisitos

- .NET SDK 10
- Docker
- Docker Compose
- Node.js
- pnpm

## Base de Datos

Levantar PostgreSQL:

```bash
docker compose up -d
```

Verificar:

```bash
docker compose ps
```

Detener:

```bash
docker compose down
```

Eliminar datos locales:

```bash
docker compose down -v
```

## Conexion

```text
Host=localhost;Port=5432;Database=sistema_bancario;Username=admin;Password=admin
```

Configurada en:

```text
Sistema_bancario_backend/appsettings.json
```

## Backend

Restaurar:

```bash
dotnet restore Sistema_bancario_backend.sln
```

Compilar:

```bash
dotnet build Sistema_bancario_backend.sln
```

Ejecutar:

```bash
dotnet run --project Sistema_bancario_backend/Sistema_bancario_backend.csproj
```

URL HTTPS:

```text
https://localhost:7205
```

OpenAPI:

```text
https://localhost:7205/openapi/v1.json
```

## Frontend

Instalar:

```bash
cd client
pnpm install
```

Ejecutar:

```bash
pnpm dev
```

URL:

```text
https://localhost:5173
```

Build:

```bash
pnpm build
```

## Orden Recomendado

1. `docker compose up -d`
2. `dotnet run --project Sistema_bancario_backend/Sistema_bancario_backend.csproj`
3. `cd client`
4. `pnpm dev`
5. Abrir `https://localhost:5173`

## Cuentas Demo

```text
Admin:
email: admin@test.com
password: 123456

Usuario:
email: usuario@test.com
password: 123456

Admin principal:
email: admin@bank.local
password: admin123
```

## Validacion

Backend:

```bash
dotnet test Tests/Tests.csproj
```

Frontend:

```bash
cd client
pnpm build
```

Docker:

```bash
docker compose config
```
