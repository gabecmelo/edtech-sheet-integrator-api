# EdTech Sheet-Integrator API

A production-grade .NET 9 SaaS backend that automates the grading of student spreadsheet submissions.  
An instructor uploads an Excel (`.xlsx`) or CSV answer sheet; the system parses it, evaluates each response against the assessment's answer key, computes a score, and persists the grading result for retrieval and analytics.

Built as a portfolio piece demonstrating **Clean Architecture**, **Quality Engineering** (xUnit + Stryker.NET mutation testing), and **DevOps** (multi-stage Docker + GitHub Actions CI).

---

## Architecture

The solution follows a strict four-layer Clean Architecture with enforced dependency rules (validated by NetArchTest in CI):

```
┌──────────────────────────────────────────────┐
│  API  (Minimal APIs, JWT, OpenAPI, Serilog)  │
├──────────────────────────────────────────────┤
│  Infrastructure  (EF Core 9, ClosedXML, CSV) │
├──────────────────────────────────────────────┤
│  Application  (Use Cases, FluentValidation)  │
├──────────────────────────────────────────────┤
│  Domain  (Aggregates, Value Objects, Rules)  │
└──────────────────────────────────────────────┘
          ↑  dependency direction
```

**Dependency rule:** each layer may only reference layers below it. Domain has zero external dependencies.

### Project layout

```
src/
├── EdTech.SheetIntegrator.Domain/          # Entities, value objects, domain logic
├── EdTech.SheetIntegrator.Application/     # Use cases, DTOs, repository interfaces
├── EdTech.SheetIntegrator.Infrastructure/  # EF Core, sheet parsers, Serilog
└── EdTech.SheetIntegrator.Api/             # Minimal API endpoints, JWT, health checks

tests/
├── EdTech.SheetIntegrator.Domain.UnitTests/
├── EdTech.SheetIntegrator.Application.UnitTests/
├── EdTech.SheetIntegrator.Infrastructure.IntegrationTests/  # Testcontainers SQL Server
├── EdTech.SheetIntegrator.Api.IntegrationTests/             # WebApplicationFactory
└── EdTech.SheetIntegrator.ArchTests/                        # NetArchTest dependency rules
```

### Key domain concepts

| Concept | Description |
|---|---|
| `Assessment` | Aggregate root — holds an answer key (list of `Question`s) and exposes a pure `Grade()` method |
| `Question` | Value object — `QuestionId`, correct answer, points, `MatchMode` (Exact / CaseInsensitive / Numeric±tolerance) |
| `StudentSubmission` | Aggregate root — student answers; transitions from *ungraded* to *graded* via `AttachResult()` |
| `GradingResult` | Value object — per-question outcomes + `Score` (earned / total / percentage) |

---

## Tech stack

| Concern | Choice |
|---|---|
| Framework | ASP.NET Core 9 Minimal APIs |
| ORM | EF Core 9 + SQL Server (JSON columns for owned types) |
| Excel parsing | ClosedXML |
| CSV parsing | CsvHelper |
| Validation | FluentValidation |
| Logging | Serilog (structured JSON) |
| Auth | JWT HS256 bearer — `Instructor` role policy |
| OpenAPI | `Microsoft.AspNetCore.OpenApi` + Scalar UI |
| Unit tests | xUnit + FluentAssertions + NSubstitute |
| Integration tests | Testcontainers for .NET (SQL Server 2022) |
| Architecture tests | NetArchTest.Rules |
| Mutation testing | Stryker.NET |
| Container | Multi-stage Docker → `aspnet:9.0-noble-chiseled` (non-root, distroless-like) |
| CI | GitHub Actions |

---

## Local development

### Prerequisites

