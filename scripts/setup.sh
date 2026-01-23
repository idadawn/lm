#!/bin/bash
# ============================================
# Docker 服务完整重建脚本（无 Volumes 版本）
# 用途：清理旧数据、重建目录结构、启动服务
# 使用：./scripts/setup.sh
# ============================================

set -e

# 颜色定义
RED='\033[0;31m'
YELLOW='\033[1;33m'
GREEN='\033[0;32m'
BLUE='\033[0;34m'
NC='\033[0m' # No Color

echo -e "${BLUE}============================================${NC}"
echo -e "${BLUE}Docker 服务完整重建（无 Volumes 版本）${NC}"
echo -e "${BLUE}============================================${NC}"
echo ""

# 1. 停止并移除所有容器
echo -e "${YELLOW}[1/9] 停止并移除容器...${NC}"
docker-compose -f docker-compose.infra.yml down --remove-orphans 2>/dev/null || true
echo -e "${GREEN}✓ 容器已清理${NC}"
echo ""

# 2. 完全删除主机数据目录（这是关键步骤）
echo -e "${YELLOW}[2/9] 删除旧数据目录...${NC}"
read -p "是否删除 ./deploy 目录？这将清除所有数据！[y/N]: " -n 1 -r
echo
if [[ $REPLY =~ ^[Yy]$ ]]; then
    rm -rf ./deploy
    mkdir -p ./deploy
    echo -e "${GREEN}✓ 旧数据已删除${NC}"
else
    echo -e "${YELLOW}⊘ 保留现有数据${NC}"
fi
echo ""

# 3. 重新创建目录结构
echo -e "${YELLOW}[3/9] 创建目录结构...${NC}"
mkdir -p ./deploy/mysql/{data,config}
mkdir -p ./deploy/redis/data
mkdir -p ./deploy/qdrant/storage
mkdir -p ./deploy/tei/cache
mkdir -p ./deploy/vllm/models
echo -e "${GREEN}✓ 目录结构已创建${NC}"
echo ""

# 4. 设置目录权限
echo -e "${YELLOW}[4/9] 设置目录权限...${NC}"
chmod -R 777 ./deploy
echo -e "${GREEN}✓ 权限已设置（777）${NC}"
echo ""

# 5. 创建 MySQL 配置文件
echo -e "${YELLOW}[5/9] 创建 MySQL 配置文件...${NC}"
cat > ./deploy/mysql/config/custom.cnf << 'EOF'
[mysqld]
# 基础字符集配置
character-set-server=utf8mb4
collation-server=utf8mb4_unicode_ci
default_authentication_plugin=mysql_native_password

# 网络配置 - 允许远程连接
skip-host-cache
skip-name-resolve
bind-address=0.0.0.0

# 安全配置
local_infile=0
symbolic-links=0

# 时区设置
default-time-zone='+08:00'

# 性能配置
innodb_buffer_pool_size=256M
max_connections=200
innodb_flush_method=O_DIRECT

# 严格模式
sql_mode=STRICT_TRANS_TABLES,NO_ZERO_IN_DATE,NO_ZERO_DATE,ERROR_FOR_DIVISION_BY_ZERO,NO_ENGINE_SUBSTITUTION

[client]
default-character-set=utf8mb4

[mysql]
default-character-set=utf8mb4
EOF
chmod 644 ./deploy/mysql/config/custom.cnf
echo -e "${GREEN}✓ MySQL 配置已创建${NC}"
echo ""

# 6. 创建 .env 文件（如果不存在）
echo -e "${YELLOW}[6/9] 检查 .env 文件...${NC}"
if [ ! -f .env ]; then
    echo -e "${YELLOW}  创建 .env 文件...${NC}"
    cat > .env << 'EOF'
# 基础配置
CONTAINER_PREFIX=lm
NETWORK_NAME=lm-network
DEPLOY_DIR=./deploy

