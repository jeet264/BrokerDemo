# Host BrokerOS

Two ways to show a public demo:

| Path | Cost | When to use |
|---|---|---|
| **A. Free tunnel** (this machine) | ₹0 | A distributor meeting. Your laptop/PC must stay on. The URL changes each time. |
| **B. VPS** (always on) | ~₹400–800 / month | A stable `demo.yourdomain.in` you can send ahead of time. |

SQL Server is what makes “always-on cloud for ₹0” hard. Free Node/Postgres hosts will not run this stack. For a meeting, **path A is enough**.

---

## A. Free public demo (no VPS)

This publishes the app you already run locally. Visitors open an `https://….trycloudflare.com` link. Nothing is billed.

### A1. One-time: install Cloudflare’s tunnel tool

Pick one:

**Windows (winget):**
```text
winget install --id Cloudflare.cloudflared
```

**macOS (Homebrew):**
```text
brew install cloudflared
```

**Or Docker** (no install): skip this and use the `docker run` command in A3.

Confirm:
```bash
cloudflared --version
```

### A2. Start BrokerOS locally (same as everyday development)

Terminal 1 — SQL:
```bash
docker compose up -d
```

Terminal 2 — API:
```bash
dotnet run --project src/BrokerOS.Api
```

Terminal 3 — website (tunnel mode, one public origin, API proxied):
```bash
cd frontend/web
npm install
npm run dev:public
```

Leave all three running. The site is still at http://localhost:5173 on your machine.

### A3. Open the public HTTPS link

**New terminal:**
```bash
cloudflared tunnel --url http://localhost:5173
```

**Or with Docker:**
```bash
docker run --rm -it cloudflare/cloudflared tunnel --url http://host.docker.internal:5173
```

On Linux without `host.docker.internal`, use:
```bash
docker run --rm -it --network host cloudflare/cloudflared tunnel --url http://127.0.0.1:5173
```

Wait for a line like:

```text
https://random-words-here.trycloudflare.com
```

That is the demo URL. Send it to the meeting. Login is still `admin@apexbrokers.in` / `Demo@12345`.

### A4. Rules for this free URL

- Keep the three terminals (SQL, API, web) **and** `cloudflared` running for the whole meeting.
- If you stop the tunnel, the link dies. Next time you get a **new** random URL.
- Do not put real client data behind this link. Anyone with the URL can reach the demo.
- Sleeping the laptop or closing the lid drops the demo.

There is no good **always-on ₹0** host for this stack. Vercel/Netlify/Render free tiers do not run SQL Server. Azure SQL has a free database, but the .NET API still needs a server (App Service F1 sleeps and usually needs a card on the Azure account). Use the tunnel for meetings; rent a VPS when you need a link that stays up.

---

## B. Always-on host (paid VPS)

You need a **4 GB** Linux VPS. SQL Server will not run reliably on 1 GB or 2 GB.

### B0. What you will end up with

On the server, Docker runs four containers:

| Container | Role |
|---|---|
| `sqlserver` | Database (not exposed to the internet) |
| `api` | .NET API + renewal reminder worker |
| `web` | React app (static files) |
| `caddy` | Public HTTPS/HTTP entry — `/api` and `/health` go to the API, everything else is the website |

First boot uses **HTTP** on the server IP so you can test immediately. After DNS is ready, you switch Caddy to your domain and it gets a free HTTPS certificate.

---

## 1. Buy a server

1. Create a Ubuntu 22.04 or 24.04 VPS with **4 GB RAM** and **2 vCPU**.
2. Region: **Mumbai / Bangalore / Central India** if you can (DigitalOcean, AWS Lightsail, Azure). Any region is fine for fictional demo data.
3. Add your SSH key. Note the public IP, for example `203.0.113.10`.
4. Optional now, required for HTTPS later: buy a domain and create an **A record** `demo.yourdomain.in` → that IP. Wait until `ping demo.yourdomain.in` hits the VPS before step 9.

---

## 2. SSH in and install Docker

```bash
ssh root@YOUR_SERVER_IP
```

If you log in as a sudo user, prefix the install with `sudo`.

```bash
apt-get update
apt-get install -y ca-certificates curl git openssl ufw
curl -fsSL https://get.docker.com | sh
systemctl enable --now docker

ufw allow OpenSSH
ufw allow 80/tcp
ufw allow 443/tcp
ufw --force enable
```

Confirm Docker works:

```bash
docker version
docker compose version
```

---

## 3. Clone the repo

```bash
mkdir -p /opt
cd /opt
git clone https://github.com/jeet264/BrokerDemo.git brokeros
cd /opt/brokeros
git checkout main
```

If the repo is private, create a GitHub personal access token and clone with it, or copy the files with `scp`.

---

## 4. Create the secret file

```bash
cd /opt/brokeros
cp .env.production.example .env
nano .env
```

Set these values:

