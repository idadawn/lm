# lm / nlq-agent 对话体验优化建议：从普通 Chat 升级为 Agent 工作台

作者：**Manus AI**  
日期：2026-05-14

## 一、结论摘要

当前 `lm` 项目的对话体验之所以显得“差”，核心原因不是单一视觉问题，而是**消息模型、事件协议、前端渲染结构与移动端交互模型没有围绕 Agent 工作流设计**。桌面 Web 端的 `NlqChatPanel` 目前仍以传统问答气泡为中心，工具调用只在加载时显示一个极简列表，知识图谱推理链只挂在最后一条助手消息内部；手机端 `mobile/pages/chat/chat.vue` 则把推理链放在整个消息列表之后，并且 `mobile/utils/sse-client.js` 当前不消费 `tool_start`、`tool_end` 事件，因此无法形成“正在理解问题、正在查图谱、正在查数据、正在生成结论”的过程感。[1] [2] [3]

建议把对话体验升级为 **Agent Conversation Timeline**：每一次用户提问生成一个完整的 `assistant turn`，其中包含 **推理摘要面板、工具调用卡片、数据/图表卡片、最终回答、引用/依据**。桌面端可以采用居中主时间线加右侧上下文抽屉的结构；手机端应采用单栏时间线、底部吸附输入、默认折叠卡片和轻量状态条，避免把桌面布局直接压缩到小屏。

> 重要说明：界面中不应展示模型内部原始 Chain-of-Thought，而应展示**可审计、可产品化的推理摘要**，例如“识别指标 → 解析时间范围 → 调用知识图谱 → 执行 SQL → 汇总结论”。这既能让用户理解系统在做什么，又不会把不可控的内部推理文本直接暴露给终端用户。

| 改造层级 | 当前问题 | 建议目标 | 优先级 |
|---|---|---|---|
| 消息模型 | `ChatMessageItem` 只有 `role/content`，结构无法承载工具、推理、图表、错误状态。 | 引入 `AgentTurn`，把一次助手响应拆为 `thinking/tool/result/final` 多个可渲染块。 | P0 |
| SSE 协议 | Web 端能接收工具事件但只显示名称；移动端直接丢弃工具事件。 | 标准化 `tool_start/tool_delta/tool_end/tool_error/reasoning_step/artifact` 事件。 | P0 |
| Web UI | 气泡式布局过窄，工具调用与回答割裂，缺少时间线。 | 800px 主时间线，工具卡片全宽，推理面板默认折叠，最后回答突出。 | P0 |
| 手机 UI | 推理链不属于具体消息；输入栏和卡片缺少 Agent 状态反馈。 | 单栏 Turn 卡片，工具卡压缩，底部输入支持安全区、键盘、停止生成。 | P0 |
| 视觉风格 | 蓝白气泡偏 Demo；缺少层级、状态色与工业领域质感。 | 使用中性色、微渐变、状态徽章、细边框、可读的代码/JSON 区。 | P1 |

## 二、当前实现的关键问题

### 2.1 Web 端：对话仍是“气泡聊天”，不是 Agent 工作流

`NlqChatPanel.tsx` 中的消息类型非常简单，仅包含 `id`、`role`、`content`。这意味着图表、推理链、工具调用都只能依赖全局状态或最后一条消息的临时状态进行附着，而不能成为消息本身的一部分。[1]

当前 Web 端在接收到 `tool_start` 时只保存 `{ name, status }`，在 `tool_end` 时也只更新状态，不保留 `tool_input` 与 `tool_output`。因此用户只能看到“调用了某个工具”，看不到“为什么调用、输入是什么、结果摘要是什么”。这会严重削弱 Agent 的可信度。[1]

同时，`reasoningSteps`、`chartConfig`、`calculationExplanation`、`gradeJudgment` 都是组件级状态，并且只在 `index === messages.length - 1` 时渲染。这会导致历史消息丢失结构化上下文：用户滚动回上一轮对话时，无法看到那一轮对应的推理链与图表。[1]

### 2.2 后端：SSE 已有基础，但事件语义还不够产品化

