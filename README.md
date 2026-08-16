# BrokerOS

Insurance Broker Operations & Renewal Automation Platform — MVP/demo for Indian insurance brokers.

This repository is implemented in phases. **Phase 6** adds renewal management: automatic renewal records, reminder tasks, and dashboard totals.

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

```bash
cp .env.example .env
docker compose up -d
```

Database creation and EF migrations start in Phase 2. After SQL Server is running:

```bash
dotnet ef database update --project src/BrokerOS.Infrastructure --startup-project src/BrokerOS.Api
```

Replace `BrokerOS_Demo_123` in `src/BrokerOS.Api/appsettings.Development.json` if your `sa` password is different.

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
| BrokerEmployee | employee2@apexbrokers.in | Demo@12345 |
| BrokerEmployee | employee3@apexbrokers.in | Demo@12345 |

The Development seeder loads one Apex organisation with 5 users, 10 insurers, 50 Indian corporate clients, and 100 policies (all listed policy types). The book includes overdue, due-today, 7/30/60-day, completed, and lost renewals, plus sample tasks and timeline activity. Client and contact details are fictional.

To restore a clean book after a live demo, sign in as BrokerAdmin and use **Settings → Reset Demo Data**, or `POST /api/dev/reset-demo-data`. This exists only when `ASPNETCORE_ENVIRONMENT=Development` **and** `BrokerOS:EnableDemoReset` is true (Development appsettings). Production-configured APIs return 404. The frontend Settings link is shown only when `VITE_ENABLE_DEMO_RESET=true`.

In Swagger, click **Authorize** and paste the `accessToken` from `/api/auth/login`.

### 3. Frontend

```bash
cd frontend/web
npm install
npm run dev
```

App: http://localhost:5173

## Tests

Backend unit and API integration tests (SQL Server required for API tests; they use a dedicated `BrokerOS_Tests` database, not the demo database):

```bash
dotnet test
```

Override the test connection string with `BROKEROS_TEST_CONNECTION` if needed.

Frontend:

```bash
cd frontend/web
npm test
```


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

## Renewal APIs (Phase 6)

Purpose: never miss an insurance renewal.

When a policy is created, BrokerOS also creates a renewal (`RenewalDate` = policy expiry, status `Upcoming`, stage `NotStarted`). A background worker checks open renewals on a timer (default 15 minutes) and creates reminder tasks at 90/60/45/30/15/7/1 days. Duplicate milestone tasks are blocked by a unique index.

| Method | Route | Auth |
|---|---|---|
| GET | `/api/renewals` | Any signed-in role (employees see assigned renewals only) |
| GET | `/api/renewals/{publicId}` | Same |
| PUT | `/api/renewals/{publicId}/status` | Any signed-in role on assigned work |
| PUT | `/api/renewals/{publicId}/stage` | Same |
| POST | `/api/renewals/{publicId}/follow-up` | Same (creates activity, optional task) |
| POST/PUT | `/api/renewals/{publicId}/complete` | Same — rolls the policy into a new term |
| POST/PUT | `/api/renewals/{publicId}/lost` | Same — cancels the policy, no new term |
| GET | `/api/renewals/{publicId}/activities` | Same |
| GET | `/api/renewals/{publicId}/tasks` | Same |
| GET | `/api/dashboard/renewals` | Same (counts and premium at risk) |
| GET | `/api/policies` | Any signed-in role (default `status=Active`, current term only) |

Query parameters for `GET /api/renewals`: `search`, `status`, `stage`, `priority`, `assignedUserPublicId`, `clientPublicId`, `fromDate`, `toDate`, `dueWithinDays`, `sortBy`, `sortDir`, `page`, `pageSize`.

Search matches policy number, client name, and insurer name. Follow-up writes an activity, sets `LastFollowUpAtUtc`, and can set `NextFollowUpAtUtc` and create a task. Dashboard `premiumAtRisk` is the sum of policy premium for open renewals due within 90 days (including overdue).

**Mark Renewed** creates a new Active policy (start = old expiry + 1 day, expiry = form value or +1 year, premium defaults to the previous term), marks the old policy Expired, auto-creates the next Upcoming renewal, and writes `PolicyRenewed` / `RenewalCreated` activities. Policy rows are linked with `PreviousPolicyId` / `NextPolicyId`. Lists and the dashboard show the current term expiry, never the expired one.

**Mark Lost** sets renewal `Lost` and policy `Cancelled`. No new policy is created.

Development seed adds Sharma Logistics and six demo policies with staggered expiry dates so the worker and dashboard have data.

## Bulk import APIs

Preview never writes. Confirm inserts only valid rows into the signed-in brokerage (`OrganizationId` from the JWT; a column in the file cannot override tenant). Auth: BrokerAdmin or BrokerManager.

| Method | Route | Purpose |
|---|---|---|
| GET | `/api/import/clients/template` | Excel template |
| POST | `/api/import/clients/preview` | Parse CSV/XLSX, return per-row validation |
| POST | `/api/import/clients/confirm` | JSON `{ previewToken }` or multipart file — import valid rows |
| GET | `/api/import/policies/template` | Excel template |
| POST | `/api/import/policies/preview?matchBy=ClientCode\|NameAndPhone` | Parse and match to existing clients |
| POST | `/api/import/policies/confirm` | Same confirm pattern as clients |

Client required columns: `ClientCode` (alias `ClientExternalId`), `CompanyName`, `Phone`. Policy required: `PolicyNumber`, `PolicyType`, `StartDate`, `ExpiryDate`, `Premium`, insurer name or code, plus the match columns for the chosen strategy.

## My Day APIs

The default landing after login. Payload is org- and assignment-scoped. “Today” is IST. Each list is capped at 15; `*TotalCount` is the uncapped size for “View all”.

| Method | Route | Auth |
|---|---|---|
| GET | `/api/my-day` | Any signed-in role |
| POST | `/api/my-day/complete` | Any signed-in role (`CanUpdateAssignedWork`) |
| POST | `/api/my-day/call` | Any signed-in role (`CanCreateActivities`) |
| POST | `/api/my-day/follow-up` | Any signed-in role (`CanCreateActivities`) |

Inline actions write an `Activity` and leave the card without opening a detail page. Marking a **renewal** done only clears `NextFollowUpAtUtc` — it does not insert a new Policy term.

## Database (Phase 2)

Tables: Organizations, Users, Clients, Contacts, Insurers, Policies, Renewals, Tasks, Activities.

- Internal `Id` is `bigint` identity. Public APIs should use `PublicId` (`uniqueidentifier`).
- `StartDate`, `ExpiryDate`, and `RenewalDate` are SQL `date` / C# `DateOnly`.
- Premium, sum insured, and commission amount are `decimal(18,2)`. Commission percentage is `decimal(18,4)`.
- Tenant-owned rows are filtered by `OrganizationId` from the authenticated user. Soft-deleted rows are hidden automatically.
- Users.Email is unique among active (not deleted) accounts.
- Development seed creates Apex Insurance Brokers, three demo users, a demo client, and sample policies/renewals. No production seed.
- Task reminder milestones are unique per renewal (`ReminderMilestoneDays`).

## Current phase

Phase 6 complete: renewal desk, quotations, bulk import, and My Day as the default landing.
