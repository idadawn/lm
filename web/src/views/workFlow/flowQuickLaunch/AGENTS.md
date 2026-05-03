<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# flowQuickLaunch

## Purpose
流程快捷发起 — landing page that shows a horizontal `FlowList` and immediately opens `FlowParser` in create mode (`opType: '-1'`) when an item is selected.

## Key Files
| File | Description |
|------|-------------|
| `index.vue` | `FlowList` + `FlowParser` wiring. |
| `FlowList.vue` | Compact horizontal/grid list of available flow templates. |

## For AI Agents

### Working in this directory
- Always pass `opType: '-1'` to `FlowParser` to indicate a brand-new flow instance.
- Keep `id: ''` empty on launch — backend assigns the new instance id.
