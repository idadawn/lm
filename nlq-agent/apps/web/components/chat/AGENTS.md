<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-29 | Updated: 2026-04-29 -->

# chat

## Purpose
聊天面板 + KG 推理链折叠块。`NlqChatPanel` 是 SSE 客户端（解析 `text/event-stream` 累积 chunk + 触发 `chart`/`reasoning_step`/`response_metadata` 渲染）；`KgReasoningChain` 是无状态展示组件，把 `ReasoningStep[]` 渲染成带类型徽章的有序列表。

## Key Files

| File | Description |
|------|-------------|
| `NlqChatPanel.tsx` | 主聊天 UI（fullscreen / dock 两种 mode）：模型选择器 + 建议问题 bubbles + 消息列表 + 图表渲染 + 计算说明 + 等级判定 + KG 推理链；状态用 `useState` + `currentMessageDataRef` 累积当前一条 assistant 消息的多通道数据 |
| `KgReasoningChain.tsx` | `<details>` 折叠块（TODO: ai-elements `<Reasoning>` 安装后替换）— 渲染 `kind` 徽章 + 步骤序号 + condition 类的"期望/实际/满足"双列 |
| `KgReasoningChain.test.tsx` | vitest + @testing-library 用例：fixture 驱动渲染 |
| `index.ts` | 桶导出：`NlqChatPanel`（不导出 `KgReasoningChain` —— 仅内部用） |

## For AI Agents

### Working In This Directory
- SSE 解析逻辑只此一处，禁止复制到别的组件。新增事件类型：先在 `@nlq-agent/shared-types` 的 `StreamEventType` union 加 → 在 `NlqChatPanel.tsx` 的事件分发 switch 加 case → 在 `currentMessageDataRef` 加字段。
- `KgReasoningChain` 是跨仓共享 UI 协议（同款组件在父项目 `lm/web`、`lm/mobile` 也有），改 props 形状或 `kind` 渲染前必看 `reasoning-protocol.ts` 的 SHA-256 锚定。
- header 透传：`assignSafeHeader` 检查 latin-1 字节，避免中文 token 触发浏览器 `TypeError`。新加 header 走这个工具。

### Testing Requirements
- `pnpm --filter web test KgReasoningChain` 必跑（任何 `kind` 增删都会改快照）。
- 改 SSE 解析必跑 `tests/e2e/root-cause.spec.ts`（覆盖 ≥3 个 reasoning_step + 折叠块渲染）。

### Common Patterns
- `ReactMarkdown` 渲染助手主回答；图表 / 卡片 / 推理链放回答下方。
- 累积模式：用 `currentMessageDataRef` 而非 setState，避免 SSE chunk 高频触发 React 重渲。完成后（`response_metadata` 事件）一次性 setState。
- dock 模式：`mode="dock"` 默认收起，启动器按钮文案由 `launcherLabel`（默认 "AI问数"）控制。

## Dependencies

### Internal
- `@/components/charts`（`ChartRenderer`）、`@/components/cards`（两张卡片）。
- `@nlq-agent/shared-types`：`AuthContext`、`CalculationExplanation`、`ChatResponse`、`GradeJudgment`、`ReasoningStep`、`StreamEvent`、`ChartDescriptor`。

### External
- `react-markdown@^9`、`react@^19`、`@testing-library/react`（test only）。
