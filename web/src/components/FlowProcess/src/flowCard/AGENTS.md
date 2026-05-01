<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# flowCard

## Purpose
Recursive renderer for the workflow tree. Walks a node and its `childNode` / `conditionNodes`, drawing per-type cards (start/approver/copy/timer/subFlow/condition/interflow/branchFlow), connector lines between branches, the "+" add-node popover, and the terminal "流程结束" badge.

## Key Files
| File | Description |
|------|-------------|
| `index.vue` | TSX component. `NodeFactory(data)` produces a self node + branch wrap + recursive child; `nodes` map renders each type via `createNormalCard`/condition card. `addNodeButton` exposes the 6 insertable node kinds in an `<a-popover>` (审批节点/子流程/条件分支/选择分支/并行分支/定时器), disabling options that conflict with the current parent. `addBranchButton` emits the appropriate append-branch event for the current branch container kind. All clicks bubble through `eventLauncher(event, ...args)` which strips Vue's MouseEvent injection and emits `emits` to the parent. `deleteNode` requires modal confirmation. |

## For AI Agents

### Working in this directory
- Component is TSX — use JSX expressions; do not migrate to a `<template>` form.
- Disabled-state classes (`condition-disabled`) follow the rule: only one branch type per parent, and `subFlow` parents reject new branches; respect `canAdd*` flags when extending.
- Property edits open through `eventLauncher('edit', conf)` — Main.vue handles propPanel toggling.
- Error indicator appears only when `verifyMode && NodeUtils.checkNode(data) === false`.

### Common patterns
- `stopPro(e)` is appended via `args[args.length-1]` as Vue injects MouseEvent last.
- Preview mode hides delete/add UI and shows a status-tinted card via `conf.state` class.

## Dependencies
### Internal
- `../helper/util` (NodeUtils), `/@/hooks/web/useMessage` (createConfirm).
### External
- `ant-design-vue` (a-input, a-popover, a-tooltip, a-tag, a-alert), `vue`.

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
