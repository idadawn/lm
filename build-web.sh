#!/bin/bash
# ============================================
# Only Build Web Script
# Usage: ./build-web.sh
# ============================================

set -e

# Configuration
SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
PROJECT_ROOT="$SCRIPT_DIR"
WEB_PROJECT_PATH="${PROJECT_ROOT}/web"
OUTPUT_DIR="${PROJECT_ROOT}/apps/web"

# Colors
GREEN='\033[0;32m'
RED='\033[0;31m'
YELLOW='\033[1;33m'
NC='\033[0m' # No Color

log_info() {
    echo -e "${GREEN}[INFO]${NC} $1"
}

log_error() {
    echo -e "${RED}[ERROR]${NC} $1"
}

log_warn() {
    echo -e "${YELLOW}[WARN]${NC} $1"
}

# Check Node.js
if ! command -v node &> /dev/null; then
    log_error "node command not found. Please install Node.js >= 16.15.0."
    exit 1
fi

NODE_VERSION=$(node -v | cut -d'v' -f2 | cut -d'.' -f1)
if [ "$NODE_VERSION" -lt 16 ]; then
    log_error "Node.js version must be >= 16.15.0. Current version: $(node -v)"
    exit 1
fi

log_info "Node.js version: $(node -v)"

# Check pnpm
if ! command -v pnpm &> /dev/null; then
    log_error "pnpm command not found. Please install pnpm >= 8.1.0."
    log_info "Install pnpm: npm install -g pnpm"
    exit 1
fi

log_info "pnpm version: $(pnpm -v)"

# Check project directory
if [ ! -d "$WEB_PROJECT_PATH" ]; then
    log_error "Web project directory not found: $WEB_PROJECT_PATH"
    exit 1
fi

# Check package.json
if [ ! -f "${WEB_PROJECT_PATH}/package.json" ]; then
    log_error "package.json not found in ${WEB_PROJECT_PATH}"
    exit 1
fi

log_info "Building Web..."
log_info "Project: $WEB_PROJECT_PATH"
log_info "Output: $OUTPUT_DIR"

# Clean output directory
if [ -d "$OUTPUT_DIR" ]; then
    log_info "Cleaning output directory..."
    rm -rf "$OUTPUT_DIR"
fi

mkdir -p "$OUTPUT_DIR"

# Install dependencies
cd "$WEB_PROJECT_PATH"

log_info "Installing dependencies..."
pnpm install

# Build
cd "$WEB_PROJECT_PATH"

log_info "Building production..."
pnpm build

# Check if dist directory exists
if [ ! -d "${WEB_PROJECT_PATH}/dist" ]; then
    log_error "Build failed. dist directory not found."
    exit 1
fi

# Copy dist to output directory
log_info "Copying build artifacts to ${OUTPUT_DIR}..."
cp -r "${WEB_PROJECT_PATH}/dist/"* "$OUTPUT_DIR/"

if [ $? -eq 0 ]; then
    log_info "Build successful! Artifacts are in $OUTPUT_DIR"
    log_info "Built files:"
    ls -la "$OUTPUT_DIR"
else
    log_error "Build failed."
    exit 1
fi
