<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# chat

## Purpose
"对话" tab — natural-language Q&A interface for the LIMS. Lets users ask questions like 自然语言查询检测数据 / 分析趋势 / 生成报表; consumes the NLQ-agent SSE stream and renders both the assistant's text reply and a `KgReasoningChain` panel showing how the answer was derived.

## Key Files
| File | Description |
|------|-------------|
| `chat.vue` | Single-page chat UI: scrollable message list with role-based avatars (我 / AI), welcome state with `quickTags` shortcut chips, typing-dot loading bubble, bottom input bar with send button. Imports `KgReasoningChain` and `streamNlqChat` (SSE client). Streamed `text` chunks are appended to the assistant bubble; `reasoning_step` events accumulate into `reasoningSteps`; `response_metadata` replaces the chain with the canonical full list. Auto-scrolls via `scroll-into-view="chat-bottom"`. |

## For AI Agents

### Working in this directory
- Do not call the .NET `/api/lab/*` endpoints from here — chat goes through the NLQ agent (`utils/sse-client.js`, base URL stored under `NLQ_AGENT_API_BASE`).
- Keep the message rendering simple `<text>` (not innerHTML) to stay safe across mini-program platforms; if Markdown is needed, use a uni-app safe renderer.
- Reasoning steps are appended in order — never sort or dedupe on the client; trust the upstream protocol.
- When loading is true, the input must be `:disabled="loading"` to prevent overlapping streams.

### Common patterns
- Quick-tag chips set `inputText` and immediately call `sendMessage()`.
- Welcome state shows only when `messages.length === 0`.
- Typing dots are pure CSS animation, no images.

## Dependencies
### Internal
- `@/components/kg-reasoning-chain/kg-reasoning-chain.vue`
- `@/utils/sse-client.js` for `streamNlqChat`.

### External
- `nlq-agent` HTTP service `/api/v1/chat/stream` (chunked SSE).
- `uni.request` with `enableChunked: true`.

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
