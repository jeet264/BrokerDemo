# BrokerOS architecture

BrokerOS is a multi-tenant operations app for Indian insurance brokers. The product promise is **never miss a renewal**. This file is the map for a developer seeing the repo for the first time: where code lives, how tenancy is enforced, how policy terms are supposed to roll over, and how to run the stack locally.

Comment and naming rules live in [DOCUMENTATION.md](./DOCUMENTATION.md). Update **this** file when a later prompt adds a structurally significant piece (new module, tenancy change, rollover implementation, or a new way to run the app).

## Project structure

```text
src/BrokerOS.Api              ASP.NET Core host: controllers, JWT auth, tenant middleware
src/BrokerOS.Application      DTOs, FluentValidation, service interfaces, auth policies
src/BrokerOS.Domain           Entities, enums, domain exceptions
src/BrokerOS.Infrastructure   EF Core, SQL Server, services, JWT issuance, seed data
frontend/web                  Vite + React + TypeScript (Bootstrap)
docs/                         Architecture and documentation standard
docker-compose.yml            Local SQL Server 2022
```

Layering:

- **Api** is the HTTP edge. Controllers stay thin: validate (FluentValidation filter), authorize, call a service, wrap the result in `ApiResponse<T>`.
- **Application** holds contracts. No EF Core types here.
- **Domain** is persistence-ignorant entities. Internal PK is `long Id`; public APIs use `Guid PublicId`.
- **Infrastructure** implements services against `BrokerOsDbContext`.

There is no separate Repository layer. Services query EF Core directly. Query filters on the DbContext are the default tenant and soft-delete fence.

### What exists today vs later prompts

| Area | Status on this branch |
|---|---|
| Auth, org, clients, insurers | Implemented (API) |
| Policy / Renewal / WorkTask / Activity **entities** | In the schema; nested under client GET only |
| Policy / Renewal / Task / Activity **controllers** | Not built yet |
| Bulk import (clients + policies) | Preview/confirm APIs + Excel templates + UI wizards |
| `RenewalService.CompleteRenewalAsync` and `PreviousPolicyId` / `NextPolicyId` | Not built yet — design is documented below so Prompt 7B does not invent a different model |
| Notification entity / inbox API | Not built yet — frontend has a placeholder route only |
| Frontend | Shell + login (JWT stored) + dashboard + client list + Excel/CSV import wizards |

## Multi-tenancy (Prompt 3)

Every brokerage is an `Organization`. Almost every operational row carries `OrganizationId`. The client must never choose the tenant: **OrganizationId always comes from the JWT**, copied onto a request-scoped `ITenantContext`.

```text
Authorization: Bearer <jwt>
        │
        ▼
CurrentUserService reads claims (UserId, PublicUserId, OrganizationId, Role, Email)
        │
        ▼
TenantResolutionMiddleware copies OrganizationId + user identifier onto ITenantContext
        │
        ▼
BrokerOsDbContext global query filters: OrganizationId == CurrentOrganizationId
        │
        ▼
AssignmentScope.ForCurrentUser (employees: AssignedUserId == current user)
```

Pipeline order in `Program.cs` is load-bearing: `UseAuthentication` → `TenantResolutionMiddleware` → `UseAuthorization`. Tenant context is empty on anonymous routes (login, register, `/health`).

### Query filters

Defined in `BrokerOsDbContext.ApplyTenantAndSoftDeleteFilters`:

- **Organization**: `Id == CurrentOrganizationId` so `GET /api/organizations/current` cannot see another brokerage.
- **User, Client, Contact, Policy, WorkTask**: current org **and** `IsDeleted == false`.
- **Renewal, Activity**: current org only. They are not soft-deleted (renewals are a workflow record; activities are an append-only timeline).
- **Insurer**: `OrganizationId == null` (global / system panel) **or** `OrganizationId == current org`. Unauthenticated `CurrentOrganizationId == 0` matches nothing, so anonymous callers cannot list insurers.

`SaveChanges` converts `EntityState.Deleted` into a soft delete for `ISoftDeletable` types. Insurers are **not** soft-deletable: delete is a hard remove and is refused when any policy (any tenant) still points at the insurer.

### Login and register bypass filters

Login and organization registration run **before** tenant context is set, and emails are unique across the whole database. Those queries use `IgnoreQueryFilters()` and still exclude `IsDeleted` users in application code. Do not copy `IgnoreQueryFilters()` into ordinary CRUD — that is how you leak another tenant.

### Assignment scope (404, not 403)

`AssignmentScope.ForCurrentUser` is a second fence on top of tenancy:

- **BrokerAdmin / BrokerManager**: full book of the org.
- **BrokerEmployee**: only rows where `AssignedUserId == currentUser.UserId`. Unassigned rows are invisible.

Missing, other-tenant, or out-of-scope records throw `NotFoundException` (HTTP 404) via `EnsureFound` / `EnsureCanAccessAssigned`. That is intentional: a 403 would confirm that the id exists in another book.

Role checks that **are** 403: mutating a system insurer, updating org settings as a non-admin, or failing an `[Authorize(Policy = ...)]` attribute.

### Roles and policies

| Policy | Roles |
|---|---|
| `AdminOnly` | BrokerAdmin |
| `CanManageOrganization` | BrokerAdmin |
| `CanManageOperations` | BrokerAdmin, BrokerManager |
| `CanCreateActivities` / `CanUpdateAssignedWork` | all three roles |

JWT claims (`JwtTokenService`): `UserId`, `PublicUserId`, `OrganizationId`, `Role`, `Email`. `MapInboundClaims` is false so these names are not rewritten to the long ClaimTypes URIs.

