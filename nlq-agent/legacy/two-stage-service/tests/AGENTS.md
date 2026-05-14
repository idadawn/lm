<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# tests

## Purpose
nlq-agent 的 pytest 测试套件。覆盖 SSE 事件序列化、Pydantic schemas 序列化/反序列化，以及两阶段 Pipeline 的集成测试（使用 mock 替代 LLM/Qdrant/MySQL 等外部依赖）。

## Key Files
| File | Description |
|------|-------------|
| `test_pipeline.py` | `TestSSEEmitter`、AgentContext / ReasoningStep 序列化、Stage1+Stage2 集成场景（statistical / conceptual / out_of_scope） |
| `__init__.py` | 子包标记 |

## For AI Agents

### Working in this directory
- 外部服务统一用 `unittest.mock.AsyncMock` / `MagicMock` 打桩；不要在测试中真连 Qdrant/MySQL/LLM。
- 新增端点或意图分支时必须补对应的集成测试，命名 `test_<intent>_<behavior>`。
- SSE 断言用"包含 `data:` 前缀 + 解析 JSON 后比对字段"两段式，避免硬编码完整字符串带来的脆弱性。
- 运行命令：`cd nlq-agent && pytest -q`；CI 环境必须保证零外部依赖。
- 测试 fixture 中的 DDL / 业务数据要与 `src/models/ddl.py` 真实列名一致，防止测试通过但生产报错。

### Common patterns
- pytest class 风格组织，`Test<Component>` 命名。
- 异步测试使用 `pytest.mark.asyncio`（依赖 `pytest-asyncio`）。

## Dependencies
### Internal
- `src/models/schemas.py`、`src/services/sse_emitter.py`、`src/pipelines/*`

### External
- `pytest`, `pytest-asyncio`, `unittest.mock`

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
