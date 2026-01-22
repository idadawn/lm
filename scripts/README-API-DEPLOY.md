# API 部署指南 - Ubuntu 24.04.3 LTS

## 目录结构

```
lm/
├── scripts/
│   ├── publish-api.sh          # 发布脚本
│   ├── install-api.sh          # 首次安装脚本
│   └── lm-api.service          # systemd 服务文件
├── api/                        # 后端源码
└── publish/api/                # 临时发布目录（自动生成）
```

## 首次部署

### 1. 安装服务

```bash
cd /home/dawn/project/lm
sudo ./scripts/install-api.sh
```

此脚本会自动完成：
- 安装 .NET SDK 10.0
- 创建目录结构
- 安装 systemd 服务
- 配置防火墙

### 2. 修改配置

根据生产环境修改配置文件：

```bash
# 编辑生产环境配置
sudo vi /var/www/lm-api/Configurations/ConnectionStrings.production.json

# 如果需要创建生产配置，可以从 dev 配置复制：
cp api/src/application/Poxiao.API.Entry/Configurations/ConnectionStrings.dev.json \
   api/src/application/Poxiao.API.Entry/Configurations/ConnectionStrings.production.json
```

主要配置项：
- 数据库连接（ConnectionStrings.production.json）
- Redis 配置（Cache.production.json）
- API 配置（App.production.json）
- AI 服务配置（AI.json）

### 3. 首次发布

```bash
./scripts/publish-api.sh deploy
```

## 日常更新

### 完整部署（推荐）

包含备份的完整部署流程：

```bash
./scripts/publish-api.sh deploy
```

流程：
1. 编译发布 API
2. 停止服务
3. 备份当前版本
4. 部署新版本
5. 启动服务
6. 显示服务状态

### 快速部署（无备份）

不进行备份，直接部署：

```bash
./scripts/publish-api.sh quick
```

### 回滚到上一个版本

```bash
./scripts/publish-api.sh rollback
```

### 查看服务状态

```bash
./scripts/publish-api.sh status
```

或直接使用：

```bash
systemctl status lm-api
```

### 重启服务

```bash
./scripts/publish-api.sh restart
```

或：

```bash
sudo systemctl restart lm-api
```

## 配置说明

### 编辑发布脚本配置

编辑 `scripts/publish-api.sh` 顶部的配置区域：

```bash
# 项目路径
PROJECT_DIR="/home/dawn/project/lm"

# 环境类型
ENVIRONMENT="production"  # dev, production, staging

# 服务名称
SERVICE_NAME="lm-api"

# 其他配置...
```

### systemd 服务配置

服务文件位于：`/etc/systemd/system/lm-api.service`

常用命令：

```bash
# 重新加载服务配置
sudo systemctl daemon-reload

# 启用开机自启
sudo systemctl enable lm-api

# 禁用开机自启
sudo systemctl disable lm-api

# 启动服务
sudo systemctl start lm-api

# 停止服务
sudo systemctl stop lm-api

# 重启服务
sudo systemctl restart lm-api

# 查看服务状态
sudo systemctl status lm-api
```

## 日志查看

### 查看服务日志

```bash
# 实时查看
sudo journalctl -u lm-api -f

# 查看最近 50 行
sudo journalctl -u lm-api -n 50

# 查看最近 1 小时
sudo journalctl -u lm-api --since "1 hour ago"

# 查看今天
sudo journalctl -u lm-api --since today
```

### 查看应用日志

应用日志存储在：

```bash
# 日志目录
/var/www/lm-api/logs/
```

## 监控端口

API 默认监听端口：**9530**

```bash
# 检查端口
sudo netstat -tlnp | grep 9530

# 或使用 ss
sudo ss -tlnp | grep 9530
```

## 备份管理

### 备份位置

```
/var/backups/lm-api/
├── lm-api-20250122-103000/
├── lm-api-20250122-140000/
└── ...
```

### 调整保留备份数量

编辑 `scripts/publish-api.sh`：

```bash
MAX_BACKUPS=5  # 保留最近 5 个备份
```

## 故障排查

### 服务启动失败

1. 查看详细日志：
```bash
sudo journalctl -u lm-api -n 100
```

2. 检查配置文件：
```bash
cd /var/www/lm-api
dotnet Poxiao.API.Entry.dll --configuration-check
```

### 端口冲突

检查端口占用：
```bash
sudo lsof -i :9530
```

### 权限问题

确保目录权限正确：
```bash
sudo chown -R www-data:www-data /var/www/lm-api
sudo chmod -R 755 /var/www/lm-api
```

## API 测试

服务启动后测试：

```bash
# 健康检查
curl http://localhost:9530/health

# API 测试
curl http://localhost:9530/api/system/version
```

## 环境变量

服务环境变量定义在 systemd 服务文件中：

```ini
Environment=ASPNETCORE_ENVIRONMENT=production
Environment=ASPNETCORE_URLS=http://+:9530
```

修改后需重新加载：

```bash
sudo systemctl daemon-reload
sudo systemctl restart lm-api
```

## 卸载

如需卸载服务：

```bash
# 停止并禁用服务
sudo systemctl stop lm-api
sudo systemctl disable lm-api

# 删除服务文件
sudo rm /etc/systemd/system/lm-api.service
sudo systemctl daemon-reload

# 删除部署文件（可选）
sudo rm -rf /var/www/lm-api
sudo rm -rf /var/backups/lm-api
```

## 高级配置

### 反向代理配置（Nginx）

```nginx
upstream lm_api {
    server 127.0.0.1:9530;
}

server {
    listen 80;
    server_name api.example.com;

    location / {
        proxy_pass http://lm_api;
        proxy_http_version 1.1;
        proxy_set_header Upgrade $http_upgrade;
        proxy_set_header Connection 'upgrade';
        proxy_set_header Host $host;
        proxy_cache_bypass $http_upgrade;
        proxy_set_header X-Real-IP $remote_addr;
        proxy_set_header X-Forwarded-For $proxy_add_x_forwarded_for;
        proxy_set_header X-Forwarded-Proto $scheme;
    }
}
```

### SSL 配置

使用 Let's Encrypt：

```bash
sudo apt install certbot python3-certbot-nginx
sudo certbot --nginx -d api.example.com
```
