# 回滚脚本
# 用法: .\rollback.ps1 -ServiceName "lab-api" -BackupName "lab-api-20260318-143000"

param(
    [Parameter(Mandatory=$false)]
    [string]$ServiceName = "",
    
    [Parameter(Mandatory=$false)]
    [string]$BackupName = "",
    
    [Parameter(Mandatory=$false)]
    [switch]$List
)

$ErrorActionPreference = "Stop"
$ScriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
$BackupRoot = Join-Path $ScriptDir "backups"
$ConfigFile = Join-Path $ScriptDir "config.ps1"

# 加载配置
. $ConfigFile

function Write-Log {
    param([string]$Message, [string]$Level = "INFO")
    $timestamp = Get-Date -Format "yyyy-MM-dd HH:mm:ss"
    $logMessage = "[$timestamp] [$Level] $Message"
    Write-Host $logMessage
}

# 列出所有备份
function List-Backups {
    Write-Host "`n===== 可用备份 =====" -ForegroundColor Cyan
    
    if (-not (Test-Path $BackupRoot)) {
        Write-Host "暂无备份" -ForegroundColor Yellow
        return
    }
    
    $backups = Get-ChildItem -Path $BackupRoot -Directory | Sort-Object LastWriteTime -Descending
    
    foreach ($backup in $backups) {
        $versionFile = Join-Path $backup.FullName "version.json"
        $version = "未知"
        if (Test-Path $versionFile) {
            $v = Get-Content $versionFile | ConvertFrom-Json
            $version = $v.Version
        }
        
        Write-Host "  $($backup.Name) - $version - $($backup.LastWriteTime)" -ForegroundColor White
    }
    Write-Host ""
}

# 获取服务配置
function Get-ServiceConfig {
    param([string]$Name)
    
    foreach ($service in $Services) {
        if ($service.name -eq $Name) {
            return $service
        }
    }
    throw "未找到服务: $Name"
}

# 回滚
function Rollback {
    param([string]$Name, [string]$Backup)
    
    $service = Get-ServiceConfig -Name $Name
    $backupPath = Join-Path $BackupRoot $Backup
    
    if (-not (Test-Path $backupPath)) {
        throw "备份不存在: $Backup"
    }
    
    Write-Log "========== 开始回滚 =========="
    Write-Log "服务: $Name"
    Write-Log "备份: $Backup"
    Write-Log "目标: $($service.deployPath)"
    
    # 停止服务
    Write-Log "停止服务..."
    nssm stop $Name 2>$null
    Start-Sleep -Seconds 2
    
    # 恢复文件
    Write-Log "恢复文件..."
    Copy-Item -Path "$backupPath\*" -Destination $service.deployPath -Recurse -Force
    
    # 启动服务
    Write-Log "启动服务..."
    nssm start $Name
    Start-Sleep -Seconds 3
    
    # 健康检查
    if ($service.healthCheck) {
        Write-Log "健康检查..."
        try {
            $response = Invoke-WebRequest -Uri $service.healthCheck -TimeoutSec 10 -UseBasicParsing
            if ($response.StatusCode -eq 200) {
                Write-Log "回滚成功!" "SUCCESS"
            }
        }
        catch {
            Write-Log "健康检查失败，请手动检查" "WARN"
        }
    }
}

# 主逻辑
if ($List) {
    List-Backups
}
elseif ($ServiceName -and $BackupName) {
    Rollback -Name $ServiceName -Backup $BackupName
}
else {
    Write-Host "用法:" -ForegroundColor Yellow
    Write-Host "  列出备份: .\rollback.ps1 -List"
    Write-Host "  回滚:     .\rollback.ps1 -ServiceName 'lab-api' -BackupName 'lab-api-20260318-143000'"
}
