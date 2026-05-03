<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# Design

## Purpose
Portal designer surface. `index.vue` is a fullscreen `BasicModal` that hosts the toolbar (undo/redo, save, save-and-publish, preview, PC/APP toggle) and embeds `PortalDesigner` plus the right-side property panel. Persists layout JSON via `/@/api/onlineDev/portal`.

## Key Files
| File | Description |
|------|-------------|
| `index.vue` | Designer entry — fullscreen modal, header actions, dispatches save / publish / preview, manages undo/redo stack. |

## Subdirectories
| Directory | Purpose |
|-----------|---------|
| `components/` | Designer UI: `PortalDesigner`, `Parser`, `AddBtn`, `RightPanel`, `Preview` (see `components/AGENTS.md`). |
| `helper/` | Static maps and lunar calendar lib (`componentMap`, `dataMap`, `calendar`) used by toolbox and demo data. |
| `hooks/` | Shared composables (`useCommon`, `useEChart`, `useTable`) used by runtime widgets. |
| `rightComponents/` | Property-panel sub-forms keyed by widget kind (see `rightComponents/AGENTS.md`). |

## For AI Agents

### Working in this directory
- Save flow has two buttons: `handleSubmit(1)` saves and publishes, `handleSubmit()` saves only — preserve both code paths.
- The `showType` toggle (`'pc' | 'app'`) drives both layout (grid vs draggable) and which property fields are visible — when adding a panel, gate with `v-if="showType == ..."` consistently.
- Header logo is `assets/images/zhichang.png`; do not relocate without updating both designer and preview.

### Common patterns
- BasicModal + `useModal` register/close pattern.
- Undo/redo via `getCanUndo`/`getCanRedo` flags exposed by `PortalDesigner` ref.

## Dependencies
### Internal
- `/@/components/Modal`, `/@/hooks/web/useI18n`
- `./components/*`, `./helper/*`, `./hooks/*`

### External
- `ant-design-vue`

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