后端 `chat.py` 已经通过 SSE 输出 `text`、`tool_start`、`tool_end`、`reasoning_step`、`chart`、`response_metadata`、`error`、`done` 等事件，说明后端具备构建 Agent 对话体验的基础。[3] 问题在于现有事件没有稳定的 `tool_call_id`，前端只能用 `tool_name` 匹配工具开始与结束；当同一个工具在一轮对话中调用多次时，状态可能错配。[1] [3]

另外，工具执行没有 `tool_error`、`tool_delta` 和 `artifact` 事件。对于 SQL 查询、图谱路径检索、文件生成、长耗时分析等场景，用户需要看到“正在执行到哪一步”，而不是等待一个最终状态。[3]

### 2.3 手机端：移动端事件能力比 Web 端更弱

手机端 `streamNlqChat` 只分发 `text`、`reasoning_step`、`response_metadata`、`error`、`done`，没有处理 `tool_start` 和 `tool_end`。这意味着即使后端已经发送工具调用事件，手机端也完全无法展示工具卡片、状态徽章和执行结果。[2]

手机端页面还存在两个体验问题。第一，`reasoningSteps` 是页面级状态，被渲染在消息列表尾部，而不是绑定到具体的助手消息；第二，请求中 `session_id` 使用 `mobile-${Date.now()}`，没有复用 `currentSessionId`，会削弱连续对话体验。[4]

## 三、目标体验：Agent Conversation Timeline

建议不要继续把界面定义为“聊天气泡 + 附属组件”，而应定义为**面向工业数据问答的 Agent 工作台**。用户输入问题后，系统在同一条助手响应中逐步呈现：理解问题、检索图谱、调用工具、读取数据、生成图表、给出结论。

| 区块 | 桌面 Web 展示 | 手机端展示 | 默认状态 |
|---|---|---|---|
| 用户消息 | 右侧气泡或完整行卡片，最大宽度 70%。 | 右侧浅蓝气泡，最大宽度 82%。 | 展开 |
| 推理摘要 | 助手回答上方灰色 `ThinkingBlock`，可折叠。 | 单行状态条，点击底部抽屉展开。 | 收起 |
| 工具调用 | 全宽 `ToolCallCard`，按时间线排列。 | 紧凑卡片，只显示工具名、状态、摘要。 | 结果收起 |
| 数据/图表 | 独立 Artifact 卡片，避免塞进文本气泡。 | 图表横向滚动或点击全屏查看。 | 展开 |
| 最终回答 | 白底正文卡片，Markdown 排版。 | 白底正文卡片，字号 15px，行高 1.7。 | 展开 |
| 依据引用 | “数据来源/规则依据/图谱路径”折叠区。 | “依据”按钮打开底部弹层。 | 收起 |

> 推荐产品表达：不要叫“Chain-of-Thought”，而叫“推理过程”“执行过程”“分析步骤”或“可解释过程”。面向企业用户时，这比暴露模型内部思维更安全、更可控，也更容易与审计、权限和数据来源绑定。

## 四、桌面 Web 改造方案

### 4.1 UI 结构

桌面端建议采用 **三层结构**。第一层是全局 Shell，包括顶部标题、当前会话、模型/数据源状态和新建会话按钮。第二层是主时间线，最大宽度保持在 `800px–880px`，用于呈现用户消息、助手执行过程和最终回答。第三层是可选右侧上下文抽屉，用于展示知识图谱子图、SQL、数据表、规则详情和调试信息。

当前 `NlqChatPanel` 的 `max-w-3xl` 与普通气泡适合短文本问答，但不适合工具卡片、图表和知识图谱推理。建议将助手消息从单个气泡改为 `AgentTurn` 容器，其中普通正文可以保持卡片化，而工具和图表应全宽展示。[1]

