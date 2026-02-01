#!/usr/bin/env bash
set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "$0")" && pwd)"
APPS_DIR="$SCRIPT_DIR/apps"
ENV_FILE="$APPS_DIR/.env.apps"

if [ -f "$APPS_DIR/.env.apps.local" ]; then
  ENV_FILE="$APPS_DIR/.env.apps.local"
fi

if [ ! -d "$APPS_DIR" ]; then
  echo "[ERROR] 未找到 apps 目录: $APPS_DIR" >&2
  exit 1
fi

if [ ! -f "$ENV_FILE" ]; then
  echo "[ERROR] 未找到环境变量文件: $ENV_FILE" >&2
  exit 1
fi

cd "$APPS_DIR"

echo "[STEP] 使用环境文件: $ENV_FILE"
docker compose --env-file "$ENV_FILE" up -d

echo "[INFO] 前后端服务已启动"