- [.NET 9 SDK](https://dotnet.microsoft.com/download/dotnet/9)
- [Docker Desktop](https://www.docker.com/products/docker-desktop/) (for Testcontainers and `docker compose`)
- SQL Server — or spin one up via compose (see below)

### Option A — docker compose (recommended)

```bash
# 1. Copy the secrets template and fill in values
cp .env.example .env

# 2. Start SQL Server + API (builds the image automatically)
docker compose up -d

# 3. Apply EF migrations (first run only)
dotnet tool restore
dotnet ef database update --project src/EdTech.SheetIntegrator.Infrastructure

# 4. OpenAPI UI
open http://localhost:8080/docs
```

### Option B — dotnet run (bare metal)

```bash
# 1. Start SQL Server separately (or use docker run)
docker run --name edtech-sql \
  -e "ACCEPT_EULA=Y" \
  -e "SA_PASSWORD=Your_password123" \
  -p 1433:1433 -d \
  mcr.microsoft.com/mssql/server:2022-latest

# 2. Set user secrets
dotnet user-secrets set "ConnectionStrings:Default" \
  "Server=localhost,1433;Database=EdTechSheetIntegrator;User Id=sa;Password=Your_password123;TrustServerCertificate=True;" \
  --project src/EdTech.SheetIntegrator.Api

# 3. Apply migrations
dotnet tool restore
dotnet ef database update --project src/EdTech.SheetIntegrator.Infrastructure

# 4. Run the API
dotnet run --project src/EdTech.SheetIntegrator.Api
# → http://localhost:5000  |  OpenAPI UI: http://localhost:5000/docs
```

---

## Configuration

All settings are in `appsettings.json` and can be overridden by environment variables (use `__` as separator, e.g. `Jwt__SigningKey`).

| Key | Default | Description |
|---|---|---|
| `ConnectionStrings:Default` | *(required)* | SQL Server connection string |
| `Jwt:Issuer` | `edtech` | JWT issuer claim |
| `Jwt:Audience` | `edtech-api` | JWT audience claim |
| `Jwt:SigningKey` | *(required, ≥ 32 chars)* | HS256 signing key |
| `Jwt:TokenLifetimeMinutes` | `60` | Token expiry |
| `Upload:MaxFileSizeBytes` | `10485760` | 10 MB upload cap |

---

## Running tests

```bash
# Unit + architecture tests (no Docker needed)
dotnet test EdTech.SheetIntegrator.sln \
  --filter "FullyQualifiedName!~IntegrationTests"

# Infrastructure integration tests (requires Docker)
dotnet test tests/EdTech.SheetIntegrator.Infrastructure.IntegrationTests/

# API integration tests (requires Docker)
dotnet test tests/EdTech.SheetIntegrator.Api.IntegrationTests/

# All tests
dotnet test EdTech.SheetIntegrator.sln
```

---

## Mutation testing

[Stryker.NET](https://stryker-mutator.io/docs/stryker-net/introduction/) targets the Domain and Application layers.

```bash
dotnet tool restore

# Locate MSBuild (required by Stryker's Buildalyzer)
SDK_BASE=$(dotnet --info | grep -i "Base Path" | awk '{print $NF}')

# Domain (target: ≥ 85 %)
dotnet stryker \
  --project EdTech.SheetIntegrator.Domain.csproj \
  --msbuild-path "${SDK_BASE}MSBuild.dll"

# Application (target: ≥ 70 %)
dotnet stryker \
  --project EdTech.SheetIntegrator.Application.csproj \
  --msbuild-path "${SDK_BASE}MSBuild.dll"
```

HTML reports are written to `StrykerOutput/` (git-ignored).

| Project | Mutation score | Gate |
|---|---|---|
| Domain | 96.8 % | ≥ 85 % (high) |
| Application | 70.7 % | ≥ 70 % (break) |

---

## API reference

> Start the API and visit **http://localhost:5000/docs** for the interactive Scalar UI.

### Health

| Method | Path | Auth | Description |
|---|---|---|---|
| `GET` | `/health/live` | — | Process liveness |
| `GET` | `/health/ready` | — | Database connectivity |

### Assessments

| Method | Path | Auth | Description |
|---|---|---|---|
| `POST` | `/api/v1/assessments` | Instructor | Create assessment + answer key |
| `GET` | `/api/v1/assessments/{id}` | — | Fetch assessment |

### Submissions

| Method | Path | Auth | Description |
|---|---|---|---|
| `POST` | `/api/v1/assessments/{id}/submissions` | Instructor | Upload answer sheet (xlsx/csv) and grade |
| `GET` | `/api/v1/assessments/{id}/submissions` | — | List submissions (paged) |
| `GET` | `/api/v1/submissions/{id}` | — | Fetch graded submission |

### Dev-only

| Method | Path | Description |
|---|---|---|
| `POST` | `/dev/token` | Mint an Instructor JWT (Development environment only) |

All error responses follow [RFC 7807 Problem Details](https://www.rfc-editor.org/rfc/rfc7807) with a stable `code` extension field.

---

## Manual smoke test

See [`docs/manual-test.md`](docs/manual-test.md) for step-by-step curl commands and a `.http` file for VS Code REST Client / Rider.

---

## CI pipeline

GitHub Actions (`.github/workflows/ci.yml`) runs on every push and pull request to `main`:

```
format ──► build-test ──► integration-test
                     │
                     ├──► mutation (Domain)
                     │    mutation (Application)
                     │
                     └──► docker build
```

| Job | What it checks |
|---|---|
| **format** | `dotnet format --verify-no-changes` |
| **build-test** | `-warnaserror` build + unit/arch tests + coverage |
| **integration-test** | Testcontainers SQL Server + WebApplicationFactory |
| **mutation** | Stryker.NET score vs thresholds |
| **docker** | Multi-stage image builds successfully |

---

## Potential future work

- **Outbox pattern** — reliable domain event dispatch (SubmissionGraded)
- **Chiseled image health check** — custom HTTP probe binary for `HEALTHCHECK` in Dockerfile
- **Container registry push** — gated on `main` branch with image signing
- **Dashboard reporter** — Stryker.NET dashboard integration for mutation score history
- **Role expansion** — Student role for self-service submission retrieval
