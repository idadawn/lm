#Requires -Version 5.1
<#
.SYNOPSIS
    上传 Android APK 到蒲公英 (Pgyer)
.DESCRIPTION
    通过蒲公英 API v2 上传 APK，所有参数可从 scripts/.env 读取默认值。

    优先级：命令行参数 > scripts/.env > 自动探测
    - ApkPath 缺省：自动取 unpackage/release/apk/ 中最新 APK
    - ApiKey  缺省：从 .env 中 PGYER_API_KEY 读

    需要在 https://www.pgyer.com/account/api 获取 API Key。
.PARAMETER ApkPath
    APK 文件路径。空时自动找 unpackage/release/apk/ 下最新的 .apk
.PARAMETER ApiKey
    蒲公英 API Key。空时读 .env 中 PGYER_API_KEY
.PARAMETER Description
    更新说明。空时读 .env 中 PGYER_DESCRIPTION
.PARAMETER InstallType
    安装类型：1=公开, 2=密码, 3=邀请。空时读 .env 中 PGYER_INSTALL_TYPE，默认 1
.PARAMETER Password
    安装密码（仅当 InstallType=2 必填）。空时读 .env 中 PGYER_PASSWORD
.EXAMPLE
    # 全部从 .env 读 + 自动找最新 APK
    .\upload-to-pgyer.ps1
    # 临时覆盖更新说明
    .\upload-to-pgyer.ps1 -Description "v1.2 修复班次统计 bug"
#>
param(
    [string]$ApkPath      = "",
    [string]$ApiKey       = "",
    [string]$Description  = "",
    [int]$InstallType     = 0,
    [string]$Password     = "",
    [string]$BuildVersion = "",
    [string]$BuildVersionNo = ""
)

$ErrorActionPreference = "Stop"

# ── 加载 .env（命令行参数优先） ─────────────────────
. (Join-Path $PSScriptRoot "_dotenv.ps1")
$envFile = Join-Path $PSScriptRoot ".env"
$envMap  = Get-DotEnv -Path $envFile

$ApiKey      = Coalesce-EnvValue -ParamValue $ApiKey      -EnvMap $envMap -Key "PGYER_API_KEY"
$Description = Coalesce-EnvValue -ParamValue $Description -EnvMap $envMap -Key "PGYER_DESCRIPTION"
$Password    = Coalesce-EnvValue -ParamValue $Password    -EnvMap $envMap -Key "PGYER_PASSWORD"

if ($InstallType -le 0) {
    $itEnv = Coalesce-EnvValue -ParamValue "" -EnvMap $envMap -Key "PGYER_INSTALL_TYPE" -Default "1"
    $InstallType = [int]$itEnv
}
if (@(1, 2, 3) -notcontains $InstallType) {
    Write-Host "❌ PGYER_INSTALL_TYPE 必须为 1/2/3，当前: $InstallType" -ForegroundColor Red
    exit 1
}

# ── 必填校验：API Key ────────────────────────────────
if (-not $ApiKey) {
    Write-Host "❌ 未提供蒲公英 API Key。" -ForegroundColor Red
    Write-Host "   请把 API Key 写入 $envFile：" -ForegroundColor Yellow
    Write-Host "     PGYER_API_KEY=你的key" -ForegroundColor Cyan
    Write-Host "   或通过命令行参数传入：-ApiKey 'xxx'" -ForegroundColor Cyan
    exit 1
}

# ── ApkPath 自动探测 ───────────────────────────────
if (-not $ApkPath) {
    $apkDir = Join-Path (Split-Path -Parent $PSScriptRoot) "unpackage\release\apk"
    if (-not (Test-Path $apkDir)) {
        Write-Error "未指定 -ApkPath 且 $apkDir 不存在。"
        exit 1
    }
    $latest = Get-ChildItem -Path "$apkDir\*.apk" -ErrorAction SilentlyContinue |
        Sort-Object LastWriteTime -Descending | Select-Object -First 1
    if (-not $latest) {
        Write-Error "未指定 -ApkPath 且 $apkDir 下没有任何 .apk 文件。"
        exit 1
    }
    $ApkPath = $latest.FullName
    Write-Host "ℹ️  自动选取最新 APK: $ApkPath" -ForegroundColor DarkGray
}

if (-not (Test-Path -Path $ApkPath)) {
    Write-Error "APK 文件不存在: $ApkPath"
    exit 1
}

