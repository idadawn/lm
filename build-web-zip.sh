#!/bin/bash
# ============================================
# Deploy Web from dist.zip
# Usage: ./build-web-zip.sh [zip_file_path]
# Default zip file: ./web/dist.zip
# ============================================

set -e

# Configuration
SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
PROJECT_ROOT="$SCRIPT_DIR"
DEFAULT_ZIP_FILE="${PROJECT_ROOT}/web/dist.zip"
OUTPUT_DIR="${PROJECT_ROOT}/apps/dist"

# Get zip file path from argument or use default
ZIP_FILE="${1:-$DEFAULT_ZIP_FILE}"

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

# Check unzip command
if ! command -v unzip &> /dev/null; then
    log_error "unzip command not found. Please install unzip."
    log_info "Install: sudo apt-get install -y unzip"
    exit 1
fi

# Check if zip file exists
if [ ! -f "$ZIP_FILE" ]; then
    log_error "Zip file not found: $ZIP_FILE"
    log_info "Usage: $0 [path/to/dist.zip]"
    log_info "Default: ./web/dist.zip"
    exit 1
fi

log_info "Source: $ZIP_FILE"
log_info "Output: $OUTPUT_DIR"

# Clean output directory
if [ -d "$OUTPUT_DIR" ]; then
    log_info "Cleaning output directory..."
    rm -rf "$OUTPUT_DIR"
fi

# Create output directory
mkdir -p "$OUTPUT_DIR"

# Unzip
cd "$OUTPUT_DIR"
log_info "Extracting files..."
unzip -o "$ZIP_FILE" -d "$OUTPUT_DIR"

if [ $? -eq 0 ]; then
    log_info "Deploy successful! Files are in $OUTPUT_DIR"
    log_info "Deployed files:"
    ls -la "$OUTPUT_DIR"
else
    log_error "Deploy failed."
    exit 1
fi
