@echo off
chcp 65001 >nul
echo ========================================
echo   LM 开发环境启动脚本
echo ========================================
echo.

set "REPO_ROOT=%~dp0.."

echo [1/2] 启动 nlq-agent (port 18100)...
start "nlq-agent" cmd /k "cd /d %REPO_ROOT%\nlq-agent\services\agent-api && uv run uvicorn app.main:app --host 0.0.0.0 --port 18100 --reload"

timeout /t 3 /nobreak >nul

echo [2/2] 启动 Web 前端 (port 3102)...
start "web-dev" cmd /k "cd /d %REPO_ROOT%\web && pnpm dev"

timeout /t 3 /nobreak >nul

echo.
echo ========================================
echo   服务启动完成!
echo ========================================
echo   Web:       http://localhost:3102
echo   nlq-agent: http://localhost:18100
echo   Redis:     127.0.0.1:6380
echo   RabbitMQ:  127.0.0.1:8005
echo ========================================
echo.
echo 提示: 关闭本窗口不会停止已启动的服务。
echo       手动关闭 cmd 窗口即可停止对应服务。
pause
