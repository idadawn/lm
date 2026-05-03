<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# src

## Purpose
Single SFC for the `DepSelectTag` component.

## Key Files
| File | Description |
|------|-------------|
| `index.vue` | `<a-select mode="multiple" show-search>` with `fieldNames={ label: 'name', value: 'id' }`. Watches `dataArr`/`checkedArr`; injects `{ id: 'all', name: '全部' }` head row; emits `update:checkedArr`, `depSelectEmitsTag`, `depSelectItemEmitsTag`. |

## For AI Agents

### Working in this directory
- Keep `<script setup>` form. The `update:checkedArr` emit is what enables `v-model:checkedArr` — do not drop it when refactoring.
- `filterOption` is case-insensitive on `name`; mirror the schema if you swap to a different label key.

### Common patterns
- "全部" expansion replaces the selection with every row's `id` plus the row objects in `targetCheckedArrWeiduItem`.

## Dependencies
### External
- `ant-design-vue`, `vue`.

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
