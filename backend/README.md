# CLS — Canadian Logistics Solution

.NET 9 · Clean Architecture · CQRS · PostgreSQL · MinIO

## Quick Start (Dev)

```bash
# 1. Start Postgres + MinIO
docker compose -f docker-compose.dev.yml up -d

# 2. Run the API
./run_backend.sh
# → http://localhost:40585/swagger
```

Run smoke tests (requires Postgres via Docker):

```bash
dotnet test tests/Cls.Tests
```

**Default dev accounts:**

| Role | Email | Password |
|------|-------|----------|
| Admin | `admin@cls.local` | `Admin@2026!` |
| Manager | `manager@cls.local` | `Manager@2026!` |
| Employee | `employee@cls.local` | `Employee@2026!` |

## Profiles

| Profile | Environment | How to run |
|---------|-------------|------------|
| **Dev** | `Development` | `./run_backend.sh` (API runs locally, DB + MinIO in Docker) |
| **Prod** | `Production` | `docker compose up -d` (entire stack in Docker) |

## Project Structure

```
backend/
├── src/
│   ├── Cls.Api              # REST API, controllers, middleware
│   ├── Cls.Application      # CQRS commands/queries, MediatR handlers
│   ├── Cls.Domain           # Domain entities
│   ├── Cls.Infrastructure   # EF Core, MinIO, JWT, reCAPTCHA
│   └── Cls.Shared           # DTOs, contracts, exceptions
├── tests/Cls.Tests          # Smoke / integration tests
├── docker-compose.yml       # Prod: full stack (API + Postgres + MinIO)
├── docker-compose.dev.yml   # Dev: dependencies only (Postgres + MinIO)
├── .env                     # Dev environment variables
└── run_backend.sh           # Dev convenience script
```

## Configuration

All config flows through **environment variables**. Sources by profile:

- **Dev:** `launchSettings.json` → overrides `appsettings.json`
- **Prod:** `.env` → `docker-compose.yml` → container env vars

Key variables:

| Variable | Purpose |
|----------|---------|
| `ConnectionStrings__Default` | PostgreSQL connection |
| `Jwt__Key` | JWT signing key |
| `Minio__AccessKey` / `Minio__SecretKey` | MinIO credentials |
| `Recaptcha__Enabled` | Enable/disable reCAPTCHA |
