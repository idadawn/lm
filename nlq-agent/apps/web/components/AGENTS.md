<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-29 | Updated: 2026-04-29 -->

# components

## Purpose
所有可复用 UI 组件。三类：图表（`charts/`，全部 dynamic+ssr:false 包装）、信息卡片（`cards/`，纯展示）、聊天容器（`chat/`，含 SSE 客户端 + KG 推理链折叠块）。

## Subdirectories

| Directory | Purpose |
|-----------|---------|
| `charts/` | `@ant-design/charts` 包装：TrendLine / MetricGauge / GradePie / HorizontalBar / ScatterPlot / RadarChart + `ChartRenderer` 调度（见 `charts/AGENTS.md`） |
| `cards/` | `CalculationExplanationCard`（公式来源 / 字段 / 自然语言）、`GradeJudgmentCard`（等级 / 状态 / 匹配规则）（见 `cards/AGENTS.md`） |
| `chat/` | `NlqChatPanel`（SSE 客户端 + 主聊天 UI）、`KgReasoningChain`（推理链折叠块）+ vitest 用例（见 `chat/AGENTS.md`） |

## For AI Agents

### Working In This Directory
- 任何用 Canvas 的图表组件必须放 `charts/` 并通过 `charts/index.tsx` 的 `dynamic(() => import(...), { ssr: false })` 暴露；不要在页面里直接 import。
- 卡片类组件保持纯函数：props in → JSX out，不调 fetch、不放 state（除非折叠/展开）。
- 全部组件假设运行在 `"use client"` 边界内（容器组件由父级决定）。

### Testing Requirements
- 单测放在组件同目录：`Component.test.tsx`，用 `@testing-library/react` + `vitest`。
- 改 `KgReasoningChain` 必须跑 `pnpm --filter web test KgReasoningChain` + `tests/e2e/root-cause.spec.ts`（双保险，因为它是跨仓共享的 UI 协议）。

### Common Patterns
- 颜色编码统一走 Tailwind palette（`bg-emerald-100 text-emerald-700`），严禁内联 `style={{color: "#xxx"}}`，除非来自后端的 `ChartDescriptor.meta.gradeThresholds` / `JudgmentRule.color`（业务数据）。
- shimmer 占位符内联在 `charts/index.tsx` + `chat/`、`cards/` 各自需要时复制（轻量，不抽公共组件）。

## Dependencies

### Internal
- `@nlq-agent/shared-types`（`ChartDescriptor`、`CalculationExplanation`、`GradeJudgment`、`ReasoningStep`、`StreamEvent` 等）。

### External
- `@ant-design/charts@^2.2`、`react-markdown`、`lucide-react`、`clsx`、`tailwind-merge`、`zustand@^5`、`ai-elements@^1.8`。
