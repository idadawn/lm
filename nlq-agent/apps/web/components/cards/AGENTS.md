<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-29 | Updated: 2026-04-29 -->

# cards

## Purpose
聊天回复辅助卡片。后端 QueryAgent 在 `state` 里返回 `calculation_explanation` + `grade_judgment`，前端在主回答下方按需渲染这两张卡片。纯展示组件 — 输入即 props，无状态。

## Key Files

| File | Description |
|------|-------------|
| `CalculationExplanationCard.tsx` | 计算方式说明：公式来源（来自 `lab_intermediate_data_formula.F_FORMULA`）+ 数据字段 chips（`column_name`）+ 自然语言描述 |
| `GradeJudgmentCard.tsx` | 等级判定明细：等级徽章 + 指标值 + 品质状态 + 匹配规则详情（含条件 JSON）+ 折叠的全部规则列表 |
| `index.ts` | 桶导出（barrel）：`CalculationExplanationCard`、`GradeJudgmentCard` |

## For AI Agents

### Working In This Directory
- 加新卡片：新建 `XxxCard.tsx` + 在 `index.ts` 加 `export`，并在 `chat/NlqChatPanel.tsx` 的累积 ref 字段里加上对应 state key。
- 卡片 props 类型一律来自 `@nlq-agent/shared-types`（如 `CalculationExplanation`、`GradeJudgment`），不要在卡片内重复定义类型。
- 不要在卡片里调 `fetch`（容器组件 `NlqChatPanel` 负责数据传入）。

### Testing Requirements
- 单测：在本目录加 `<Component>.test.tsx`，用 `@testing-library/react` 渲染并断言关键文案/aria。
- 视觉回归：暂未引入；改样式时手工跑 `pnpm --filter web dev` 看 `/` 页面。

### Common Patterns
- 颜色映射：`getColorClasses(colorCode)` 把后端 `#10B981`/`#22C55E`/`#3B82F6`/`#F59E0B`/`#EF4444`/`#6B7280` 映射到 Tailwind class，未匹配回落到灰色。
- 中文 i18n：`getQualityStatusLabel(status)` 把 `excellent/good/qualified/unqualified/normal/abnormal` 转中文。
- 条件 JSON 用 `<code>{JSON.stringify(condition)}</code>` 简单展示（不解析为表格，避免格式假设）。

## Dependencies

### Internal
- `@nlq-agent/shared-types`：`CalculationExplanation`、`GradeJudgment`、`GradeJudgmentDetail`。

### External
- `react@^19`、Tailwind utility classes。
