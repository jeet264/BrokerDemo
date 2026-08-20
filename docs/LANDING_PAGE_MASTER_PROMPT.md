# BrokerOS Landing Page — Master Specification Prompt

> **Purpose**: Use this production-grade, battle-tested prompt in Antigravity IDE, Next.js 15+ App Router, React 19, or modern static frameworks to build or extend the BrokerOS B2B Marketing Landing Page.

```markdown
# Role & Project Identity
You are an expert Principal Frontend Engineer and Fintech UI/UX Specialist. Build a high-end, responsive, accessible, and tri-lingual marketing landing page for **BrokerOS** using Next.js 15 (App Router, Tailwind CSS, Lucide / Bootstrap Icons) or React + Vite.

---

## 1. Product Positioning (Do Not Invent a Different Business)
BrokerOS is a specialized B2B renewal-operations workspace for **Indian insurance brokerages** (IRDAI-licensed brokers managing commercial and retail policy books of 50 to 300+ policies).

- **One-line pitch**: BrokerOS is the desk that stops an Indian brokerage missing a renewal.
- **Problem solved**: Brokers track expiries in Excel sheets, personal WhatsApp chats, and disconnected emails. Cover lapses when nobody owns the file, next action is not recorded, expired terms get overwritten onto new terms, and reminders are ad-hoc.
- **Core system concept**: The **Renewal File** is the single operational system of record. Every policy approaching expiry has an owner, IST expiry timeline, ₹ premium at risk, 2-3 insurer quotes, and clean 1-click "Mark Renewed" (rolls clean new term) or "Mark Lost".

### Strict What BrokerOS is NOT:
- NOT a consumer portal (like Policybazaar or RenewBuy) to buy online motor/health insurance.
- NOT a POSP MLM commission distribution network.
- NOT an insurer or underwriting portal.
- NOT live automated WhatsApp Business API sending (it generates clean drafts and previews; do not claim live auto-sending).
- NOT an accounting / GST billing AMS replacement.

---

## 2. Design System & Visual Aesthetic
- **Visual Vibe**: Senior, calm, institutional B2B fintech (think Mercury / Stripe / Linear / Zerodha for insurance brokers). No startup hype slang ("synergy", "disrupt", "10x"). Short, direct sentences.
- **Color Tokens**:
  - Navy 950: `#071824` (Primary deep dark)
  - Navy 900: `#0b2b43` (Header / cards background)
  - Navy 800: `#0f3d5e` (Secondary hero gradient)
  - Gold Accent: `#c9a227` (Primary CTA and highlights)
  - Gold Soft: `#f4e4b3` (Subtle badges)
  - Paper Background: `#eef2f6` (Clean contrasting body)
  - Ink Text: `#15232e` (Body text)
  - Muted: `#5a6b78` (Secondary text)
  - Border Line: `#e3eaf0`
  - Danger (Overdue): `#b42318` / `#fef3f2`
  - Warning (Due 7d): `#b54708` / `#fffaeb`
  - Success (Renewed): `#1b7a4e` / `#eef6ee`
- **Radius**: `16px` (main cards), `10px` (chips/inputs)
- **Shadow**: `0 8px 28px rgba(7, 24, 36, 0.06)`
- **Typography**: Plus Jakarta Sans, with fallbacks to Noto Sans Devanagari & Noto Sans Gujarati.

---

## 3. Localization & Language Support
Support seamless client-side language switching between:
1. **English** (`en`) — Default
2. **हिन्दी** (`hi`) — Natural Indian business Hindi
3. **ગુજરાતી** (`gu`) — Authentic Gujarati commercial brokerage terminology

- Maintain "BrokerOS", "My Day", policy codes (`POL-D008`), and Indian Rupees (`₹`) naturally across all languages.
- Persist selection in `localStorage` (`brokeros.language`) and update `<html lang="...">`.

---

## 4. Page Architecture & Section Specifications

### A. Sticky Navigation Bar
- **Logo**: Gold rounded-square `B` badge + `BrokerOS` wordmark + `India · B2B` badge.
- **Jump Links**: `#product`, `#how-it-works`, `#roles`, `#faq`.
- **Language Switcher**: Compact pill button group (`EN` | `हिन्दी` | `ગુજરાતી`).
- **Actions**:
  - Secondary: `Sign in` (links to `/login` for existing brokerage members).
  - Primary CTA: `Request a demo` (smoothly scrolls to demo form).

### B. Hero Section with Live Desk UI Mock
- **Eyebrow**: "Insurance broker operations"
- **H1**: "Never miss a renewal"
- **Sub**: "One file per policy: owner, next action, IST dates, and the current term — not another spreadsheet."
- **Proof Chips**:
  - 🕒 `IST dates & Indian timing`
  - 💰 `₹ premium at risk tracking`
  - 🔒 `Employee sees only assigned work`
