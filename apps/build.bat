@echo off
REM ============================================
REM Docker 镜像构建脚本 (Windows)
REM 使用 host 网络模式和详细输出
REM ============================================

setlocal enabledelayedexpansion

REM 读取环境变量
for /f "tokens=*" %%a in ('type .env ^| findstr /v "^#"') do set %%a

REM 设置默认值
if "%IMAGE_TAG%"=="" set IMAGE_TAG=latest
if "%API_IMAGE%"=="" set API_IMAGE=lm-api
if "%WEB_IMAGE%"=="" set WEB_IMAGE=lm-web

echo [INFO] 开始构建镜像...
echo [INFO] 版本: %IMAGE_TAG%

REM 构建 API 镜像
echo [INFO] 正在构建 API 镜像: %API_IMAGE%:%IMAGE_TAG%
docker build ^
    --progress=plain ^
    --network=host ^
    -f Dockerfile ^
    -t %API_IMAGE%:%IMAGE_TAG% ^
    .

if errorlevel 1 (
    echo [ERROR] API 镜像构建失败
    exit /b 1
)

echo [SUCCESS] API 镜像构建完成

REM 构建 Web 镜像
echo [INFO] 正在构建 Web 镜像: %WEB_IMAGE%:%IMAGE_TAG%
docker build ^
    --progress=plain ^
    --network=host ^
    -f web/Dockerfile ^
    -t %WEB_IMAGE%:%IMAGE_TAG% ^
    .

if errorlevel 1 (
    echo [ERROR] Web 镜像构建失败
    exit /b 1
)

echo [SUCCESS] Web 镜像构建完成
echo [SUCCESS] 所有镜像构建完成！

echo.
echo 镜像列表:
docker images | findstr /C:"lm-api" /C:"lm-web"
