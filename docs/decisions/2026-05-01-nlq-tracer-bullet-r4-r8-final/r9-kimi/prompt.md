# 你是 nlq-agent round-9 KIMI 工人 — F1 前端 KgReasoningChain 接入 lab dashboard

## 工作区（严格）
- worktree：**`/data/project/lm-team/kimi/`**（分支 `omc-team/r9/kimi-frontend-wire`，基于 main `3a86c86`）
- **第一件事**：`pwd` + `git rev-parse --abbrev-ref HEAD` 确认 cwd 与分支。错则 BLOCKER（写到 `/data/project/lm/.omc/team-nlq-r9/kimi/BLOCKER.md`）。
- **绝对禁止** 在 `/data/project/lm` 主仓库执行任何 git 操作（switch/checkout/reset/branch/add/commit）。
- **绝对禁止** `cd` 出 `/data/project/lm-team/kimi/` 范围。
- 主仓库与 GLM worktree 禁止触碰。

## 范围 — F1 真实前端接入

目标：把 `web/src/components/KgReasoningChain/` 真接到一个 lab 视图，让前端能向 nlq-agent 发问题、流式渲染推理链 + 文本回答。

### 第一步：调研（必须）
1. 读 `web/src/components/KgReasoningChain/index.vue` 看 props/emits/slots
2. 读 `web/src/api/nlqAgent.ts` 看 `streamNlqChat` 接口、回调名、ReasoningStep 类型
3. 读 `web/src/types/reasoning-protocol.ts`（如存在）
4. 读 `web/src/views/lab/dashboard/index.vue` 评估是否合适接入；若不合适改用 `monthly-dashboard` 或新建独立 `web/src/views/lab/nlq-chat/`

### 第二步：实作（任选其一）
**方案 A（推荐）**：在 `web/src/views/lab/dashboard/index.vue` 加一个折叠面板/Drawer：
- 触发按钮：右上角"AI 问答"
- 面板内：a-input.TextArea 输入框 + 提交按钮 + KgReasoningChain 渲染区 + 文本输出区
- 提交时调 `streamNlqChat`：
  - `onReasoningStep` → push 到响应式 `reasoningSteps` 数组（KgReasoningChain 消费）
  - `onText` → append 到响应式 `answerText` 字符串
  - `onResponseMetadata` → 用 metadata.reasoning_steps（canonical list）覆盖 reasoningSteps（state-first）
  - `onDone` → 复位 loading
  - `onError` → message.error 提示

**方案 B**：新建 `web/src/views/lab/nlq-chat/index.vue` 独立路由（如 `web/src/router/modules/lab.ts` 已存在则加路由项；否则文档里写一句"路由配置由 PR 评审决定"）。

选 A 还是 B 视你读完 dashboard/index.vue 后判断 —— 哪个改动更小、回归面更窄就选哪个。

### 第三步：类型 & 校验
- `pnpm type:check` 必须通过（vue-tsc 0 错误）
- `pnpm lint:eslint` 0 错误（warn 可保留）
- 不跑 `pnpm dev` / `pnpm build`（环境可能没装满 chromium 等依赖，浪费时间）

### 第四步：单测（best effort）
- 如已有 vitest 体系：加一个简单的组件渲染断言（KgReasoningChain props 透传、空状态、流式 push 后渲染）
- 如无则跳过，但在 REPORT 里说明为什么跳

## 执行约束
- **不准** spawn 子代理
- **不准**改 `nlq-agent/` 下任何代码（这一轮纯前端）
- **不准**改 `api/` 下任何 .NET 代码
- **不准**改 `web/src/api/nlqAgent.ts`（就用现有接口；如发现 bug 写到 REPORT，lead 单独处理）
- **不准**改 `web/src/components/KgReasoningChain/` 内部（它已稳定；只外部消费）
- **不准**新装 npm 包（前端依赖锁死）
- **绝对禁止**主仓库 git 操作
- 跑外部服务 — 不准

## 提交规范
≥1 atomic commit（单一 feat 即可，如改动够大可拆 2 个）：
- `feat(web): wire KgReasoningChain to lab dashboard via nlqAgent SSE`
- `Co-Authored-By: Claude Opus 4.7` attribution
- `git status` 干净

## 输出
1. `git log --oneline 3a86c86..HEAD`：≥1 commit
2. `pnpm type:check` 输出片段（必须 0 错）
3. `pnpm lint:eslint --no-fix web/src/views/lab/...` 输出片段
4. `/data/project/lm/.omc/team-nlq-r9/kimi/REPORT.md`：
   - 调研发现（KgReasoningChain props/emits、nlqAgent 回调、目标视图）
   - 选 A 还是 B 及理由
   - 改动文件列表 + LOC
   - type:check / lint 结果
   - 已知限制（"未跑 pnpm dev 验证 UI 渲染"等）

阻塞写到 `/data/project/lm/.omc/team-nlq-r9/kimi/BLOCKER.md`。

## 时间约束
~30 min。优先调研 + 实作；测试和 lint 保底；UI 渲染验证看时间。

开始 — 先 pwd 验证 cwd！
