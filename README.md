# BrokerOS

Insurance Broker Operations & Renewal Automation Platform — MVP/demo for Indian insurance brokers.

This repository is implemented in phases. **Phase 3** adds JWT authentication, tenant context from the logged-in user, and Development-only demo users.

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

`src/BrokerOS.Api/appsettings.Development.json` contains **local-only placeholders**. Replace the SQL password and JWT key before sharing a machine or hosting the app.

## Run locally

### 1. SQL Server

```bash
cp .env.example .env
docker compose up -d
```

Database creation and EF migrations start in Phase 2. After SQL Server is running:

```bash
dotnet ef database update --project src/BrokerOS.Infrastructure --startup-project src/BrokerOS.Api
```

Replace `REPLACE_WITH_MSSQL_SA_PASSWORD` in `src/BrokerOS.Api/appsettings.Development.json` (or set `ConnectionStrings__DefaultConnection`) before applying migrations.

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

## Database (Phase 2)

Tables: Organizations, Users, Clients, Contacts, Insurers, Policies, Renewals, Tasks, Activities.

- Internal `Id` is `bigint` identity. Public APIs should use `PublicId` (`uniqueidentifier`).
- `StartDate`, `ExpiryDate`, and `RenewalDate` are SQL `date` / C# `DateOnly`.
- Premium, sum insured, and commission amount are `decimal(18,2)`. Commission percentage is `decimal(18,4)`.
- Tenant-owned rows are filtered by `OrganizationId` from the authenticated user. Soft-deleted rows are hidden automatically.
- Users.Email is unique among active (not deleted) accounts.
- Development seed creates Apex Insurance Brokers and three demo users. No production seed.

## Current phase

Phase 3 complete. Do not start the next phase until instructed.
