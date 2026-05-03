<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# DepSelectTag

## Purpose
Tag-style multi-select for `id`/`name` shaped data (organization tags, departments, simple lookup tables). Sibling of `DepSelect` but bound to a different schema and supports `v-model:checkedArr`.

## Key Files
| File | Description |
|------|-------------|
| `index.ts` | Default-exports `./src/index.vue`. |

## Subdirectories
| Directory | Purpose |
|-----------|---------|
| `src/` | Implementation SFC (see `src/AGENTS.md`). |

## For AI Agents

### Working in this directory
- Use this component when source rows have `id`/`name`; for `field`/`fieldName` rows use `DepSelect`.
- Emits both `depSelectEmitsTag`/`depSelectItemEmitsTag` and `update:checkedArr` so it works as a `v-model` target.

### Common patterns
- "全部" sentinel uses `id: 'all'`, `name: '全部'`.

## Dependencies
### External
- `ant-design-vue` (a-select), `vue`.

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
