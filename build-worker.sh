#!/bin/bash
# ============================================
# Only Build Worker Script
# Usage: ./build-worker.sh
# ============================================

set -e

# Configuration
SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
PROJECT_ROOT="$SCRIPT_DIR"
WORKER_PROJECT_PATH="${PROJECT_ROOT}/api/src/application/Poxiao.Lab.CalcWorker/Poxiao.Lab.CalcWorker.csproj"
OUTPUT_DIR="${PROJECT_ROOT}/apps/worker"

# Colors
GREEN='\033[0;32m'
RED='\033[0;31m'
NC='\033[0m' # No Color

log_info() {
    echo -e "${GREEN}[INFO]${NC} $1"
}

log_error() {
    echo -e "${RED}[ERROR]${NC} $1"
}

# Check dotnet
if ! command -v dotnet &> /dev/null; then
    log_error "dotnet command not found. Please install .NET SDK."
    exit 1
fi

# Check project file
if [ ! -f "$WORKER_PROJECT_PATH" ]; then
    log_error "Project file not found: $WORKER_PROJECT_PATH"
    exit 1
fi

log_info "Building Worker..."
log_info "Project: $WORKER_PROJECT_PATH"
log_info "Output: $OUTPUT_DIR"

# Clean output directory
if [ -d "$OUTPUT_DIR" ]; then
    log_info "Cleaning output directory..."
    rm -rf "$OUTPUT_DIR"
fi

# Publish
log_info "Publishing..."
dotnet publish "$WORKER_PROJECT_PATH" \
    -c Release \
    -o "$OUTPUT_DIR" \
    --nologo

if [ $? -eq 0 ]; then
    log_info "Build successful! Artifacts are in $OUTPUT_DIR"
else
    log_error "Build failed."
    exit 1
fi
