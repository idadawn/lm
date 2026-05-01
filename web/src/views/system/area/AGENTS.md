<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# area

## Purpose
区域 (Area) 管理：维护省市区/自定义区域字典，支持启用禁用、排序、新建/编辑。常用于地址级联选择器的数据源。

## Key Files
| File | Description |
|------|-------------|
| `index.vue` | 列表页：`getAreaList` + `delArea`，列含名称/编码/排序/状态 |
| `Form.vue` | 新建/编辑表单（`BasicModal`） |

## For AI Agents

### Working in this directory
- `defineOptions({ name: 'system-task' })` 是 system 模块下复用的命名（与其他页冲突），keep-alive 行为需注意。
- 通过 `key = ref(+new Date())` 强刷新表格。

### Common patterns
- `BasicTable` + `useTable` + `useModal` 三件套。

## Dependencies
### Internal
- `/@/api/system/area`、`/@/components/Table`、`/@/components/Modal`

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
