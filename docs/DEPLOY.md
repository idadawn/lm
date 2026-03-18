# LM 自动化部署方案

## 部署脚本目录结构

```
deploy/
├── deploy.ps1              # 主部署脚本
├── rollback.ps1            # 回滚脚本
├── backup.ps1              # 备份脚本
├── health-check.ps1        # 健康检查
├── config.json             # 部署配置
├── logs/                   # 部署日志
└── backups/               # 版本备份
    ├── 2026-03-18-001/
    ├── 2026-03-17-002/
    └── ...
```

## 服务配置 (config.json)

```json
{
  "version": "1.0.2",
  "backupRetention": 10,
  "services": [
    {
      "name": "lab-api",
      "displayName": "API 服务",
      "deployPath": "D:\\Lab\\API",
      "backupPath": "D:\\Lab\\Backups\\api",
      "port": 5000,
      "healthCheck": "http://localhost:5000/",
      "stopCommand": "nssm stop lab-api",
      "startCommand": "nssm start lab-api"
    },
    {
      "name": "lab-nginx",
      "displayName": "Nginx 服务",
      "deployPath": "D:\\Lab\\Web",
      "backupPath": "D:\\Lab\\Backups\\web",
      "port": 80,
      "healthCheck": "http://localhost/",
      "stopCommand": "nssm stop lab-nginx",
      "startCommand": "nssm start lab-nginx"
    }
  ]
}
```

## 部署流程

### 1. 部署前检查
- 检查服务状态
- 检查磁盘空间
- 验证部署文件

### 2. 备份当前版本
- 停止服务
- 复制当前文件到备份目录
- 记录版本信息

### 3. 部署新版本
- 复制新文件到部署目录
- 启动服务
- 健康检查

### 4. 记录部署日志
- 记录部署时间、版本、操作人
- 记录部署结果
