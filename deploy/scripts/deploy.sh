#!/bin/bash
set -e

echo "=========================================="
echo "BrokerOS Deployment Script"
echo "=========================================="

# Colors for output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
NC='\033[0m'

print_status() {
    echo -e "${GREEN}[✓]${NC} $1"
}

print_warning() {
    echo -e "${YELLOW}[!]${NC} $1"
}

print_error() {
    echo -e "${RED}[✗]${NC} $1"
}

# Configuration
APP_DIR="/opt/brokeros"
COMPOSE_FILE="docker-compose.prod.yml"
ENV_FILE=".env"
BACKUP_DIR="/opt/brokeros-backups"

# Navigate to app directory
cd "$APP_DIR"

# Check if .env exists
if [ ! -f "$ENV_FILE" ]; then
    print_error ".env file not found! Copy from .env.production.example first."
    exit 1
fi

echo ""
echo "Step 1: Creating backup of current deployment..."
mkdir -p "$BACKUP_DIR"
BACKUP_NAME="backup-$(date +%Y%m%d-%H%M%S)"
if docker compose -f "$COMPOSE_FILE" ps -q sqlserver 2>/dev/null | grep -q .; then
    print_status "Creating database backup..."
    docker compose -f "$COMPOSE_FILE" exec -T sqlserver /opt/mssql-tools18/bin/sqlcmd \
        -S localhost -U sa -P "$(grep MSSQL_SA_PASSWORD .env | cut -d'=' -f2)" -C \
        -Q "BACKUP DATABASE [BrokerOS] TO DISK = N'/var/opt/mssql/backup/${BACKUP_NAME}.bak' WITH INIT" 2>/dev/null || true
    print_status "Backup created: ${BACKUP_NAME}"
else
    print_warning "SQL Server not running, skipping database backup"
fi

echo ""
echo "Step 2: Pulling latest code..."
git fetch origin main
git reset --hard origin/main
print_status "Code updated to latest main branch"

echo ""
echo "Step 3: Building and starting containers..."
docker compose -f "$COMPOSE_FILE" --env-file "$ENV_FILE" up -d --build
print_status "Containers started"

echo ""
echo "Step 4: Waiting for services to be healthy..."
RETRY_COUNT=0
MAX_RETRIES=30

while [ $RETRY_COUNT -lt $MAX_RETRIES ]; do
    if curl -fsS http://127.0.0.1/health > /dev/null 2>&1; then
        print_status "API is healthy!"
        break
    fi
    RETRY_COUNT=$((RETRY_COUNT + 1))
    echo "Waiting for API... (attempt $RETRY_COUNT/$MAX_RETRIES)"
    sleep 10
done

if [ $RETRY_COUNT -eq $MAX_RETRIES ]; then
    print_error "API failed to become healthy. Check logs with:"
    echo "  docker compose -f $COMPOSE_FILE logs api"
    exit 1
fi

echo ""
echo "Step 5: Cleaning up old images..."
docker image prune -f
print_status "Old images cleaned up"

echo ""
echo "Step 6: Verifying deployment..."
docker compose -f "$COMPOSE_FILE" ps

echo ""
echo "=========================================="
echo -e "${GREEN}Deployment complete!${NC}"
echo "=========================================="
echo ""
echo "Health check: $(curl -sS http://127.0.0.1/health | head -c 100)"
echo ""