- **Dual CTAs**: "Request a demo" & "See how a renewal file works".
- **Interactive UI Mock of Apex Insurance Brokers**:
  - Tabs: `[Overview]` / `[Renewal File POL-D008]` / `[My Day Desk]`.
  - **Overview tab**: Overdue renewals (14), Due in 7 days (8), Due in 30 days (23), Premium at risk (₹38.4 Lakhs), Urgent queue.
  - **Renewal File tab**: Client: Malabar Spices Pvt Ltd, Policy: Fire & Special Perils, Premium: ₹4,85,000, 27d overdue, Owner: Rajesh Sharma, Next Action: "Present ICICI & HDFC quote to CFO by 4:00 PM IST", Quote Comparison Cards (ICICI Lombard ₹4,68,200 vs HDFC ERGO ₹4,52,000 vs New India ₹4,91,500), Action buttons ("Mark Renewed", "WhatsApp Quote Preview").
  - **My Day tab**: Morning queue in IST with 1-click status actions ("Call", "Done").

### C. Problem Strip (The Spreadsheets Trap)
3 structured problem cards:
1. ⚠️ **No owner on the file**: When three executives think someone else called the client, cover lapses silently.
2. ⏳ **Next action not written down**: Quotes sit in WhatsApp chats while the 15-day expiry window passes without a callback date.
3. 🔄 **Expired term mixed with new term**: Spreadsheets overwrite old policy rows, destroying claims history.
- Conclusion banner: "BrokerOS makes the renewal file the single operational system of record."

### D. Product Features Grid (6 Core Operational Engines)
1. **Overview Dashboard**: Triage overdue, 7-day, 30-day expiries & ₹ premium at risk.
2. **My Day Desk**: Morning priority checklist in IST.
3. **The Renewal File**: Centralized system of record per expiring policy with insurer quotes.
4. **Excel / CSV Import**: Bulk ingest existing client books and active policies without retyping.
5. **Instant Search & Quick Note**: Search client, phone, policy number, or vehicle number in 1 keystroke; log call notes in 10 seconds.
6. **Three-Tier Roles**: Broker Admin (full book), Manager (operations), Employee (strictly assigned files).

### E. How It Works (3 Operational Steps)
- **Step 01**: Policy approaches expiry → Renewal file opens (Automated 90/60/45/30/15/7/1 day triggers).
- **Step 02**: Owner assigned + Next action recorded (Contact client, request 2-3 quotes, prepare comparison preview).
- **Step 03**: Client decides → Mark Renewed or Mark Lost (Mark Renewed rolls clean new term; Mark Lost closes without new term).

### F. Roles & Permission Boundaries
- **Broker Admin**: Full book visibility, organization settings, team invites, Excel import/export, audit logs.
- **Manager**: Full book operations, create/edit clients and policies, assign tasks, review quotes.
- **Employee**: Strictly restricted to assigned files only; zero visibility into other brokers' books or overall brokerage financials.

### G. Who It Is For vs. Who It Is NOT For
- **For**: IRDAI-licensed brokerages (50 to 300+ policies), Principal Brokers wanting zero lapses, Operations Managers managing commercial line quotes.
- **NOT For**: Retail consumers looking for online vehicle/health insurance, POSP lead generation networks, insurer direct portals, or accounting ERP replacements.

### H. Honest FAQ Accordion
- WhatsApp message sending (Draft/preview only; live auto-send is an upcoming add-on).
- Policy history preservation (Expired term stays in audit records; new term rolls cleanly).
- Multi-tenancy & staff isolation (Employees only see assigned work).
- Commission calculation (Computed automatically from Premium × %; never manually typed).
- Excel import support (Built-in CSV/Excel mapping).
- Live walkthrough availability (12-minute walkthrough with Apex demo workspace).

### I. Final CTA Band & Demo Booking Form
- **Navy background (`#071824`) with Gold button**.
- Form Fields:
  - Name (required)
  - Brokerage Name (required)
  - City (required)
  - Work Email (required)
  - Phone / WhatsApp (required)
  - Role dropdown (Principal Broker / Ops Manager / Distributor / Executive)
  - Book Size dropdown (50–150 / 150–300 / 300–1,000 / 1,000+)
- **Submission Handler**:
  - Configurable `DEMO_INQUIRY_EMAIL = "demo@brokeros.in"`.
  - Client-side validation.
  - Smooth transition to confirmation state.
  - Generates `mailto:` draft with structured inquiry parameters.
  - Logs structured JSON to console for analytics hooks.

### J. Footer
- BrokerOS mark + wordmark + "Never miss a renewal".
- India B2B footer links, disclaimer regarding IRDAI intermediary status, and contact mailto.

---

## 5. Technical Requirements
- Semantic HTML5, ARIA labels for accessibility, WCAG AA color contrast.
- Fully responsive across mobile (375px), tablet (768px), and wide desktop (1280px+).
- Respect `prefers-reduced-motion`.
- Modular code architecture with zero external runtime bloat.
```
