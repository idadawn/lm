<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# ContextMenu

## Purpose
Imperative right-click menu component. Exports `createContextMenu` / `destroyContextMenu` factory functions used to programmatically attach a floating Ant Design menu at a given mouse position — typically wired to `@contextmenu` handlers on tables, trees and tabs.

## Key Files
| File | Description |
|------|-------------|
| `index.ts` | Barrel export — re-exports the factory and `typing` (ContextMenuItem, Axis, options). |

## Subdirectories
| Directory | Purpose |
|-----------|---------|
| `src/` | Component implementation, factory and types (see `src/AGENTS.md`). |

## For AI Agents

### Working in this directory
- Do not convert to a declarative `<ContextMenu>` SFC consumer pattern — the project relies on the imperative factory style.
- Items must conform to `ContextMenuItem` (label, handler, icon, disabled, hidden, divider, children).

### Common patterns
- Caller pattern: `createContextMenu({ event, items: [{ label, icon, handler }] })`; the factory auto-attaches to `document.body` and removes itself on body click/scroll.

## Dependencies
### Internal
- `/@/components/Icon`, `/@/utils/is`.
### External
- `ant-design-vue` (Menu, Divider), `vue`.

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
