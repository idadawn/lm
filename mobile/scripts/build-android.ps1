#Requires -Version 5.1
<#
.SYNOPSIS
    uni-app Android 打包 + 蒲公英上传向导
.DESCRIPTION
    检查 HBuilderX 环境，引导完成 Android APK 打包，并支持自动上传到蒲公英。
    如未安装 HBuilderX，将提示下载地址和手动打包步骤。
.PARAMETER PgyerApiKey
    蒲公英 API Key（选填，如不提供则跳过上传）
.PARAMETER Description
    更新说明（选填）
.EXAMPLE
    .\build-android.ps1 -PgyerApiKey "your_api_key" -Description "修复已知问题"
#>
param(
    [string]$PgyerApiKey = "",
    [string]$Description = ""
)

$ErrorActionPreference = "Stop"
$ProjectRoot = Split-Path -Parent $PSScriptRoot
$HBuilderXPath = $null

# 自动加载同目录下的配置文件（如果存在）
$configPath = Join-Path $PSScriptRoot ".pgyer-config.ps1"
if (Test-Path $configPath) {
    . $configPath
    if (-not $PgyerApiKey -and (Get-Variable -Name PgyerApiKey -ValueOnly -ErrorAction SilentlyContinue)) {
        $PgyerApiKey = (Get-Variable -Name PgyerApiKey -ValueOnly)
    }
}

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "  检测室数据分析 - Android 打包向导" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

# 1. 查找 HBuilderX
$searchPaths = @(
    "C:\Program Files\HBuilderX\cli.exe",
    "C:\Program Files (x86)\HBuilderX\cli.exe",
    "C:\HBuilderX\cli.exe",
    "D:\HBuilderX\cli.exe",
    "E:\HBuilderX\cli.exe",
    "$env:LOCALAPPDATA\HBuilderX\cli.exe",
    "$env:USERPROFILE\HBuilderX\cli.exe"
)

foreach ($path in $searchPaths) {
    if (Test-Path $path) {
        $HBuilderXPath = $path
        break
    }
}

# 也尝试从 PATH 中查找
if (-not $HBuilderXPath) {
    $cliInPath = Get-Command "cli.exe" -ErrorAction SilentlyContinue
    if ($cliInPath) {
        $HBuilderXPath = $cliInPath.Source
    }
}

Write-Host "【步骤 1/4】环境检查" -ForegroundColor Yellow
Write-Host ""

if ($HBuilderXPath) {
    Write-Host "✅ 已找到 HBuilderX: $HBuilderXPath" -ForegroundColor Green
}
else {
    Write-Host "⚠️ 未找到 HBuilderX" -ForegroundColor Red
    Write-Host ""
    Write-Host "uni-app Android 打包需要 HBuilderX，请按以下步骤操作：" -ForegroundColor Yellow
    Write-Host ""
    Write-Host "1. 下载 HBuilderX (Windows 版):" -ForegroundColor White
    Write-Host "   https://www.dcloud.io/hbuilderx.html" -ForegroundColor Cyan
    Write-Host ""
    Write-Host "2. 解压到任意目录（如 C:\HBuilderX）" -ForegroundColor White
    Write-Host ""
    Write-Host "3. 重新运行此脚本" -ForegroundColor White
    Write-Host ""
    Write-Host "或者，您可以手动打包：" -ForegroundColor Yellow
    Write-Host "   a) 用 HBuilderX 打开 mobile 目录" -ForegroundColor White
    Write-Host "   b) 点击菜单 [发行] -> [原生App-云打包]" -ForegroundColor White
    Write-Host "   c) 选择 Android，配置证书后点击打包" -ForegroundColor White
    Write-Host ""

    $choice = Read-Host "是否现在下载 HBuilderX? (y/n，默认 n)"
    if ($choice -eq 'y' -or $choice -eq 'Y') {
        $downloadUrl = "https://download1.dcloud.net.cn/download/HBuilderX.4.57.2025012307.windows.zip"
        Write-Host ""
        Write-Host "正在打开下载页面..." -ForegroundColor Yellow
        Start-Process "https://www.dcloud.io/hbuilderx.html"
    }
    exit 1
}

