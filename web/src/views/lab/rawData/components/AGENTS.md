<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# components

## Purpose
原始数据子组件：自定义排序控件、Excel 导入向导各步骤、产品规格/特征选择弹窗、数据复核弹窗。

## Key Files
| File | Description |
|------|-------------|
| `CustomSortControl.vue` | 自定义排序控件（受控）|
| `CustomSortControl-debug.vue` | 调试版本，勿引用 |
| `CustomSortEditor.vue` | 排序编辑器内嵌组件 |
| `SortSelector.vue` | 排序字段选择器 |
| `Step1UploadAndParse.vue` | 向导 Step1：上传解析 |
| `Step2DataPreview.vue` | 向导 Step2：数据预览 |
| `Step2ProductSpec.vue` | 向导 Step2 备选：选择产品规格 |
| `Step3AppearanceFeature.vue` | 向导 Step3：外观特征匹配 |
| `Step4ReviewAndComplete.vue` | 向导 Step4：复核入库 |
| `StepImportWizard.vue` | 步骤向导容器（与父级同名是历史遗留）|
| `ImportDetailModal.vue` | 导入详情弹窗（含失败/成功/有效/操作日志四 Tab）|
| `ProductSpecModal.vue` | 产品规格选择/查看弹窗 |
| `FeatureSelectDialog.vue` | 外观特征选择 |
| `DataReviewModal.vue` | 行级数据复核弹窗 |

## Subdirectories
| Directory | Purpose |
|-----------|---------|
| `import-detail/` | 导入详情各 Tab 表 (see `import-detail/AGENTS.md`) |

## For AI Agents

### Working in this directory
- 步骤组件通过父向导 props 接收 wizard 状态，emit `next/prev/finish`。
- 调试/旧版组件（`-debug`、与父级同名 wizard）保留但禁止新代码引用。

### Common patterns
- 步骤组件单文件 SFC，内部独立 `useForm`/`useTable`。

## Dependencies
### Internal
- `/@/api/lab/rawData`, `/@/api/lab/product`, `/@/api/lab/appearance`
### External
- `ant-design-vue`

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
