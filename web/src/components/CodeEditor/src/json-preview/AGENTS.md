<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# json-preview

## Purpose
Read-only JSON viewer used to display API payloads, form models, and schema dumps in modals and developer panels. Built on top of the CodeMirror editor in JSON mode with `readonly: true`.

## Key Files
| File | Description |
|------|-------------|
| `JsonPreview.vue` | Renders provided JSON object/string via `CodeEditor` in readonly + auto-format mode. |

## For AI Agents

### Working in this directory
- Pass either an object or a string for `value`; the component handles `JSON.stringify` formatting.
- Do not introduce write capability here — for editing, use `CodeEditor` directly.

### Common patterns
- Single-file readonly wrapper.

## Dependencies
### Internal
- `../CodeEditor.vue` (CodeMirror in JSON mode).

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