Write-Host ""
Write-Host "【步骤 2/4】打开项目" -ForegroundColor Yellow
Write-Host ""

# 尝试用 HBuilderX 打开项目
& $HBuilderXPath open --project $ProjectRoot 2>$null
if ($LASTEXITCODE -eq 0 -or $LASTEXITCODE -eq $null) {
    Write-Host "✅ 已在 HBuilderX 中打开项目" -ForegroundColor Green
}
else {
    Write-Host "⚠️ 自动打开项目失败，请手动在 HBuilderX 中打开: $ProjectRoot" -ForegroundColor Yellow
}

Write-Host ""
Write-Host "【步骤 3/4】打包 APK" -ForegroundColor Yellow
Write-Host ""

# 尝试用 HBuilderX CLI 生成本地 App 资源（云打包仍需在 GUI 中操作）
Write-Host "正在尝试生成 App 资源..." -ForegroundColor Yellow
Write-Host "（如提示未登录，请先在 HBuilderX 中登录 DCloud 账号）" -ForegroundColor DarkYellow
Write-Host ""

# HBuilderX CLI 生成本地 App 资源（type 必填）
$publishResult = & $HBuilderXPath publish --platform APP --project $ProjectRoot --type appResource 2>&1
$publishResult | ForEach-Object { Write-Host $_ }

Write-Host ""
Write-Host "----------------------------------------" -ForegroundColor DarkGray
Write-Host "请按以下步骤在 HBuilderX 中完成打包：" -ForegroundColor Yellow
Write-Host "  1. 点击菜单 [发行] -> [原生App-云打包]" -ForegroundColor White
Write-Host "  2. 选择 Android，勾选 使用公共测试证书" -ForegroundColor White
Write-Host "  3. 点击 [打包] 按钮，等待控制台提示完成" -ForegroundColor White
Write-Host "  4. APK 默认输出到: unpackage/release/apk/" -ForegroundColor White
Write-Host "----------------------------------------" -ForegroundColor DarkGray
Write-Host ""

# 查找最近生成的 APK
$apkPaths = @(
    "$ProjectRoot\unpackage\release\apk\*.apk",
    "$ProjectRoot\unpackage\release\*.apk",
    "$env:USERPROFILE\Documents\HBuilderProjects\*.apk"
)

$foundApk = $null
foreach ($pattern in $apkPaths) {
    $apk = Get-ChildItem -Path $pattern -ErrorAction SilentlyContinue | Sort-Object LastWriteTime -Descending | Select-Object -First 1
    if ($apk) {
        $foundApk = $apk.FullName
        break
    }
}

if ($foundApk) {
    Write-Host "发现最近生成的 APK: $foundApk" -ForegroundColor Green
    $useIt = Read-Host "是否使用此 APK 上传到蒲公英? (y/n，默认 y)"
    if ($useIt -ne 'n' -and $useIt -ne 'N') {
        if ($PgyerApiKey) {
            & "$PSScriptRoot\upload-to-pgyer.ps1" -ApkPath $foundApk -ApiKey $PgyerApiKey -Description $Description
        }
        else {
            Write-Host ""
            Write-Host "未提供蒲公英 API Key，跳过上传。" -ForegroundColor Yellow
            Write-Host "如需上传，请运行:" -ForegroundColor White
            Write-Host "  .\upload-to-pgyer.ps1 -ApkPath `"$foundApk`" -ApiKey `"your_api_key`"" -ForegroundColor Cyan
        }
    }
}
else {
    Write-Host "暂未发现 APK 文件。打包完成后可运行：" -ForegroundColor Yellow
    Write-Host "  .\upload-to-pgyer.ps1 -ApkPath `"<apk路径>`" -ApiKey `"your_api_key`"" -ForegroundColor Cyan
}

Write-Host ""
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "打包向导结束" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
