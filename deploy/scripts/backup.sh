#!/bin/bash
set -e

echo "=========================================="
echo "BrokerOS Database Backup Script"
echo "=========================================="

# Colors
GREEN='\033[0;32m'
RED='\033[0;31m'
NC='\033[0m'

print_status() {
    echo -e "${GREEN}[✓]${NC} $1"
}

print_error() {
    echo -e "${RED}[✗]${NC} $1"
}

# Configuration
APP_DIR="/opt/brokeros"
COMPOSE_FILE="docker-compose.prod.yml"
BACKUP_DIR="/opt/brokeros-backups"
RETENTION_DAYS=30

# Navigate to app directory
cd "$APP_DIR"

# Check if .env exists
if [ ! -f ".env" ]; then
    print_error ".env file not found!"
    exit 1
fi

# Get SQL password from .env
SQL_PASSWORD=$(grep MSSQL_SA_PASSWORD .env | cut -d'=' -f2)

# Create backup directory
mkdir -p "$BACKUP_DIR"

# Generate backup filename
BACKUP_NAME="brokeros-$(date +%Y%m%d-%H%M%S).bak"
BACKUP_PATH="/var/opt/mssql/backup/$BACKUP_NAME"

echo ""
echo "Creating backup: $BACKUP_NAME"

# Create backup directory in container if it doesn't exist
docker compose -f "$COMPOSE_FILE" exec -T sqlserver mkdir -p /var/opt/mssql/backup

# Run backup
docker compose -f "$COMPOSE_FILE" exec -T sqlserver /opt/mssql-tools18/bin/sqlcmd \
    -S localhost -U sa -P "$SQL_PASSWORD" -C \
    -Q "BACKUP DATABASE [BrokerOS] TO DISK = N'$BACKUP_PATH' WITH INIT, COMPRESSION"

# Copy backup to host
docker compose -f "$COMPOSE_FILE" cp sqlserver:$BACKUP_PATH "$BACKUP_DIR/$BACKUP_NAME"

print_status "Backup created: $BACKUP_DIR/$BACKUP_NAME"

# Calculate backup size
BACKUP_SIZE=$(du -h "$BACKUP_DIR/$BACKUP_NAME" | cut -f1)
echo "Backup size: $BACKUP_SIZE"

# Clean up old backups (keep last RETENTION_DAYS days)
echo ""
echo "Cleaning up backups older than $RETENTION_DAYS days..."
find "$BACKUP_DIR" -name "brokeros-*.bak" -type f -mtime +$RETENTION_DAYS -delete
print_status "Old backups cleaned up"

# List current backups
echo ""
echo "Current backups:"
ls -lh "$BACKUP_DIR"/*.bak 2>/dev/null | tail -10

echo ""
echo "=========================================="
echo -e "${GREEN}Backup complete!${NC}"
echo "=========================================="
