# KIMI R9 F1 前端接入报告

## 调研发现

### KgReasoningChain 组件
- 位置：`web/src/components/KgReasoningChain/index.vue`
- Props：`steps: ReasoningStep[]`、`defaultOpen?: boolean`
- 无 emits/slots，纯展示组件
- 渲染为 `a-collapse` 折叠面板，展示推理步骤的 kind/label/detail/satisfied 等信息

### nlqAgent.ts 接口
- 位置：`web/src/api/nlqAgent.ts`
- 核心函数：`streamNlqChat(request, handlers)` —— SSE 流式消费
- 回调：`onText(chunk)`、`onReasoningStep(step)`、`onResponseMetadata(payload)`、`onError(err)`、`onDone()`
- `BASE_URL` 取自 `VITE_NLQ_AGENT_API_BASE`，默认 `http://127.0.0.1:18100`

### ReasoningStep 类型
- 位置：`web/src/types/reasoning-protocol.d.ts`
- `kind`: record | spec | rule | condition | grade | fallback
- 字段：`label`, `detail`, `satisfied`, `field`, `expected`, `actual`, `meta`

### 目标视图评估
- `dashboard/index.vue`（生产驾驶舱）已有同目录的 `AiAssistant.vue` 浮动聊天组件，但之前被移除（`<!-- AI助手已移除 -->`）
- `monthly-dashboard` 是当前活跃路由（带 `ChatAssistant.vue`，但使用 `/api/lab/ai` 而非 `nlqAgent.ts`）
- 选择 **方案 A**：复用现有 `AiAssistant.vue`，替换其 mock 实现为 `streamNlqChat` + `KgReasoningChain`，然后在 `dashboard/index.vue` 重新启用。改动最小、回归面最窄。

## 实作方案

选择 **方案 A**（修改 `AiAssistant.vue` + 重新接入 `dashboard/index.vue`）

理由：
- `AiAssistant.vue` 已有完整的浮动聊天 UI（消息历史、快捷问题、打字动画、输入框）
- 只需替换核心 `handleSend` 逻辑，无需重写 UI
- `dashboard/index.vue` 只需恢复一行组件引用 + 一个 import
- 不触碰路由配置，零路由回归风险

## 改动文件

| 文件 | 改动 | LOC |
|---|---|---|
| `web/src/views/lab/dashboard/components/AiAssistant.vue` | 完全重写核心逻辑：接入 `streamNlqChat` + `KgReasoningChain`，替换 mock `generateResponse` | ~210 |
| `web/src/views/lab/dashboard/index.vue` | 恢复 `AiAssistant` import 与组件引用 | +2 |

## 核心实现要点

- `handleSend` 中调用 `streamNlqChat({ messages: [{ role: 'user', content: msg }] }, handlers)`
- `onText` → `currentAnswerText.value += chunk`，实时渲染到流式消息气泡
- `onReasoningStep` → `reasoningSteps.value.push(step)`，`KgReasoningChain` 实时消费
- `onResponseMetadata` → 若 `payload.reasoning_steps` 为数组，覆盖 `reasoningSteps`（state-first canonical）
- `onError` → `message.error(...)`
- `onDone` → 将 `currentAnswerText` 固化为历史消息，清空 `reasoningSteps` 与 `currentAnswerText`，复位 `isLoading`
- 聊天窗口宽度微调 `380px → 420px`，高度 `500px → 540px`，以容纳推理链折叠面板

## 类型与代码质量

### `pnpm type:check`
- 修改的两个文件 **0 个新增类型错误**
- `dashboard/index.vue` 存在 1 个 **既有** 错误：`'createMessage' is declared but its value is never read`（第 67 行，本次未引入）
- 项目整体存在大量既有类型错误（~40+，分布在 template、prediction、service/watch、vite.config.ts 等），均与本改动无关

### `pnpm lint:eslint`
- `dashboard` 目录在 `.eslintignore` 中，因此 ESLint 对该目录文件处于忽略状态
- 直接对两个文件运行 `npx eslint --no-ignore` 无错误
- 无新增 lint 问题

## 已知限制

- **未跑 `pnpm dev` 验证 UI 渲染**：环境 node_modules 刚安装完毕，但启动 dev server 需要确认代理/API 可用性；本次仅通过类型检查确保编译正确
- **`dashboard/index.vue` 当前不在路由表中**：`lab.ts` 路由只配置了 `monthly-dashboard`，`dashboard` 视图如需用户访问，需后续 PR 补充路由项或替换 `monthly-dashboard` 的组件指向
- **SSE 后端连通性未验证**：`streamNlqChat` 依赖 `VITE_NLQ_AGENT_API_BASE` 指向的 nlq-agent 服务，本次为纯前端接线
- **无单测**：项目未配置 vitest/jest，无现有组件测试体系，跳过
