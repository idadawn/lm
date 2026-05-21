#!/bin/bash
set -e

# 配置
SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
PACKAGE_OUTPUT_PATH="${SCRIPT_DIR}/nupkgs"
SOURCE_URL="http://localhost:7000/v3/index.json"
API_KEY="NUGET-SERVER-API-KEY"

echo "========================================="
echo "  Poxiao Framework Pack & Push Script"
echo "========================================="
echo ""

# 1. 清理旧包
echo "[1/4] 清理旧的 nupkg 文件..."
if [ -d "$PACKAGE_OUTPUT_PATH" ]; then
    rm -f "$PACKAGE_OUTPUT_PATH"/*.nupkg "$PACKAGE_OUTPUT_PATH"/*.snupkg
    echo "       已清理 $PACKAGE_OUTPUT_PATH"
else
    mkdir -p "$PACKAGE_OUTPUT_PATH"
    echo "       已创建 $PACKAGE_OUTPUT_PATH"
fi
echo ""

# 2. 查找 framework 下的所有类库项目（排除测试项目和备份项目）
echo "[2/4] 查找 framework 项目..."
PROJECTS=()
while IFS= read -r proj; do
    proj_name=$(basename "$proj")
    if [[ "$proj_name" != *Backup* && "$proj_name" != *Test* && "$proj" != *\tests\* ]]; then
        PROJECTS+=("$proj")
    fi
done < <(find "$SCRIPT_DIR" -maxdepth 2 -name "*.csproj" | sort)

if [ ${#PROJECTS[@]} -eq 0 ]; then
    echo "错误：未找到任何可打包的 csproj 文件。"
    exit 1
fi

echo "       发现 ${#PROJECTS[@]} 个项目："
for proj in "${PROJECTS[@]}"; do
    echo "         - $(basename "$proj")"
done
echo ""

# 3. 构建并打包（Release）
echo "[3/4] 构建 Release 并生成 NuGet 包..."
for proj in "${PROJECTS[@]}"; do
    echo -n "       构建 $(basename "$proj") ..."
    dotnet pack "$proj" -c Release --verbosity quiet
    echo " 完成"
done
echo ""

# 4. 检查生成的包并推送
echo "[4/4] 检查生成的包并推送..."
PACKAGES=($(find "$PACKAGE_OUTPUT_PATH" -maxdepth 1 -name "*.nupkg"))
if [ ${#PACKAGES[@]} -eq 0 ]; then
    echo "错误：未找到任何 .nupkg 文件，打包失败。"
    exit 1
fi

echo "       发现 ${#PACKAGES[@]} 个包，开始推送..."
for pkg in "${PACKAGES[@]}"; do
    echo -n "       推送 $(basename "$pkg") ..."
    if dotnet nuget push "$pkg" --source "$SOURCE_URL" --api-key "$API_KEY" --skip-duplicate > /dev/null 2>&1; then
        echo " 成功"
    else
        echo " 失败"
    fi
done

echo ""
echo "========================================="
echo "  完成！所有包已推送至私有 NuGet 服务器"
echo "  浏览地址: http://localhost:7000"
echo "========================================="
