<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# components

## Purpose
Designer-side Vue components: the grid editor, the dynamic widget renderer, the add-widget dropdown, the right property panel, and the preview modal.

## Key Files
| File | Description |
|------|-------------|
| `PortalDesigner.vue` | Main canvas — `grid-layout`/`grid-item` for PC, `draggable` for APP. Holds the active layout, click/move/resize handlers, copy/delete actions, undo/redo stack. |
| `Parser.vue` | Recursive renderer that maps `item.jnpfKey` to a runtime component (`HChart`, `HCard`, `HTab`, ...) using `componentMap`. Handles special `card` and `tab` containers with their own children/`add-btn`. |
| `AddBtn.vue` | Dropdown grouping `layoutComponents`/`systemComponents`/`basicComponents`/`chartComponents` for inserting a new widget. |
| `RightPanel.vue` | Tabbed property panel (`控件属性` / `门户属性`) — composes the per-widget panels from `../rightComponents` based on `activeData.jnpfKey` and chart-vs-non-chart heuristics. |
| `Preview.vue` | Fullscreen preview modal — switches between custom-page (component or iframe) and `PortalLayout` for grid layouts. |

## For AI Agents

### Working in this directory
- `noNeedMaskList` / `noNeedTypeSetList` / `chartList` are the magic id sets that drive UI gating — extend them when adding new widget kinds.
- `Parser.vue` is recursive (children inside `card`/`tab`); preserve `:detailed` and `:activeId` propagation.
- `PortalDesigner` exposes the undo/redo state to the parent modal — keep `defineExpose` contract stable.

### Common patterns
- `componentMap` provides icon/label/default option for each toolbox item.
- Inline icons use `icon-ym icon-ym-*` font.

## Dependencies
### Internal
- `../helper/componentMap`, `/@/components/Container`, `/@/components/Modal`
- `../../Portal/*` runtime components

### External
- `ant-design-vue`, `vue3-grid-layout`, `vuedraggable`

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
