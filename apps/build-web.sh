#!/bin/bash
# ============================================
# Build Web Docker Image Script
# Usage: ./build-web.sh
# ============================================

set -e

# Configuration
SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
PROJECT_ROOT="$SCRIPT_DIR/.."
WEB_DIR="${PROJECT_ROOT}/web"
DOCKERFILE="${WEB_DIR}/Dockerfile.build"

# Load .env
if [ -f "$SCRIPT_DIR/.env" ]; then
    export $(grep -v '^#' "$SCRIPT_DIR/.env" | xargs)
fi

IMAGE_NAME="${WEB_IMAGE:-lm-web}"

# Colors
GREEN='\033[0;32m'
NC='\033[0m' # No Color

log_info() {
    echo -e "${GREEN}[INFO]${NC} $1"
}

# Check if Dockerfile exists
if [ ! -f "$DOCKERFILE" ]; then
    echo "Error: Dockerfile not found at $DOCKERFILE"
    exit 1
fi

log_info "Building Docker image: $IMAGE_NAME"
log_info "Context: $WEB_DIR"
log_info "Dockerfile: $DOCKERFILE"

# Execute docker build
docker build \
  -t "$IMAGE_NAME" \
  -f "$DOCKERFILE" \
  "$WEB_DIR" \
  --progress=plain \
  --network=host

log_info "Build complete for image: $IMAGE_NAME"
