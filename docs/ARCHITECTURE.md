# BrokerOS architecture

BrokerOS is a multi-tenant operations app for Indian insurance brokers. The product promise is **never miss a renewal**. Product walkthrough: [OVERVIEW.md](./OVERVIEW.md). Live meeting script: [DEMO.md](./DEMO.md). Comment contract: [DOCUMENTATION.md](./DOCUMENTATION.md) when that file is present on the branch.

## Project structure

```text
src/BrokerOS.Api              ASP.NET Core host: controllers, JWT auth, tenant middleware
src/BrokerOS.Application      DTOs, FluentValidation, service interfaces, auth policies
src/BrokerOS.Domain           Entities, enums, domain exceptions
src/BrokerOS.Infrastructure   EF Core, SQL Server, services, JWT issuance, seed data, workers
frontend/web                  Vite + React + TypeScript (Bootstrap)
docs/                         Architecture, product overview, hosting runbook
docker-compose.yml            Local SQL Server
docker-compose.prod.yml       Public demo: SQL + API + web + Caddy
```

## Notifications and WhatsApp (plug-in point)

Indian brokers chase clients on **WhatsApp**, not email. Milestone reminders are therefore drafted as WhatsApp messages for client-facing copy. Email is kept for **internal desk notes** (90/60/45-day planning) and **insurer quotation requests**. Nothing is actually sent in this demo.

Delivery is an abstraction so a real WhatsApp Business API can be plugged in later **without restructuring callers**:

```text
RenewalReminderWorker
        │
        ▼
INotificationSender.SendAsync(Notification)
        │
        ├── SimulatedNotificationSender   (registered today — records the row, Status = Simulated)
        └── WhatsAppBusinessApiSender     (not built — Twilio / Gupshup / Interakt / Meta Cloud API)
```

**Where to plug in a real provider**

1. Add `WhatsAppBusinessApiSender : INotificationSender` in `src/BrokerOS.Infrastructure/Notifications`.
2. In `DependencyInjection.cs`, replace

   `services.AddScoped<INotificationSender, SimulatedNotificationSender>();`

   with the live class. That registration is the **only** intended swap. The worker, list APIs, and preview UI should not change.
3. A live sender should POST to the provider, then set `Notification.Status` to `Sent` (add `Failed` if you need to surface provider errors). Keep writing the same `Notifications` row so the in-app preview still works.

Do not call Twilio/Gupshup/Interakt from the worker or controllers. Always go through `INotificationSender`.

## Quick notes (desk capture)

`POST /api/quick-notes` is for jotting a note between calls without a full task form. It always writes an `Activity` (`ActivityType.Note`). Client and renewal PublicIds are optional. A follow-up `Task` is created only when `createFollowUpTask` is true.

This version **does not parse the note with AI/NLP**. Do not add keyword matching or intent detection here. The checkbox is the intended plug-in point for later: the same workstream as **AI document scanning** can later *suggest* the follow-up flag from the wording (or from an uploaded slip), but `IQuickNoteService.CreateAsync` should remain the single write path.

## Global search

`GET /api/search?q=` looks up **client name/phone** and **policy number/vehicle number** in one call, scoped to the current organisation (employees: assigned book only). Results are a unified list with `type` (`Client` / `Policy`) so the header can route to the right record.

`SearchService.SearchAsync` uses EF `Contains` (SQL LIKE) and ranks exact matches ahead of prefix/partial, capped at 10. **That method is the swap point** for SQL Server full-text search or an external search service if match quality or load becomes an issue. Do not scatter a second search implementation in controllers or the React header.

## Renewal quotations (manual comparison)

Brokers in India typically call or email two or three insurers and type the quotes into the renewal file. This is **not** RFQ automation and not a new-business sales module.

`Quotation` hangs off a `Renewal`. Status `Selected` is the one chosen option (siblings become `Rejected`). Share uses `INotificationSender` so a WhatsApp-style `Notification` is recorded as Simulated — the same plug-in point as milestone reminders. Mark Renewed (`RenewalService.CompleteAsync`) uses the selected quotation as the source of truth for next-term **premium** and **insurer** unless the broker overrides those fields; with no selected quote, rollover still copies the expiring policy.

## My Day (morning briefing)

The first screen after login is a short, assignment-scoped checklist of what to do **today**, not a stats dashboard.

```text
GET  /api/my-day              overdue / due-today / upcoming-urgent cards (capped at 15 each)
POST /api/my-day/complete     finish a task, or clear a renewal chase (does not roll over the policy)
POST /api/my-day/call         log a Call activity
POST /api/my-day/follow-up    log WhatsApp + push next chase two IST days
```

“Today” is the IST calendar date. Employees see only assigned work. The Overview dashboard stays at `/dashboard`.

## Bulk import (Excel / CSV)

Brokers arrive with 100–300 policies in a spreadsheet. Import is **preview then confirm** so a bad phone or duplicate policy number is visible before anything is written.

```text
GET  /api/import/clients/template     .xlsx column guide
POST /api/import/clients/preview      parse + validate, no writes
POST /api/import/clients/confirm      insert valid rows only (JSON previewToken)
```

Same four routes exist under `/api/import/policies`. Policy rows must match an **existing** client. OrganizationId in the file is ignored — every inserted row uses the JWT tenant. Auth: BrokerAdmin or BrokerManager.

UI: Clients and Policies each have **Import from Excel/CSV**.
