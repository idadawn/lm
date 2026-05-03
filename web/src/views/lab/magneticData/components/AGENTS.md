<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# components

## Purpose
磁性数据导入向导的步骤组件。两步走：上传/解析 → 复核/补全 → 完成入库。

## Key Files
| File | Description |
|------|-------------|
| `Step1UploadAndParse.vue` | Step1：选择 Excel → 上传 → 后端解析 → 预览 |
| `Step2ReviewAndComplete.vue` | Step2：复核解析结果，补全/修正后提交入库 |

## For AI Agents

### Working in this directory
- 步骤间通过父向导 emit/props 传递 `parsedData`；不要在子步骤里写父级状态。
- 提交前必须校验产品规格、关键字段非空。

### Common patterns
- `defineEmits(['next', 'prev', 'finish'])` 由父向导监听。

## Dependencies
### Internal
- `/@/api/lab/magneticData`
- `/@/components/Upload`
### External
- `ant-design-vue`

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
