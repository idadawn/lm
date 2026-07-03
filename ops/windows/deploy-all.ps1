#Requires -Version 5.1
<#
.SYNOPSIS
    一键部署：基础环境（install-infra.ps1）+ 应用服务（install-services.ps1）。
.DESCRIPTION
    需管理员身份。全部输出通过 Start-Transcript 落盘到 <DeployRoot>\logs\deploy-all.log，
    便于从非提权终端发起（Start-Process -Verb RunAs）后回读执行结果。
    基础环境失败时中止（api 依赖 Redis/RabbitMQ，没有它们只会 crash-loop）。
.PARAMETER Build
    传入时应用部署执行完整构建；默认 -SkipBuild（假定产物已就位，适合 CI/预构建流程）。
#>
param(
    [switch]$Build
)

$ErrorActionPreference = "Stop"
$here = $PSScriptRoot

# 读取 DEPLOY_ROOT 决定日志位置
. (Join-Path $here "_dotenv.ps1")
$envMap = Get-DotEnv -Path (Join-Path $here ".env")
$deployRoot = Coalesce-EnvValue -ParamValue "" -EnvMap $envMap -Key "DEPLOY_ROOT" -Default (Join-Path $env:USERPROFILE "lm-deploy")
$logDir = Join-Path $deployRoot "logs"
New-Item -ItemType Directory -Force -Path $logDir | Out-Null
$logFile = Join-Path $logDir "deploy-all.log"

Start-Transcript -Path $logFile -Force | Out-Null
$exitCode = 0
try {
    Write-Host "### [1/2] 基础环境 install-infra.ps1 ###" -ForegroundColor Cyan
    & (Join-Path $here "install-infra.ps1")

    Write-Host ""
    Write-Host "### [2/2] 应用服务 install-services.ps1 ###" -ForegroundColor Cyan
    if ($Build) {
        & (Join-Path $here "install-services.ps1")
    } else {
        & (Join-Path $here "install-services.ps1") -SkipBuild
    }
    Write-Host ""
    Write-Host "### deploy-all 完成 ###" -ForegroundColor Green
}
catch {
    $exitCode = 1
    Write-Host ""
    Write-Host "### deploy-all 失败: $_ ###" -ForegroundColor Red
    Write-Host $_.ScriptStackTrace -ForegroundColor DarkGray
}
finally {
    Stop-Transcript | Out-Null
}
exit $exitCode
