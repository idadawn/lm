<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# FlowChart

## Purpose
Generic graph editor built on `@logicflow/core` and `@logicflow/extension`. Provides a draggable canvas with toolbar (zoom/snapshot/preview), BPMN element conversion via Turbo adapter, and a default DnD palette of start/user-task/system-task/diamond/end shapes. Used for visual configuration of pipelines and BPMN-style models.

## Key Files
| File | Description |
|------|-------------|
| `index.ts` | Default-exports the `FlowChart` component. |

## Subdirectories
| Directory | Purpose |
|-----------|---------|
| `src/` | LogicFlow component, toolbar, dnd config, adapter (see `src/AGENTS.md`). |

## For AI Agents

### Working in this directory
- This is the *generic* graph editor — for the laboratory approval workflow use the sibling `FlowProcess` component instead.
- The dark-mode background is hard-coded (`#f7f9ff` light / `#151515` dark) via `useAppStore.getDarkMode`.
- Plugins enabled by default: `Snapshot`, `BpmnElement`, `Menu`, `DndPanel`, `SelectionSelect`. Add new ones via `LogicFlow.use(...)` inside `init()`.

### Common patterns
- Graph data is set declaratively through `data` prop; mutations trigger `onRender → lf.render(toLogicFlowData(data))`.

## Dependencies
### Internal
- `/@/store/modules/app`, `/@/components/Modal`, `/@/components/CodeEditor` (JsonPreview), `/@/hooks/web/useDesign`.
### External
- `@logicflow/core`, `@logicflow/extension`, `vue`.

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
