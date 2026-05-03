<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# tag

## Purpose
Tag (datapoint) management for collectors. Two tabs: 普通标签 (`tag.vue`) and 逻辑标签 (`tagLogic.vue`). Each provides CRUD via dynamic schema form, manual value override, and inline historical curve / table modals.

## Key Files
| File | Description |
|------|-------------|
| `index.vue` | Tab host (`普通标签` / `逻辑标签`); `defineOptions({ name: 'tagIndex' })` |
| `tag.vue` | 普通标签 list + add/edit modal (dynamic `z-temp-field` elements), 修改值 modal, 历史曲线 / 历史表格 modals embedding `echartsLine` and `historyTable` |
| `tagLogic.vue` | 逻辑标签 (computed/expression) variant with the same UX as `tag.vue` |

## Subdirectories
| Directory | Purpose |
|-----------|---------|
| `components/` | Inline modal contents — `echartsLineArea` curve and `historyTable` (see `components/AGENTS.md`) |

## For AI Agents

### Working in this directory
- `tag.vue` and `tagLogic.vue` are near-duplicates; if you fix a bug in one, audit the other (form schema, valueType decode, `state.history_visible*` modals, search column).
- `state.history_visible` opens the curve modal, `state.history_visible_table` opens the table modal. Don't combine them — distinct API endpoints.
- Manual value modal (`state.tag_visible`) uses a separate `formState_tag` — keep it independent from `formState`.

### Common patterns
- `defineOptions({ name: 'tagIndex' })` ensures KeepAlive caches the parent tabs.
- `:label-col="{span:4}" :wrapper-col="{span:16}"` per modal-form convention.

## Dependencies
### Internal
- `/@/api/collector` (tag CRUD endpoints), `/@/components/Table`, `./components/echartsLineArea`, `./components/historyTable`
### External
- `ant-design-vue`

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
