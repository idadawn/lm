<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# style

## Purpose
LESS styles for `BasicColumnDesign`. Defines layout for head tabs, main canvas, and right config panel using the project namespace prefix and theme variables.

## Key Files
| File | Description |
|------|-------------|
| `index.less` | Class prefix `~'@{namespace}-basic-column-design'`; head-tabs (42px, absolute, right-aligned with 350px right gutter), `.ant-btn` overrides, hover states using `@primary-color`, borders via `@border-color-base1`. |

## For AI Agents

### Working in this directory
- Use existing LESS variables (`@namespace`, `@primary-color`, `@text-color`, `@component-background`, `@border-color-base1`) — do not hard-code colors.
- Keep selectors scoped under `.@{prefix-cls}` to prevent leakage.

### Common patterns
- BEM-ish nested LESS with `@prefix-cls` block.

## Dependencies
### External
- `less` (project-wide LESS variables defined in `web/src/design/`).

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
