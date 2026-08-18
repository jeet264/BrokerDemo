# BrokerOS — live demo for an insurance distributor

Use this in the meeting. Product walkthrough and architecture: [OVERVIEW.md](./OVERVIEW.md).

**Positioning (one sentence):** BrokerOS is the desk that stops an Indian brokerage missing a renewal — owners, IST dates, rupees, WhatsApp-style chase, and a clean next term when cover is bound.

This build is an **MVP you can click through**, not a hosted production system and not a POSP / new-business sales CRM.

---

## Before they sit down (10 minutes)

1. `git checkout main` and `git pull origin main`
2. SQL running (`docker compose up -d`)
3. API in **Development**, frontend at http://localhost:5173
4. Sign in as **Admin** → Settings → **Reset Demo Data** so the Apex book is clean
5. Sign out (or stay signed in as Admin)

If login fails or lists are empty, SQL is not up — do not start the meeting until Overview shows overdue counts.

---

## Accounts

Password for every role: `Demo@12345`

| Show as | Email | What they see |
|---|---|---|
| Admin | `admin@apexbrokers.in` | Full Apex book, Settings / reset |
| Manager | `manager@apexbrokers.in` | Full book, can create clients/policies |
| Employee | `employee@apexbrokers.in` | Only assigned work |

---

## 12-minute script

Stay on screens that have real data. Do not open Activity, Insurers, or Team — those are placeholders.

### 1. Login (1 min)

Choose **Admin**, Continue. Land on **Overview**.

Say: three roles, same password, employee is locked to assigned files.

### 2. Overview (2 min)

Point at overdue, due in 7 days, due in 30 days, **premium at risk**, today’s tasks.

Click an overdue row → **View** / kebab into the renewal file.

Say: the file is the system of record, not Excel.

### 3. Renewal file (4 min) — strongest screen

On one overdue or 7-day file, show in this order:

1. Client, policy number, insurer, premium (₹), expiry, days left  
2. Owner  
3. Stage and next action  
4. **Quotations** — add two quotes, **Select** one, share preview (WhatsApp-style, not actually sent)  
5. Timeline  
6. **Mark Renewed** — new term starts the day after old expiry; selected quote prefills premium/insurer  

Say: the expired term stays as history; lists always show the **current term**.

If they ask about WhatsApp: “The message is drafted and stored. Live send is a plug-in — we have not connected Gupshup/Interakt yet.”

### 4. My Day (2 min)

Open **My Day**. Overdue / due today / next three days. Call, Follow-up, Mark done without opening the file.

Say: morning list is IST “today”, not UTC.

### 5. Search + Quick Note (1 min)

Header search: a client name or `POL-D001`.  
**+ Quick Note** — log a call note, optional follow-up task.

### 6. Employee vs Admin (1 min)

Sign out, sign in as **Employee**. Show a thinner book.

Say: they cannot see another RM’s clients.

### 7. Import (optional, 1 min)

Clients or Policies → **Import from Excel/CSV**. Download template, say preview-then-confirm so a bad phone does not load 300 rows.

---

## What to say yes / no to

| They ask | You say |
|---|---|
| Can we run our book tomorrow? | Demo tenant today. Production host, their Excel import, and user invites are the next paid slice. |
| Does WhatsApp go to the client? | Preview only. Same plug-in point as live WhatsApp Business API. |
| New business / leads / POSP network? | Not this product. This is **renewal operations** for an existing book. |
| GST invoice, IRDAI filing, claims? | Not built. Commission % is on the policy; amount is calculated, not typed. |
| Documents / policy PDF? | Not attached yet. Next desk items: insurer panel, team invite, documents on the file. |
| Multi-office / SaaS login for their staff? | One org in the demo. Multi-tenant is in the model; hosting and MFA are not this build. |

---

## What this stage already proves

- Never-miss-a-renewal desk: auto renewal on policy create, 90→1 day tasks, overdue dashboard  
- Indian desk: IST, ₹, WhatsApp-shaped chase, motor vehicle number in search  
- Roles: Admin / Manager / Employee  
- Rollover: Mark Renewed / Mark Lost without corrupting history  
- Quotes on the file (manual, 2–3 insurers)  
- Excel/CSV import with preview  
- 50 clients / 100 policies seeded so the room is not empty  

## What you do **not** need to build before this meeting

Insurer UI, team admin, activity feed, live WhatsApp, claims, GST, POSP, client portal. Those are the follow-up conversation after they care about the renewal file.

---

## After the meeting

Admin → Settings → **Reset Demo Data**. Do not leave a half-marked-renewed book for the next showing.
