#Requires -Version 5.1
<#
.SYNOPSIS
    上传 Android APK 到蒲公英 (Pgyer)
.DESCRIPTION
    通过蒲公英 API v2 上传 APK 文件，支持设置密码、更新说明等。
    需要在 https://www.pgyer.com/account/api 获取 API Key。
.PARAMETER ApkPath
    APK 文件路径
.PARAMETER ApiKey
    蒲公英 API Key
.PARAMETER Description
    更新说明（选填）
.PARAMETER InstallType
    安装类型：1=公开, 2=密码, 3=邀请（默认 1）
.PARAMETER Password
    安装密码，当 InstallType=2 时必填
.EXAMPLE
    .\upload-to-pgyer.ps1 -ApkPath "..\unpackage\release\android.apk" -ApiKey "your_api_key" -Description "修复已知问题"
#>
param(
    [Parameter(Mandatory = $true)]
    [string]$ApkPath,

    [Parameter(Mandatory = $true)]
    [string]$ApiKey,

    [string]$Description = "",

    [ValidateSet(1, 2, 3)]
    [int]$InstallType = 1,

    [string]$Password = "",

    [string]$BuildVersion = "",

    [string]$BuildVersionNo = ""
)

$ErrorActionPreference = "Stop"

# 自动加载同目录下的配置文件（如果存在）
$configPath = Join-Path $PSScriptRoot ".pgyer-config.ps1"
if (Test-Path $configPath) {
    . $configPath
    if (-not $ApiKey -and $PgyerApiKey) {
        $ApiKey = $PgyerApiKey
    }
}

# 检查 APK 文件
if (-not (Test-Path -Path $ApkPath)) {
    Write-Error "APK 文件不存在: $ApkPath"
    exit 1
}

# 自动读取 manifest.json 版本号（如果未手动指定）
$manifestPath = Join-Path $PSScriptRoot "..\manifest.json"
$manifestVersion = ""
$manifestVersionCode = ""
$manifestMtime = $null

if (Test-Path $manifestPath) {
    try {
        $manifest = Get-Content -Raw -Path $manifestPath | ConvertFrom-Json
        $manifestVersion = $manifest.versionName
        $manifestVersionCode = [string]$manifest.versionCode
        $manifestMtime = (Get-ItemProperty -Path $manifestPath).LastWriteTime
        if ([string]::IsNullOrEmpty($BuildVersion) -and $manifestVersion) {
            $BuildVersion = $manifestVersion
        }
        if ([string]::IsNullOrEmpty($BuildVersionNo) -and $manifestVersionCode) {
            $BuildVersionNo = $manifestVersionCode
        }
    }
    catch {
        Write-Warning "读取 manifest.json 失败: $_"
    }
}

# 校验 APK 是否为最新版本（对比修改时间）
$apkMtime = (Get-ItemProperty -Path $ApkPath).LastWriteTime
if ($manifestMtime -and $apkMtime -lt $manifestMtime) {
    Write-Host ""
    Write-Host "⚠️ 警告: APK 文件可能不是最新版本!" -ForegroundColor Red
    Write-Host "   manifest.json 修改时间: $manifestMtime (版本 $manifestVersion)" -ForegroundColor Yellow
    Write-Host "   APK 文件修改时间    : $apkMtime" -ForegroundColor Yellow
    Write-Host "   APK 修改时间早于 manifest.json，说明这是旧版本 APK。" -ForegroundColor Yellow
    Write-Host ""
    Write-Host "可能原因:" -ForegroundColor White
    Write-Host "   1. 云打包尚未完成或未重新打包" -ForegroundColor White
    Write-Host "   2. 新的 APK 生成在其他路径（如 unpackage/release/apk/）" -ForegroundColor White
    Write-Host "   3. 使用的是之前残留的 android.apk" -ForegroundColor White
    Write-Host ""
    $continueUpload = Read-Host "是否仍要继续上传此旧 APK? (y/n，默认 n)"
    if ($continueUpload -ne 'y' -and $continueUpload -ne 'Y') {
        Write-Host "已取消上传。请确认云打包完成后再试。" -ForegroundColor Yellow
        exit 0
    }
}

$ApkPath = Resolve-Path -Path $ApkPath | Select-Object -ExpandProperty Path
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "  蒲公英 (Pgyer) APK 上传工具" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""
Write-Host "APK 文件: $ApkPath" -ForegroundColor White
$typeLabel = if ($InstallType -eq 1) { '公开' } elseif ($InstallType -eq 2) { '密码' } else { '邀请' }
Write-Host "安装方式: $typeLabel" -ForegroundColor White
if ($BuildVersion) {
    Write-Host "版本号  : $BuildVersion (Build $BuildVersionNo)" -ForegroundColor White
}
if ($Description) {
    Write-Host "更新说明: $Description" -ForegroundColor White
}
Write-Host ""
Write-Host "正在上传，请稍候..." -ForegroundColor Yellow

# 构建 curl 命令参数
$curlArgs = @(
    "-s", "-S",
    "-F", "file=@$ApkPath",
    "-F", "_api_key=$ApiKey",
    "-F", "buildInstallType=$InstallType"
)

if ($Description) {
    $curlArgs += "-F"
    $curlArgs += "buildUpdateDescription=$Description"
}

if ($BuildVersion) {
    $curlArgs += "-F"
    $curlArgs += "buildVersion=$BuildVersion"
}

if ($BuildVersionNo) {
    $curlArgs += "-F"
    $curlArgs += "buildVersionNo=$BuildVersionNo"
}

if ($InstallType -eq 2 -and $Password) {
    $curlArgs += "-F"
    $curlArgs += "buildPassword=$Password"
}

$curlArgs += "https://www.pgyer.com/apiv2/app/upload"

try {
    $jsonResult = & curl.exe @curlArgs
    if ($LASTEXITCODE -ne 0) {
        Write-Error "curl 执行失败 (exit code: $LASTEXITCODE)"
    }

    $response = $jsonResult | ConvertFrom-Json

    if ($response.code -eq 0) {
        $data = $response.data
        Write-Host ""
        Write-Host "✅ 上传成功!" -ForegroundColor Green
        Write-Host ""
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
    }
}
catch {
    Write-Error "上传过程发生错误: $_"
}
