<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# src

## Purpose
Implementation of the `BasicTree` and `BasicLeftTree` components. Holds the TSX setup logic, the icon renderer, and child folders for headers, hooks, and types.

## Key Files
| File | Description |
|------|-------------|
| `BasicTree.vue` | Core tree (TSX) — wraps `a-tree`, manages `expandedKeys`/`selectedKeys`/`checkedKeys`, search filtering with highlight, async loadData, right-click context menu, exposes `TreeActionType`. |
| `BasicLeftTree.vue` | Sidebar variant (`<script setup>`) with `LeftTreeHeader`, debounced `InputSearch`, single-select handler, embeds `BasicTree`. |
| `TreeIcon.ts` | Functional component that renders a string-class `<i>` icon or delegates to `Icon` for object icons. |

## Subdirectories
| Directory | Purpose |
|-----------|---------|
| `components/` | `TreeHeader` and `LeftTreeHeader` toolbar/title strips (see `components/AGENTS.md`). |
| `hooks/` | `useTree` for key-set helpers (see `hooks/AGENTS.md`). |
| `types/` | Props, emits, enums, and action interfaces (see `types/AGENTS.md`). |

## For AI Agents

### Working in this directory
- `BasicTree.vue` uses `defineComponent` + TSX render in setup; preserve the `expose(...)` action surface — call sites depend on it.
- Field name `title` is internally rewired to `<title>_title` to allow custom slot rendering — keep this when editing `getBindValues`.
- When changing search behavior, also update `expandOnSearch` / `checkOnSearch` / `selectedOnSearch` flag handling together.

### Common patterns
- `reactive` state objects for tree state and search state.
- Emits: `update:expandedKeys`, `update:selectedKeys`, `update:value`, `change`, `check`, `update:searchValue`, `select`.
- BEM with `createBEM('tree')` and `createBEM('basic-left-tree')`.

## Dependencies
### Internal
- `./components/*`, `./hooks/useTree`, `./types/tree`, `./TreeIcon`
- `/@/components/Tree`, `/@/components/Container`, `/@/components/ContextMenu`
- `/@/utils/helper/{tsxHelper,treeHelper}`, `/@/utils/{bem,is}`

### External
- `vue`, `ant-design-vue`, `lodash-es`, `@vueuse/core`

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
