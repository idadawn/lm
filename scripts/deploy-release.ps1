# LM 系统部署入口脚本
# 快捷入口，调用 publish/deploy.ps1
#
# 用法:
#   部署全部:   .\deploy-release.ps1 -Version "1.1.0"
#   仅 API:     .\deploy-release.ps1 -Version "1.1.0" -Component api
#   仅 Web:     .\deploy-release.ps1 -Version "1.1.0" -Component web

[CmdletBinding()]
param(
    [Parameter(Mandatory=$true)]
    [string]$Version,
    
    [Parameter(Mandatory=$false)]
    [ValidateSet("all", "api", "worker", "web")]
    [string]$Component = "all",
    
    [Parameter(Mandatory=$false)]
    [ValidateSet("development", "test", "staging", "production")]
    [string]$Environment = "production",
    
    [Parameter(Mandatory=$false)]
    [switch]$SkipBackup,
    
    [Parameter(Mandatory=$false)]
    [switch]$SkipHealthCheck
)

$ScriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
$DeployScript = Join-Path $ScriptDir "publish\deploy.ps1"

if (-not (Test-Path $DeployScript)) {
    Write-Host "错误: 部署脚本不存在: $DeployScript" -ForegroundColor Red
    exit 1
}

# 执行部署脚本
& $DeployScript -Version $Version -Component $Component -Environment $Environment -SkipBackup:$SkipBackup -SkipHealthCheck:$SkipHealthCheck
