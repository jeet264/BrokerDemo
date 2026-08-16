# BrokerOS

Insurance Broker Operations & Renewal Automation Platform — MVP/demo for Indian insurance brokers.

This repository is implemented in phases. **Phase 5** adds insurer management (search, paging, active filter, org-scoped uniqueness, and read-only system insurers).

Developer map (tenancy, intended renewal rollover, local run): [`docs/ARCHITECTURE.md`](docs/ARCHITECTURE.md). Comment contract: [`docs/DOCUMENTATION.md`](docs/DOCUMENTATION.md).

## Prerequisites

- .NET 8 SDK
- Node.js 20+
- Docker (for SQL Server)

## Repository layout

```text
src/BrokerOS.Api              ASP.NET Core Web API host
src/BrokerOS.Application      DTOs, validation, application services
src/BrokerOS.Domain           Entities and domain exceptions
src/BrokerOS.Infrastructure   EF Core, SQL Server, clock, workers
frontend/web                  Vite + React + TypeScript
docker-compose.yml            Local SQL Server
```

## Configuration

Do not commit real secrets. Copy the examples and override with environment variables.

| Setting | Example |
|---|---|
| SQL Server password | `MSSQL_SA_PASSWORD` (see `.env.example`) |
| API connection string | `ConnectionStrings__DefaultConnection` |
| JWT signing key | `Jwt__Key` (required from Phase 3) |
| Frontend API URL | `VITE_API_BASE_URL` |

`src/BrokerOS.Api/appsettings.Development.json` uses the local demo SQL password `BrokerOS_Demo_123` (same as `.env.example`). Change it before sharing a machine or hosting the app.

## Run locally

### 1. SQL Server

Error **10061** (`target machine actively refused it`) means nothing is listening on port **1433**. SQL Server is not running. You do not need to install the full Windows SQL Server installer if Docker is available.

**Option A — Docker (recommended)**

1. Install and start [Docker Desktop](https://www.docker.com/products/docker-desktop/).
2. From the repo root:

```bash
cp .env.example .env
docker compose up -d
```

3. Wait until the container is healthy (`docker compose ps` shows `healthy`), then apply migrations:

```bash
dotnet ef database update --project src/BrokerOS.Infrastructure --startup-project src/BrokerOS.Api
```

**Option B — SQL Server Developer (Windows, no Docker)**

1. Install [SQL Server Developer](https://www.microsoft.com/en-us/sql-server/sql-server-downloads) with mixed-mode authentication.
2. Enable TCP/IP and confirm it listens on port 1433.
3. Set the `sa` password to `BrokerOS_Demo_123`, or update `ConnectionStrings:DefaultConnection` to match your password.
4. Run the same `dotnet ef database update` command as above.

The Development connection string is:

`Server=localhost,1433;Database=BrokerOS;User Id=sa;Password=BrokerOS_Demo_123;TrustServerCertificate=True;Encrypt=True`

### 2. API

```bash
dotnet run --project src/BrokerOS.Api
```

- Swagger: http://localhost:5000/swagger
- Health: http://localhost:5000/health
- System status: http://localhost:5000/api/system/status

Development demo login (seeded only when `ASPNETCORE_ENVIRONMENT=Development` and the database is available):

| Role | Email | Password |
|---|---|---|
| BrokerAdmin | admin@apexbrokers.in | Demo@12345 |
| BrokerManager | manager@apexbrokers.in | Demo@12345 |
| BrokerEmployee | employee@apexbrokers.in | Demo@12345 |

In Swagger, click **Authorize** and paste the `accessToken` from `/api/auth/login`.

### 3. Frontend

```bash
cd frontend/web
npm install
npm run dev
```

App: http://localhost:5173

## Phase 1 APIs

| Method | Route | Purpose |
|---|---|---|
| GET | `/health` | Process liveness |
| GET | `/api/system/status` | Product, environment, UTC time, whether a connection string is configured |

## Auth APIs (Phase 3)

| Method | Route | Auth |
|---|---|---|
| POST | `/api/auth/login` | Anonymous |
| POST | `/api/auth/register-organization` | Anonymous |
| GET | `/api/auth/me` | Bearer JWT |
| GET | `/api/organizations/current` | Any authenticated role |
| PUT | `/api/organizations/current` | BrokerAdmin only |

JWT claims: `UserId`, `PublicUserId`, `OrganizationId`, `Role`, `Email`. Tenant filters always use `OrganizationId` from the token, never from the request body.

## Client APIs (Phase 4)

| Method | Route | Auth |
|---|---|---|
| GET | `/api/clients` | Any signed-in role (employees see assigned clients only) |
| GET | `/api/clients/{publicId}` | Same |
| POST | `/api/clients` | BrokerAdmin, BrokerManager |
| PUT | `/api/clients/{publicId}` | BrokerAdmin, BrokerManager |
| DELETE | `/api/clients/{publicId}` | BrokerAdmin, BrokerManager (soft delete) |
| GET | `/api/clients/{publicId}/policies` | Same as GET client |
| GET | `/api/clients/{publicId}/renewals` | Same as GET client |
| GET | `/api/clients/{publicId}/activities` | Same as GET client |

Query parameters for `GET /api/clients`: `search`, `clientType`, `industry`, `assignedUserPublicId`, `isActive`, `sortBy`, `sortDir` (`asc`/`desc`), `page`, `pageSize`.

Search matches company name, client code, email, and phone. Cross-tenant or unassigned employee access returns 404.

## Insurer APIs (Phase 5)

| Method | Route | Auth |
|---|---|---|
| GET | `/api/insurers` | Any signed-in role (org insurers plus system insurers) |
| GET | `/api/insurers/{publicId}` | Same |
| POST | `/api/insurers` | BrokerAdmin, BrokerManager |
| PUT | `/api/insurers/{publicId}` | BrokerAdmin, BrokerManager |
| DELETE | `/api/insurers/{publicId}` | BrokerAdmin only |

Query parameters for `GET /api/insurers`: `search`, `isActive`, `sortBy`, `sortDir` (`asc`/`desc`), `page`, `pageSize`.

Search matches name, code, email, and phone. Names and codes must be unique within the organization and must not collide with a system insurer. Tenants cannot create or change system insurers (`isGlobal: true`). Delete is a hard delete and returns 409 when policies are linked.

## Database (Phase 2)

Tables: Organizations, Users, Clients, Contacts, Insurers, Policies, Renewals, Tasks, Activities.

- Internal `Id` is `bigint` identity. Public APIs should use `PublicId` (`uniqueidentifier`).
- `StartDate`, `ExpiryDate`, and `RenewalDate` are SQL `date` / C# `DateOnly`.
- Premium, sum insured, and commission amount are `decimal(18,2)`. Commission percentage is `decimal(18,4)`.
- Tenant-owned rows are filtered by `OrganizationId` from the authenticated user. Soft-deleted rows are hidden automatically.
- Users.Email is unique among active (not deleted) accounts.
- Development seed creates Apex Insurance Brokers, three demo users, and a small set of global Indian insurers. No production seed.
- Insurer names are unique per organization (`OrganizationId IS NOT NULL`) and unique among system insurers (`OrganizationId IS NULL`).

## Current phase

Phase 5 (Insurer Management) complete. Do not start the next phase until instructed.
