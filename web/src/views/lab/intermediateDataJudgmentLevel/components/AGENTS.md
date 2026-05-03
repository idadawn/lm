<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# components

## Purpose
判定等级页面专用组件：条件单元格、等级条件弹窗、批量复制弹窗、单条复制弹窗、表单。

## Key Files
| File | Description |
|------|-------------|
| `form.vue` | 等级新增/编辑表单（小写 form 命名，沿用旧约定）|
| `LevelConditionModal.vue` | 等级条件编辑弹窗（核心）|
| `ConditionCell.vue` | 表格内显示条件摘要的单元格 |
| `BatchCopyModal.vue` | 跨产品/规格批量复制等级条件 |
| `CopyConditionModal.vue` | 单条条件复制 |

## For AI Agents

### Working in this directory
- `form.vue` 名称小写为历史遗留，引用时大小写需准确（macOS 不敏感、Linux 严格）。
- 复制功能服务端事务保护，前端 emit `success` 后父页 reload。
- 条件 schema 与 `intermediateDataFormula/components/types.ts` 保持兼容。

### Common patterns
- `useModal` + `useForm`，emit `register`/`success`。

## Dependencies
### Internal
- `/@/api/lab/intermediateDataJudgmentLevel`
- `/@/components/Modal`, `/@/components/Form`
### External
- `ant-design-vue`

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
