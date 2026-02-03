# ============================================
# 实验室数据分析系统 - Windows 统一构建脚本
# 功能：
#   1. 发布后端 API (.NET)
#   2. 打包后端 Docker 镜像
#   3. 打包前端 Docker 镜像
# ============================================

[CmdletBinding()]
param(
    [Parameter()]
    [switch]$SkipApi,
    
    [Parameter()]
    [switch]$SkipWeb,
    
    [Parameter()]
    [switch]$SkipDocker,
    
    [Parameter()]
    [switch]$Push,
    
    [Parameter()]
    [string]$Registry = "",
    
    [Parameter()]
    [switch]$Help
)

$ErrorActionPreference = "Stop"

# ============================================
# 颜色输出函数
# ============================================
function Write-Step { param([string]$msg) Write-Host "[STEP] $msg" -ForegroundColor Cyan }
function Write-Info { param([string]$msg) Write-Host "[INFO] $msg" -ForegroundColor Green }
function Write-Warn { param([string]$msg) Write-Host "[WARN] $msg" -ForegroundColor Yellow }
function Write-Error { param([string]$msg) Write-Host "[ERROR] $msg" -ForegroundColor Red }
function Write-Success { param([string]$msg) Write-Host "[SUCCESS] $msg" -ForegroundColor Green -BackgroundColor Black }

# ============================================
# 帮助信息
# ============================================
function Show-Help {
    @"
========================================
实验室数据分析系统 - Windows 构建脚本
========================================

用法: .\build-all.ps1 [选项]

选项:
    -SkipApi        跳过后端 API 发布和镜像构建
    -SkipWeb        跳过前端构建和镜像构建
    -SkipDocker     跳过 Docker 镜像构建（仅发布）
    -Push           构建后推送到镜像仓库
    -Registry       镜像仓库地址 (如: registry.cn-hangzhou.aliyuncs.com/myrepo)
    -Help           显示此帮助信息

示例:
    .\build-all.ps1                          # 构建所有
    .\build-all.ps1 -SkipWeb                 # 只构建后端
    .\build-all.ps1 -SkipApi                 # 只构建前端
    .\build-all.ps1 -Push -Registry "xxx"    # 构建并推送

========================================
"@ | Write-Host
}

# ============================================
# 初始化路径和版本
# ============================================
function Initialize-Environment {
    Write-Step "初始化环境..."
    
    $script:ProjectRoot = Resolve-Path (Join-Path $PSScriptRoot "..")
    $script:ApiProject = Join-Path $script:ProjectRoot "api\src\application\Poxiao.API.Entry\Poxiao.API.Entry.csproj"
    $script:WebDir = Join-Path $script:ProjectRoot "web"
    $script:PublishApiDir = Join-Path $script:ProjectRoot "publish\api"
    $script:PublishWebDir = Join-Path $script:ProjectRoot "publish\web"
    
    # 读取版本号
    $versionFile = Join-Path $script:ProjectRoot "VERSION"
    $script:Version = "latest"
    if (Test-Path $versionFile) {
        $script:Version = (Get-Content $versionFile -Raw).Trim()
        if ([string]::IsNullOrWhiteSpace($script:Version)) { $script:Version = "latest" }
    }
    
    Write-Info "项目根目录: $($script:ProjectRoot)"
    Write-Info "版本号: $($script:Version)"
}

# ============================================
# 检查 .NET SDK
# ============================================
function Test-DotNet {
    Write-Step "检查 .NET SDK..."
    
    if (-not (Get-Command dotnet -ErrorAction SilentlyContinue)) {
        Write-Error ".NET SDK 未安装，请先安装 .NET 10.0 SDK"
        Write-Info "下载地址: https://dotnet.microsoft.com/download"
        exit 1
    }
    
    $dotnetVersion = dotnet --version
    Write-Info ".NET SDK 版本: $dotnetVersion"
}

