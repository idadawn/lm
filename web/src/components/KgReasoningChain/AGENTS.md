<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# KgReasoningChain

## Purpose
知识图谱推理链可视化组件。在检测结果界面以折叠面板（`a-collapse`）形式展示「命中记录 → 产品规格 → 判定规则 → 条件评估 → 最终结论 / 降级」的推理步骤序列，是 NLQ Agent 与判定引擎结果回放的关键 UI。

## Key Files
| File | Description |
|------|-------------|
| `index.vue` | 单文件组件；接受 `steps: ReasoningStep[]` 与 `defaultOpen`，对每个 `kind` 渲染中文标签与颜色（`record/spec/rule/condition/grade/fallback`），`condition` 步骤额外展示「期望/实际/满足/不满足」。 |

## For AI Agents

### Working in this directory
- `ReasoningStep` / `ReasoningStepKind` 类型来自 `/@/types/reasoning-protocol`，新增 kind 必须先扩展该类型以及 `KIND_LABEL_MAP` / `KIND_COLOR_MAP`。
- 步骤索引由组件自动生成（`index + 1`），后端不要再下发显式编号。
- 该组件无需 `withInstall`，外部以 `import KgReasoningChain from '/@/components/KgReasoningChain/index.vue'` 直接使用。

### Common patterns
- 通过 `withDefaults(defineProps<Props>(), {...})` 声明默认值。
- 使用 BEM 风格类名（`kg-reasoning-chain__row` 等），样式 `scoped`。

## Dependencies
### Internal
- `/@/types/reasoning-protocol`
### External
- `ant-design-vue`（`Collapse`、`Tag`）

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
