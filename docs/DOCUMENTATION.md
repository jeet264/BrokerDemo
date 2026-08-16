# BrokerOS documentation standard

This file is the comment and naming contract for BrokerOS. Follow it on every new feature, and retrofit existing code when you touch it. Comments exist so a developer (including future-you) can understand **why** a type or method exists without tracing the whole codebase.

Prefer a descriptive name over a comment that restates what the code obviously does. A comment should explain **why**, a business rule, a state dependency, or a gotcha — not narrate `i++`.

## Backend (C#)

### Entities and models

Every entity class gets an XML `<summary>` in **business** terms: what it represents, how it relates to other records, and any lifecycle rule that is easy to get wrong.

```csharp
/// <summary>
/// Represents a single insurance policy term for a client.
/// When a policy is renewed, a NEW Policy record is created for the
/// next term rather than mutating this one — this one gets marked Expired.
/// </summary>
```

Comment any property that is not self-explanatory, especially:

- Fields whose meaning depends on another field's state (for example `Status = Expired` means look at the next term, not this one).
- `DateOnly` vs `DateTime` — say **why** (cover/business date vs audit timestamp) so nobody "fixes" a `DateOnly` to `DateTime`.
- Nullable foreign keys — when the value is null vs populated.
- Money and commission fields — stored vs calculated, and decimal precision.

### Controllers

Every action gets:

```csharp
/// <summary>What this endpoint does, in plain English.</summary>
/// <remarks>
/// Auth: [which roles can call this]
/// Tenant scope: [how OrganizationId is enforced]
/// </remarks>
```

Add inline comments inside the method only for non-obvious order of operations or business rules. Do not restate `return Ok(...)`.

### Services

- Put a 1–2 sentence file header at the top of each Service/Repository explaining its responsibility.
- Every method with real logic (not a one-line CRUD passthrough) comments the **business rule it implements**, not what each line does.
- Example: completing a renewal creates a **new** Policy because the expired term is an audit/history record.

### DTOs

Comment any field that does not map 1:1 to the entity, or that is computed/derived, including:

- Enums serialized as strings (`PolicyStatus.ToString()`).
- Names loaded from a navigation (`AssignedUser.FullName`).
- `DateOnly` properties (JSON will be `yyyy-MM-dd` strings on the wire).
- UTC timestamps vs IST display (display is a frontend concern).

### Public ids vs internal ids

APIs expose `PublicId` (`Guid`). Internal `Id` (`long`) stays in the database and in JWT claims used only on the server. Never accept `OrganizationId` from a request body or query string as the tenant key.

## Frontend (React / TypeScript)

1. Every custom hook gets a comment: what it does and when to use it.
2. Every non-trivial component gets a short block: what it renders, what props it expects, and gotchas (especially dates: API `DateOnly` arrives as an ISO date **string**, not a `Date`).
3. API client functions note the HTTP endpoint and response shape when the TypeScript types do not already make it obvious. All JSON bodies use the envelope `{ success, data, message, errors, traceId }`.
4. Workarounds, hacks, and temporary shortcuts are labeled `TODO:` or `workaround:` so they are not mistaken for intentional design.

## Architecture notes

Keep `docs/ARCHITECTURE.md` short and current. Update it whenever a prompt in this series adds something structurally significant (new bounded area, tenancy rule, rollover behavior, or how to run the stack).
