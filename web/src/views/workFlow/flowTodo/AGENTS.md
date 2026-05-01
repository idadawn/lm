<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# flowTodo

## Purpose
待办 — tasks awaiting the current user's approval. Supports 批量审批 via a separate batch list.

## Key Files
| File | Description |
|------|-------------|
| `index.vue` | Todo list with status tags, completion progress and per-row approval. |
| `BatchList.vue` | Multi-select batch approval dialog/popup. |

## For AI Agents

### Working in this directory
- Action column should drive single-flow approval through `FlowParser`; batch approval is intentionally separated.
- `record.delegateUser` 委托 tag is shown identically to `flowLaunch`.
