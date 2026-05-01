<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# codemirror

## Purpose
CodeMirror integration for the `CodeEditor`. Wraps a CodeMirror instance in a Vue component, syncing `value` ↔ doc, switching language modes, and toggling readonly state.

## Key Files
| File | Description |
|------|-------------|
| `CodeMirror.vue` | Vue wrapper exposing `value`, `mode`, `readonly` props; emits `change`. |
| `codeMirror.ts` | Sets up CodeMirror with addons (mode loading, theme, key bindings). |
| `codemirror.css` | Base CodeMirror stylesheet imported once. |

## For AI Agents

### Working in this directory
- When adding a new language mode, register it in `codeMirror.ts` and update the `MODE` enum in `../typing.ts`.
- Avoid leaking instances — destroy the editor in `onBeforeUnmount`.

### Common patterns
- DOM-mounted editor synced to a Vue ref.

## Dependencies
### External
- `codemirror`.

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
