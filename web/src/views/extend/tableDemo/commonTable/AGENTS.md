<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# commonTable

## Purpose
最基础的 `BasicTable` 示例：搜索栏 + 列表 + 操作列 + 关联 `commonForm` 弹窗的「项目」CRUD 模板。是 LIMS 前端开发新列表页的最小起步骨架。

## Key Files
| File | Description |
|------|-------------|
| `index.vue` | 注册 `useTable`、列定义、字典回填、新建/编辑/删除按钮、`useModal` 唤起共享 Form。 |

## For AI Agents

### Working in this directory
- 命名 `extend-tableDemo-commonTable`，保留 `defineOptions` 以利 keep-alive。
- Form 来源是 `../commonForm/index.vue`（**不要**复制一份到本目录）。
- `customRender` 用 `state.industryTypeList` 翻译 `projectType` 字段；字典从 `useBaseStore().getDictionaryData('IndustryType')`。

### Common patterns
- `useTable({ api: getTableList, columns, useSearchForm: true })`
- `getTableActions(record)` 返回查看/编辑/删除 ActionItem 数组

## Dependencies
### Internal
- `/@/components/Table`, `/@/components/Modal`, `/@/api/extend/table`, `/@/store/modules/base`, `../commonForm`

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