# MySQL 配置
MYSQL_VERSION=8.0
MYSQL_ROOT_PASSWORD=root123
MYSQL_DATABASE=lumeidb
MYSQL_USER=lumei
MYSQL_PASSWORD=lumei123
MYSQL_PORT=3307
MYSQL_TIMEZONE=Asia/Shanghai

# Redis 配置
REDIS_VERSION=8.0-alpine
REDIS_PORT=6380
REDIS_PASSWORD=redis123456

# Qdrant 配置
QDRANT_VERSION=v1.12.1
QDRANT_HTTP_PORT=6333
QDRANT_GRPC_PORT=6334

# TEI 配置
TEI_VERSION=1.6.0
TEI_PORT=8081

# vLLM 配置
VLLM_VERSION=latest
VLLM_PORT=8082
VLLM_MODEL_PATH=Qwen/Qwen2.5-7B-Instruct
VLLM_GPU_MEMORY_UTILIZATION=0.7
VLLM_MAX_MODEL_LEN=4096
EOF
    echo -e "${GREEN}✓ .env 文件已创建${NC}"
else
    echo -e "${GREEN}✓ .env 文件已存在，跳过${NC}"
fi
echo ""

# 7. 启动基础服务
echo -e "${YELLOW}[7/9] 启动基础服务...${NC}"
docker-compose -f docker-compose.infra.yml up -d
echo -e "${GREEN}✓ 服务已启动${NC}"
echo ""

# 8. 等待服务初始化
echo -e "${YELLOW}[8/9] 等待服务初始化...${NC}"
for i in {45..1}; do
    echo -ne "\r  倒计时: ${i} 秒..."
    sleep 1
done
echo -e "\r${GREEN}✓ 初始化等待完成${NC}          "
echo ""

# 9. 验证服务状态
echo -e "${YELLOW}[9/9] 验证服务状态...${NC}"
echo ""
docker ps -a --filter "name=lm-" --format "table {{.Names}}\t{{.Status}}\t{{.Ports}}"
echo ""

# 详细检查 MySQL
echo -e "${BLUE}━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━${NC}"
echo -e "${BLUE}MySQL 日志检查：${NC}"
docker logs lm-mysql 2>&1 | tail -20
echo ""

# 测试 MySQL 连接
echo -e "${BLUE}测试 MySQL 连接：${NC}"
if docker exec lm-mysql mysql -uroot -proot123 -e "SELECT 'Connection successful!' as result;" 2>/dev/null; then
    echo -e "${GREEN}✓ MySQL 连接成功${NC}"
else
    echo -e "${RED}✗ MySQL 连接失败${NC}"
fi
echo ""

# 测试 Redis 连接
echo -e "${BLUE}测试 Redis 连接：${NC}"
if docker exec lm-redis redis-cli -a redis123456 ping 2>/dev/null | grep -q PONG; then
    echo -e "${GREEN}✓ Redis 连接成功${NC}"
else
    echo -e "${RED}✗ Redis 连接失败${NC}"
fi
echo ""

# 检查数据目录权限
echo -e "${BLUE}━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━${NC}"
echo -e "${BLUE}数据目录权限检查：${NC}"
ls -la ./deploy/mysql/data 2>/dev/null | head -3 || echo "MySQL 数据目录尚未初始化"
ls -la ./deploy/redis/data 2>/dev/null | head -3 || echo "Redis 数据目录尚未初始化"
echo ""

# 完成
echo -e "${GREEN}============================================${NC}"
echo -e "${GREEN}✓ 重建完成！${NC}"
echo -e "${GREEN}============================================${NC}"
echo ""
echo -e "${BLUE}查看日志：${NC}"
echo -e "  MySQL:    docker logs lm-mysql -f"
echo -e "  Redis:    docker logs lm-redis -f"
echo -e "  Qdrant:   docker logs lm-qdrant -f"
echo ""
echo -e "${BLUE}快速测试：${NC}"
echo -e "  mysql -h 127.0.0.1 -P 3307 -uroot -proot123"
echo -e "  redis-cli -h 127.0.0.1 -p 6380 -a redis123456"
echo ""
