<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# hooks

## Purpose
Composables that walk and mutate the tree's reactive data using the configured `FieldNames`. Used by `BasicTree.vue` to implement the `TreeActionType` surface (key collection, insert/delete/update by key, half-checked propagation, level filtering).

## Key Files
| File | Description |
|------|-------------|
| `useTree.ts` | `useTree(treeDataRef, getFieldNames)` returning helpers: `getAllKeys`, `getEnabledKeys`, `getChildrenKeys`, `getParentKeys`, `getCheckedKeys`, `insertNodeByKey`, `insertNodesByKey`, `deleteNodeByKey`, `updateNodeByKey`, `filterByLevel`, `getSelectedNode`. |

## For AI Agents

### Working in this directory
- All helpers respect dynamic `keyField`/`childrenField` from `FieldNames` — never hardcode `'id'` or `'children'`.
- Mutation helpers `cloneDeep` then mutate via `forEach` from `treeHelper`; they reassign `treeDataRef.value` to keep reactivity. Preserve this pattern when adding new mutators.
- Recursive helpers accept an optional `list` argument for sub-tree calls; keep that signature for testability.

### Common patterns
- Iterate with `for` index loops (not `forEach` from Array) so we can short-circuit when needed.
- `unref(getFieldNames)` is destructured at the top of each helper — avoid re-unwrapping inside loops.

## Dependencies
### Internal
- `../types/tree` (`InsertNodeParams`, `KeyType`, `FieldNames`, `TreeItem`)
- `/@/utils/helper/treeHelper` (`forEach`)

### External
- `vue` (`Ref`, `ComputedRef`, `unref`), `lodash-es` (`cloneDeep`)
- `ant-design-vue/es/tree/Tree` (`TreeDataItem` type)

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
