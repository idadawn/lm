<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-29 | Updated: 2026-04-29 -->

# scripts

## Purpose
运维 / 验证类脚本。当前只有 `test_kg_api.py` —— 拉真实运行的 agent-api（默认 `http://localhost:8000`）跑一遍 KG REST 烟雾测试：health → refresh → specs → metrics → first-inspection config → rules search。

## Key Files

| File | Description |
|------|-------------|
| `test_kg_api.py` | 异步 httpx 脚本，依次测六个 KG 端点；找不到服务则打印中文错误提示并 exit 1 |

## For AI Agents

### Working In This Directory
- 脚本是命令行工具（不是 pytest），通过 `cd services/agent-api && uv run python scripts/<file>.py` 运行。
- 因此不要把这里的脚本写成 `def test_xxx`（会被 pytest 误抓）。如要 pytest 化请挪到 `tests/`。
- 新脚本必须是自包含的：用 `httpx.AsyncClient(timeout=...)` + `asyncio.run(main())`，明确 `API_BASE` 常量。

### Testing Requirements
- 脚本本身无单测，靠人工运行验证。
- 改 KG REST schema 后建议跑一遍此脚本作为快速回归。

### Common Patterns
- 中文进度提示用 emoji 标识阶段（🔍 / 🔄 / 📦 / 📊 / ⚙️ / 🔎），失败用 ❌，成功用 ✅。
- 错误处理：`httpx.ConnectError` → 友好提示先启 `uv run uvicorn app.main:app --reload --port 8000`，其它异常打 traceback。

## Dependencies

### Internal
- 无 — 脚本通过 HTTP 黑盒调用 agent-api。

### External
- `httpx>=0.27`（已在 `pyproject.toml`）。
