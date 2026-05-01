<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# import-detail

## Purpose
原始数据"导入详情"弹窗内的四个 Tab 表：失败明细、成功明细、有效数据、操作日志。`ImportDetailModal.vue` 通过 Tab 切换展示。

## Key Files
| File | Description |
|------|-------------|
| `FailedDataTable.vue` | 导入失败行明细（错误原因列）|
| `SuccessDataTable.vue` | 成功导入行明细 |
| `ValidDataTable.vue` | 有效数据明细（可能与 success 不同：success=入库成功，valid=校验通过）|
| `OperationLogTable.vue` | 该次导入的操作日志 |

## For AI Agents

### Working in this directory
- 每个表接收 `importBatchId` 等 props，独立加载分页数据。
- 不要在此处实现全局错误处理；交给父 Modal。

### Common patterns
- `BasicTable` 受控分页 + `defineProps`。

## Dependencies
### Internal
- `/@/api/lab/rawData`, `/@/components/Table`
### External
- `ant-design-vue`

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
