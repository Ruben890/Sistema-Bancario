# Backend

## Tecnologia

- .NET 10
- ASP.NET Core Web API
- Entity Framework Core
- PostgreSQL
- JWT Bearer Authentication
- MemoryCache
- xUnit

## Estructura

```text
Domain/
Application/
Persistence/
Infrastructure/
Sistema_bancario_backend/
Tests/
```

## Capas

### Domain

Contiene reglas puras:

- `User`
- `Loan`
- `UserRole`
- `LoanStatus`
- `DomainException`

Ejemplos de reglas:

- Un prestamo debe tener monto mayor que cero.
- El plazo debe estar entre 1 y 120 meses.
- Solo prestamos pendientes se pueden aprobar, rechazar, editar o eliminar.
- Un rechazo requiere motivo.

### Application

Contiene casos de uso:

- `AuthService`
- `UserService`
- `LoanService`

Tambien define contratos:

- `IUserRepository`
- `ILoanRepository`
- `IUnitOfWork`
- `IJwtTokenService`
- `IJwtTokenBlacklist`
- `ILoanStatusCache`
- `IAuthCookieService`

Los services devuelven `Result<T>`. Los errores esperados de aplicacion, como entidad no encontrada o email duplicado, se devuelven como `Result.Failure(...)`.

### Persistence

Implementa acceso a datos:

- `BankingDbContext`
- `UserRepository`
- `LoanRepository`
- `UnitOfWork`
- Configuraciones EF por entidad

Usa schema:

```text
banking
```

Tablas principales:

- `banking.users`
- `banking.loans`

### Infrastructure

Implementa detalles tecnicos:

- JWT
- Cookies `HttpOnly`
- Blacklist/whitelist de tokens por `userId + jti`
- Cache en memoria para estados de prestamos
- Hashing de password con PBKDF2 + salt aleatorio

### API

Proyecto:

```text
Sistema_bancario_backend/
```

Contiene:

- Controllers
- `StandardResponseFilter`
- `GlobalExceptionHandler`
- `DatabaseBootstrapper`

Los controllers devuelven `new ObjectResult(result)` y el filtro aplica el status code que viene en `Result<T>`.

## Autenticacion

Flujo:

1. `POST /api/auth/login`
2. El backend valida credenciales.
3. Genera JWT.
4. Registra `userId + jti` en memoria.
5. Escribe JWT en cookie `HttpOnly`.
6. En cada request, `JwtBearer` lee la cookie.
7. `OnTokenValidated` valida que el token siga permitido.

Logout:

1. `POST /api/auth/logout`
2. Se toma el `jti` del token validado.
3. Se mueve a blacklist.
4. Se limpia la cookie.

## Errores

- `DomainException` se captura en `GlobalExceptionHandler` y devuelve `400`.
- Errores no controlados devuelven `500`.
- `UnauthorizedAccessException` devuelve `401`.
- `Result.Failure` permite errores esperados sin lanzar excepciones.

## Transacciones

La aprobacion y rechazo usan:

```text
IUnitOfWork.ExecuteInTransactionAsync(...)
```

Esto garantiza consistencia al cambiar el estado del prestamo.

## Cache

`LoanService.GetStatusAsync` usa `ILoanStatusCache` para reducir consultas repetitivas del estado de un prestamo.

Cuando un prestamo cambia de estado, la cache se actualiza.

## Scripts de Base de Datos

Archivo:

```text
scripts/init-db.sql
```

Crea:

- Base de datos `sistema_bancario`
- Schema `banking`
- Tablas
- Indices
- Foreign keys
- Constraints

## Tests

Proyecto:

```text
Tests/
```

Incluye pruebas para:

- Creacion de prestamos pendientes.
- Aprobacion.
- Bloqueo de doble revision.
- Rechazo con motivo requerido.
- Cache de estado.
