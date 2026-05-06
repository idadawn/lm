"""知识图谱 API 测试脚本.

运行此脚本测试知识图谱功能是否正常工作。

用法:
    cd services/agent-api
    uv run python scripts/test_kg_api.py
"""

import asyncio
import sys

import httpx


# API 基础 URL
API_BASE = "http://localhost:8000/api/v1/kg"


async def test_health():
    """测试健康检查."""
    print("\n🔍 测试健康检查...")
    async with httpx.AsyncClient() as client:
        response = await client.get(f"{API_BASE}/health")
        result = response.json()
        print(f"   状态: {'✅ 就绪' if result['ready'] else '❌ 未初始化'}")
        print(f"   后端: {result['backend']}")
        return result["ready"]


async def test_refresh():
    """测试刷新知识图谱."""
    print("\n🔄 测试刷新知识图谱...")
    async with httpx.AsyncClient(timeout=60.0) as client:
        response = await client.post(f"{API_BASE}/refresh")
        result = response.json()
        print(f"   ✅ {result['message']}")


async def test_specs():
    """测试获取产品规格."""
    print("\n📦 测试获取产品规格...")
    async with httpx.AsyncClient() as client:
        response = await client.get(f"{API_BASE}/specs")
        specs = response.json()
        print(f"   ✅ 找到 {len(specs)} 个产品规格")

        if specs:
            # 显示前 3 个规格
            for spec in specs[:3]:
                print(f"      - {spec['code']}: {spec['name']}")

            # 测试获取规格详情
            first_spec = specs[0]
            print(f"\n📋 测试获取规格详情 ({first_spec['code']})...")
            response = await client.get(f"{API_BASE}/specs/{first_spec['code']}")
            detail = response.json()
            print(f"   ✅ 属性: {len(detail['attributes'])} 个")
            print(f"   ✅ 判定类型: {len(detail['judgment_types'])} 个")

            # 测试获取规格判定规则
            print(f"\n📜 测试获取判定规则 ({first_spec['code']})...")
            response = await client.get(f"{API_BASE}/specs/{first_spec['code']}/rules")
            rules = response.json()
            print(f"   ✅ 找到 {len(rules)} 条判定规则")


async def test_metrics():
    """测试获取指标公式."""
    print("\n📊 测试获取指标公式...")
    async with httpx.AsyncClient() as client:
        response = await client.get(f"{API_BASE}/metrics")
        metrics = response.json()
        print(f"   ✅ 找到 {len(metrics)} 个指标公式")

        if metrics:
            # 显示前 3 个指标
            for metric in metrics[:3]:
                print(f"      - {metric['name']}: {metric['columnName']}")


async def test_config():
    """测试获取报表配置."""
    print("\n⚙️  测试获取报表配置...")
    async with httpx.AsyncClient() as client:
        response = await client.get(f"{API_BASE}/first-inspection/config")
        config = response.json()
        print(f"   ✅ 合格等级: {', '.join(config['grades'])}")
        if config.get("description"):
            print(f"   说明: {config['description']}")


async def test_search():
    """测试规则搜索."""
    print("\n🔎 测试规则搜索 (关键词: 带)...")
    async with httpx.AsyncClient() as client:
        response = await client.get(f"{API_BASE}/rules/search?keyword=带")
        rules = response.json()
        print(f"   ✅ 找到 {len(rules)} 条相关规则")
        if rules:
            for rule in rules[:3]:
                print(f"      - {rule['name']} ({rule.get('spec_code', 'N/A')})")


async def main():
    """运行所有测试."""
    print("=" * 60)
    print("🧪 知识图谱 API 测试")
    print("=" * 60)

    try:
        # 1. 健康检查
        is_ready = await test_health()

        if not is_ready:
            print("\n⚠️  知识图谱未初始化，正在初始化...")
            await test_refresh()

        # 2. 测试产品规格
        await test_specs()

        # 3. 测试指标公式
        await test_metrics()

        # 4. 测试报表配置
        await test_config()

        # 5. 测试规则搜索
        await test_search()

        print("\n" + "=" * 60)
        print("✅ 所有测试完成！")
        print("=" * 60)
        print("\n💡 现在可以访问前端页面:")
        print("   - 聊天页面: http://localhost:3000")
        print("   - 知识图谱: http://localhost:3000/kg")

    except httpx.ConnectError:
        print("\n❌ 错误: 无法连接到 API 服务器")
        print("   请确保后端服务正在运行:")
        print("   cd services/agent-api")
        print("   uv run uvicorn app.main:app --reload --port 8000")
        sys.exit(1)
    except Exception as e:
        print(f"\n❌ 测试失败: {e}")
        import traceback

        traceback.print_exc()
        sys.exit(1)


if __name__ == "__main__":
    asyncio.run(main())
