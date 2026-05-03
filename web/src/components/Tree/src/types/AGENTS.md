<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# types

## Purpose
Single-file public type surface for the Tree components: `ToolbarEnum`, `treeProps` (built with `buildProps`), emit list, action interface, and supporting node/menu shapes.

## Key Files
| File | Description |
|------|-------------|
| `tree.ts` | `ToolbarEnum`, `TreeState`, `FieldNames`, `KeyType`, `CheckKeys`, `treeProps`/`treeEmits`, `ContextMenuItem`/`ContextMenuOptions`, `TreeItem`, `TreeActionItem`, `DropDownActionItem`, `selectKeysId`, `InsertNodeParams`, `TreeActionType`. |

## For AI Agents

### Working in this directory
- `treeProps` defaults `fieldNames` to `{ key: 'id', title: 'fullName', children: 'children' }` — matching backend DTOs (`F_Id` mapped to `id`, list endpoints returning `fullName`).
- The `filterFn`, `highlight`, `expandOnSearch`, `checkOnSearch`, `selectedOnSearch` props customize search behavior; document any new search prop here.
- `TreeActionType` is the imperative contract exposed by `BasicTree`; keep it in sync with `BasicTree.vue`'s `expose({...})`.

### Common patterns
- Props built with `/@/utils/props` `buildProps` so they're frozen/readonly at runtime.
- Chinese inline comments document the search-related props.

## Dependencies
### Internal
- `/@/utils/props` (`buildProps`)

### External
- `vue` (`ExtractPropTypes`, `PropType`)
- `ant-design-vue/es/tree/Tree` (`TreeDataItem`)

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
