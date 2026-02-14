@echo off
REM ============================================
REM 计算 Worker (Poxiao.Lab.CalcWorker) 发布脚本
REM 输出目录: api/publish/CalcWorker
REM ============================================

setlocal enabledelayedexpansion

set "SCRIPT_DIR=%~dp0"
set "PROJECT_DIR=%SCRIPT_DIR%src\application\Poxiao.Lab.CalcWorker"
set "OUTPUT_DIR=%SCRIPT_DIR%publish\CalcWorker"
set "CONFIGURATION=Release"
if not "%1"=="" set "CONFIGURATION=%1"

echo [INFO] 发布计算 Worker 服务...
echo [INFO] 项目目录: %PROJECT_DIR%
echo [INFO] 输出目录: %OUTPUT_DIR%
echo [INFO] 配置: %CONFIGURATION%
echo.

if not exist "%PROJECT_DIR%\Poxiao.Lab.CalcWorker.csproj" (
    echo [ERROR] 未找到项目文件，请确保在 api 目录下执行此脚本
    exit /b 1
)

dotnet publish "%PROJECT_DIR%\Poxiao.Lab.CalcWorker.csproj" ^
    -c %CONFIGURATION% ^
    -o "%OUTPUT_DIR%" ^
    --no-self-contained

if errorlevel 1 (
    echo [ERROR] 发布失败
    exit /b 1
)

echo.
echo [SUCCESS] 计算 Worker 已发布到: %OUTPUT_DIR%
echo.
echo 运行方式:
echo   cd "%OUTPUT_DIR%"
echo   Poxiao.Lab.CalcWorker.exe
echo.
echo 或指定环境:
echo   set ASPNETCORE_ENVIRONMENT=Production
echo   Poxiao.Lab.CalcWorker.exe
echo.
exit /b 0
