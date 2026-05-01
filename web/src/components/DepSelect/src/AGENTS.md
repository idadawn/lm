<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# src

## Purpose
Single SFC powering the `DepSelect` component.

## Key Files
| File | Description |
|------|-------------|
| `index.vue` | `<a-select mode="multiple" show-search>` with `fieldNames={ label: 'fieldName', value: 'field' }`. Watches `dataArr`/`checkedArr` props; prepends `{ field: 'all', fieldName: '全部' }`; emits `depSelectEmits` and `depSelectItemEmits` on change. Includes case-insensitive `filterOption`. |

## For AI Agents

### Working in this directory
- Component is `<script setup>` — keep `defineProps`/`defineEmits` declarative form, no `defineComponent` rewrite.
- "全部" handling expands the selection to every row in `dataArr`; if you change the sentinel, update `targetWeiduChange` to match.

### Common patterns
- All UI strings are Chinese (e.g. `请选择`, `全部`); preserve when editing.

## Dependencies
### External
- `ant-design-vue`, `vue`.

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
