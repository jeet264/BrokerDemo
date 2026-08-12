# BrokerOS

Insurance Broker Operations & Renewal Automation Platform — MVP/demo for Indian insurance brokers.

This repository is implemented in phases. **Phase 1** is project setup only: the API host, frontend shell, SQL Server compose file, and shared cross-cutting concerns. Authentication, database entities, and business modules come in later phases.

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

Database creation and EF migrations start in Phase 2. Phase 1 only needs the container to start.

### 2. API

```bash
dotnet run --project src/BrokerOS.Api
```

- Swagger: http://localhost:5000/swagger
- Health: http://localhost:5000/health
- System status: http://localhost:5000/api/system/status

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

## Current phase

Phase 1 complete. Do not start Phase 2 until instructed.
