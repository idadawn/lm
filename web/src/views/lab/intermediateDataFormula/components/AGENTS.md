<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# components

## Purpose
公式维护子组件：公式构建器（变量+运算符）、判定规则编辑器（条件组/条件行）、规则卡片预览、判定等级查看弹窗、高级判定编辑（多分支）。包含本地 TS 类型定义。

## Key Files
| File | Description |
|------|-------------|
| `FormulaBuilder.vue` | 公式表达式构建器（变量、运算符、函数）|
| `IntermediateDataFormulaForm.vue` | 公式新增/编辑表单 |
| `JudgmentRuleEditor.vue` | 判定规则编辑（基础）|
| `AdvancedJudgmentEditor.vue` | 高级判定编辑（多条件分支）|
| `AdvancedJudgmentModal.vue` | 高级判定弹窗容器 |
| `ConditionGroupEditor.vue` | 条件组（AND/OR）|
| `ConditionRow.vue` | 单条件行（变量 + 比较符 + 值）|
| `RuleCard.vue` | 规则卡片展示 |
| `RulePreviewCard.vue` | 规则预览卡（只读）|
| `JudgmentLevelViewModal.vue` | 判定等级关联查看 |
| `types.ts` | 公式/规则核心类型 |
| `advancedJudgmentTypes.ts` | 高级判定相关类型 |

## For AI Agents

### Working in this directory
- 类型集中在 `types.ts` / `advancedJudgmentTypes.ts`；新增字段先扩 type 再改组件。
- 条件树支持嵌套（Group 内含 Row 或 Group），递归渲染。
- 公式表达式建议使用受控 string + token 数组双轨保存，避免拼接错误。

### Common patterns
- `useForm` 注册外层表单；条件编辑器为受控组件接收 v-model。

## Dependencies
### Internal
- `/@/api/lab/intermediateDataFormula`, `/@/api/lab/intermediateDataJudgmentLevel`
- `/@/components/Form`, `/@/components/Modal`
### External
- `ant-design-vue`

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
