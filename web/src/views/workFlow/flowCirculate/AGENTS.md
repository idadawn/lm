<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# flowCirculate

## Purpose
抄送 (cc'd) flow list — flows where the current user is a cc recipient. Read-only viewer that opens `FlowParser` for details.

## Key Files
| File | Description |
|------|-------------|
| `index.vue` | List with status tags and a single action that opens `FlowParser`. |

## For AI Agents

### Working in this directory
- Pure viewer — do not add 审批/退回 actions here; route those through `flowTodo`.
