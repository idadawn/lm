# 部署配置
# 请根据实际环境修改以下配置

# 部署版本号
$Global:DeployVersion = "1.0.2"

# 服务配置
$Global:Services = @(
    @{
        name = "lab-api"
        displayName = "API 服务"
        deployPath = "D:\Lab\API"
        backupPath = "D:\Lab\Backups\api"
        port = 5000
        healthCheck = "http://localhost:5000/"
    },
    @{
        name = "lab-nginx"
        displayName = "Web 前端 (Nginx)"
        deployPath = "D:\Lab\Web"
        backupPath = "D:\Lab\Backups\web"
        port = 80
        healthCheck = "http://localhost/"
    },
    @{
        name = "lab-redis"
        displayName = "Redis 缓存"
        deployPath = "D:\Lab\Redis"
        backupPath = "D:\Lab\Backups\redis"
        port = 6379
        healthCheck = $null
    }
)

# 备份保留数量
$Global:BackupRetention = 10

# 部署源路径 (包含 api, nginx 等子目录)
$Global:DefaultSourcePath = "D:\Lab\Deploy\v1.0.2"

# 通知配置
$Global:NotifyEnabled = $false
$Global:NotifyWebhook = ""  # 钉钉/企业微信 webhook
