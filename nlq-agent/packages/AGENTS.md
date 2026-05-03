<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# packages

## Purpose
跨语言共享代码包目录。当前仅包含 `shared-types/`，为 nlq-agent 与前端 (`web/`) / 移动端 (`mobile/`) 共享 TypeScript 类型契约的唯一真相源（SSOT）。

## Subdirectories
| Directory | Purpose |
|-----------|---------|
| `shared-types/` | TypeScript 类型定义包，含 reasoning-protocol（推理链协议） |

## For AI Agents

### Working in this directory
- 此目录不直接被 Python 后端代码引用，但 `src/models/schemas.py` 中的 Pydantic 模型必须与之保持字段同步。
- 修改 reasoning-protocol 后必须同步：(1) `src/models/schemas.py` 的 `ReasoningStepKind` 枚举与 `ReasoningStep`；(2) `web/src/types/reasoning-protocol.d.ts`（由 SSOT 派生）。
- 不要在此目录添加 Python 代码；它是 TypeScript-only 的契约层。

### Common patterns
- 类型定义使用 `export interface` / `export type`，配 JSDoc 注释。
- 文件头部明确标记 SSOT 与同步检查脚本路径。

## Dependencies
### Internal
- 同步目标：`web/src/types/reasoning-protocol.d.ts`、`src/models/schemas.py`
- 同步检查：`scripts/check-reasoning-protocol-sync.ps1`（仓库根）

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
