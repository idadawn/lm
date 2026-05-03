<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# monacoEditor

## Purpose
Monaco-based editor variant for richer language services (TypeScript/JS/HTML/CSS) where CodeMirror is insufficient. Exposed as `MonacoEditor` from the package barrel.

## Key Files
| File | Description |
|------|-------------|
| `MonacoEditor.vue` | Vue wrapper around the `monaco-editor` instance; bridges `value`, `language`, `theme`, `readonly` props. |
| `monacoEditorType.ts` | Type aliases for languages, themes, and editor options. |

## For AI Agents

### Working in this directory
- Mind bundle size — Monaco is large. Lazy-import only on routes that need it.
- Dispose the model + editor in `onBeforeUnmount` to free workers.

### Common patterns
- ESM dynamic import of `monaco-editor`.

## Dependencies
### External
- `monaco-editor`.

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