# ============================================
# 检查 Node.js 和 pnpm
# ============================================
function Test-NodeEnvironment {
    Write-Step "检查 Node.js 环境..."
    
    if (-not (Get-Command node -ErrorAction SilentlyContinue)) {
        Write-Error "Node.js 未安装，请先安装 Node.js 18+"
        exit 1
    }
    
    $nodeVersion = node -v
    Write-Info "Node.js 版本: $nodeVersion"
    
    if (-not (Get-Command pnpm -ErrorAction SilentlyContinue)) {
        Write-Error "pnpm 未安装，请先安装 pnpm"
        Write-Info "安装命令: npm install -g pnpm"
        exit 1
    }
    
    $pnpmVersion = pnpm -v
    Write-Info "pnpm 版本: $pnpmVersion"
}

# ============================================
# 检查 Docker
# ============================================
function Test-Docker {
    Write-Step "检查 Docker..."
    
    if (-not (Get-Command docker -ErrorAction SilentlyContinue)) {
        Write-Error "Docker 未安装，请先安装 Docker Desktop"
        exit 1
    }
    
    $dockerVersion = docker --version
    Write-Info "Docker: $dockerVersion"
    
    # 检查 Docker 是否正在运行
    try {
        $null = docker info 2>$null
        Write-Info "Docker 守护进程运行正常"
    }
    catch {
        Write-Error "Docker 守护进程未运行，请启动 Docker Desktop"
        exit 1
    }
}

# ============================================
# 发布后端 API
# ============================================
function Publish-Api {
    Write-Step "发布后端 API..."
    
    # 检查项目文件
    if (-not (Test-Path $script:ApiProject)) {
        Write-Error "找不到 API 项目: $($script:ApiProject)"
        exit 1
    }
    
    # 清理旧发布目录
    if (Test-Path $script:PublishApiDir) {
        Write-Info "清理旧发布目录..."
        Remove-Item -Recurse -Force $script:PublishApiDir
    }
    New-Item -ItemType Directory -Force -Path $script:PublishApiDir | Out-Null
    
    # 执行发布
    Write-Info "执行 dotnet publish..."
    $publishArgs = @(
        $script:ApiProject,
        "-c", "Release",
        "-r", "linux-x64",
        "--self-contained", "false",
        "-o", $script:PublishApiDir,
        "-v", "q"
    )
    
    & dotnet publish @publishArgs
    if ($LASTEXITCODE -ne 0) {
        Write-Error "API 发布失败"
        exit 1
    }
    
    # 复制配置文件和资源
    $apiEntryDir = Split-Path $script:ApiProject -Parent
    $configurationsDir = Join-Path $apiEntryDir "Configurations"
    $resourcesDir = Join-Path $script:ProjectRoot "api\resources"
    $libDir = Join-Path $apiEntryDir "lib"
    
    if (Test-Path $configurationsDir) {
        Copy-Item -Recurse -Force $configurationsDir $script:PublishApiDir
        Write-Info "已复制: Configurations"
    }
    
    if (Test-Path $resourcesDir) {
        Copy-Item -Recurse -Force $resourcesDir $script:PublishApiDir
        Write-Info "已复制: resources"
    }
    
    if (Test-Path $libDir) {
        Copy-Item -Recurse -Force $libDir $script:PublishApiDir
        Write-Info "已复制: lib"
    }
    
    Write-Success "API 发布完成: $($script:PublishApiDir)"
    
    # 计算目录大小
    $size = (Get-ChildItem $script:PublishApiDir -Recurse | Measure-Object -Property Length -Sum).Sum
    $sizeMB = [math]::Round($size / 1MB, 2)
    Write-Info "发布目录大小: $sizeMB MB"
}

