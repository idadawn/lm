#!/bin/bash
# ============================================
# Build API Docker Image Script
# Usage: ./build-api.sh
# ============================================

set -e

# Configuration
SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
PROJECT_ROOT="$SCRIPT_DIR/.."
API_DIR="${PROJECT_ROOT}/api"
DOCKERFILE="${API_DIR}/Dockerfile.build"

# Load .env
if [ -f "$SCRIPT_DIR/.env" ]; then
    export $(grep -v '^#' "$SCRIPT_DIR/.env" | xargs)
fi

IMAGE_NAME="${API_IMAGE:-lm-api}"

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

log_info "Building Docker image: $IMAGE_NAME:${IMAGE_TAG:-latest}"
log_info "Context: $API_DIR"
log_info "Dockerfile: $DOCKERFILE"

# Execute docker build
docker build \
  -t "$IMAGE_NAME:${IMAGE_TAG:-latest}" \
  -f "$DOCKERFILE" \
  "$API_DIR" \
  --progress=plain \
  --network=host

log_info "Build complete for image: $IMAGE_NAME:${IMAGE_TAG:-latest}"

