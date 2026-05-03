<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# importAndExport

## Purpose
员工档案的批量导入 / 导出演示页面，展示 `BasicTable` + 自定义 Modal 的标准 CRUD + 导入导出工作流，是 `extend` 模块下批量数据处理的参考实现。

## Key Files
| File | Description |
|------|-------------|
| `index.vue` | 主页面：`BasicTable` 列出员工档案，提供“导出 / 导入”按钮，调用 `getEmployeeList` / `delEmployee` API；通过 `useModal` 注册导入导出弹窗。 |
| `ImportModal.vue` | 三步导入向导（上传文件 → 数据预览 → 导入数据），使用 `<a-steps>`、`<a-upload>`，限制 xls/xlsx，500KB，最多 1000 条。 |
| `ExportModal.vue` | 导出弹窗：选择字段并下载，配合 `getFetchParams` 复用表格筛选条件。 |

## For AI Agents

### Working in this directory
- 使用 `/@/api/extend/employee` 中的 `getEmployeeList` / `delEmployee`；新增字段时同步更新 `columns` 与表单 `schemas`。
- 日期字段（`birthday`、`attendWorkTime`、`graduationTime` 等）需在 `afterFetch` 中将空字符串转 `null`，避免 ant 表格 format 报错。
- 上传 action / headers 通过 `getAction`、`getHeaders` 计算，不要硬编码 token。

### Common patterns
- `useTable({ api, columns, useSearchForm, formConfig, actionColumn, afterFetch })`；`useModal` 双弹窗注册。
- 表头按钮使用 `icon-ym-btn-download` / `icon-ym-btn-upload` 图标 + `t('common.exportText'/'common.importText')`。

## Dependencies
### Internal
- `/@/api/extend/employee`、`/@/components/Table`、`/@/components/Modal`、`/@/hooks/web/useMessage`、`/@/hooks/web/useI18n`、`/@/store/modules/organize`
### External
- `ant-design-vue`（Steps、Upload、Button）