| 组件 | 职责 | 关键字段 |
|---|---|---|
| `AgentChatShell` | 页面布局、Header、消息区、输入区。 | `mode`, `isMobile`, `sessionId` |
| `ConversationTimeline` | 渲染多轮会话。 | `turns`, `activeTurnId` |
| `UserMessageBubble` | 用户输入展示。 | `content`, `createdAt` |
| `AgentTurn` | 一次助手响应的结构化容器。 | `blocks`, `status`, `artifacts` |
| `ThinkingBlock` | 推理摘要与步骤。 | `steps`, `status`, `defaultCollapsed` |
| `ToolCallCard` | 工具调用过程与结果。 | `toolCallId`, `name`, `input`, `output`, `status` |
| `ArtifactCard` | 图表、表格、知识图谱子图等结果。 | `kind`, `title`, `payload` |
| `ComposerBar` | 输入、停止、重试、快捷问题。 | `value`, `loading`, `onStop` |

### 4.2 交互逻辑

用户发起问题后，应立即生成一个 `AgentTurn` 占位。每收到一个 SSE 事件，就向当前 `AgentTurn.blocks` 追加或更新一个块。这样历史消息不会丢失推理、图表和工具结果，也不会依赖“最后一条消息”的全局状态。

建议事件到 UI 的映射如下。

| SSE 事件 | 当前处理 | 建议处理 |
|---|---|---|
| `text` | 追加到最后一条 assistant content。 | 追加到当前 turn 的 `final_answer` block。 |
| `reasoning_step` | 更新全局 `reasoningSteps`。 | 追加到当前 turn 的 `thinking` block。 |
| `tool_start` | 只记录工具名和 running。 | 创建 `tool_call` block，保存 `tool_call_id/name/input/status`。 |
| `tool_end` | 用工具名更新 completed。 | 按 `tool_call_id` 更新 output、duration、summary。 |
| `chart` | 存到全局 `chartConfig`。 | 创建 `artifact` block，类型为 `chart`。 |
| `response_metadata` | 兜底填充当前消息和全局卡片。 | 合并最终响应、图表、推理链、数据来源。 |
| `error` | 把 assistant content 改成错误文本。 | 当前 turn 状态设为 error，并在错误卡片提供重试。 |

### 4.3 视觉风格

建议采用“工业智能 + Agent 控制台”的视觉语言，而不是通用客服聊天。主色可以继续保留蓝色，但应降低大面积纯蓝的使用，更多使用中性背景、细边框、状态徽章和浅色块，突出专业感和可读性。

| 元素 | 建议样式 |
|---|---|
| 页面背景 | `#F8FAFC` 或 `#F6F7FB`，避免纯白造成层级不足。 |
| 主卡片 | 白底、`border-slate-200`、`rounded-2xl`、轻阴影。 |
| ThinkingBlock | `bg-slate-50`、灰色文字、左侧细线时间轴。 |
| ToolCallCard Calling | `#F59E0B` 状态点，标题为“正在调用”。 |
| ToolCallCard Success | `#10B981` 状态点，标题为“已完成”。 |
| ToolCallCard Error | `#EF4444` 状态点，标题为“失败”。 |
| 最终回答 | 正文 `15px–16px`、行高 `1.75`，Markdown 表格增加横向滚动。 |
| 代码/JSON | `font-mono`、`bg-slate-950` 或浅灰代码块，可复制。 |

## 五、手机端改造方案

手机端不应照搬桌面端的工具卡片密度，而要围绕**单手操作、键盘遮挡、弱网、短屏滚动、触控折叠**来设计。当前移动端已经有 `scroll-view`、底部输入栏和键盘高度适配，这是良好基础，但需要把推理链、工具卡和最终回答绑定到具体消息回合。[4]

### 5.1 手机端 Layout

建议手机端采用单栏结构：顶部为轻量会话栏，中间为 `scroll-view` 时间线，底部为吸附输入区。用户发送问题后，助手区域不是一个普通气泡，而是一个 `agent-turn-card`，里面按顺序显示状态条、工具步骤、最终回答。

