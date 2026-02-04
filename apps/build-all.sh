#!/bin/bash
# ============================================
# Unified Build Script
# Usage: ./build-all.sh
# ============================================

set -e

# Configuration
SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
BUILD_WEB_SCRIPT="${SCRIPT_DIR}/build-web.sh"
BUILD_API_SCRIPT="${SCRIPT_DIR}/build-api.sh"

# Colors
GREEN='\033[0;32m'
BLUE='\033[0;34m'
NC='\033[0m'

log_info() {
    echo -e "${GREEN}[INFO]${NC} $1"
}

log_step() {
    echo -e "${BLUE}[STEP]${NC} $1"
}

# Check scripts
if [ ! -f "$BUILD_WEB_SCRIPT" ]; then
    echo "Error: Web build script not found at $BUILD_WEB_SCRIPT"
    exit 1
fi

if [ ! -f "$BUILD_API_SCRIPT" ]; then
    echo "Error: API build script not found at $BUILD_API_SCRIPT"
    exit 1
fi

echo ""
echo "========================================"
echo "  Starting Unified Build"
echo "========================================"
echo ""

# Build Web
log_step "【1/2】Building Web Image..."
bash "$BUILD_WEB_SCRIPT"

# Build API
echo ""
log_step "【2/2】Building API Image..."
bash "$BUILD_API_SCRIPT"

echo ""
echo "========================================"
log_info "All build tasks completed successfully!"
echo "========================================"
echo ""
