#Requires -Version 5.1
<#
.SYNOPSIS
    打包并推送 Poxiao Framework 到私有 NuGet 服务器
.DESCRIPTION
    1. 清理旧的 nupkg 文件
    2. 仅构建 api/framework 目录下的类库项目并打包
    3. 将生成的包推送到私有 BaGet 服务器
.NOTES
    请确保 BaGet 服务器已启动（docker-compose up -d）
#>
$ErrorActionPreference = "Stop"

# 配置
$ScriptDir = $PSScriptRoot
$PackageOutputPath = Join-Path $ScriptDir "nupkgs"
$SourceUrl = "http://localhost:7000/v3/index.json"
$ApiKey = "NUGET-SERVER-API-KEY"

Write-Host "=========================================" -ForegroundColor Cyan
Write-Host "  Poxiao Framework Pack & Push Script" -ForegroundColor Cyan
Write-Host "=========================================" -ForegroundColor Cyan
Write-Host ""

# 1. 清理旧包
Write-Host "[1/4] 清理旧的 nupkg 文件..." -ForegroundColor Yellow
if (Test-Path $PackageOutputPath) {
    Get-ChildItem -Path $PackageOutputPath -Include "*.nupkg","*.snupkg" | Remove-Item -Force
    Write-Host "       已清理 $PackageOutputPath" -ForegroundColor Gray
} else {
    New-Item -ItemType Directory -Path $PackageOutputPath | Out-Null
    Write-Host "       已创建 $PackageOutputPath" -ForegroundColor Gray
}
Write-Host ""

# 2. 查找 framework 下的所有类库项目（排除测试项目和备份项目）
Write-Host "[2/4] 查找 framework 项目..." -ForegroundColor Yellow
$projects = Get-ChildItem -Path $ScriptDir -Recurse -Filter "*.csproj" | Where-Object {
    $_.Name -notlike "*Backup*" -and
    $_.Name -notlike "*Test*" -and
    $_.FullName -notlike "*\tests\*"
} | Select-Object -ExpandProperty FullName

if ($projects.Count -eq 0) {
    Write-Error "未找到任何可打包的 csproj 文件。"
    exit 1
}

Write-Host "       发现 $($projects.Count) 个项目：" -ForegroundColor Gray
$projects | ForEach-Object { Write-Host "         - $(Split-Path $_ -Leaf)" -ForegroundColor Gray }
Write-Host ""

# 3. 构建并打包（Release）
Write-Host "[3/4] 构建 Release 并生成 NuGet 包..." -ForegroundColor Yellow
foreach ($proj in $projects) {
    Write-Host "       构建 $(Split-Path $proj -Leaf) ..." -ForegroundColor Gray -NoNewline
    dotnet pack $proj -c Release --verbosity quiet
    if ($LASTEXITCODE -ne 0) { throw "构建失败: $proj" }
    Write-Host " 完成" -ForegroundColor Green
}
Write-Host ""

# 4. 检查生成的包
Write-Host "[4/4] 检查生成的包并推送..." -ForegroundColor Yellow
$packages = Get-ChildItem -Path $PackageOutputPath -Filter "*.nupkg"
if ($packages.Count -eq 0) {
    Write-Error "未找到任何 .nupkg 文件，打包失败。"
    exit 1
}
Write-Host "       发现 $($packages.Count) 个包，开始推送..." -ForegroundColor Gray

foreach ($pkg in $packages) {
    Write-Host "       推送 $($pkg.Name) ..." -ForegroundColor Gray -NoNewline
    dotnet nuget push $pkg.FullName --source $SourceUrl --api-key $ApiKey --skip-duplicate | Out-Null
    if ($LASTEXITCODE -eq 0) {
        Write-Host " 成功" -ForegroundColor Green
    } else {
        Write-Host " 失败" -ForegroundColor Red
    }
}

Write-Host ""
Write-Host "=========================================" -ForegroundColor Cyan
Write-Host "  完成！所有包已推送至私有 NuGet 服务器" -ForegroundColor Cyan
Write-Host "  浏览地址: http://localhost:7000" -ForegroundColor Cyan
Write-Host "=========================================" -ForegroundColor Cyan
