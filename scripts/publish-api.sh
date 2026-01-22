#!/bin/bash
# ============================================
# 实验室数据分析系统 - API 发布脚本
# 适用于 Ubuntu 24.04.3 LTS
# ============================================

set -e  # 遇到错误立即退出

# ============================================
# 配置区域 - 请根据实际情况修改
# ============================================

# 项目路径
PROJECT_DIR="/home/dawn/project/lm"
API_ENTRY_DIR="${PROJECT_DIR}/api/src/application/Poxiao.API.Entry"

# 发布配置
RUNTIME="linux-x64"
CONFIGURATION="Release"
PUBLISH_OUTPUT="${PROJECT_DIR}/publish/api"
DEPLOY_DIR="/var/www/lm-api"
BACKUP_DIR="/var/backups/lm-api"
SERVICE_NAME="lm-api"
ENVIRONMENT="production"  # dev, production, staging

# 日志
LOG_FILE="${PROJECT_DIR}/logs/publish-$(date +%Y%m%d-%H%M%S).log"
LOG_DIR="${PROJECT_DIR}/logs"

# 保留备份数量
MAX_BACKUPS=5

# ============================================
# 颜色输出
# ============================================
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
NC='\033[0m' # No Color

# ============================================
# 日志函数
# ============================================
log_info() {
    echo -e "${GREEN}[INFO]${NC} $1" | tee -a "$LOG_FILE"
}

log_warn() {
    echo -e "${YELLOW}[WARN]${NC} $1" | tee -a "$LOG_FILE"
}

log_error() {
    echo -e "${RED}[ERROR]${NC} $1" | tee -a "$LOG_FILE"
}

log_step() {
    echo -e "${BLUE}[STEP]${NC} $1" | tee -a "$LOG_FILE"
}

# ============================================
# 检查依赖
# ============================================
check_dependencies() {
    log_step "检查依赖项..."

    if ! command -v dotnet &> /dev/null; then
        log_error ".NET SDK 未安装，请先安装 .NET 10.0 SDK"
        exit 1
    fi

    local dotnet_version=$(dotnet --version)
    log_info ".NET 版本: $dotnet_version"

    if ! command -v systemctl &> /dev/null; then
        log_error "systemctl 未找到，此脚本需要 systemd 支持"
        exit 1
    fi
}

# ============================================
# 清理旧发布文件
# ============================================
clean_old_publish() {
    log_step "清理旧发布文件..."
    if [ -d "$PUBLISH_OUTPUT" ]; then
        rm -rf "$PUBLISH_OUTPUT"
        log_info "已清理: $PUBLISH_OUTPUT"
    fi
}

# ============================================
# 编译发布
# ============================================
publish_api() {
    log_step "开始发布 API..."
    log_info "运行时: $RUNTIME"
    log_info "配置: $CONFIGURATION"
    log_info "环境: $ENVIRONMENT"

    cd "$API_ENTRY_DIR"

    dotnet publish "Poxiao.API.Entry.csproj" \
        -c "$CONFIGURATION" \
        -r "$RUNTIME" \
        --self-contained false \
        -o "$PUBLISH_OUTPUT" \
        -p:Environment="$ENVIRONMENT" \
        /p:PublishTrimmed=false \
        2>&1 | tee -a "$LOG_FILE"

    if [ ${PIPESTATUS[0]} -eq 0 ]; then
        log_info "发布成功!"
    else
        log_error "发布失败!"
        exit 1
    fi
}

# ============================================
# 停止服务
# ============================================
stop_service() {
    log_step "停止服务 $SERVICE_NAME..."

    if systemctl is-active --quiet "$SERVICE_NAME"; then
        sudo systemctl stop "$SERVICE_NAME"
        log_info "服务已停止"
    else
        log_info "服务未运行"
    fi
}

# ============================================
# 备份当前版本
# ============================================
backup_current_version() {
    log_step "备份当前版本..."

    if [ ! -d "$BACKUP_DIR" ]; then
        sudo mkdir -p "$BACKUP_DIR"
        sudo chown $USER:$USER "$BACKUP_DIR"
    fi

    if [ -d "$DEPLOY_DIR" ]; then
        local backup_name="lm-api-$(date +%Y%m%d-%H%M%S)"
        sudo cp -r "$DEPLOY_DIR" "$BACKUP_DIR/$backup_name"
        log_info "备份至: $BACKUP_DIR/$backup_name"

        # 清理旧备份
        cleanup_old_backups
    else
        log_info "无需备份（首次部署）"
        sudo mkdir -p "$DEPLOY_DIR"
    fi
}

