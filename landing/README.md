# BrokerOS Marketing Landing Page

High-end marketing landing page for **BrokerOS** — the renewal-operations workspace for Indian insurance brokerages.

---

## 📁 What is in this directory

- `index.html` — Semantic, accessible, SEO-ready HTML5 landing page.
- `styles.css` — High-performance modern CSS styled to the BrokerOS navy (`#071824`) and gold (`#c9a227`) design system.
- `app.js` — Client-side tri-lingual switcher (English, हिन्दी, ગુજરાતી), interactive renewal desk simulator, FAQ accordion, and inquiry form handler.

---

## ⚙️ How to Change the Demo Inquiry Email

Open `landing/app.js` (or `frontend/web/src/features/landing/LandingPage.tsx` for the React app):

```javascript
// Change this constant at the top of landing/app.js:
const DEMO_INQUIRY_EMAIL = 'your-team@yourbrokerage.com';
```

When a visitor submits the "Request a 12-Minute Demo" form:
1. The form validates all required fields (Name, Brokerage, City, Work Email, Phone).
2. It transitions to a clean confirmation screen.
3. It launches a pre-filled email draft to `DEMO_INQUIRY_EMAIL` via `mailto:` with structured inquiry details.
4. It logs the structured inquiry JSON to the browser console for debugging or analytics hooks.

---

## 🚀 How to Run Locally

### Option 1: Static Web Server
From the root of the repository:

```bash
# Python 3
python -m http.server 3000 --directory landing

# Or using Node.js npx serve
npx serve landing -p 3000
```
Open `http://localhost:3000` in your browser.

---

### Option 2: Running via the Main React App (Vite)
The landing page is also built as a native component in the main React application (`frontend/web`):

```bash
cd frontend/web
npm install
npm run dev
```
Open `http://localhost:5173/` to see the live landing page at the root route.
- Click **Sign in** to go to `/login`.
- Authenticated brokers automatically access `/dashboard` and `/my-day`.

---

## 🌐 Deploying Standalone

You can deploy the `landing/` folder directly to any static host without building a server:

- **Cloudflare Pages**: Link repo and set Root Directory to `landing`.
- **Vercel**: Deploy with `vercel --cwd landing`.
- **Netlify**: Set publish directory to `landing`.
- **Nginx / S3 + CloudFront**: Copy `index.html`, `styles.css`, `app.js` directly to your webroot.

---

## 🇮🇳 Languages Supported

- **English** (Default)
- **हिन्दी (Hindi)** — Natural Indian insurance business phrasing
- **ગુજરાતી (Gujarati)** — Authentic Gujarati commercial brokerage terminology

Language choice is automatically persisted in `localStorage` (`brokeros.language`) and updates `html lang`.
