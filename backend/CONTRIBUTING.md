# Backend Contributing Guide

## Architecture

- **Cls.Api** — HTTP transport only (controllers, middleware, auth attributes)
- **Cls.Application** — CQRS handlers, validators, application services
- **Cls.Domain** — entities and domain enums
- **Cls.Infrastructure** — EF Core, JWT, MinIO, background jobs
- **Cls.Shared** — API contracts (DTOs) and cross-cutting exceptions

## Naming

| Artifact | Convention |
|----------|------------|
| Command | `{Verb}{Entity}Command` |
| Query | `{Verb}{Entity}Query` or `List{Entities}Query` |
| Validator | `{CommandOrQuery}Validator` — always validates MediatR requests |
| EF config | `{Entity}Configuration.cs` |
| Controller | `{Resource}Controller.cs` — keep thin |

## Application layout

```
Auth/
  Commands/AuthenticateCommand.cs
  Validators/

Users/
  Commands/     CreateUser, UpdateUser, CreateEmployee, UpdateEmployee, ...
  Queries/      ListUsers, GetUserById, GetEmployeeById
  Validators/
  Services/     IPasswordHasher
  Helpers/      UserCommandHelpers (internal)

Clients/ & Providers/
  Queries/      return ClientResponse / ProviderResponse DTOs
  Guards/       deletion guards per entity

General/
  Currencies/Queries/ListCurrenciesQuery.cs

Api/Services/
  IProfilePictureOrchestrator   shared client/provider profile upload
  IOrderFileOrchestrator         shared order file/invoice upload helpers

Orders/
  Queries/GetOrderDetailQuery    enriched order read model
  Services/OrderLogVisibility    visible log type filter
  Services/OrderContractEnricher payment/note URL enrichment
```

1. **API contract is frozen** — do not change routes, request fields, or response JSON shapes without explicit approval.
2. **No DB migrations** unless explicitly approved.
3. **Validators target Commands/Queries**, never API DTOs directly.
4. **Handlers do not call `SaveChangesAsync`** except order flows that require a mid-command flush (document with a comment).
5. **One DB user lookup per request** — `UserCheckerMiddleware` loads the user; `ClsAuthorizeAttribute` reads from `HttpContext.Items`.
6. **No sync-over-async** in filters or middleware.
7. **Errors** use `{ status, detail }` JSON via `ExceptionHandlerMiddleware`.
8. **Validation failures** return HTTP 422 with the same error shape.

## Running locally

```bash
docker compose -f docker-compose.dev.yml up -d   # Postgres + MinIO
./run_backend.sh                                  # API on :40585
```

Or full stack:

```bash
docker compose up -d
```

## Tests

Smoke and regression tests require Postgres (Docker) on `localhost:5432`:

```bash
dotnet test tests/Cls.Tests
```

Regression tests cover role guards, backup conflict/error shapes, assigned-employee order access, and validation (422) responses.

## Pull request checklist

- [ ] Smoke tests pass
- [ ] No route or response contract changes
- [ ] No migration files added
- [ ] Validators wired to MediatR requests
