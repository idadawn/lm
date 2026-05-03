<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# nlq-two-stage

## Purpose
OMC skill (level 4)：路美 nlq-agent **两阶段问答 Pipeline** 的总开发指南。Stage 1 = Semantic & KG Agent（Qdrant 检索 + reasoning_step 推送），Stage 2 = Data & SQL Agent（基于 AgentContext 生成只读 SQL 并回填 condition）。SSE 协议与前端 `web/src/api/nlqAgent.ts` 和 `mobile/utils/sse-client.js` 完全兼容。

## Key Files
| File | Description |
|------|-------------|
| `SKILL.md` | 项目结构图、SSE 事件协议、`AgentContext` 数据流、新增/调试两阶段功能的工作流 |

## For AI Agents

### Working in this directory
- 改动 SKILL 时严禁打破 SSE 协议（`reasoning_step` / `text` / `response_metadata` / `done` / `error` 五种事件）；前端解析依赖。
- 新增功能优先遵循"先扩 schemas → 改 prompt → 改 stage agent → 改 orchestrator → 加测试"五步法。
- Skill level 为 4，意味着调用此 skill 的任务通常需要修改 5+ 文件，工作前应先 `read` 全部相关模块再行动。
- 端口 18100 是固定契约；CORS 与 `Cache-Control: no-cache` 头是 SSE 必需，不要在示例中省略。
- 不要把 `mobile/types/reasoning-protocol.d.ts` 当作真值——上游真值在 `nlq-agent/packages/shared-types`。

### Common patterns
- SKILL 用 ASCII 项目树展示 `nlq-agent/src/` 布局，方便 Claude 快速定位。
- 协议字段名采用 snake_case，与 Python Pydantic 默认输出一致。

## Dependencies
### Internal
- 整个 `nlq-agent/src/` 目录（main / orchestrator / stage1 / stage2 / services / models / utils）
- 前端消费方：`web/src/api/nlqAgent.ts`、`mobile/utils/sse-client.js`、`mobile/components/kg-reasoning-chain/`

### External
- FastAPI、OpenAI SDK、aiomysql、qdrant-client、httpx

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