## Renewal rollover (Prompt 7B — intended design)

A `Policy` is **one term**, not a forever-living policy that mutates in place. Indian brokers need last year's premium, dates, and insurer on file after the client renews.

When a renewal is completed (future `RenewalService.CompleteRenewalAsync`):

1. Insert a **new** `Policy` for the next term (new start/expiry, premium, insurer as agreed).
2. Mark the current term `Expired` (do not overwrite its dates or premium).
3. Link the chain with `PreviousPolicyId` / `NextPolicyId` once those columns exist.
4. Mark the `Renewal` `Renewed` / stage `Completed`.

Lists, dashboards, and "current cover" should follow `NextPolicyId == null` (or `Status == Active` / `PendingRenewal`), not "the row the user originally opened." Historical terms stay queryable for audit.

`PreviousPolicyId` / `NextPolicyId` are **not in the schema yet**. Do not mutate `StartDate` / `ExpiryDate` on an expired term as a shortcut while waiting for Prompt 7B.

`Renewal.RenewalDate` is a `DateOnly` business date, typically the policy's `ExpiryDate`. Follow-up instants (`LastFollowUpAtUtc`, `NextFollowUpAtUtc`) are UTC `DateTime` because they are timestamps, not cover dates.

## Dates, money, and the API envelope

- **Cover / business dates** (`Policy.StartDate`, `Policy.ExpiryDate`, `Renewal.RenewalDate`): C# `DateOnly` / SQL `date`. JSON serializes as `yyyy-MM-dd`. The frontend must treat these as strings (or parse as calendar dates), not `new Date(value)` in local time — that shifts the day around IST.
- **Audit and follow-up timestamps**: UTC `DateTime` (`CreatedAtUtc`, `LastLoginAtUtc`, …). Display in IST (`Asia/Kolkata`) in the UI.
- **Money**: premium, sum insured, commission amount `decimal(18,2)`. Commission percentage `decimal(18,4)`. Display INR as `₹8,50,000.00` when the UI is built.
- **Envelope**: `{ success, data, message, errors, traceId }`. Frontend `getApiData` unwraps `data` and throws if `success` is false.

## How to run locally

Prerequisites: .NET 8 SDK, Node.js 20.19+ or 22+ (Vite 8), Docker **or** a local SQL Server instance.

### 1. Database

**Docker (default in this repo):**

```bash
cp .env.example .env
docker compose up -d
# wait until docker compose ps shows healthy
dotnet ef database update --project src/BrokerOS.Infrastructure --startup-project src/BrokerOS.Api
```

Connection string (Development): `Server=localhost,1433;Database=BrokerOS;User Id=sa;Password=BrokerOS_Demo_123;TrustServerCertificate=True;Encrypt=True`

**Windows SQL Server without Docker:** use a named instance and Windows Auth via User Secrets if that is how the machine is set up, for example `Server=localhost\\MSSQLSERVER01;Database=BrokerOS;Trusted_Connection=True;TrustServerCertificate=True`. Package Manager Console `Update-Database` needs an EF Tools package this solution does not reference — use `dotnet ef` as above.

### 2. API

```bash
dotnet run --project src/BrokerOS.Api
```

- API: http://localhost:5000
- Swagger: http://localhost:5000/swagger
- Health: http://localhost:5000/health
- Status: http://localhost:5000/api/system/status

Development seed (only when `ASPNETCORE_ENVIRONMENT=Development` and SQL is reachable) creates Apex Insurance Brokers:

| Role | Email | Password |
|---|---|---|
| BrokerAdmin | admin@apexbrokers.in | Demo@12345 |
| BrokerManager | manager@apexbrokers.in | Demo@12345 |
| BrokerEmployee | employee@apexbrokers.in | Demo@12345 |

JWT signing key (`Jwt:Key`, ≥ 32 characters) is required. Prefer User Secrets over committing a real key.

### 3. Frontend

```bash
cd frontend/web
npm install
npm run dev
```

App: http://localhost:5173. Override the API with `VITE_API_BASE_URL` (default `http://localhost:5000`).

On this branch, sign-in calls `POST /api/auth/login` and stores the JWT in localStorage so client list and Excel import can authorize. Demo password: `Demo@12345`.

## Bulk import (Excel / CSV)

Brokers arrive with 100–300 policies in a spreadsheet. Import is **preview then confirm** so a bad phone or duplicate policy number is visible before anything is written.

```text
GET  /api/import/clients/template     .xlsx column guide
POST /api/import/clients/preview      parse + validate, no writes
POST /api/import/clients/confirm      insert valid rows only (JSON previewToken, or re-upload the file)
```

Same four routes exist under `/api/import/policies`. Policy rows must match an **existing** client:

- `matchBy=ClientCode` (default) — `ClientCode` or `ClientExternalId` column
- `matchBy=NameAndPhone` — `ClientName`/`CompanyName` + `Phone` (digits compared)

Insurer is matched by code or name against the current org panel **plus** global/system insurers. **OrganizationId in the file is ignored** — every inserted row uses the JWT tenant. Auth: BrokerAdmin or BrokerManager. Preview tokens live ~30 minutes in memory and are bound to OrganizationId.

UI: Clients and Policies each have **Import from Excel/CSV**. Invalid preview rows are highlighted; confirm imports only the valid count.

## Related reading

- Product overview and phase notes: repository `README.md`
- Comment contract: [DOCUMENTATION.md](./DOCUMENTATION.md)
