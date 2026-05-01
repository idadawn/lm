<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# bigData

## Purpose
海量数据列表示例。展示 `BasicTable` + 服务端分页的大数据量加载方案，提供编码 / 名称 / 创建时间列以及关键字检索，并支持一键 mock 写入大批数据用于性能验证。

## Key Files
| File | Description |
|------|-------------|
| `index.vue` | 单页：`BasicTable` 绑定 `getBigDataList`，新建数据按钮调用 `createBigData` 批量造数 |

## For AI Agents

### Working in this directory
- 不要切到前端全量分页，列表必须保留服务端分页参数（与海量场景定位一致）。
- 列定义保持 `BasicColumn[]`；时间列使用 `format: 'date|YYYY-MM-DD HH:mm'` 公共格式化。
- 演示页造数 API 不要在生产环境暴露，新增按钮需在路由层挂权限。

### Common patterns
- `useTable({ api, columns, useSearchForm, formConfig })` 一行启用搜索表单 + 分页表格。
- `useI18n()` 关键字 placeholder 走 `common.keyword` / `common.enterKeyword`。

## Dependencies
### Internal
- `/@/api/extend/bigData`
- `/@/components/Table`
- `/@/hooks/web/{useMessage,useI18n}`

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
