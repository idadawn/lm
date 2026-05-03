<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# printSelect

## Purpose
"请选择打印模板" picker modal. Given a list of template IDs, fetches their metadata via `getPrintDevByIds`, displays a clickable list, and emits the selected template id to the caller. Used when a single business action has multiple available print templates.

## Key Files
| File | Description |
|------|-------------|
| `index.vue` | 400px-wide `BasicModal` (footer disabled); maps `state.printListOptions` to clickable `template-item` rows; emits `change(item.id)` and closes. |

## For AI Agents

### Working in this directory
- Init payload is `ids` passed via `useModalInner(init)`; loading state is toggled with `changeLoading` while the request is in flight.
- Pure visual styling lives in scoped LESS — keep `.template-item` class for compatibility with overrides.

### Common patterns
- Single-purpose modal pattern: register → init(args) → emit('change') → closeModal.

## Dependencies
### Internal
- `/@/components/Modal` (`BasicModal`, `useModalInner`), `/@/api/system/printDev` (`getPrintDevByIds`).
### External
- `ant-design-vue`.

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