| 区域 | 手机端建议 |
|---|---|
| 顶部栏 | 标题“智能问数”，右侧新建会话/历史按钮，高度控制在 48px。 |
| 消息列表 | 纵向单栏，左右 padding 12–16px，避免 70% 气泡导致中文换行过多。 |
| 用户消息 | 右侧气泡，最大宽度 82%，浅蓝背景，不显示头像也可。 |
| 助手 Turn | 左侧或全宽卡片，推荐全宽，便于放置工具卡和图表。 |
| 推理状态 | 默认只显示“已完成 4 个步骤”，点击展开。 |
| 工具卡 | 紧凑模式，每张卡一行摘要，点击展开输入/输出。 |
| 图表 | 默认卡片预览，点击进入全屏图表页或底部弹层。 |
| 输入栏 | 固定底部，支持 safe-area，发送时按钮变“停止”。 |

### 5.2 手机端交互细节

手机端最关键的是“默认克制”。推理和工具结果默认收起，只在执行中显示当前步骤的短句。例如：`正在识别指标…`、`正在查询知识图谱…`、`正在执行数据查询…`、`正在生成结论…`。这比连续展示大量 JSON 或日志更适合小屏。

| 场景 | 手机端行为 |
|---|---|
| 发送问题 | 输入栏清空，键盘保持或收起可配置；立即插入助手 Turn。 |
| 执行中 | Turn 顶部显示动态状态条和细进度线。 |
| 工具调用中 | 当前工具卡高亮，显示 spinner 与一句话摘要。 |
| 工具完成 | 卡片变为绿色勾选，正文仍收起。 |
| 失败 | 卡片红色，提供“重试本步骤”或“重新提问”。 |
| 长回答 | 自动滚动到底部，但用户向上滚动时暂停自动滚动。 |
| 图表 | 小屏只显示关键数字和缩略图，点击全屏查看。 |

### 5.3 手机端代码层必须改的点

第一，`streamNlqChat` 必须分发 `tool_start`、`tool_end`、`chart` 事件。否则手机端无法实现工具卡片。[2]

第二，`chat.vue` 的 `reasoningSteps` 应从页面级变量移动到具体助手消息或 `turn.blocks` 中。否则历史消息无法保留各自的推理过程。[4]

第三，请求 `session_id` 应使用 `currentSessionId.value`，而不是每次发送时新建 `mobile-${Date.now()}`。否则连续追问的上下文会被破坏。[4]

第四，移动端应该增加“停止生成”能力。`uni.request` 返回的 `requestTask` 可以保存到 `currentRequestTask`，用户点击停止时调用 `abort()`，并把当前 Turn 标记为 `cancelled`。

## 六、建议的数据结构与事件协议

### 6.1 前端统一消息模型

建议在 `shared-types` 中新增跨 Web / Mobile 的结构化消息模型。它不取代后端 LangGraph 状态，而是作为前端渲染协议。

```ts
export type AgentBlockType =
  | 'thinking'
  | 'tool_call'
  | 'artifact'
  | 'final_answer'
  | 'error';

export type AgentRunStatus =
  | 'queued'
  | 'thinking'
  | 'calling_tool'
  | 'answering'
  | 'completed'
  | 'error'
  | 'cancelled';

export interface AgentTurn {
  id: string;
  userMessageId: string;
  status: AgentRunStatus;
  createdAt: number;
  updatedAt: number;
  blocks: AgentBlock[];
}

export type AgentBlock =
  | ThinkingBlock
  | ToolCallBlock
  | ArtifactBlock
  | FinalAnswerBlock
  | ErrorBlock;

export interface ThinkingBlock {
  id: string;
  type: 'thinking';
  title: string;
  steps: ReasoningStep[];
  collapsed: boolean;
}

export interface ToolCallBlock {
  id: string;
  type: 'tool_call';
  toolCallId: string;
  name: string;
  status: 'running' | 'success' | 'error';
  input?: Record<string, unknown>;
  output?: Record<string, unknown>;
  summary?: string;
  startedAt?: number;
  endedAt?: number;
}

export interface ArtifactBlock {
  id: string;
  type: 'artifact';
  kind: 'chart' | 'table' | 'kg_subgraph' | 'sql' | 'file';
  title: string;
  payload: unknown;
}

export interface FinalAnswerBlock {
  id: string;
  type: 'final_answer';
  content: string;
}

export interface ErrorBlock {
  id: string;
  type: 'error';
  message: string;
  retryable: boolean;
}
```

