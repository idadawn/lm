#Requires -Version 5.1
<#
.SYNOPSIS
    uni-app Android 自动云打包 + 蒲公英上传
.DESCRIPTION
    1. 检查 HBuilderX 环境
    2. 用 CLI 自动生成 App 资源
    3. 尝试用 CLI 自动触发原生 App 云打包（公共测试证书）
    4. 轮询等待 APK 生成
    5. 自动上传到蒲公英
.PARAMETER PgyerApiKey
    蒲公英 API Key（选填，如不提供则跳过上传）
.PARAMETER Description
    更新说明（选填）
.PARAMETER MaxWaitMinutes
    云打包最大等待时间（分钟，默认 8）
.EXAMPLE
    .\build-android.ps1 -PgyerApiKey "your_api_key" -Description "修复已知问题"
#>
param(
    [string]$PgyerApiKey = "",
    [string]$Description = "",
    [int]$MaxWaitMinutes = 8
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
Write-Host "  检测室数据分析 - Android 自动打包" -ForegroundColor Cyan
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

Write-Host "【步骤 1/5】环境检查" -ForegroundColor Yellow
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

    $choice = Read-Host "是否现在下载 HBuilderX? (y/n，默认 n)"
    if ($choice -eq 'y' -or $choice -eq 'Y') {
        Start-Process "https://www.dcloud.io/hbuilderx.html"
    }
    exit 1
}

Write-Host ""
Write-Host "【步骤 2/5】打开项目" -ForegroundColor Yellow
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
Write-Host "【步骤 3/5】生成 App 资源" -ForegroundColor Yellow
Write-Host "（如提示未登录，请先在 HBuilderX 中登录 DCloud 账号）" -ForegroundColor DarkYellow
Write-Host ""

# HBuilderX CLI 生成本地 App 资源
$publishResult = & $HBuilderXPath publish --platform APP --project $ProjectRoot --type appResource 2>&1
$publishResult | ForEach-Object { Write-Host $_ }

Write-Host ""
Write-Host "【步骤 4/5】云打包 APK" -ForegroundColor Yellow
Write-Host ""

# 尝试用 CLI 自动触发云打包（公共测试证书）
Write-Host "正在尝试自动云打包（使用公共测试证书）..." -ForegroundColor Yellow
Write-Host ""

$packResult = & $HBuilderXPath publish --platform APP --project $ProjectRoot --type nativeApp --iscustom false 2>&1
$packResult | ForEach-Object { Write-Host $_ }

# 检查 CLI 是否明确提示需要手动操作
$needManual = $packResult | Where-Object { $_ -match "参数 type 值不能为空|请手动|不支持|error|失败" }

if ($needManual) {
    Write-Host ""
    Write-Host "⚠️ CLI 自动云打包失败，请手动在 HBuilderX 中完成：" -ForegroundColor Red
    Write-Host "----------------------------------------" -ForegroundColor DarkGray
    Write-Host "  1. 点击菜单 [发行] -> [原生App-云打包]" -ForegroundColor White
    Write-Host "  2. 选择 Android，勾选 使用公共测试证书" -ForegroundColor White
    Write-Host "  3. 点击 [打包] 按钮，等待控制台提示完成" -ForegroundColor White
    Write-Host "  4. APK 默认输出到: unpackage/release/apk/" -ForegroundColor White
    Write-Host "----------------------------------------" -ForegroundColor DarkGray
    Write-Host ""
}
else {
    Write-Host "✅ 云打包任务已提交，正在等待 APK 生成..." -ForegroundColor Green
    Write-Host "   最大等待时间: $MaxWaitMinutes 分钟" -ForegroundColor White
    Write-Host ""

    # 轮询等待 APK 生成
    $apkDir = "$ProjectRoot\unpackage\release\apk"
    $startTime = Get-Date
    $foundApk = $null

    while (((Get-Date) - $startTime).TotalMinutes -lt $MaxWaitMinutes) {
        Start-Sleep -Seconds 15

        $elapsed = [math]::Round(((Get-Date) - $startTime).TotalSeconds)
        Write-Host "  [$elapsed`s] 正在检查 APK..." -ForegroundColor DarkGray -NoNewline

        if (Test-Path $apkDir) {
            $apk = Get-ChildItem -Path "$apkDir\*.apk" -ErrorAction SilentlyContinue | Sort-Object LastWriteTime -Descending | Select-Object -First 1
            if ($apk -and $apk.LastWriteTime -gt $startTime) {
                $foundApk = $apk.FullName
                Write-Host "  ✅ 发现新 APK!" -ForegroundColor Green
                break
            }
        }
        Write-Host ""
    }

    if (-not $foundApk) {
        Write-Host ""
        Write-Host "⏰ 等待超时，未检测到新生成的 APK。" -ForegroundColor Yellow
        Write-Host "   可能云打包仍在队列中，请稍后手动检查。" -ForegroundColor White
        Write-Host ""
    }
}

Write-Host ""
Write-Host "【步骤 5/5】上传 APK" -ForegroundColor Yellow
Write-Host ""

# 如果没有通过轮询找到 APK，再尝试查找一次（包括旧 APK）
if (-not $foundApk) {
    $apkPaths = @(
        "$ProjectRoot\unpackage\release\apk\*.apk",
        "$ProjectRoot\unpackage\release\*.apk"
    )
    foreach ($pattern in $apkPaths) {
        $apk = Get-ChildItem -Path $pattern -ErrorAction SilentlyContinue | Sort-Object LastWriteTime -Descending | Select-Object -First 1
        if ($apk) {
            $foundApk = $apk.FullName
            break
        }
    }
}

if ($foundApk) {
    $apkInfo = Get-ItemProperty -Path $foundApk
    Write-Host "发现 APK: $foundApk" -ForegroundColor Green
    Write-Host "大小: $([math]::Round($apkInfo.Length / 1MB, 2)) MB | 修改时间: $($apkInfo.LastWriteTime)" -ForegroundColor White
    Write-Host ""

    # 检查 APK 是否是新版本（修改时间应在最近 30 分钟内）
    $isRecent = ($apkInfo.LastWriteTime -gt (Get-Date).AddMinutes(-30))
    if (-not $isRecent) {
        Write-Host "⚠️ 警告: 此 APK 生成时间较早，可能不是最新版本!" -ForegroundColor Yellow
        Write-Host ""
    }

    $useIt = Read-Host "是否使用此 APK 上传到蒲公英? (y/n，默认 y)"
    if ($useIt -ne 'n' -and $useIt -ne 'N') {
        if ($PgyerApiKey) {
            & "$PSScriptRoot\upload-to-pgyer.ps1" -ApkPath $foundApk -ApiKey $PgyerApiKey -Description $Description
        }
        else {
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
