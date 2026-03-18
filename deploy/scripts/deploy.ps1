# LM 自动化部署脚本
# 用法: .\deploy.ps1 -Version "1.0.2" -SourcePath "D:\Lab\Deploy"

param(
    [Parameter(Mandatory=$false)]
    [string]$Version = "",
    
    [Parameter(Mandatory=$false)]
    [string]$SourcePath = "",
    
    [Parameter(Mandatory=$false)]
    [switch]$SkipBackup,
    
    [Parameter(Mandatory=$false)]
    [switch]$DryRun
)

# 配置
$ErrorActionPreference = "Stop"
$ScriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
$ConfigFile = Join-Path $ScriptDir "config.ps1"
$LogDir = Join-Path $ScriptDir "logs"
$BackupRoot = Join-Path $ScriptDir "backups"

# 加载配置
. $ConfigFile

# 创建日志目录
if (-not (Test-Path $LogDir)) {
    New-Item -ItemType Directory -Path $LogDir -Force | Out-Null
}

# 日志函数
function Write-Log {
    param([string]$Message, [string]$Level = "INFO")
    $timestamp = Get-Date -Format "yyyy-MM-dd HH:mm:ss"
    $logMessage = "[$timestamp] [$Level] $Message"
    Write-Host $logMessage
    $logFile = Join-Path $LogDir "deploy-$(Get-Date -Format 'yyyyMMdd').log"
    Add-Content -Path $logFile -Value $logMessage
}

# 备份函数
function Backup-Service {
    param([string]$ServiceName, [string]$DeployPath)
    
    if (-not (Test-Path $DeployPath)) {
        Write-Log "部署路径不存在: $DeployPath" "WARN"
        return $null
    }
    
    $timestamp = Get-Date -Format "yyyyMMdd-HHmmss"
    $backupName = "$ServiceName-$timestamp"
    $backupPath = Join-Path $BackupRoot $backupName
    
    Write-Log "正在备份 $ServiceName..."
    
    try {
        # 创建备份目录
        New-Item -ItemType Directory -Path $backupPath -Force | Out-Null
        
        # 复制文件
        Copy-Item -Path "$DeployPath\*" -Destination $backupPath -Recurse -Force
        
        # 保存版本信息
        $versionInfo = @{
            Version = $Version
            BackupTime = $timestamp
            ServiceName = $ServiceName
            DeployPath = $DeployPath
        }
        $versionInfo | ConvertTo-Json | Set-Content (Join-Path $backupPath "version.json")
        
        Write-Log "备份完成: $backupPath" "SUCCESS"
        return $backupPath
    }
    catch {
        Write-Log "备份失败: $_" "ERROR"
        throw
    }
}

# 停止服务
function Stop-Service-WithNssm {
    param([string]$ServiceName)
    
    Write-Log "正在停止服务: $ServiceName"
    
    try {
        $status = nssm status $ServiceName
        if ($status -eq "SERVICE_RUNNING") {
            nssm stop $ServiceName
            Start-Sleep -Seconds 3
            Write-Log "服务已停止: $ServiceName" "SUCCESS"
        }
        else {
            Write-Log "服务未运行: $ServiceName" "WARN"
        }
    }
    catch {
        Write-Log "停止服务失败: $_" "ERROR"
        throw
    }
}

# 启动服务
function Start-Service-WithNssm {
    param([string]$ServiceName)
    
    Write-Log "正在启动服务: $ServiceName"
    
    try {
        nssm start $ServiceName
        Start-Sleep -Seconds 3
        
        $status = nssm status $ServiceName
        if ($status -eq "SERVICE_RUNNING") {
            Write-Log "服务已启动: $ServiceName" "SUCCESS"
        }
        else {
            throw "服务启动失败，状态: $status"
        }
    }
    catch {
        Write-Log "启动服务失败: $_" "ERROR"
        throw
    }
}

