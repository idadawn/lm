<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# components

## Purpose
Sub-panels of the JNPF 列表设计器. Each `.vue` file is a focused editor for one aspect of column / list configuration (button events, conditional formatting, scripts, main canvas, app-mode preview, template upload).

## Key Files
| File | Description |
|------|-------------|
| `Main.vue` | Desktop main canvas — column drag & drop and preview. |
| `MainApp.vue` | Mobile-app variant of the main canvas. |
| `BtnEvent.vue` | Editor for row/toolbar button click handlers (`defaultBtnFunc` template). |
| `ConditionModal.vue` | Conditional row/cell style rule editor. |
| `FormScript.vue` | `afterOnload` / `rowStyle` / `cellStyle` script editor (uses `CodeEditor`). |
| `UpLoadTpl.vue` | Excel/print template upload + binding. |

## For AI Agents

### Working in this directory
- Script editors should use `/@/components/CodeEditor` so default templates from `helper/config.ts` (`defaultFuncsData`) keep their formatting.
- New sub-panels should accept the parent `columnData` via props and emit upward — avoid global state mutation.

### Common patterns
- Sub-panels embedded inside the right-config drawer of `BasicColumnDesign.vue`.

## Dependencies
### Internal
- `/@/components/CodeEditor`, `/@/components/FormGenerator`.
### External
- `ant-design-vue`.

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