# ============================================
# 构建前端
# ============================================
function Build-Web {
    Write-Step "构建前端..."
    
    if (-not (Test-Path $script:WebDir)) {
        Write-Error "找不到前端目录: $($script:WebDir)"
        exit 1
    }
    
    Push-Location $script:WebDir
    try {
        # 配置国内镜像
        Write-Info "配置 pnpm 镜像..."
        & pnpm config set registry https://registry.npmmirror.com
        
        # 安装依赖
        Write-Info "安装依赖..."
        & pnpm install
        if ($LASTEXITCODE -ne 0) {
            Write-Error "依赖安装失败"
            exit 1
        }
        
        # 构建
        Write-Info "执行构建..."
        & pnpm run build
        if ($LASTEXITCODE -ne 0) {
            Write-Error "前端构建失败"
            exit 1
        }
    }
    finally {
        Pop-Location
    }
    
    # 复制到发布目录
    $webDist = Join-Path $script:WebDir "dist"
    if (-not (Test-Path $webDist)) {
        Write-Error "前端构建失败，未找到 dist 目录"
        exit 1
    }
    
    if (Test-Path $script:PublishWebDir) {
        Remove-Item -Recurse -Force $script:PublishWebDir
    }
    New-Item -ItemType Directory -Force -Path $script:PublishWebDir | Out-Null
    Copy-Item -Recurse -Force (Join-Path $webDist "*") $script:PublishWebDir
    
    Write-Success "前端构建完成: $($script:PublishWebDir)"
    
    # 计算目录大小
    $size = (Get-ChildItem $script:PublishWebDir -Recurse | Measure-Object -Property Length -Sum).Sum
    $sizeMB = [math]::Round($size / 1MB, 2)
    Write-Info "发布目录大小: $sizeMB MB"
}

# ============================================
# 构建后端 Docker 镜像
# ============================================
function Build-ApiImage {
    Write-Step "构建后端 Docker 镜像..."
    
    $dockerfile = Join-Path $script:ProjectRoot "api\Dockerfile.build"
    if (-not (Test-Path $dockerfile)) {
        Write-Error "找不到 Dockerfile: $dockerfile"
        exit 1
    }
    
    # 检查发布目录
    if (-not (Test-Path $script:PublishApiDir)) {
        Write-Error "API 发布目录不存在，请先执行 API 发布"
        exit 1
    }
    
    $imageName = "lm-api:$($script:Version)"
    $imageNameLatest = "lm-api:latest"
    
    Write-Info "构建镜像: $imageName"
    
    & docker build --progress=plain -t $imageName -f $dockerfile $script:ProjectRoot
    if ($LASTEXITCODE -ne 0) {
        Write-Error "后端镜像构建失败"
        exit 1
    }
    
    # 打 latest 标签
    & docker tag $imageName $imageNameLatest
    Write-Info "已打标签: $imageNameLatest"
    
    Write-Success "后端镜像构建完成: $imageName"
    
    # 推送镜像
    if ($Push) {
        Push-Image -ImageName $imageName -ImageNameLatest $imageNameLatest
    }
}

# ============================================
# 构建前端 Docker 镜像
# ============================================
function Build-WebImage {
    Write-Step "构建前端 Docker 镜像..."
    
    $dockerfile = Join-Path $script:WebDir "Dockerfile"
    if (-not (Test-Path $dockerfile)) {
        Write-Error "找不到 Dockerfile: $dockerfile"
        exit 1
    }
    
    # 检查发布目录
    if (-not (Test-Path $script:PublishWebDir)) {
        Write-Error "Web 发布目录不存在，请先执行前端构建"
        exit 1
    }
    
    $imageName = "lm-web:$($script:Version)"
    $imageNameLatest = "lm-web:latest"
    
    Write-Info "构建镜像: $imageName"
    
    # 注意：前端 Dockerfile 需要 dist 目录，它在 web/Dockerfile 中使用 COPY dist
    & docker build --progress=plain -t $imageName -f $dockerfile $script:WebDir
    if ($LASTEXITCODE -ne 0) {
        Write-Error "前端镜像构建失败"
        exit 1
    }
    
    # 打 latest 标签
    & docker tag $imageName $imageNameLatest
    Write-Info "已打标签: $imageNameLatest"
    
    Write-Success "前端镜像构建完成: $imageName"
    
    # 推送镜像
    if ($Push) {
        Push-Image -ImageName $imageName -ImageNameLatest $imageNameLatest
    }
}

