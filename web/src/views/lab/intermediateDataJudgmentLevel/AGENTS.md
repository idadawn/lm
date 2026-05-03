<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# intermediateDataJudgmentLevel

## Purpose
"判定等级"页面：基于产品规格 + 判定项目（公式列）维护判定等级条件。左侧产品规格选择 + 判定项目列表，右侧编辑某项目的等级条件。

## Key Files
| File | Description |
|------|-------------|
| `index.vue` | 左右两栏布局：左侧产品规格 + 判定列，右侧条件编辑面板 |

## Subdirectories
| Directory | Purpose |
|-----------|---------|
| `components/` | 等级条件单元格、批量复制、复制条件、等级条件弹窗 (see `components/AGENTS.md`) |

## For AI Agents

### Working in this directory
- 必须先选 `selectedProductSpecId` 才能加载 `formulaList`，再选具体公式编辑等级条件。
- API：`/@/api/lab/intermediateDataJudgmentLevel`、`/@/api/lab/intermediateDataFormula`、产品规格列表。
- 与 `intermediateDataFormula` 强耦合（公式 → 判定等级）。

### Common patterns
- Tailwind + 左侧 240px 固定宽。
- `loadingFormula`/选中态切换样式。

## Dependencies
### Internal
- `/@/api/lab/intermediateDataJudgmentLevel`
- `/@/api/lab/intermediateDataFormula`
- `/@/api/lab/product`
### External
- `ant-design-vue`

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
