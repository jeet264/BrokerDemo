# BrokerOS architecture

BrokerOS is a multi-tenant operations app for Indian insurance brokers. The product promise is **never miss a renewal**. Product walkthrough and demo script: [OVERVIEW.md](./OVERVIEW.md). Comment contract: [DOCUMENTATION.md](./DOCUMENTATION.md) when that file is present on the branch.

## Project structure

```text
src/BrokerOS.Api              ASP.NET Core host: controllers, JWT auth, tenant middleware
src/BrokerOS.Application      DTOs, FluentValidation, service interfaces, auth policies
src/BrokerOS.Domain           Entities, enums, domain exceptions
src/BrokerOS.Infrastructure   EF Core, SQL Server, services, JWT issuance, seed data, workers
frontend/web                  Vite + React + TypeScript (Bootstrap)
docs/                         Architecture and product overview
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
