<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# commonWords

## Purpose
常用语 (Common Words) 管理：维护各应用模块的常用文本片段（用于聊天/工单/快捷输入等），按"所属应用"分组，支持启用禁用、排序、CRUD。

## Key Files
| File | Description |
|------|-------------|
| `index.vue` | 列表页：`getCommonWordsList` + `delCommonWords`，列含 `systemNames`（所属应用）、`commonWordsText`、`sortCode` |
| `Form.vue` | 常用语编辑表单 |

## For AI Agents

### Working in this directory
- `systemNames` 是计算字段（多个应用名拼接），不要在 Form 中直接绑定。
- `defineOptions({ name: 'commonWords' })`。

### Common patterns
- 标准 `BasicTable` + `useModal` + `useTable` CRUD 模板。

## Dependencies
### Internal
- `/@/api/system/commonWords`、`/@/components/Table`、`/@/components/Modal`

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
