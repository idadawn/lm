<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# src

## Purpose
Implementation of the `Authority` slot-gating component. Reads a `value` prop (string/number/array of permission codes or roles) and renders its default slot only when `usePermission().hasColumnP(value)` returns true.

## Key Files
| File | Description |
|------|-------------|
| `Authority.vue` | `defineComponent({ name: 'Authority' })` with a `renderAuth()` function used in the render return. Empty value passes through. |

## For AI Agents

### Working in this directory
- Keep this component logic-only — no styling. It returns either `getSlot(slots)` or `null`.
- If introducing new permission modes (e.g., column-level), extend `usePermission` rather than this file.

### Common patterns
- `defineComponent` + `setup` returning a render function.

## Dependencies
### Internal
- `/@/hooks/web/usePermission`, `/@/utils/helper/tsxHelper`.
### External
- `vue@3.3`.

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
