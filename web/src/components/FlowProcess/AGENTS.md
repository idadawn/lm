<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# FlowProcess

## Purpose
Approval workflow designer (审批流) for the lab/工作流模块. Renders a tree of approval/condition/copy/timer/sub-flow/parallel/branch nodes from a JSON config, lets users add/edit/delete nodes inline, supports zoom (0–200%), preview mode (with completed/in-progress/无 status legend), and exposes a property panel for per-node configuration.

## Key Files
| File | Description |
|------|-------------|
| `index.ts` | Imports `style/index.less`, `style/propPanel.less`; exports `FlowProcess` (the `src/index.vue` host SFC). |

## Subdirectories
| Directory | Purpose |
|-----------|---------|
| `src/` | Host editor (`index.vue`), main canvas (`Main.vue`), property panel and helpers (see `src/AGENTS.md`). |
| `style/` | LESS files for the canvas and property panel — imported by `index.ts`. |

## For AI Agents

### Working in this directory
- This is the *business-specific* workflow designer; the generic graph editor lives in `FlowChart/`. Don't conflate the two.
- Node types: `start | approver | condition | copy | timer | subFlow | empty`, plus approver flags `isInterflow` (并行) and `isBranchFlow` (选择分支). Use `NodeUtils.is*Node` predicates from `helper/util.ts`.
- The component owns multiple flow tabs in `state.flowList`; `getData()` exposed via `defineExpose` validates each via `NodeUtils.checkAllNode` and rejects with the offending flow's name.

### Common patterns
- All configuration uses Chinese labels (流程名称/审批节点/条件分支/选择分支/并行分支/定时器/子流程).
- Templates pull defaults from `getMockData()` and merge per-flow `flowTemplateJson`.

## Dependencies
### Internal
- `/@/api/workFlow/formDesign` (getInfo), `/@/components/Modal`, `/@/components/Form`, `/@/utils/uuid` (buildBitUUID), `/@/hooks/web/useDesign`, `/@/hooks/web/useMessage`.
### External
- `vuedraggable`, `@ant-design/icons-vue`, `ant-design-vue`, `lodash-es`, `vue`.

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
