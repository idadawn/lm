#Requires -Version 5.1
<#
.SYNOPSIS
    在检测设备电脑（Windows）上把「采集网关」(Poxiao.Lab.CollectorAgent) 安装为
    NSSM 托管的常驻 Windows 服务。
.DESCRIPTION
    最小安装脚本，只做一件事：把 CollectorDir 下已发布好的
    Poxiao.Lab.CollectorAgent.exe 注册为 Windows 服务。不做 dotnet publish
    （发布产物需提前拷贝到设备电脑，参见项目 README「发布」章节）。

    幂等：服务已存在时会先 stop + remove 再重装，可放心重复执行来更新配置。
    必须以管理员身份运行 PowerShell（安装/卸载 Windows 服务的硬性要求）。
.PARAMETER CollectorDir
    采集网关发布产物所在目录（须包含 Poxiao.Lab.CollectorAgent.exe）。默认 D:\collector。
.PARAMETER ServiceName
    Windows 服务名。默认 lm-collector。
.PARAMETER Uninstall
    停止并移除服务，不做安装。
.EXAMPLE
    # 安装（以管理员身份运行）
    .\install-collector.ps1
.EXAMPLE
    # 指定发布目录
    .\install-collector.ps1 -CollectorDir "D:\collector"
.EXAMPLE
    # 卸载
    .\install-collector.ps1 -Uninstall
#>
param(
    [string]$CollectorDir = "D:\collector",
    [string]$ServiceName = "lm-collector",
    [switch]$Uninstall
)

$ErrorActionPreference = "Stop"
$ScriptDir = $PSScriptRoot

# ── 0) 管理员权限检查（安装/卸载 Windows 服务的硬性要求）─────────
$isAdmin = ([Security.Principal.WindowsPrincipal][Security.Principal.WindowsIdentity]::GetCurrent()).IsInRole([Security.Principal.WindowsBuiltInRole]::Administrator)
if (-not $isAdmin) {
    Write-Host "需要管理员权限。请以「管理员身份」重新打开 PowerShell 再运行本脚本。" -ForegroundColor Red
    exit 1
}

# PS 5.1 陷阱：对原生命令用 2>&1 会把 stderr 包装成 ErrorRecord，
# 在 ErrorActionPreference=Stop 下即使 | Out-Null 也会变成终止错误
#（例如 nssm stop 一个未启动的服务）。统一经此函数调用原生命令。
function Invoke-Native {
    param([string]$Exe, [string[]]$Arguments)
    $eap = $ErrorActionPreference
    $ErrorActionPreference = "Continue"
    try { (& $Exe @Arguments 2>&1) | ForEach-Object { "$_" } } finally { $ErrorActionPreference = $eap }
}

function Get-Nssm {
    # 1) 脚本同目录
    $local = Join-Path $ScriptDir "nssm.exe"
    if (Test-Path $local) { return $local }

    # 2) PATH
    $cmd = Get-Command "nssm.exe" -ErrorAction SilentlyContinue
    if ($cmd) { return $cmd.Source }

    throw "未找到 nssm.exe。请把 nssm.exe 放到本脚本同目录（$ScriptDir），或安装到 PATH 中后重试。下载地址: https://nssm.cc/download"
}

function Remove-ServiceIfExists {
    param([string]$NssmExe, [string]$Name)
    if (Get-Service -Name $Name -ErrorAction SilentlyContinue) {
        Write-Host "  服务 $Name 已存在，先停止并移除 ..." -ForegroundColor DarkGray
        Invoke-Native $NssmExe @('stop', $Name) | Out-Null
        Start-Sleep -Seconds 1
        Invoke-Native $NssmExe @('remove', $Name, 'confirm') | Out-Null
        Start-Sleep -Milliseconds 500
    }
}

# ── 卸载分支 ───────────────────────────────────────────────────
if ($Uninstall) {
    $nssmExe = Get-Nssm
    Remove-ServiceIfExists -NssmExe $nssmExe -Name $ServiceName
    Write-Host "已卸载服务: $ServiceName" -ForegroundColor Green
    exit 0
}

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "  采集网关 - Windows 服务安装" -ForegroundColor Cyan
Write-Host "  安装目录: $CollectorDir" -ForegroundColor Cyan
Write-Host "  服务名  : $ServiceName" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

$exePath = Join-Path $CollectorDir "Poxiao.Lab.CollectorAgent.exe"
if (-not (Test-Path $exePath)) {
    throw "未找到 $exePath。请先把 `dotnet publish` 的发布产物拷贝到 $CollectorDir（参见项目 README「发布」章节），再运行本脚本。"
}

$nssmExe = Get-Nssm
Write-Host "NSSM: $nssmExe" -ForegroundColor DarkGray

$logDir = Join-Path $CollectorDir "logs"
New-Item -ItemType Directory -Force -Path $logDir | Out-Null

Remove-ServiceIfExists -NssmExe $nssmExe -Name $ServiceName

Write-Host "  → 注册服务 $ServiceName ..." -ForegroundColor DarkGray
& $nssmExe install $ServiceName $exePath | Out-Null
& $nssmExe set $ServiceName AppDirectory $CollectorDir | Out-Null
& $nssmExe set $ServiceName DisplayName $ServiceName | Out-Null
& $nssmExe set $ServiceName Description "检测室数据分析系统 - 采集网关（设备端数据采集与上报）" | Out-Null
& $nssmExe set $ServiceName Start SERVICE_AUTO_START | Out-Null
& $nssmExe set $ServiceName AppStdout (Join-Path $logDir "stdout.log") | Out-Null
& $nssmExe set $ServiceName AppStderr (Join-Path $logDir "stderr.log") | Out-Null
& $nssmExe set $ServiceName AppRotateFiles 1 | Out-Null
& $nssmExe set $ServiceName AppRotateOnline 1 | Out-Null
& $nssmExe set $ServiceName AppRotateBytes 10485760 | Out-Null
& $nssmExe set $ServiceName AppExit Default Restart | Out-Null
& $nssmExe set $ServiceName AppRestartDelay 5000 | Out-Null

Write-Host "  已注册服务 $ServiceName" -ForegroundColor Green
Write-Host ""

Write-Host "  → 启动服务 ..." -ForegroundColor DarkGray
Invoke-Native $nssmExe @('start', $ServiceName) | Out-Null
Start-Sleep -Seconds 1
$svc = Get-Service -Name $ServiceName -ErrorAction SilentlyContinue
$status = if ($svc) { $svc.Status } else { "未知" }
$statusColor = if ($status -eq "Running") { "Green" } else { "Yellow" }
Write-Host "  $ServiceName : $status" -ForegroundColor $statusColor

Write-Host ""
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "完成。日志目录: $logDir\{stdout,stderr}.log" -ForegroundColor Cyan
Write-Host "常用操作: nssm status $ServiceName | nssm restart $ServiceName | Get-Service $ServiceName" -ForegroundColor DarkGray
Write-Host "配置文件: $CollectorDir\Configurations\appsettings.json（改完需 nssm restart $ServiceName 生效）" -ForegroundColor DarkGray
Write-Host "========================================" -ForegroundColor Cyan
