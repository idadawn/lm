<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# error-log

## Purpose
Developer-facing runtime error log viewer. Displays Vue, resource, promise, and AJAX errors collected by `errorLogStore` (set up in `web/src/logics/error-handle`), with three buttons to deliberately fire each kind for testing. A detail modal expands a row's full stack trace.

## Key Files
| File | Description |
|------|-------------|
| `index.vue` | Page entry — wires `BasicTable` to `errorLogStore.getErrorLogInfoList`, exposes fire-error toolbar buttons, opens `DetailModal` |
| `data.tsx` | TSX column definitions; renders a colored `<Tag>` per `ErrorTypeEnum` (VUE/RESOURCE/PROMISE/AJAX) and exposes `getDescSchema()` for the detail panel |
| `DetailModal.vue` | Modal that displays the full error record fields |

## For AI Agents

### Working in this directory
- This page is only enabled when `useGlobSetting().useErrorHandle` is true and routes are registered via `web/src/router/routes/modules/error-log.ts`.
- `data.tsx` (not `.ts`) — Ant Design `<Tag>` is rendered via JSX in `customRender`. Keep `.tsx` extension.
- The fire-* buttons rely on the global error handler installed in `logics/error-handle`; do not refactor without verifying capture still works.

### Common patterns
- `watch` on `errorLogStore.getErrorLogInfoList` with `cloneDeep` + `setTableData` keeps the table immutable.
- `ErrorTypeEnum` lives in `/@/enums/exceptionEnum`.

## Dependencies
### Internal
- `/@/store/modules/errorLog`, `/@/components/Table` (`BasicTable`, `useTable`, `TableAction`), `/@/components/Modal`, `/@/hooks/web/useI18n`, `/@/enums/exceptionEnum`
### External
- `ant-design-vue` (`Tag`), `lodash-es` (`cloneDeep`)

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
