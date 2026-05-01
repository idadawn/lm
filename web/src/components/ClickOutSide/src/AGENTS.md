<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# src

## Purpose
Implementation of `ClickOutSide`. Renders a default slot and attaches a document-level mousedown listener; if the event target is not inside the wrapper element, emits `clickOutside` to the parent.

## Key Files
| File | Description |
|------|-------------|
| `ClickOutSide.vue` | Single-slot wrapper component; emits `clickOutside`. |

## For AI Agents

### Working in this directory
- Clean up the document listener in `onBeforeUnmount` to avoid leaks — keep this when refactoring.
- Do not stop event propagation here; consumers may rely on bubbling.

### Common patterns
- `defineEmits(['clickOutside'])`.

## Dependencies
### External
- `vue@3.3`.

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
