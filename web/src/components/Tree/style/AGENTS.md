<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# style

## Purpose
LESS theming for the `BasicTree` / `BasicLeftTree` components. Overrides `ant-tree` selected/hover backgrounds, switcher line-height, checkbox margins, and node-content layout to match the project's component theme.

## Key Files
| File | Description |
|------|-------------|
| `index.ts` | Imports `index.less` so the styles register when `Tree/index.ts` re-exports `'./style'`. |
| `index.less` | Defines `@tree-prefix-cls = ~'@{namespace}-tree'`, overrides for `.ant-tree-treenode`, `.ant-tree-switcher`, `.ant-tree-checkbox`, `.ant-tree-node-content-wrapper` plus `remove-active-tree` modifier. |

## For AI Agents

### Working in this directory
- Use the global `@namespace` LESS variable; do not hardcode the prefix.
- Selected/hover colors must come from theme variables (`@selected-hover-bg`, `@tree-node-selected-bg`, `@component-background`) so dark mode flips correctly.
- Keep `.ant-tree-title` absolutely positioned — `BasicTree` relies on it for the highlight overlay layout.

### Common patterns
- One root selector `.@{tree-prefix-cls}` containing nested overrides (BEM-friendly nesting).

## Dependencies
### External
- LESS theme variables provided globally by the build (no direct imports).

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
