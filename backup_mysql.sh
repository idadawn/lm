#!/bin/bash
# ============================================
# MySQL数据备份脚本
# 通过Docker方式备份，无需直接拷贝
# ============================================

set -e

# 配置
BACKUP_DIR="$HOME/opt/mysql"
MYSQL_CONTAINER="lm-mysql"
DATE=$(date +%Y%m%d_%H%M%S)
BACKUP_FILE="mysql_backup_${DATE}.sql"

# 颜色
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

# 检查备份目录
if [ ! -d "$BACKUP_DIR" ]; then
    mkdir -p "$BACKUP_DIR"
fi

# 检查容器是否存在和运行
if docker ps | grep -q "$MYSQL_CONTAINER"; then
    log_info "容器 $MYSQL_CONTAINER 正在运行，开始备份..."
elif docker ps -a | grep -q "$MYSQL_CONTAINER"; then
    log_warn "容器 $MYSQL_CONTAINER 存在但已停止，尝试启动..."
    docker start "$MYSQL_CONTAINER"
else
    log_error "容器 $MYSQL_CONTAINER 不存在"
    exit 1
fi

# 等待容器完全启动
sleep 5

# 获取数据库列表
DATABASES=$(docker exec "$MYSQL_CONTAINER" mysql -u root -p'Lm@Mysql#2025App' -h localhost -e "SHOW DATABASES;" -B -N 2>/dev/null | grep -v "information_schema\|performance_schema\|mysql\|sys" || echo "lumei")

if [ -z "$DATABASES" ]; then
    log_warn "未找到需要备份的数据库，备份 lumei 数据库"
    DATABASES="lumei"
fi

log_info "将要备份的数据库: $DATABASES"

# 备份每个数据库
for DB in $DATABASES; do
    log_info "备份数据库 $DB..."
    docker exec "$MYSQL_CONTAINER" mysqldump -u root -p'Lm@Mysql#2025App' --single-transaction --routines --triggers "$DB" > "${BACKUP_DIR}/${DB}_${BACKUP_FILE}" 2>/dev/null

    if [ $? -eq 0 ]; then
        log_info "数据库 $DB 备份成功: ${BACKUP_DIR}/${DB}_${BACKUP_FILE}"
        echo "$(du -sh "${BACKUP_DIR}/${DB}_${BACKUP_FILE}")" >> "${BACKUP_DIR}/${BACKUP_FILE}.size"
    else
        log_error "数据库 $DB 备份失败"
    fi
done

# 创建数据库结构备份
docker exec "$MYSQL_CONTAINER" mysql -u root -p'Lm@Mysql#2025App' -e "SHOW GRANTS FOR 'poxiao'@'%'; SHOW GRANTS FOR 'lumei'@'%';" 2>/dev/null > "${BACKUP_DIR}/grants_${BACKUP_FILE}"

# 压缩备份文件
cd "$BACKUP_DIR"
for file in *_${BACKUP_FILE}; do
    if [ -f "$file" ]; then
        gzip "$file"
        log_info "文件 $file 已压缩"
    fi
done

gzip "grants_${BACKUP_FILE}"

# 显示备份统计
log_info "备份完成！统计信息："
ls -lh "$BACKUP_DIR"/*_${date}.sql.gz 2>/dev/null || ls -lh "$BACKUP_DIR"/*_${date}.gz 2>/dev/null || ls -lh "$BACKUP_DIR"

echo ""
log_info "备份文件位置: $BACKUP_DIR"
log_info "要恢复数据，可使用以下命令："
log_info "docker exec -i $MYSQL_CONTAINER mysql -u root -p'Lm@Mysql#2025App' < backup_file.sql"