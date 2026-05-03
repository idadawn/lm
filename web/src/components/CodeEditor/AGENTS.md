<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# CodeEditor

## Purpose
Code-editing component family. The default `CodeEditor` uses CodeMirror with auto-format support for JSON; a Monaco-based variant is exported for richer language tooling, and `JsonPreview` renders read-only formatted JSON.

## Key Files
| File | Description |
|------|-------------|
| `index.ts` | Exports `CodeEditor`, `MonacoEditor`, `JsonPreview` and re-exports types from `src/typing`. |

## Subdirectories
| Directory | Purpose |
|-----------|---------|
| `src/` | Top-level `CodeEditor.vue` + `typing.ts` (see `src/AGENTS.md`). |
| `src/codemirror/` | CodeMirror integration (see `src/codemirror/AGENTS.md`). |
| `src/json-preview/` | Read-only JSON viewer (see `src/json-preview/AGENTS.md`). |
| `src/monacoEditor/` | Monaco editor wrapper (see `src/monacoEditor/AGENTS.md`). |

## For AI Agents

### Working in this directory
- Use `MODE` from `typing.ts` (JSON/JS/HTML/...) when configuring the editor.
- For embedding-heavy bundles, prefer `CodeEditor` (CodeMirror) over `MonacoEditor`.

### Common patterns
- `withInstall` barrel + type re-export.

## Dependencies
### External
- `codemirror`, `monaco-editor`.

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
