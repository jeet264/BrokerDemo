#!/bin/bash
set -e

echo "=========================================="
echo "BrokerOS Server Setup Script"
echo "For Contabo VPS (Ubuntu 22.04/24.04)"
echo "=========================================="

# Colors for output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
NC='\033[0m' # No Color

print_status() {
    echo -e "${GREEN}[✓]${NC} $1"
}

print_warning() {
    echo -e "${YELLOW}[!]${NC} $1"
}

print_error() {
    echo -e "${RED}[✗]${NC} $1"
}

# Check if running as root
if [ "$EUID" -ne 0 ]; then
    print_error "Please run as root (use: sudo bash setup-server.sh)"
    exit 1
fi

echo ""
echo "Step 1: Updating system packages..."
apt-get update
apt-get upgrade -y
print_status "System packages updated"

echo ""
echo "Step 2: Installing required packages..."
apt-get install -y \
    ca-certificates \
    curl \
    git \
    openssl \
    ufw \
    htop \
    vim \
    fail2ban
print_status "Required packages installed"

echo ""
echo "Step 3: Installing Docker..."
if ! command -v docker &> /dev/null; then
    curl -fsSL https://get.docker.com | sh
    systemctl enable --now docker
    print_status "Docker installed and started"
else
    print_status "Docker already installed"
fi

# Verify Docker
docker --version
docker compose version

echo ""
echo "Step 4: Configuring firewall (UFW)..."
ufw allow OpenSSH
ufw allow 80/tcp
ufw allow 443/tcp
ufw --force enable
print_status "Firewall configured (SSH, HTTP, HTTPS allowed)"

echo ""
echo "Step 5: Configuring fail2ban for SSH protection..."
cat > /etc/fail2ban/jail.local << 'EOF'
[DEFAULT]
bantime = 1h
findtime = 10m
maxretry = 5

[sshd]
enabled = true
port = ssh
filter = sshd
logpath = /var/log/auth.log
maxretry = 3
EOF
systemctl enable fail2ban
systemctl restart fail2ban
print_status "fail2ban configured for SSH protection"

echo ""
echo "Step 6: Creating application directory..."
mkdir -p /opt/brokeros
print_status "Created /opt/brokeros"

echo ""
echo "Step 7: Setting up swap (recommended for 4GB servers)..."
if [ ! -f /swapfile ]; then
    fallocate -l 2G /swapfile
    chmod 600 /swapfile
    mkswap /swapfile
    swapon /swapfile
    echo '/swapfile none swap sw 0 0' >> /etc/fstab
    print_status "2GB swap file created"
else
    print_status "Swap already configured"
fi

echo ""
echo "Step 8: Optimizing Docker for SQL Server..."
# SQL Server needs specific memory settings
cat > /etc/sysctl.d/99-mssql.conf << 'EOF'
vm.max_map_count=262144
vm.swappiness=10
EOF
sysctl --system
print_status "System optimized for SQL Server"

echo ""
echo "=========================================="
echo -e "${GREEN}Server setup complete!${NC}"
echo "=========================================="
echo ""
echo "Next steps:"
echo "1. Clone your repository to /opt/brokeros:"
echo "   cd /opt/brokeros"
echo "   git clone <your-repo-url> ."
echo ""
echo "2. Create your .env file:"
echo "   cp .env.production.example .env"
echo "   nano .env"
echo ""
echo "3. Start the application:"
echo "   docker compose -f docker-compose.prod.yml --env-file .env up -d --build"
echo ""
echo "4. Check status:"
echo "   docker compose -f docker-compose.prod.yml ps"
echo "   curl http://127.0.0.1/health"
echo ""
