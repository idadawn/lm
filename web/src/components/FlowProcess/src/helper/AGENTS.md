<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# helper

## Purpose
Pure-JS data model and dictionaries for the workflow designer. Owns the `NodeUtils` static class (CRUD, type guards, validation, branching maths), default node templates (`config.ts`), shared dictionary options (`define.ts`), and the formula DSL data used by condition expressions.

## Key Files
| File | Description |
|------|-------------|
| `util.ts` | `NodeUtils` static class. Type guards (`isStartNode/isApproverNode/isConditionNode/isCopyNode/isTimerNode/isSubFlowNode/isInterflowNode/isBranchFlowNode`). Tree ops: `getPreviousNode(prevId)`, `deleteNode(node, processData)` (handles empty-parent cleanup, branch collapse), plus add/append helpers, `checkNode`/`checkAllNode`, `resortPriorityByCNode`, `setDefaultCondition`, `createNode(type, prevId)` (clones from `config.ts` and assigns a `buildBitUUID()`). |
| `config.ts` | Default node templates keyed by type, including default `properties`, `nodeId` placeholders, and condition `priority`. |
| `define.ts` | Dictionaries: `defaultStep`, `typeOptions` (指定成员/发起者本人/...), `overTimeOptions`, `noticeOptions`, `systemFieldOptions` (`@flowId`, `@taskId`, `@launchUserName`, ...). |
| `formulaData.ts` | Operator/function tokens for condition formula expressions. |

## For AI Agents

### Working in this directory
- All tree mutations live here — keep `NodeUtils` static (no instance state); use `JSON.parse(JSON.stringify(...))` for deep cloning the configs (already pattern in `createNode`).
- `prevId` invariant: every child must reference its parent's `nodeId`. `concatChild` re-stitches `prevId` after deletion — preserve when extending.
- Empty-branch cleanup: when deleting a node whose parent is `type:'empty'`, recursion may delete the parent too if it would lose its last branch. Don't shortcut this.

### Common patterns
- System field placeholders use `@`-prefixed keys (`@flowId`, `@taskNodeId`); UI labels are Chinese.

## Dependencies
### Internal
- `/@/utils/uuid` (buildBitUUID), `./config` (default node templates).
### External
- None — pure TS.

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