# ── 读 manifest.json 版本号（自动注入到上传字段）─────
$manifestPath = Join-Path (Split-Path -Parent $PSScriptRoot) "manifest.json"
$manifestVersion     = ""
$manifestVersionCode = ""
$manifestMtime       = $null

if (Test-Path $manifestPath) {
    try {
        $manifest = Get-Content -Raw -Path $manifestPath | ConvertFrom-Json
        $manifestVersion     = $manifest.versionName
        $manifestVersionCode = [string]$manifest.versionCode
        $manifestMtime       = (Get-ItemProperty -Path $manifestPath).LastWriteTime
        if (-not $BuildVersion -and $manifestVersion)         { $BuildVersion   = $manifestVersion }
        if (-not $BuildVersionNo -and $manifestVersionCode)   { $BuildVersionNo = $manifestVersionCode }
    }
    catch { Write-Warning "读取 manifest.json 失败: $_" }
}

# ── APK 新鲜度校验（防止上传旧包） ─────────────────
$apkMtime = (Get-ItemProperty -Path $ApkPath).LastWriteTime
if ($manifestMtime -and $apkMtime -lt $manifestMtime) {
    Write-Host ""
    Write-Host "⚠️ 警告: APK 比 manifest.json 还旧，可能不是最新版本!" -ForegroundColor Red
    Write-Host "   manifest.json: $manifestMtime  (版本 $manifestVersion / build $manifestVersionCode)" -ForegroundColor Yellow
    Write-Host "   APK 文件     : $apkMtime" -ForegroundColor Yellow
    Write-Host ""
    $continueUpload = Read-Host "仍要继续上传? (y/n，默认 n)"
    if ($continueUpload -ne 'y' -and $continueUpload -ne 'Y') {
        Write-Host "已取消上传。" -ForegroundColor Yellow
        exit 0
    }
}

$ApkPath = (Resolve-Path -Path $ApkPath).Path
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "  蒲公英 (Pgyer) APK 上传" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "APK     : $ApkPath" -ForegroundColor White
$typeLabel = if ($InstallType -eq 1) { '公开' } elseif ($InstallType -eq 2) { '密码' } else { '邀请' }
Write-Host "安装方式: $typeLabel" -ForegroundColor White
if ($BuildVersion)   { Write-Host "版本号  : $BuildVersion (Build $BuildVersionNo)" -ForegroundColor White }
if ($Description)    { Write-Host "更新说明: $Description" -ForegroundColor White }
Write-Host ""
Write-Host "正在上传..." -ForegroundColor Yellow

# ── curl 上传 ─────────────────────────────────────────
$curlArgs = @(
    "-s", "-S",
    "-F", "file=@$ApkPath",
    "-F", "_api_key=$ApiKey",
    "-F", "buildInstallType=$InstallType"
)
if ($Description)    { $curlArgs += @("-F", "buildUpdateDescription=$Description") }
if ($BuildVersion)   { $curlArgs += @("-F", "buildVersion=$BuildVersion") }
if ($BuildVersionNo) { $curlArgs += @("-F", "buildVersionNo=$BuildVersionNo") }
if ($InstallType -eq 2 -and $Password) {
    $curlArgs += @("-F", "buildPassword=$Password")
}
$curlArgs += "https://www.pgyer.com/apiv2/app/upload"

try {
    $jsonResult = & curl.exe @curlArgs
    if ($LASTEXITCODE -ne 0) {
        Write-Error "curl 执行失败 (exit code: $LASTEXITCODE)"
        exit 1
    }
    $response = $jsonResult | ConvertFrom-Json

    if ($response.code -eq 0) {
        $data = $response.data
        Write-Host ""
        Write-Host "✅ 上传成功!" -ForegroundColor Green
        Write-Host "应用名称: $($data.buildName)" -ForegroundColor White
        Write-Host "版本号  : $($data.buildVersion) (Build $($data.buildVersionNo))" -ForegroundColor White
        Write-Host "包名    : $($data.buildIdentifier)" -ForegroundColor White
        Write-Host ""
        Write-Host "下载链接: https://www.pgyer.com/$($data.buildShortcutUrl)" -ForegroundColor Cyan
        Write-Host "二维码  : $($data.buildQRCodeURL)" -ForegroundColor Cyan
        Write-Host ""
        Write-Host "========================================" -ForegroundColor Green
    }
    else {
        Write-Error "上传失败: $($response.message) (code: $($response.code))"
        exit 1
    }
}
catch {
    Write-Error "上传过程发生错误: $_"
    exit 1
}
