@echo off
chcp 65001 >nul
echo ========================================
echo 启动后端API (Dev环境)
echo ========================================
echo.

cd /d "%~dp0"

REM 切换到API项目目录
cd api\src\application\Poxiao.API.Entry

echo 当前目录: %CD%
echo.
echo 使用环境变量: ASPNETCORE_ENVIRONMENT=dev
echo 启动端口: http://*:10089
echo 热加载模式: 已启用 (使用 dotnet watch)
echo.
echo 提示: 修改代码后会自动重新编译和重启
echo.

REM 设置环境变量并运行（使用 watch 模式支持热加载）
set ASPNETCORE_ENVIRONMENT=dev
dotnet watch run --launch-profile dev

pause