1. `PUBLIC_ORIGIN=http://YOUR_SERVER_IP` (example `http://203.0.113.10`)
2. `ACME_EMAIL=` a real email you own (used later for HTTPS)
3. `MSSQL_SA_PASSWORD=` a strong password (letters, numbers, symbol, 8+ characters; SQL Server rejects weak ones)
4. `JWT_KEY=` at least 32 characters. Generate one:

```bash
openssl rand -base64 48
```

Paste that output into `JWT_KEY`. Leave `SITE_ADDRESS=:80` for the first start.

Save and exit (`Ctrl+O`, Enter, `Ctrl+X` in nano).

---

## 5. Start BrokerOS

Still in `/opt/brokeros`:

```bash
docker compose -f docker-compose.prod.yml --env-file .env up -d --build
```

First build takes several minutes (downloads SQL Server, .NET, Node). Watch:

```bash
docker compose -f docker-compose.prod.yml ps
docker compose -f docker-compose.prod.yml logs -f api
```

Wait until the API log shows the database is ready and `Starting BrokerOS API`. `Ctrl+C` stops following logs; containers keep running.

Check health:

```bash
curl -sS http://127.0.0.1/health
```

You should see a healthy JSON payload.

---

## 6. Open the site and sign in

In a browser:

```text
http://YOUR_SERVER_IP
```

Demo logins (Apex book, fictional data):

| Role | Email | Password |
|---|---|---|
| Admin | admin@apexbrokers.in | Demo@12345 |
| Manager | manager@apexbrokers.in | Demo@12345 |
| Employee | employee@apexbrokers.in | Demo@12345 |

This public demo still uses those passwords unless you change them in the database. Do not put real client data on this server.

Smoke-check: **Overview** → one overdue renewal → **My Day**.

---

## 7. Put it on a domain with HTTPS

Do this only after the A record for `demo.yourdomain.in` points at the VPS.

1. Edit `/opt/brokeros/.env`:

```text
SITE_ADDRESS=demo.yourdomain.in
PUBLIC_ORIGIN=https://demo.yourdomain.in
```

2. Recreate Caddy and the API so they pick up the new values:

```bash
cd /opt/brokeros
docker compose -f docker-compose.prod.yml --env-file .env up -d
```

3. Open `https://demo.yourdomain.in`. Caddy requests a Let's Encrypt certificate automatically. Ports **80 and 443** must stay open.

If HTTPS fails, DNS is usually not pointing at this machine yet. Check:

```bash
docker compose -f docker-compose.prod.yml logs caddy
```

---

## 8. Update the app later

On the server:

```bash
cd /opt/brokeros
git pull origin main
docker compose -f docker-compose.prod.yml --env-file .env up -d --build
```

Database migrations run on API start when `BrokerOS__ApplyMigrationsOnStartup` is true (already set in the compose file).

---

## 9. Backups

SQL data lives in the Docker volume `brokeros_sql_data`. Copy it off the box at least weekly, and before a meeting if you have been typing live notes.

Example dump from inside the SQL container (replace the password):

```bash
docker compose -f docker-compose.prod.yml exec sqlserver \
  /opt/mssql-tools18/bin/sqlcmd -S localhost -U sa -P 'YOUR_SQL_PASSWORD' -C \
  -Q "BACKUP DATABASE [BrokerOS] TO DISK = N'/var/opt/mssql/backup/brokeros.bak' WITH INIT"
```

Then copy `/var/opt/mssql/backup/` out of the volume to object storage or your laptop.

---

## 10. Turn this into a real customer host

When you are ready for a live brokerage (not Apex demo data):

1. Use a **new** VPS or wipe volumes (`docker compose -f docker-compose.prod.yml down -v` destroys the database).
2. In `.env` set:

```text
SEED_DEMO_DATA=false
ENABLE_DEMO_RESET=false
```

3. Rebuild:

```bash
docker compose -f docker-compose.prod.yml --env-file .env up -d --build
```

4. Register a real organisation with `POST /api/auth/register-organization`, or insert the first admin yourself. Change the SQL `sa` password and restrict SSH.

Do not run a customer book on the same database as the Apex demo.

---

## Troubleshooting

| Symptom | What to do |
|---|---|
| `docker compose ps` shows `sqlserver` restarting | Password too weak, or the VPS has less than 4 GB RAM. Check `logs sqlserver`. |
| Site loads, login fails | Wait for `logs api` to say the database is ready. Confirm `.env` `JWT_KEY` is 32+ characters. |
| Browser cannot reach the site | Security group / `ufw` must allow 80 (and 443 after step 7). |
| `https://` certificate error | DNS A record must already point here. `SITE_ADDRESS` must be the hostname only, no `https://`. |
| Blank page after a frontend change | Rebuild `web`: `docker compose -f docker-compose.prod.yml --env-file .env up -d --build web caddy` |
| Need a clean Apex book | Admin **Settings → Reset Demo Data**, or wipe volumes and start again. |

Stop everything:

```bash
cd /opt/brokeros
docker compose -f docker-compose.prod.yml down
```

That keeps the database volume. Add `-v` only if you intend to destroy demo data.