### 6.2 后端 SSE 事件建议

后端当前事件已经可用，但建议增加 ID、阶段和摘要字段，使前端能稳定渲染时间线。[3]

```json
{
  "type": "tool_start",
  "turn_id": "turn_123",
  "tool_call_id": "tool_001",
  "tool_name": "kg_find_reasoning_path",
  "display_name": "查询知识图谱路径",
  "tool_input": { "metric": "叠片系数", "product": "B1" },
  "summary": "正在查询指标、规格与判定规则之间的关系"
}
```

```json
{
  "type": "tool_end",
  "turn_id": "turn_123",
  "tool_call_id": "tool_001",
  "tool_name": "kg_find_reasoning_path",
  "status": "success",
  "tool_output": { "paths": 3, "matched_rules": 2 },
  "summary": "找到 3 条图谱路径与 2 条判定规则",
  "duration_ms": 842
}
```

## 七、可直接交给 Claude / Claude Code 的 Prompt

下面这段可以直接作为给 Claude 的开发任务说明。建议把它和相关文件路径一起提交，例如 `NlqChatPanel.tsx`、`KgReasoningChain.tsx`、`chat.py`、`mobile/pages/chat/chat.vue`、`mobile/utils/sse-client.js`。

```text
请帮我把 lm 项目的 nlq-agent 对话体验升级为 Agent Conversation Timeline，兼顾桌面 Web 与手机端。

背景：
当前 Web 对话组件位于 nlq-agent/apps/web/components/chat/NlqChatPanel.tsx。它现在使用普通 user/assistant 气泡，tool_start/tool_end 只显示工具名和状态，reasoning_steps、chartConfig、gradeJudgment 等都是全局状态并只挂在最后一条 assistant 消息上。手机端位于 mobile/pages/chat/chat.vue，当前把 reasoningSteps 放在消息列表末尾，不绑定具体助手消息；mobile/utils/sse-client.js 目前没有处理 tool_start/tool_end/chart 事件。

目标：
把对话界面改成类似 Kimi Agent / Manus Agent 的工作流体验，但不要展示原始 Chain-of-Thought，只展示可审计的“推理过程摘要”。核心交互包括：可折叠 ThinkingBlock、ToolCallCard、ArtifactCard、FinalAnswerCard、Timeline 容器、流式状态更新、失败与重试状态。

桌面 Web 要求：
1. 主内容 max-width 800px 到 880px 居中，消息流改成 ConversationTimeline。
2. 每次用户提问生成一个 AgentTurn，助手响应内部包含 blocks，而不是只用 role/content。
3. ThinkingBlock 默认折叠，标题显示“推理过程 · N 步”，执行中显示“思考中…”。
4. ToolCallCard 全宽展示，Header 包含工具图标、工具名、状态徽章；Body 默认折叠，展示 input/output JSON。
5. 状态色使用 Calling=#F59E0B、Success=#10B981、Error=#EF4444。
6. chart、grade judgment、calculation explanation 都作为 ArtifactCard 放入当前 AgentTurn，不再依赖全局最后一条消息状态。
7. 输入区需要支持“停止生成”、Enter 发送、Shift+Enter 换行，并在移动宽度下切换为底部吸附布局。

手机端要求：
1. mobile/pages/chat/chat.vue 改为单栏 Agent Turn 卡片，不要把 reasoningSteps 放在全局列表尾部。
2. 每个 assistant turn 内部显示紧凑状态条、可折叠推理步骤、可折叠工具卡片和最终回答。
3. mobile/utils/sse-client.js 必须处理 tool_start、tool_end、chart、error、done，并提供 onToolStart/onToolEnd/onChart 回调。
4. session_id 应复用 currentSessionId.value，不要每次发送都 mobile-${Date.now()}。
5. 输入栏固定底部，适配 safe-area 和键盘高度；发送中按钮变为“停止”，调用 requestTask.abort()。
6. 工具输出和图表在手机端默认收起，点击进入底部弹层或全屏查看。

协议要求：
1. 前端先兼容当前 SSE 协议：text、tool_start、tool_end、reasoning_step、chart、response_metadata、error、done。
2. 如果后端没有 tool_call_id，前端临时生成 `${tool_name}-${index}`，但请在 TODO 中标注后端应补充 tool_call_id。
3. 所有状态都必须绑定到当前 AgentTurn，历史消息不能丢失图表、推理链或工具调用记录。

请输出：
1. 修改后的 React + Tailwind 组件结构或代码。
2. 修改后的 mobile uni-app 结构或关键代码。
3. 必要的 shared-types 类型定义。
4. 简要说明迁移步骤与兼容策略。
```

