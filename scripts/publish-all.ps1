param(
    [switch]$Start
)

$ErrorActionPreference = "Stop"

function Write-Step($msg) { Write-Host "[STEP] $msg" -ForegroundColor Cyan }
function Write-Info($msg) { Write-Host "[INFO] $msg" -ForegroundColor Green }
function Write-Warn($msg) { Write-Host "[WARN] $msg" -ForegroundColor Yellow }

$root = Resolve-Path (Join-Path $PSScriptRoot "..")
$versionFile = Join-Path $root "VERSION"
$version = "latest"
if (Test-Path $versionFile) {
    $version = (Get-Content $versionFile -Raw).Trim()
    if ([string]::IsNullOrWhiteSpace($version)) { $version = "latest" }
}

$publishApiDir = Join-Path $root "publish/api"
$publishWebDir = Join-Path $root "publish/web"
$bundleDir = Join-Path $root "deploy-package"

Write-Step "准备输出目录"
New-Item -ItemType Directory -Force -Path $publishApiDir | Out-Null
New-Item -ItemType Directory -Force -Path $publishWebDir | Out-Null
New-Item -ItemType Directory -Force -Path $bundleDir | Out-Null

Write-Step "构建后端 API"
$apiEntry = Join-Path $root "api/src/application/Poxiao.API.Entry/Poxiao.API.Entry.csproj"
if (-not (Test-Path $apiEntry)) { throw "找不到 API 项目: $apiEntry" }

# 清理旧产物
Remove-Item -Recurse -Force -ErrorAction SilentlyContinue $publishApiDir
New-Item -ItemType Directory -Force -Path $publishApiDir | Out-Null

& dotnet publish $apiEntry -c Release -r linux-x64 --self-contained false -o $publishApiDir
Write-Info "API 发布完成: $publishApiDir"

Write-Step "构建前端 Web"
if (-not (Get-Command pnpm -ErrorAction SilentlyContinue)) {
    throw "未找到 pnpm，请先安装 pnpm"
}
$webDir = Join-Path $root "web"
Push-Location $webDir
try {
    & pnpm install
    & pnpm run build
} finally {
    Pop-Location
}

# 复制 dist
$webDist = Join-Path $webDir "dist"
if (-not (Test-Path $webDist)) { throw "前端构建失败，未找到 dist 目录" }
Remove-Item -Recurse -Force -ErrorAction SilentlyContinue $publishWebDir
New-Item -ItemType Directory -Force -Path $publishWebDir | Out-Null
Copy-Item -Recurse -Force (Join-Path $webDist "*") $publishWebDir
Write-Info "Web dist 复制完成: $publishWebDir"

Write-Step "打包部署目录"
# 仅打包应用相关内容（不包含基础设施）
$bundleApps = Join-Path $bundleDir "apps"
$bundlePublishApi = Join-Path $bundleDir "publish/api"
$bundlePublishWeb = Join-Path $bundleDir "publish/web"

Remove-Item -Recurse -Force -ErrorAction SilentlyContinue $bundleApps
Remove-Item -Recurse -Force -ErrorAction SilentlyContinue (Join-Path $bundleDir "publish")

New-Item -ItemType Directory -Force -Path $bundlePublishApi | Out-Null
New-Item -ItemType Directory -Force -Path $bundlePublishWeb | Out-Null

Copy-Item -Recurse -Force (Join-Path $root "apps/*") $bundleApps
Copy-Item -Recurse -Force (Join-Path $publishApiDir "*") $bundlePublishApi
Copy-Item -Recurse -Force (Join-Path $publishWebDir "*") $bundlePublishWeb

Write-Info "部署包生成完成: $bundleDir"

Write-Step "构建 Docker 镜像"
if (-not (Get-Command docker -ErrorAction SilentlyContinue)) {
    throw "未找到 docker，请先安装 Docker"
}

# API 镜像（使用 publish/api）
& docker build -f (Join-Path $root "api/Dockerfile.build") -t "lm-api:$version" $root
& docker tag "lm-api:$version" "lm-api:latest"

# Web 镜像（Dockerfile.build 内部构建）
& docker build -f (Join-Path $root "web/Dockerfile.build") -t "lm-web:$version" (Join-Path $root "web")
& docker tag "lm-web:$version" "lm-web:latest"

Write-Info "镜像构建完成: lm-api:$version, lm-web:$version"

if ($Start) {
    Write-Step "启动前后端服务"
    $appsDir = Join-Path $root "apps"
    $envFile = Join-Path $appsDir ".env.apps"
    if (Test-Path (Join-Path $appsDir ".env.apps.local")) {
        $envFile = Join-Path $appsDir ".env.apps.local"
    }

    Push-Location $appsDir
    try {
        & docker compose --env-file $envFile up -d
    } finally {
        Pop-Location
    }

    Write-Info "前后端服务已启动"
} else {
    Write-Warn "未启动服务。可使用: scripts/publish-all.ps1 -Start"
}

Write-Info "完成"
