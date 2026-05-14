# 启动开发环境服务（Web + nlq-agent）
# 用法: PowerShell 中执行 .\scripts\start-dev-servers.ps1

$ErrorActionPreference = "Stop"
$repoRoot = Split-Path -Parent $PSScriptRoot

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "  LM 开发环境启动脚本" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

# 检查端口占用
function Test-PortAvailable($port) {
    $conn = Get-NetTCPConnection -LocalPort $port -ErrorAction SilentlyContinue
    return $null -eq $conn
}

# 启动 nlq-agent
Write-Host "[1/2] 启动 nlq-agent (port 18100)..." -ForegroundColor Yellow
$nlqAgentDir = Join-Path $repoRoot "nlq-agent\services\agent-api"
Start-Process powershell -ArgumentList @(
    "-NoExit",
    "-Command",
    "cd `"$nlqAgentDir`"; uv run uvicorn app.main:app --host 0.0.0.0 --port 18100 --reload"
) -WindowStyle Normal

Start-Sleep -Seconds 3

# 启动 Web
Write-Host "[2/2] 启动 Web 前端 (port 3102)..." -ForegroundColor Yellow
$webDir = Join-Path $repoRoot "web"
Start-Process powershell -ArgumentList @(
    "-NoExit",
    "-Command",
    "cd `"$webDir`"; pnpm dev"
) -WindowStyle Normal

Write-Host ""
Write-Host "========================================" -ForegroundColor Green
Write-Host "  服务启动完成!" -ForegroundColor Green
Write-Host "========================================" -ForegroundColor Green
Write-Host "  Web:      http://localhost:3102" -ForegroundColor White
Write-Host "  nlq-agent:http://localhost:18100" -ForegroundColor White
Write-Host "  Redis:    127.0.0.1:6380" -ForegroundColor White
Write-Host "  RabbitMQ: 127.0.0.1:8005" -ForegroundColor White
Write-Host "========================================" -ForegroundColor Green
Write-Host ""
Write-Host "提示: 关闭本窗口不会停止已启动的服务。" -ForegroundColor Gray
Write-Host "      手动关闭 PowerShell 窗口即可停止对应服务。" -ForegroundColor Gray
