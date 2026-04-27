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
if (([string]::IsNullOrEmpty($BuildVersion) -or [string]::IsNullOrEmpty($BuildVersionNo)) -and (Test-Path $manifestPath)) {
    try {
        $manifest = Get-Content -Raw -Path $manifestPath | ConvertFrom-Json
        if ([string]::IsNullOrEmpty($BuildVersion) -and $manifest.versionName) {
            $BuildVersion = $manifest.versionName
        }
        if ([string]::IsNullOrEmpty($BuildVersionNo) -and $manifest.versionCode) {
            $BuildVersionNo = [string]$manifest.versionCode
        }
    }
    catch {
        Write-Warning "读取 manifest.json 失败: $_"
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
