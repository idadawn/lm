<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# src

## Purpose
Implementation of the enhanced button family. `BasicButton` extends Ant's `Button` (registered as `AButton`) and adds icon slot wiring; `PopConfirmButton` and `ModelConfirmButton` wrap it with confirmation flows.

## Key Files
| File | Description |
|------|-------------|
| `BasicButton.vue` | Template binds `getBindValue` and `getButtonClass`; `extends: Button`, `inheritAttrs: false`; supports `preIcon` / `postIcon`. |
| `PopConfirmButton.vue` | Wraps `BasicButton` in an Ant `Popconfirm`. |
| `ModelConfirmButton.vue` | Triggers a `Modal.confirm(...)` with configured title / okType / content. |
| `props.ts` | Shared `buttonProps` (color, type, loading, preIcon, postIcon, iconSize, ...). |

## For AI Agents

### Working in this directory
- Keep the registered `name: 'AButton'` — the global registration in `web/src/components/registerGlobComp.ts` relies on it.
- Use `useAttrs` (from `/@/hooks/core/useAttrs`) when forwarding `$attrs` to the underlying Ant `Button`.

### Common patterns
- `defineProps(buttonProps)` + computed `getBindValue` merging attrs and props.

## Dependencies
### Internal
- `/@/hooks/core/useAttrs`.
### External
- `ant-design-vue` (`Button`, `Popconfirm`, `Modal`).

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