# ============================================
# 清理旧备份
# ============================================
cleanup_old_backups() {
    log_step "清理旧备份（保留最近 $MAX_BACKUPS 个）..."

    local backup_count=$(ls -1t "$BACKUP_DIR" 2>/dev/null | grep "^lm-api-" | wc -l)

    if [ "$backup_count" -gt "$MAX_BACKUPS" ]; then
        ls -1t "$BACKUP_DIR" | grep "^lm-api-" | tail -n +$((MAX_BACKUPS + 1)) | while read old_backup; do
            sudo rm -rf "$BACKUP_DIR/$old_backup"
            log_info "已删除旧备份: $old_backup"
        done
    fi
}

# ============================================
# 部署新版本
# ============================================
deploy_new_version() {
    log_step "部署新版本..."

    sudo cp -r "$PUBLISH_OUTPUT"/* "$DEPLOY_DIR"/
    sudo chown -R www-data:www-data "$DEPLOY_DIR"
    sudo chmod -R 755 "$DEPLOY_DIR"

    log_info "部署完成: $DEPLOY_DIR"
}

# ============================================
# 启动服务
# ============================================
start_service() {
    log_step "启动服务 $SERVICE_NAME..."

    sudo systemctl daemon-reload
    sudo systemctl start "$SERVICE_NAME"
    sudo systemctl enable "$SERVICE_NAME"

    # 等待服务启动
    sleep 3

    if systemctl is-active --quiet "$SERVICE_NAME"; then
        log_info "服务启动成功!"
    else
        log_error "服务启动失败!"
        log_info "查看日志: sudo journalctl -u $SERVICE_NAME -n 50"
        exit 1
    fi
}

# ============================================
# 检查服务状态
# ============================================
check_service_status() {
    log_step "检查服务状态..."

    echo ""
    systemctl status "$SERVICE_NAME" --no-pager
    echo ""

    log_info "最近日志:"
    sudo journalctl -u "$SERVICE_NAME" -n 20 --no-pager
}

# ============================================
# 回滚功能
# ============================================
rollback() {
    log_warn "开始回滚..."

    if [ ! -d "$BACKUP_DIR" ]; then
        log_error "备份目录不存在: $BACKUP_DIR"
        exit 1
    fi

    local latest_backup=$(ls -1t "$BACKUP_DIR" | grep "^lm-api-" | head -1)

    if [ -z "$latest_backup" ]; then
        log_error "未找到备份"
        exit 1
    fi

    log_info "回滚到备份: $latest_backup"

    stop_service

    sudo rm -rf "$DEPLOY_DIR"/*
    sudo cp -r "$BACKUP_DIR/$latest_backup"/* "$DEPLOY_DIR"/
    sudo chown -R www-data:www-data "$DEPLOY_DIR"

    start_service
    check_service_status
}

# ============================================
# 显示帮助信息
# ============================================
show_help() {
    cat << EOF
实验室数据分析系统 - API 发布脚本

用法: $0 [选项]

选项:
    deploy      完整部署（默认）：编译 -> 停止服务 -> 备份 -> 部署 -> 启动
    quick       快速部署：不备份，直接部署
    rollback    回滚到上一个版本
    status      查看服务状态
    restart     重启服务

示例:
    $0 deploy       # 完整部署
    $0 quick        # 快速部署
    $0 rollback     # 回滚

配置:
    编辑脚本顶部的配置区域以修改项目路径、环境等设置

EOF
}

# ============================================
# 创建日志目录
# ============================================
mkdir -p "$LOG_DIR"

# ============================================
# 主流程
# ============================================
main() {
    local action=${1:-deploy}

    log_info "=========================================="
    log_info "API 发布脚本开始执行"
    log_info "操作: $action"
    log_info "=========================================="

    case "$action" in
        deploy)
            check_dependencies
            clean_old_publish
            publish_api
            stop_service
            backup_current_version
            deploy_new_version
            start_service
            check_service_status
            ;;
        quick)
            check_dependencies
            clean_old_publish
            publish_api
            stop_service
            deploy_new_version
            start_service
            check_service_status
            ;;
        rollback)
            rollback
            ;;
        status)
            check_service_status
            ;;
        restart)
            log_step "重启服务..."
            sudo systemctl restart "$SERVICE_NAME"
            check_service_status
            ;;
        help|--help|-h)
            show_help
            ;;
        *)
            log_error "未知操作: $action"
            show_help
            exit 1
            ;;
    esac

    log_info "=========================================="
    log_info "API 发布脚本执行完成"
    log_info "=========================================="
}

# 执行主流程
main "$@"
