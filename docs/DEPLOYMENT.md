# BrokerOS Deployment Guide

Complete guide for deploying BrokerOS to a Contabo VPS with Docker and CI/CD.

## Current Production Deployment

| Service | URL |
|---------|-----|
| **Frontend** | http://217.217.249.136:8000 |
| **Backend API** | http://217.217.249.136:8000/api |
| **Swagger Docs** | http://217.217.249.136:8000/swagger |
| **Health Check** | http://217.217.249.136:8000/health |

**Server**: Contabo VPS at `217.217.249.136`  
**App Directory**: `~/BrokerDemo`

## Table of Contents

1. [Prerequisites](#prerequisites)
2. [Architecture Overview](#architecture-overview)
3. [Server Setup (Contabo)](#server-setup-contabo)
4. [Manual Deployment](#manual-deployment)
5. [CI/CD Setup (GitHub Actions)](#cicd-setup-github-actions)
6. [Domain & HTTPS Setup](#domain--https-setup)
7. [Backup & Restore](#backup--restore)
8. [Monitoring & Maintenance](#monitoring--maintenance)
9. [Troubleshooting](#troubleshooting)

---

## Prerequisites

### Contabo Server Requirements

| Requirement | Minimum | Recommended |
|-------------|---------|-------------|
| RAM | 4 GB | 8 GB |
| vCPU | 2 | 4 |
| Storage | 50 GB SSD | 100 GB SSD |
| OS | Ubuntu 22.04 LTS | Ubuntu 24.04 LTS |

> **Important**: SQL Server requires at least 2 GB RAM. A 4 GB VPS is the minimum for this stack.

### What You'll Need

- Contabo VPS with Ubuntu 22.04 or 24.04
- SSH access to the server
- (Optional) Domain name for HTTPS
- (Optional) GitHub account for CI/CD

---

## Architecture Overview

### With Existing Nginx (Current Setup)

```
┌──────────────────────────────────────────────────────────────┐
│                    Contabo VPS (217.217.249.136)             │
│                                                              │
│  ┌─────────────────────────────────────────────────────┐    │
│  │              Existing Nginx (:80/:443)              │    │
│  │         (iposcanner, strapi, minio, etc.)           │    │
│  └─────────────────────────────────────────────────────┘    │
│                                                              │
│  ┌─────────────────────────────────────────────────────┐    │
│  │           BrokerOS Nginx Config (:8000)             │    │
│  │                                                     │    │
│  │    /api/* ──→ API Container (8080)                  │    │
│  │    /*     ──→ Web Container (3000)                  │    │
│  └─────────────────────────────────────────────────────┘    │
│                                                              │
│  ┌─────────────────────────────────────────────────────┐    │
│  │                   Docker                             │    │
│  │  ┌─────────┐  ┌─────────┐  ┌─────────────────┐     │    │
│  │  │   API   │  │   Web   │  │   SQL Server    │     │    │
│  │  │  :8080  │  │  :3000  │  │  :1433 (internal)│     │    │
│  │  └─────────┘  └─────────┘  └─────────────────┘     │    │
│  └─────────────────────────────────────────────────────┘    │
└──────────────────────────────────────────────────────────────┘
```

| Container | Internal Port | External Access |
|-----------|---------------|-----------------|
| **brokeros-api** | 8080 | Via Nginx :8000/api |
| **brokeros-web** | 3000 | Via Nginx :8000/ |
| **brokeros-sqlserver** | 1433 | Not exposed (internal only) |

---

## Server Setup (Contabo)

### Step 1: Order and Access Your VPS

1. Order a VPS from [Contabo](https://contabo.com/) (VPS S or higher recommended)
2. Choose Ubuntu 22.04 or 24.04
3. Note your server IP address (e.g., `123.45.67.89`)
4. SSH into your server:

```bash
ssh root@YOUR_SERVER_IP
```

### Step 2: Run the Setup Script

The setup script installs Docker, configures firewall, and optimizes the system for SQL Server.

```bash
# Download and run setup script
curl -sSL https://raw.githubusercontent.com/YOUR_USERNAME/BrokerDemo/main/deploy/scripts/setup-server.sh | bash
```

Or manually run each step:

```bash
# Update system
apt-get update && apt-get upgrade -y

# Install required packages
apt-get install -y ca-certificates curl git openssl ufw htop fail2ban

# Install Docker
curl -fsSL https://get.docker.com | sh
systemctl enable --now docker

# Configure firewall
ufw allow OpenSSH
ufw allow 80/tcp
ufw allow 443/tcp
ufw --force enable

# Verify Docker
docker --version
docker compose version
```

### Step 3: Clone the Repository

```bash
mkdir -p /opt/brokeros
cd /opt/brokeros
git clone https://github.com/YOUR_USERNAME/BrokerDemo.git .
```

For private repositories, use a Personal Access Token:
```bash
git clone https://YOUR_TOKEN@github.com/YOUR_USERNAME/BrokerDemo.git .
```

---

## Manual Deployment

### Step 1: Configure Environment Variables

```bash
cd /opt/brokeros
cp .env.production.example .env
nano .env
```

Update these values:

```bash
# Your Contabo server IP (e.g., 123.45.67.89)
SITE_ADDRESS=:80
PUBLIC_ORIGIN=http://YOUR_CONTABO_IP

# Your email for HTTPS certificates
ACME_EMAIL=your-email@domain.com

# Strong SQL password (8+ chars, mixed case, numbers, symbols)
MSSQL_SA_PASSWORD=YourSecurePassword123!

# Generate JWT key: openssl rand -base64 48
JWT_KEY=your-generated-jwt-key-at-least-32-characters

# Demo settings (set both to false for production)
SEED_DEMO_DATA=true
ENABLE_DEMO_RESET=true
```

Generate a secure JWT key:
```bash
openssl rand -base64 48
```

### Step 2: Start the Application

```bash
docker compose -f docker-compose.prod.yml --env-file .env up -d --build
```

First build takes 5-10 minutes. Monitor progress:

```bash
# Watch all containers
docker compose -f docker-compose.prod.yml logs -f

# Watch specific container
docker compose -f docker-compose.prod.yml logs -f api
```

### Step 3: Verify Deployment

```bash
# Check container status
docker compose -f docker-compose.prod.yml ps

# Check API health
curl http://127.0.0.1/health

# Check from external
curl http://YOUR_CONTABO_IP/health
```

### Step 4: Access the Application

Open in browser: `http://YOUR_CONTABO_IP`

Demo credentials:
| Role | Email | Password |
|------|-------|----------|
| Admin | admin@apexbrokers.in | Demo@12345 |
| Manager | manager@apexbrokers.in | Demo@12345 |
| Employee | employee@apexbrokers.in | Demo@12345 |

---

## CI/CD Setup (GitHub Actions)

The CI/CD pipeline automatically builds, tests, and deploys when you push to `main`.

### Step 1: Configure GitHub Secrets

Go to your GitHub repository → Settings → Secrets and variables → Actions

Add these secrets:

| Secret | Description |
|--------|-------------|
| `SERVER_HOST` | Your Contabo server IP |
| `SERVER_USER` | SSH username (usually `root`) |
| `SERVER_SSH_KEY` | Private SSH key for server access |
| `SERVER_PORT` | SSH port (default: `22`) |

### Step 2: Generate SSH Key for CI/CD

On your local machine:

```bash
# Generate a new key pair for CI/CD
ssh-keygen -t ed25519 -C "github-actions" -f ~/.ssh/github_actions

# Copy public key to server
ssh-copy-id -i ~/.ssh/github_actions.pub root@YOUR_CONTABO_IP

# Copy private key content (add to GitHub as SERVER_SSH_KEY)
cat ~/.ssh/github_actions
```

### Step 3: Create GitHub Environment

1. Go to Settings → Environments → New environment
2. Name it `production`
3. (Optional) Add required reviewers for manual approval
4. (Optional) Add environment secrets specific to production

### Step 4: Test the Pipeline

Push to main branch:

```bash
git add .
git commit -m "feat: enable CI/CD deployment"
git push origin main
```

Monitor the workflow in GitHub Actions tab.

### CI/CD Workflow Overview

```
┌─────────────────┐
│   Push to main  │
└────────┬────────┘
         │
         ▼
┌─────────────────┐
│  Build & Test   │
│  - .NET build   │
│  - Run tests    │
│  - Build React  │
└────────┬────────┘
         │
         ▼
┌─────────────────┐
│  Build Docker   │
│  - API image    │
│  - Web image    │
│  - Push to GHCR │
└────────┬────────┘
         │
         ▼
┌─────────────────┐
│    Deploy       │
│  - SSH to VPS   │
│  - Pull code    │
│  - Restart      │
│  - Health check │
└─────────────────┘
```

---

## Domain & HTTPS Setup

### Step 1: Configure DNS

1. Buy a domain (e.g., `yourdomain.in`)
2. Add an A record pointing to your Contabo IP:
   - Name: `demo` (or `@` for root domain)
   - Type: `A`
   - Value: `YOUR_CONTABO_IP`
   - TTL: 300

3. Verify DNS propagation:
```bash
ping demo.yourdomain.in
```

### Step 2: Update Environment

```bash
cd /opt/brokeros
nano .env
```

Change:
```bash
SITE_ADDRESS=demo.yourdomain.in
PUBLIC_ORIGIN=https://demo.yourdomain.in
ACME_EMAIL=your-real-email@domain.com
```

### Step 3: Restart Services

```bash
docker compose -f docker-compose.prod.yml --env-file .env up -d
```

Caddy automatically obtains a Let's Encrypt certificate. Check logs:
```bash
docker compose -f docker-compose.prod.yml logs caddy
```

---

## Backup & Restore

### Automated Daily Backups

Set up a cron job for daily backups:

```bash
# Edit crontab
crontab -e

# Add daily backup at 2 AM
0 2 * * * /opt/brokeros/deploy/scripts/backup.sh >> /var/log/brokeros-backup.log 2>&1
```

### Manual Backup

```bash
cd /opt/brokeros
./deploy/scripts/backup.sh
```

Backups are stored in `/opt/brokeros-backups/`.

### Restore from Backup

```bash
# List available backups
ls -la /opt/brokeros-backups/

# Restore specific backup
./deploy/scripts/restore.sh brokeros-20240115-020000.bak
```

### Off-site Backup (Recommended)

Copy backups to external storage:

```bash
# To S3
aws s3 cp /opt/brokeros-backups/brokeros-latest.bak s3://your-bucket/backups/

# To local machine
scp root@YOUR_SERVER:/opt/brokeros-backups/*.bak ./backups/
```

---

## Monitoring & Maintenance

### Health Checks

```bash
# API health
curl http://127.0.0.1/health

# Container status
docker compose -f docker-compose.prod.yml ps

# Resource usage
docker stats
```

### View Logs

```bash
# All services
docker compose -f docker-compose.prod.yml logs -f

# Specific service
docker compose -f docker-compose.prod.yml logs -f api
docker compose -f docker-compose.prod.yml logs -f sqlserver
docker compose -f docker-compose.prod.yml logs -f caddy
```

### Update Application

```bash
cd /opt/brokeros
./deploy/scripts/deploy.sh
```

Or manually:
```bash
git pull origin main
docker compose -f docker-compose.prod.yml --env-file .env up -d --build
```

### Clean Up Docker

```bash
# Remove unused images
docker image prune -f

# Remove all unused data (caution: removes stopped containers too)
docker system prune -f
```

### System Monitoring

```bash
# Disk usage
df -h

# Memory usage
free -h

# CPU and memory by process
htop
```

---

## Troubleshooting

### Common Issues

#### SQL Server Won't Start

**Symptom**: Container keeps restarting

**Cause**: Usually weak password or insufficient RAM

**Fix**:
```bash
# Check logs
docker compose -f docker-compose.prod.yml logs sqlserver

# Ensure password is strong (8+ chars, uppercase, lowercase, number, symbol)
# Ensure server has at least 4 GB RAM
free -h
```

#### API Can't Connect to Database

**Symptom**: Login fails, 500 errors

**Fix**:
```bash
# Wait for SQL Server to be fully ready
docker compose -f docker-compose.prod.yml logs -f sqlserver

# Check connection string in .env matches SQL password
grep MSSQL_SA_PASSWORD .env
```

#### Site Not Accessible

**Symptom**: Connection refused from browser

**Fix**:
```bash
# Check firewall
ufw status

# Ensure ports are open
ufw allow 80/tcp
ufw allow 443/tcp

# Check Caddy is running
docker compose -f docker-compose.prod.yml ps caddy
docker compose -f docker-compose.prod.yml logs caddy
```

#### HTTPS Certificate Fails

**Symptom**: SSL error in browser

**Fix**:
```bash
# Verify DNS is pointing to this server
dig demo.yourdomain.in

# Check Caddy logs for ACME errors
docker compose -f docker-compose.prod.yml logs caddy

# Ensure ACME_EMAIL is set correctly
grep ACME_EMAIL .env
```

#### Out of Disk Space

**Symptom**: Docker build fails, application crashes

**Fix**:
```bash
# Check disk usage
df -h

# Clean Docker artifacts
docker system prune -af
docker volume prune -f

# Remove old logs
truncate -s 0 /var/log/*.log
```

### Useful Commands

```bash
# Restart all services
docker compose -f docker-compose.prod.yml restart

# Restart specific service
docker compose -f docker-compose.prod.yml restart api

# Stop all services
docker compose -f docker-compose.prod.yml down

# Stop and remove volumes (DANGER: deletes database!)
docker compose -f docker-compose.prod.yml down -v

# Enter container shell
docker compose -f docker-compose.prod.yml exec api bash
docker compose -f docker-compose.prod.yml exec sqlserver bash

# Run SQL query
docker compose -f docker-compose.prod.yml exec sqlserver /opt/mssql-tools18/bin/sqlcmd \
  -S localhost -U sa -P 'YOUR_PASSWORD' -C -Q "SELECT name FROM sys.databases"
```

---

## Quick Reference

### File Locations

| Path | Description |
|------|-------------|
| `/opt/brokeros` | Application root |
| `/opt/brokeros/.env` | Environment configuration |
| `/opt/brokeros-backups` | Database backups |
| `/var/lib/docker/volumes` | Docker volumes (including SQL data) |

### Ports

| Port | Service | Exposed |
|------|---------|---------|
| 80 | Caddy (HTTP) | Yes |
| 443 | Caddy (HTTPS) | Yes |
| 8080 | API | Internal only |
| 1433 | SQL Server | Internal only |

### Environment Variables

| Variable | Description |
|----------|-------------|
| `SITE_ADDRESS` | `:80` for IP, domain name for HTTPS |
| `PUBLIC_ORIGIN` | Full URL for CORS |
| `MSSQL_SA_PASSWORD` | SQL Server password |
| `JWT_KEY` | JWT signing key (32+ chars) |
| `SEED_DEMO_DATA` | Load demo data on first start |
| `ENABLE_DEMO_RESET` | Allow demo data reset |
| `ACME_EMAIL` | Email for HTTPS certificates |

---

## Support

- Check existing documentation in `/docs`
- Review logs: `docker compose -f docker-compose.prod.yml logs -f`
- For issues, create a GitHub issue with logs and environment details
