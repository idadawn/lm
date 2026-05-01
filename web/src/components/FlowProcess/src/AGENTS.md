<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# src

## Purpose
Workflow designer implementation. `index.vue` is the multi-flow host (left list of flows + center canvas + new/edit modal); `Main.vue` is the canvas wrapper with zoom/property-panel/legend; `flowCard/` recursively renders the node tree; `helper/` holds the data model (`NodeUtils`), default node configs, and option dictionaries.

## Key Files
| File | Description |
|------|-------------|
| `index.vue` | Multi-flow editor. Manages `flowList`, `activeConf`, `defaultData` (from `getMockData`), and a Form-driven add/edit modal (`fullName` validator forbids duplicates). `transformFieldList` flattens online-form fields, classifying `dataType` (number/array). `initFormOperates` builds per-node read/write/required permissions for `start`/`approver` nodes. |
| `Main.vue` | Canvas frame: scale slider (`scaleVal`, ±5 step, 0–200%), `FlowCard`, `PropPanel`, preview-mode legend. Re-renders by bumping `key` on every mutation. Forwards FlowCard events to `NodeUtils[event](...args)` and reopens prop panel on `edit`. |

## Subdirectories
| Directory | Purpose |
|-----------|---------|
| `flowCard/` | Recursive node renderer (see `flowCard/AGENTS.md`). |
| `helper/` | `NodeUtils`, default node configs, formula data, option dictionaries (see `helper/AGENTS.md`). |
| `propPanel/` | Right-side node property editor (forms per node type). |

## For AI Agents

### Working in this directory
- All canvas mutations go through static `NodeUtils.*` methods — never mutate `state.data` directly; that bypasses prevId fix-ups in `concatChild`.
- After every mutation Main.vue calls `forceUpdate()` (timestamp `key`); preserve when adding new event handlers.
- Property-panel edits roundtrip via `onPropEditConfirm(value, content)`; for condition nodes priority changes call `NodeUtils.resortPriorityByCNode` and `setDefaultCondition`.

### Common patterns
- `verifyMode` is enabled by `getData()`; nodes failing `NodeUtils.checkNode` show a red error tooltip in `flowCard`.

## Dependencies
### Internal
- `/@/api/workFlow/formDesign`, `/@/components/Modal`, `/@/components/Form`, `/@/utils/uuid`, `/@/hooks/web/useDesign`, `/@/hooks/web/useMessage`.
### External
- `vuedraggable`, `@ant-design/icons-vue`, `ant-design-vue`, `lodash-es`, `vue`.

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
