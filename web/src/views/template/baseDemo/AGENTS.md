<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# baseDemo

## Purpose
Skeleton demo for a popup-driven editor page — registers a `Form.vue` popup and shows the typical `goChartEdtor`/`openFormPopup` pattern used by dashboard editors.

## Key Files
| File | Description |
|------|-------------|
| `index.vue` | Page shell with a single 编辑仪表盘 trigger that opens the form popup with a fixed `chartId1`. |
| `Form.vue` | Demo popup hosting the chart editor. |

## For AI Agents

### Working in this directory
- `defineOptions({ name: 'permission-organize' })` is a placeholder copied from another module — when adapting this demo, rename to match the new feature.
- Use `usePopup` (not `useModal`) to remain consistent with the rest of the codebase's drawer-style editors.
