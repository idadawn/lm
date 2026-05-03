<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# Dropdown

## Purpose
Project-wide thin wrapper around `ant-design-vue` `Dropdown` + `Menu` that takes a `dropMenuList: DropMenu[]` and renders icons, dividers, popconfirm, and modal-confirm flows with one prop. Used heavily by table action columns.

## Key Files
| File | Description |
|------|-------------|
| `index.ts` | Re-exports the SFC and `DropMenu` typing. |

## Subdirectories
| Directory | Purpose |
|-----------|---------|
| `src/` | SFC + types (see `src/AGENTS.md`). |

## For AI Agents

### Working in this directory
- Configure menu items via `dropMenuList` (`event/text/icon/divider/disabled/onClick/popConfirm/modelConfirm`) — never hand-roll `<a-menu>` children.
- `popconfirm` prop toggles popover-confirm mode globally; per-item `modelConfirm` switches that single item to a `Modal.confirm` flow.
- Default trigger is `['contextmenu']` (right-click), not hover/click.

### Common patterns
- Emits `menuEvent` with the matched `DropMenu` row; callers usually `switch(menu.event)`.

## Dependencies
### Internal
- `/@/utils/is`, `/@/hooks/web/useMessage` (createConfirm).
### External
- `ant-design-vue` (Dropdown, Menu, Popconfirm), `lodash-es` (omit), `vue`.

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