# 健康检查
function Test-HealthCheck {
    param([string]$Url, [int]$Timeout = 30, [int]$Retry = 3)
    
    Write-Log "正在进行健康检查: $Url"
    
    for ($i = 1; $i -le $Retry; $i++) {
        try {
            $response = Invoke-WebRequest -Uri $Url -TimeoutSec $Timeout -UseBasicParsing -ErrorAction SilentlyContinue
            if ($response.StatusCode -eq 200) {
                Write-Log "健康检查通过" "SUCCESS"
                return $true
            }
        }
        catch {
            Write-Log "健康检查重试 $i/$Retry..." "WARN"
        }
        Start-Sleep -Seconds 5
    }
    
    Write-Log "健康检查失败" "ERROR"
    return $false
}

# 回滚
function Rollback-Backup {
    param([string]$BackupPath, [string]$DeployPath)
    
    Write-Log "正在回滚到: $BackupPath"
    
    # 停止服务
    foreach ($service in $Services) {
        Stop-Service-WithNssm -ServiceName $service.name
    }
    
    # 恢复文件
    Copy-Item -Path "$BackupPath\*" -Destination $DeployPath -Recurse -Force
    
    # 启动服务
    foreach ($service in $Services) {
        Start-Service-WithNssm -ServiceName $service.name
    }
    
    Write-Log "回滚完成" "SUCCESS"
}

# 主流程
function Main {
    $deployStartTime = Get-Date
    
    Write-Log "========== 开始部署 =========="
    Write-Log "版本: $Version"
    Write-Log "部署源: $SourcePath"
    
    if ($DryRun) {
        Write-Log "[DRY RUN] 模拟部署，不执行实际操作" "WARN"
    }
    
    $failedServices = @()
    $deployedServices = @()
    
    foreach ($service in $Services) {
        Write-Log "========== 部署服务: $($service.displayName) =========="
        
        try {
            # 1. 备份 (除非跳过)
            if (-not $SkipBackup -and -not $DryRun) {
                $backupPath = Backup-Service -ServiceName $service.name -DeployPath $service.deployPath
            }
            
            # 2. 停止服务
            if (-not $DryRun) {
                Stop-Service-WithNssm -ServiceName $service.name
            }
            
            # 3. 复制文件
            if ($SourcePath -and (Test-Path $SourcePath)) {
                $sourceDir = Join-Path $SourcePath $service.name
                if (Test-Path $sourceDir) {
                    if (-not $DryRun) {
                        Write-Log "复制文件 from $sourceDir to $($service.deployPath)"
                        Copy-Item -Path "$sourceDir\*" -Destination $service.deployPath -Recurse -Force
                    }
                }
                else {
                    Write-Log "源目录不存在: $sourceDir" "WARN"
                }
            }
            else {
                Write-Log "未指定源路径，跳过文件复制" "WARN"
            }
            
            # 4. 启动服务
            if (-not $DryRun) {
                Start-Service-WithNssm -ServiceName $service.name
            }
            
            # 5. 健康检查
            if (-not $DryRun -and $service.healthCheck) {
                $healthOk = Test-HealthCheck -Url $service.healthCheck
                if (-not $healthOk) {
                    throw "健康检查失败"
                }
            }
            
            $deployedServices += $service.name
            Write-Log "服务部署成功: $($service.displayName)" "SUCCESS"
        }
        catch {
            Write-Log "服务部署失败: $_" "ERROR"
            $failedServices += $service.name
            
            # TODO: 自动回滚逻辑
            Write-Log "需要手动回滚!" "ERROR"
        }
    }
    
    # 部署结果
    Write-Log "========== 部署完成 =========="
    Write-Log "成功: $($deployedServices -join ', ')"
    if ($failedServices.Count -gt 0) {
        Write-Log "失败: $($failedServices -join ', ')" "ERROR"
        exit 1
    }
    
    exit 0
}

Main