如果使用 Claude Code 终端工具，还可以补充如下要求：

```text
在执行多步改造时，请先输出 [Plan] 说明修改步骤。每修改一个文件前输出 [File] <path>，每完成一个关键功能输出 [Check] <summary>。不要打印模型内部 Chain-of-Thought，只打印可审计的计划、工具调用摘要和结果。终端颜色建议：计划灰色，文件青色，成功绿色，警告黄色，错误红色。
```

## 八、建议的实施路线

建议按四个迭代推进，不要一次性重写所有端。

| 迭代 | 范围 | 交付物 | 验收标准 |
|---|---|---|---|
| W1 | Web 消息模型重构 | `AgentTurn`、`ThinkingBlock`、`ToolCallCard`、`FinalAnswerBlock`。 | 历史消息能保留推理链和工具调用。 |
| W2 | Web 视觉升级 | 时间线布局、状态徽章、折叠 JSON、输入区停止生成。 | 用户能清楚看到每一步执行状态。 |
| W3 | Mobile 协议补齐 | `sse-client.js` 支持工具和图表事件，`chat.vue` 绑定 turn。 | 手机端能展示工具卡片和每轮推理。 |
| W4 | Mobile 体验打磨 | 底部输入、停止生成、全屏图表、弱网/失败态。 | 小屏下无需横向溢出，长回答可读。 |

## 九、P0 修改清单

| 文件 | 建议修改 |
|---|---|
| `nlq-agent/packages/shared-types/src/index.ts` | 增加 `AgentTurn`、`AgentBlock`、`ToolCallBlock` 等前端渲染类型；扩展 `StreamEvent` 的 `tool_call_id`、`status`、`summary` 字段。 |
| `nlq-agent/apps/web/components/chat/NlqChatPanel.tsx` | 把 `messages + 全局 chart/reasoning/toolCalls` 改为 `turns` 或结构化 `messages`；SSE 事件按 block 更新。 |
| `nlq-agent/apps/web/components/chat/KgReasoningChain.tsx` | 保留为领域推理步骤组件，但外层改名/包装为 `ThinkingBlock`，支持执行中、完成、错误状态。 |
| `nlq-agent/services/agent-api/app/api/chat.py` | `tool_start/tool_end` 增加 `tool_call_id`、`summary`、`duration_ms`；必要时增加 `tool_error`。 |
| `mobile/utils/sse-client.js` | 增加 `tool_start/tool_end/chart` 分发回调，避免手机端丢事件。 |
| `mobile/pages/chat/chat.vue` | 引入 `turns` 或把 assistant 消息扩展为 blocks；推理链和工具卡绑定到具体助手消息。 |

## 十、References

[1]: nlq-agent/apps/web/components/chat/NlqChatPanel.tsx "当前 Web 对话组件实现"  
[2]: mobile/utils/sse-client.js "当前手机端 SSE 客户端实现"  
[3]: nlq-agent/services/agent-api/app/api/chat.py "当前后端聊天 SSE 接口实现"  
[4]: mobile/pages/chat/chat.vue "当前手机端聊天页面实现"  
[5]: nlq-agent/apps/web/components/chat/KgReasoningChain.tsx "当前 Web 知识图谱推理链组件实现"
