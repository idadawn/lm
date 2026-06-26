# start-dev.ps1 — 一键拉起本地全栈：后端 + NLQ 智能体 + 前端
# 每个服务在各自独立的 PowerShell 窗口运行，脱离编辑器 / AI 会话常驻；
# 关闭对应窗口即停止该服务。
#
# 用法：仓库根目录下 右键“用 PowerShell 运行”，或终端执行：  ./start-dev.ps1
# 前置：Docker Desktop 已开；本机 MySQL(127.0.0.1:3306, root/root, 库 lumei) 已开。
# 注意：请在服务都未运行时执行（端口被占会冲突）。

$ErrorActionPreference = 'SilentlyContinue'
$root = $PSScriptRoot

Write-Host "==> 确保基础容器（Redis/RabbitMQ）在运行..." -ForegroundColor Cyan
docker start lm-redis-6380 lm-rabbitmq-8005 | Out-Null

# 后端（YY 环境，:10089；对接前端 /dev 代理；连本机 MySQL:3306 / Redis:6380 / RabbitMQ:8005）
Write-Host "==> 启动后端 (.NET, :10089)..." -ForegroundColor Cyan
Start-Process powershell -ArgumentList @(
  '-NoExit', '-Command',
  "Set-Location '$root\api\src\application\Poxiao.API.Entry'; dotnet run --launch-profile YY"
)

# NLQ 智能体（FastAPI, :8000；绕过系统代理直连 DeepSeek）
Write-Host "==> 启动 NLQ 智能体 (uvicorn, :8000)..." -ForegroundColor Cyan
Start-Process powershell -ArgumentList @(
  '-NoExit', '-Command',
  "Set-Location '$root\nlq-agent\services\agent-api'; " +
  "`$env:ALL_PROXY=''; `$env:HTTP_PROXY=''; `$env:HTTPS_PROXY=''; `$env:NO_PROXY='*'; " +
  "uv run uvicorn app.main:app --host 0.0.0.0 --port 8000"
)

# 前端（Vite, :3100）
Write-Host "==> 启动前端 (Vite, :3100)..." -ForegroundColor Cyan
Start-Process powershell -ArgumentList @(
  '-NoExit', '-Command',
  "Set-Location '$root\web'; pnpm dev"
)

Write-Host ""
Write-Host "全部已在独立窗口启动。约 30~60s 后端编译完成，访问 http://localhost:3100/（admin 登录）。" -ForegroundColor Green
Write-Host "关闭某个窗口即停止该服务；MySQL(3306) 由你本机维护，本脚本不管理。" -ForegroundColor DarkGray
