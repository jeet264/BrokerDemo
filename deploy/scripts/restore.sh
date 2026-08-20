#!/bin/bash
set -e

echo "=========================================="
echo "BrokerOS Database Restore Script"
echo "=========================================="

# Colors
GREEN='\033[0;32m'
RED='\033[0;31m'
YELLOW='\033[1;33m'
NC='\033[0m'

print_status() {
    echo -e "${GREEN}[✓]${NC} $1"
}

print_error() {
    echo -e "${RED}[✗]${NC} $1"
}

print_warning() {
    echo -e "${YELLOW}[!]${NC} $1"
}

# Configuration
APP_DIR="/opt/brokeros"
COMPOSE_FILE="docker-compose.prod.yml"
BACKUP_DIR="/opt/brokeros-backups"

# Check if backup file is provided
if [ -z "$1" ]; then
    print_error "Usage: $0 <backup-file.bak>"
    echo ""
    echo "Available backups:"
    ls -lh "$BACKUP_DIR"/*.bak 2>/dev/null || echo "No backups found in $BACKUP_DIR"
    exit 1
fi

BACKUP_FILE="$1"

# Check if backup file exists
if [ ! -f "$BACKUP_FILE" ]; then
    # Try in backup directory
    if [ -f "$BACKUP_DIR/$BACKUP_FILE" ]; then
        BACKUP_FILE="$BACKUP_DIR/$BACKUP_FILE"
    else
        print_error "Backup file not found: $BACKUP_FILE"
        exit 1
    fi
fi

# Navigate to app directory
cd "$APP_DIR"

# Get SQL password from .env
SQL_PASSWORD=$(grep MSSQL_SA_PASSWORD .env | cut -d'=' -f2)

print_warning "WARNING: This will REPLACE the current database with the backup!"
echo "Backup file: $BACKUP_FILE"
read -p "Are you sure you want to continue? (yes/no): " CONFIRM

if [ "$CONFIRM" != "yes" ]; then
    echo "Restore cancelled."
    exit 0
fi

echo ""
echo "Step 1: Stopping API to release database connections..."
docker compose -f "$COMPOSE_FILE" stop api
print_status "API stopped"

echo ""
echo "Step 2: Copying backup file to SQL Server container..."
BACKUP_NAME=$(basename "$BACKUP_FILE")
docker compose -f "$COMPOSE_FILE" cp "$BACKUP_FILE" "sqlserver:/var/opt/mssql/backup/$BACKUP_NAME"
print_status "Backup file copied"

echo ""
echo "Step 3: Restoring database..."
docker compose -f "$COMPOSE_FILE" exec -T sqlserver /opt/mssql-tools18/bin/sqlcmd \
    -S localhost -U sa -P "$SQL_PASSWORD" -C \
    -Q "ALTER DATABASE [BrokerOS] SET SINGLE_USER WITH ROLLBACK IMMEDIATE; RESTORE DATABASE [BrokerOS] FROM DISK = N'/var/opt/mssql/backup/$BACKUP_NAME' WITH REPLACE; ALTER DATABASE [BrokerOS] SET MULTI_USER;"
print_status "Database restored"

echo ""
echo "Step 4: Starting API..."
docker compose -f "$COMPOSE_FILE" start api
print_status "API started"

echo ""
echo "Step 5: Waiting for API to be healthy..."
sleep 10
for i in {1..12}; do
    if curl -fsS http://127.0.0.1/health > /dev/null 2>&1; then
        print_status "API is healthy!"
        break
    fi
    echo "Waiting... (attempt $i/12)"
    sleep 5
done

echo ""
echo "=========================================="
echo -e "${GREEN}Restore complete!${NC}"
echo "=========================================="
