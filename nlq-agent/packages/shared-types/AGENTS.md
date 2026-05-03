<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# shared-types

## Purpose
TypeScript 共享类型包。当前承载 reasoning-protocol（推理链协议）的唯一真相源，供 Web/Mobile 前端和 nlq-agent 后端协同遵循同一份 SSE 事件契约。

## Subdirectories
| Directory | Purpose |
|-----------|---------|
| `src/` | TypeScript 源类型定义文件（见 `src/AGENTS.md`） |

## For AI Agents

### Working in this directory
- 这是无构建步骤的纯类型包；目前没有 `package.json`/`tsconfig.json`，依赖消费方按相对路径导入或手动复制至前端 `*.d.ts`。
- 增加新类型文件时，确保命名 kebab-case，文件头部声明 SSOT 标识。
- 任何 breaking change 都需要在 `src/models/schemas.py` 中同步实施，并通过 `pytest` 中的协议序列化用例验证。

### Common patterns
- 类型仅描述 SSE 事件载荷与推理步骤结构，避免业务逻辑。

## Dependencies
### Internal
- 消费方：`nlq-agent/src/models/schemas.py`、`web/src/types/reasoning-protocol.d.ts`、`mobile/utils/sse-client.js`

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
