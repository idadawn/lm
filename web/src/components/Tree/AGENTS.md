<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# Tree

## Purpose
Vben-admin-style tree component wrappers around Ant Design Vue's `Tree`. Exposes `BasicTree` (full-featured tree with toolbar, search, context menu, async load) and `BasicLeftTree` (sidebar-style tree with header, debounced search input, used for left-panel navigation throughout the app).

## Key Files
| File | Description |
|------|-------------|
| `index.ts` | Public exports: `BasicTree`, `BasicLeftTree`, types from `src/types/tree`, plus `ContextMenuItem` re-export. |

## Subdirectories
| Directory | Purpose |
|-----------|---------|
| `src/` | Component implementations, hooks, and types (see `src/AGENTS.md`). |
| `style/` | LESS stylesheet `tree-prefix-cls` overrides for `ant-tree` (see `style/AGENTS.md`). |

## For AI Agents

### Working in this directory
- Default `fieldNames` are `{ key: 'id', title: 'fullName', children: 'children' }` aligned with backend DTO conventions; do not change without checking call sites.
- Trees are rendered through `ScrollContainer`; consumers must give a height-bound parent.
- Imperative actions are exposed via `TreeActionType` (`expandAll`, `setCheckedKeys`, `insertNodeByKey`, etc.) — interact via `ref`, not by mutating internal state.

### Common patterns
- BEM helper `createBEM('tree')` for class names.
- `useContextMenu` for right-click; `treeHelper` (`filter`, `treeToList`, `eachTree`) for traversal.
- Search supports highlight, expand-on-search, check-on-search, custom `filterFn`.

## Dependencies
### Internal
- `/@/components/Container` (ScrollContainer), `/@/components/Icon`, `/@/components/Basic`
- `/@/hooks/web/useContextMenu`, `/@/hooks/web/useI18n`
- `/@/utils/helper/treeHelper`, `/@/utils/bem`, `/@/utils/is`

### External
- `ant-design-vue` (Tree, Spin, Empty, Dropdown, Menu, InputSearch)
- `lodash-es`, `@vueuse/core`

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
