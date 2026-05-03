<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# components

## Purpose
Shared workflow UI primitives consumed by every flow page — flow parser, comment/record widgets, sub-flow renderer, plus a family of approval/candidate/error modals.

## Key Files
| File | Description |
|------|-------------|
| `FlowParser.vue` | Full-screen `BasicPopup` that parses a flow config and orchestrates approve/reject/转办 actions. |
| `NodeFormParser.vue` | Renders the form for the current node. |
| `SubFlowParser.vue` | Sub-process detail viewer. |
| `FlowSelect.vue` | Picker for available flow templates. |
| `RecordList.vue` | Audit-log/审批记录 list. |
| `RecordSummary.vue` | Compact summary of records. |
| `Comment.vue` | Comment input + thread component. |

## Subdirectories
| Directory | Purpose |
|-----------|---------|
| `modal/` | Approval/comment/candidate modals (see `modal/AGENTS.md`). |
| `style/` | Shared `.basic-flow-parser` styles (see `style/AGENTS.md`). |

## For AI Agents

### Working in this directory
- `FlowParser` is the single source of truth for opType handling — extend by emitting events, not by copying the file.
- Keep `flowUrgent` and 状态 enums consistent with the values used by parent index.vue files.
