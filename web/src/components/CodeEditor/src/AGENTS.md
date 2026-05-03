<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# src

## Purpose
Top-level `CodeEditor` component (CodeMirror-based) plus shared typings. Auto-formats JSON via `JSON.stringify(JSON.parse(value), null, 2)` when `autoFormat` is true and `mode === MODE.JSON`, emitting `format-error` on parse failure.

## Key Files
| File | Description |
|------|-------------|
| `CodeEditor.vue` | Wraps `CodeMirrorEditor`; props: `value`, `mode` (validated against `MODE`), `readonly`, `autoFormat`. Exposes `insert`. |
| `typing.ts` | Exports the `MODE` enum (JSON, JS, HTML, ...). |

## Subdirectories
| Directory | Purpose |
|-----------|---------|
| `codemirror/` | Underlying CodeMirror Vue component + setup (see `codemirror/AGENTS.md`). |
| `json-preview/` | Read-only JSON view (see `json-preview/AGENTS.md`). |
| `monacoEditor/` | Monaco wrapper (see `monacoEditor/AGENTS.md`). |

## For AI Agents

### Working in this directory
- Keep mode validation in sync between `MODE` enum and the runtime `validator`.
- Emits `change`, `update:value` (v-model), and `format-error` — preserve this contract.

### Common patterns
- `<script setup>` + `defineExpose({ insert })`.

## Dependencies
### External
- `vue@3.3`.

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
