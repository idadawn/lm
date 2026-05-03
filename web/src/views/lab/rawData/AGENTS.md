<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# rawData

## Purpose
"原始数据"页面：以 Tabs 展示有效检测数据 / 全量原始数据，支持自定义排序、产品规格跳转、Excel 步骤导入向导（StepImportWizard）、单步快捷导入、导入历史。

## Key Files
| File | Description |
|------|-------------|
| `index.vue` | 主页（推荐版）：检测数据/原始数据双 Tabs + 自定义排序 + 详情跳转 |
| `index_enhanced.vue` | 增强版（实验/备份），勿引用 |
| `StepImportWizard.vue` | 多步骤导入向导容器 |
| `ImportModal.vue` | 单步快捷导入弹窗 |
| `ImportHistory.vue` | 导入历史记录列表 |

## Subdirectories
| Directory | Purpose |
|-----------|---------|
| `components/` | 排序控件、向导步骤、产品规格弹窗、特征选择 (see `components/AGENTS.md`) |

## For AI Agents

### Working in this directory
- `formatToDateTime` 用于 `creatorTime` 格式化；`column.key.startsWith('detection')` 通配检测列。
- "原始数据"按 Excel 顺序保留全部行（含无效），用 `isValidData===1` 区分。
- 不要把通用校验逻辑写进 `index.vue`；放到向导步骤组件里。

### Common patterns
- `BasicTable @register="registerTable"` + `registerRawTable` 两份分别绑两个 Tab。

## Dependencies
### Internal
- `/@/api/lab/rawData`, `/@/api/lab/product`
- `/@/utils/dateUtil`
### External
- `ant-design-vue`

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
