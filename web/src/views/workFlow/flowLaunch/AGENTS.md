<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# flowLaunch

## Purpose
我发起的 — flows the current user has launched. Lists all statuses with completion progress; supports 新建流程 via the `FlowPopup` picker.

## Key Files
| File | Description |
|------|-------------|
| `index.vue` | Launched-flows list with status, completion progress and FlowParser detail. |
| `FlowPopup.vue` | Selectable list of available flow templates to launch. |

## For AI Agents

### Working in this directory
- 委托 tag is rendered when `record.delegateUser` is truthy — keep this affordance; it's reused across multiple flow lists.
- Completion uses `a-progress` only when `0 < completion < 100`.