# ============================================
# 推送镜像到仓库
# ============================================
function Push-Image {
    param(
        [string]$ImageName,
        [string]$ImageNameLatest
    )
    
    if ([string]::IsNullOrWhiteSpace($Registry)) {
        Write-Warn "未指定镜像仓库地址，跳过推送"
        return
    }
    
    Write-Step "推送镜像到仓库: $Registry"
    
    # 重新打标签为仓库格式
    $repoImageName = "$Registry/$ImageName"
    $repoImageNameLatest = "$Registry/$ImageNameLatest"
    
    & docker tag $ImageName $repoImageName
    & docker tag $ImageNameLatest $repoImageNameLatest
    
    Write-Info "推送: $repoImageName"
    & docker push $repoImageName
    if ($LASTEXITCODE -ne 0) {
        Write-Error "推送失败: $repoImageName"
        return
    }
    
    Write-Info "推送: $repoImageNameLatest"
    & docker push $repoImageNameLatest
    if ($LASTEXITCODE -ne 0) {
        Write-Error "推送失败: $repoImageNameLatest"
        return
    }
    
    Write-Success "镜像推送完成"
}

# ============================================
# 显示构建结果
# ============================================
function Show-Result {
    Write-Step "构建结果汇总"
    
    Write-Host ""
    Write-Host "========================================" -ForegroundColor Cyan
    Write-Host "          构建完成                      " -ForegroundColor Cyan
    Write-Host "========================================" -ForegroundColor Cyan
    Write-Host ""
    
    if (-not $SkipApi) {
        Write-Host "后端 API:" -ForegroundColor Yellow
        Write-Host "  发布目录: $($script:PublishApiDir)"
        if (-not $SkipDocker) {
            Write-Host "  镜像: lm-api:$($script:Version)"
            Write-Host "  镜像: lm-api:latest"
        }
        Write-Host ""
    }
    
    if (-not $SkipWeb) {
        Write-Host "前端 Web:" -ForegroundColor Yellow
        Write-Host "  发布目录: $($script:PublishWebDir)"
        if (-not $SkipDocker) {
            Write-Host "  镜像: lm-web:$($script:Version)"
            Write-Host "  镜像: lm-web:latest"
        }
        Write-Host ""
    }
    
    if (-not $SkipDocker) {
        Write-Host "Docker 镜像列表:" -ForegroundColor Yellow
        & docker images | Select-String -Pattern "lm-" | ForEach-Object { Write-Host "  $_" }
        Write-Host ""
    }
    
    Write-Host "========================================" -ForegroundColor Cyan
    Write-Host ""
    
    if (-not $SkipDocker) {
        Write-Info "启动服务命令:"
        Write-Host "  cd apps" -ForegroundColor DarkGray
        Write-Host "  docker-compose up -d" -ForegroundColor DarkGray
        Write-Host ""
    }
}

# ============================================
# 主流程
# ============================================
function Main {
    if ($Help) {
        Show-Help
        exit 0
    }
    
    Write-Host ""
    Write-Host "========================================" -ForegroundColor Cyan
    Write-Host "  实验室数据分析系统 - Windows 构建脚本" -ForegroundColor Cyan
    Write-Host "========================================" -ForegroundColor Cyan
    Write-Host ""
    
    # 初始化
    Initialize-Environment
    
    # 检查环境
    if (-not $SkipApi) {
        Test-DotNet
    }
    if (-not $SkipWeb) {
        Test-NodeEnvironment
    }
    if (-not $SkipDocker) {
        Test-Docker
    }
    
    # 发布后端
    if (-not $SkipApi) {
        Publish-Api
    }
    
    # 构建前端
    if (-not $SkipWeb) {
        Build-Web
    }
    
    # 构建镜像
    if (-not $SkipDocker) {
        if (-not $SkipApi) {
            Build-ApiImage
        }
        if (-not $SkipWeb) {
            Build-WebImage
        }
    }
    
    # 显示结果
    Show-Result
    
    Write-Success "全部完成!"
}

# 执行主流程
Main
