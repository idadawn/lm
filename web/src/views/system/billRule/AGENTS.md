<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# billRule

## Purpose
单据规则 (Bill Rule) 管理：维护各类业务单据的编号生成规则（前缀、日期、流水位等），支持启用禁用、`.bb` 文件导入/导出。

## Key Files
| File | Description |
|------|-------------|
| `index.vue` | 列表页：`getBillRuleList` + `delBillRule` + `exportTpl`，含 `<jnpf-upload-btn>` 走 `/api/system/BillRule/Action/Import` |
| `Form.vue` | 规则编辑表单 |

## For AI Agents

### Working in this directory
- 文件扩展名 `.bb` 是单据规则导入专用，不要复用其他模块的扩展名（如 `.vdd`/`.vp`）。
- 复制按钮通过 `dropDownActions` 提供。

### Common patterns
- `useBaseStore` 提供分类列表 (`categoryList`) 用于过滤。

## Dependencies
### Internal
- `/@/api/system/billRule`、`/@/components/Table`、`/@/utils/file/download`

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
