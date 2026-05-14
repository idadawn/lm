<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# src (shared-types)

## Purpose
推理链协议（reasoning-protocol）类型定义源文件目录。定义 nlq-agent SSE 事件流中 `ReasoningStep` 节点的形状，前端 `<KgReasoningChain>` 组件据此渲染推理步骤。

## Key Files
| File | Description |
|------|-------------|
| `reasoning-protocol.ts` | `ReasoningStepKind` 联合类型 + `ReasoningStep` / `ReasoningChainEvent` 接口；六种步骤类型：record / spec / rule / condition / grade / fallback |

## For AI Agents

### Working in this directory
- 这是 reasoning-protocol 的唯一真相源；修改后必须同步 `web/src/types/reasoning-protocol.d.ts` 和 `nlq-agent/services/agent-api/app/models/schemas.py`。
- `condition` 类型的步骤具有 `expected` / `actual` / `satisfied` 字段——Stage 1 仅填 `expected`，Stage 2 查询后回填 `actual`/`satisfied`。
- `meta` 字段是任意键值对（`Record<string, unknown>`），用于扩展元数据（如 score、source）；优先在该字段扩展，避免顶层字段膨胀。

### Common patterns
- 字段使用可选标记 `?:` 表示阶段性可空；`actual` 类型为 `string | number`，反映 SQL 结果可能是数值或文本。
- 文件头部 JSDoc 声明 SSOT 与同步检查脚本路径。

## Dependencies
### Internal
- 同步目标：`web/src/types/reasoning-protocol.d.ts`、`nlq-agent/services/agent-api/app/models/schemas.py`
- 校验脚本：待迁移到 `services/agent-api/tests` 或仓库级工具目录；旧脚本若需要参考，可在 `legacy/two-stage-service/scripts` 中查找。

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
