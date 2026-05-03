<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# dynamicDictionary

## Purpose
Generic dictionary editor that adapts to whichever `typeId` is configured on the menu's `route.meta.relationId`. Lists, creates, edits and deletes dictionary entries — supports flat and tree shapes (toggled via `meta.isTree`). Persisted via `/@/api/systemData/dictionary` and synced into `baseStore` so other selects refresh.

## Key Files
| File | Description |
|------|-------------|
| `index.vue` | Page entry — `BasicTable` over `getDictionaryDataList`, treeTable mode when `searchInfo.isTree`, opens `Form` modal for add/edit, calls `delDictionaryData` then `baseStore.setDictionaryList()` to refresh |
| `Form.vue` | Add/edit modal — fields for name (`fullName`), code (`enCode`), parent, sort, status (`enabledMark`); validates code uniqueness before submit |

## For AI Agents

### Working in this directory
- After any mutation, call `baseStore.setDictionaryList()` so cached selects/dropdowns repopulate. Without it stale options linger across pages.
- `route.meta.relationId` (typeId) and `route.meta.isTree` are read in `onMounted` — accessing them earlier in setup will be undefined for some KeepAlive transitions.
- `pagination: false` is intentional for tree mode; keep that branch when refactoring.

### Common patterns
- `useTable({ isTreeTable, useSearchForm, formConfig })` from `/@/components/Table`.
- `defineOptions({ name: 'dynamic-dictionary' })` (note kebab-case here — keep it).

## Dependencies
### Internal
- `/@/api/systemData/dictionary` (`getDictionaryDataList`, `delDictionaryData`), `/@/components/{Table,Modal}`, `/@/store/modules/base`
### External
- `ant-design-vue` (`a-tag`), `vue-router`

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
