#!/bin/bash
# ============================================
# Docker 清理脚本
# 用途：清理容器、数据目录和 Docker volumes
# 使用：./scripts/cleanup.sh
# ============================================

set -e

# 颜色定义
RED='\033[0;31m'
YELLOW='\033[1;33m'
GREEN='\033[0;32m'
NC='\033[0m' # No Color

# 项目配置
COMPOSE_FILE="${COMPOSE_FILE:-docker-compose.yml}"
DEPLOY_DIR="${DEPLOY_DIR:-./deploy}"
CONTAINER_PREFIX="${CONTAINER_PREFIX:-lm}"

echo -e "${YELLOW}===========================================${NC}"
echo -e "${YELLOW}Docker 清理脚本${NC}"
echo -e "${YELLOW}===========================================${NC}"
echo ""

# 1. 停止并删除容器
echo -e "${YELLOW}[1/5] 停止并删除容器...${NC}"
docker-compose -f "$COMPOSE_FILE" down --remove-orphans
echo -e "${GREEN}✓ 容器已清理${NC}"
echo ""

# 2. 删除数据目录
echo -e "${YELLOW}[2/5] 删除数据目录...${NC}"
read -p "是否删除数据目录? (MySQL/Redis/Qdrant) [y/N]: " -n 1 -r
echo
if [[ $REPLY =~ ^[Yy]$ ]]; then
    # 删除各服务数据
    rm -rf "$DEPLOY_DIR/mysql/data"
    rm -rf "$DEPLOY_DIR/redis/data"
    rm -rf "$DEPLOY_DIR/qdrant/storage"
    rm -rf "$DEPLOY_DIR/tei/cache"
    rm -rf "$DEPLOY_DIR/vllm/models"
    echo -e "${GREEN}✓ 数据目录已删除${NC}"
else
    echo -e "${YELLOW}⊘ 跳过数据目录删除${NC}"
fi
echo ""

# 3. 删除日志目录
echo -e "${YELLOW}[3/5] 删除日志目录...${NC}"
read -p "是否删除日志目录? [y/N]: " -n 1 -r
echo
if [[ $REPLY =~ ^[Yy]$ ]]; then
    rm -rf "$DEPLOY_DIR/api/logs"
    echo -e "${GREEN}✓ 日志目录已删除${NC}"
else
    echo -e "${YELLOW}⊘ 跳过日志目录删除${NC}"
fi
echo ""

# 4. 删除上传文件
echo -e "${YELLOW}[4/5] 删除上传文件...${NC}"
read -p "是否删除上传文件? [y/N]: " -n 1 -r
echo
if [[ $REPLY =~ ^[Yy]$ ]]; then
    rm -rf "$DEPLOY_DIR/api/uploads"
    echo -e "${GREEN}✓ 上传文件已删除${NC}"
else
    echo -e "${YELLOW}⊘ 跳过上传文件删除${NC}"
fi
echo ""

# 5. 清理 Docker volumes（如果存在）
echo -e "${YELLOW}[5/5] 清理未使用的 Docker volumes...${NC}"
read -p "是否清理所有未使用的 Docker volumes? [y/N]: " -n 1 -r
echo
if [[ $REPLY =~ ^[Yy]$ ]]; then
    docker volume prune -f
    echo -e "${GREEN}✓ 未使用的 volumes 已清理${NC}"
else
    echo -e "${YELLOW}⊘ 跳过 volumes 清理${NC}"
fi
echo ""

# 6. 清理 Docker 镜像（可选）
echo -e "${YELLOW}[额外] 清理悬空镜像...${NC}"
read -p "是否删除悬空的 Docker 镜像? [y/N]: " -n 1 -r
echo
if [[ $REPLY =~ ^[Yy]$ ]]; then
    docker image prune -f
    echo -e "${GREEN}✓ 悬空镜像已清理${NC}"
else
    echo -e "${YELLOW}⊘ 跳过镜像清理${NC}"
fi
echo ""

# 完成
echo -e "${GREEN}===========================================${NC}"
echo -e "${GREEN}清理完成！${NC}"
echo -e "${GREEN}===========================================${NC}"
