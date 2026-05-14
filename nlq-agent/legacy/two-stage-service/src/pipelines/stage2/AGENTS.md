<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# stage2

## Purpose
两阶段问答的 **Stage 2：Data & SQL Agent**。基于 Stage 1 输出的 `AgentContext` 与 DDL，生成只读 SQL 并在 MySQL 执行，失败时进入修正循环（最多 2 次重试），最终用 LLM 把结果合成自然语言回答，并回填 `condition` 步骤的 `actual` / `satisfied` 字段。

## Key Files
| File | Description |
|------|-------------|
| `data_sql_agent.py` | `DataSQLAgent.run()`：模板匹配 → LLM 生成 SQL（`STAGE2_SQL_GENERATION_*`）→ 安全检查 → 执行 → 失败重试（`STAGE2_SQL_CORRECTION_*`，`MAX_SQL_RETRIES=2`）→ `FINAL_ANSWER_*` 流式回答 |
| `__init__.py` | 子包标记 |

## For AI Agents

### Working in this directory
- 安全检查在 `services/database.py` 中通过 `_FORBIDDEN_PATTERNS` 拦截写操作；本层不要绕过。
- SQL 执行前必须注入 `LIMIT`（默认 200 行），保护 LLM context 与前端渲染性能。
- 修正循环上限 `MAX_SQL_RETRIES = 2`，超出后 emit `fallback` 并退出，不要无限重试。
- 回答阶段使用流式 LLM (`AsyncIterator`)，逐 chunk emit `text` SSE 事件；不要一次性 emit。
- `condition` 回填顺序：先匹配 step.field 与查询结果列名，再写入 `actual`，再依据 `expected` 比较算 `satisfied`。

### Common patterns
- 优先模板（`METRIC_SQL_TEMPLATES`）后 LLM，可减少 token 消耗与不稳定性。
- 错误信息回喂 LLM 时只保留第一行（避免 prompt 注入）。

## Dependencies
### Internal
- `src/services/database.py`、`llm_client.py`、`sse_emitter.py`
- `src/models/ddl.py`、`schemas.py`、`src/utils/prompts.py`

### External
- LLM、aiomysql

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
