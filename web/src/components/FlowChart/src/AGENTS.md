<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# src

## Purpose
LogicFlow editor components, toolbar actions, default DnD palette, and the Turbo→LogicFlow data adapter.

## Key Files
| File | Description |
|------|-------------|
| `FlowChart.vue` | Main canvas. Initializes `new LogicFlow({ container, ...defaults })` with grid, keyboard, themed background; provides graph context via `createFlowChartContext({ logicFlow })`; embeds `BasicModal` to preview JSON via `JsonPreview`. |
| `FlowChartToolbar.vue` | Toolbar buttons (zoom-in/out, fit, undo/redo, snapshot, preview). Emits `view-data` to parent. |
| `config.ts` | `nodeList`, `BpmnNode`, and `configDefaultDndPanel(lf)` — 6 base nodes (开始/矩形/用户/推送/位置/结束), 4 BPMN nodes, plus a 6-icon DnD palette (selection/start/user-task/system-task/diamond/end) with inlined PNG icons. |
| `adpterForTurbo.ts` | `toLogicFlowData(data)` — converts the Turbo flow JSON shape into LogicFlow `nodes`/`edges`. |
| `useFlowContext.ts` | `provide`/`inject` glue exposing the `LogicFlow` instance to children (toolbar). |
| `enum.ts` / `types.ts` | Node-type enums and shared TS types. |

## For AI Agents

### Working in this directory
- `setDefaultEdgeType('line')` is set at init — change centrally, not per-node.
- DnD icons are base64 PNGs embedded in `config.ts`; replace via `configDefaultDndPanel` rather than mutating exported constants.
- BPMN element rendering depends on `LogicFlow.use(BpmnElement)` being called before `setPatternItems`.

### Common patterns
- All extension styles imported once in `FlowChart.vue`: `@logicflow/core/dist/style/index.css`, `@logicflow/extension/lib/style/index.css`.

## Dependencies
### Internal
- `/@/store/modules/app`, `/@/components/Modal`, `/@/components/CodeEditor` (JsonPreview), `/@/hooks/web/useDesign`.
### External
- `@logicflow/core`, `@logicflow/extension` (Snapshot, BpmnElement, Menu, DndPanel, SelectionSelect), `vue`.

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
