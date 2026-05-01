<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# complexHeader

## Purpose
演示「复杂表头/多级嵌套 columns」的 `BasicTable` 用法。布局与 commonTable 一致，但 `columns` 中通过 `children` 字段构造分组表头，常用于检测报告中按「相关费用 → 费用金额/已用/预计」之类二级表头的展示。

## Key Files
| File | Description |
|------|-------------|
| `index.vue` | 多级 columns 配置；其余 CRUD 流程沿用 commonForm + `/@/api/extend/table`。 |

## For AI Agents

### Working in this directory
- 修改 columns 时，子列宽度之和应与父列协调；导出列时注意嵌套展开。
- 表格名称 `extend-tableDemo-lockTable`（沿用模板，可视情况修正）。

### Common patterns
- `BasicColumn[]` 的 `children` 字段构造多级表头
- 共用 `commonForm` 与字典翻译 `customRender`

## Dependencies
### Internal
- `/@/components/Table`, `../commonForm`, `/@/api/extend/table`

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
