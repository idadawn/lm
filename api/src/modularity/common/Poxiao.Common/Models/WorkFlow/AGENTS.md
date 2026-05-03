<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# WorkFlow

## Purpose
Workflow engine domain models shared across the workflow module — describe form definitions, the workflow JSON template, and the runtime task-submit payload. These are the in-memory shapes that bridge the visual flow designer JSON with the engine.

## Key Files
| File | Description |
|------|-------------|
| `FlowFormModel.cs` | Form descriptor — `id`、`enCode`、`fullName`、`category`、`urlAddress`/`appUrlAddress`、`propertyJson` (form schema)、`flowType` (0:发起 / 1:功能)、`formType` (1:系统 / 2:自定义)、`tableJson`、`dbLinkId`、`interfaceUrl`、`draftJson`、`flowId`. |
| `FlowJsonModel.cs` | Stored flow template — `id`、`templateId`、`visibleType`、`version`、`flowTemplateJson`、`category`、`enCode`、`fullName`、`type`、`flowName`. |
| `FlowTaskSubmitModel.cs` | Task-submit payload (≈3 KB) — used when an end-user advances a workflow node. |

## For AI Agents

### Working in this directory
- Namespace `Poxiao.Infrastructure.Models.WorkFlow` (CamelCase 'F').
- All string properties are nullable (`string?`) — the workflow designer can save partially-complete drafts; preserve nullability.
- `propertyJson` / `flowTemplateJson` / `tableJson` / `draftJson` are raw JSON strings — never try to strongly-type them here, the workflow engine parses them at runtime.
- `flowType` and `formType` codes are documented inline; keep XML comments accurate when extending.
- `[SuppressSniffer]` not currently present on these — do **not** add it without checking whether the workflow module relies on auto-DI scanning of these types.

### Common patterns
- camelCase props; `int?`/`long?` for optional numerics.
- One model per file.

## Dependencies
### Internal
- Consumed by the workflow module engine and 表单设计器 services.
### External
- None.

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
