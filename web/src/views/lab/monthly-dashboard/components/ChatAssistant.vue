<template>
  <div class="chat-assistant">
    <!-- 浮动按钮 -->
    <div class="chat-float-btn-wrapper">
      <a-badge :count="unreadCount" :offset="[-5, 5]">
        <a-button type="primary" shape="circle" size="large" class="chat-trigger-btn" @click="toggleChat">
          <template #icon>
            <RobotOutlined />
          </template>
        </a-button>
      </a-badge>
    </div>

    <!-- 桌面端弹窗 (>= 768px) -->
    <a-modal
      v-if="!isMobile"
      v-model:open="visible"
      title="AI 数据助手"
      :footer="null"
      width="600px"
      centered
      :destroyOnClose="true"
      wrapClassName="chat-modal-wrapper"
      :maskClosable="false"
      class="ai-chat-modal"
    >
      <template #closeIcon>
        <CloseOutlined />
      </template>
      <ChatContent
        :messages="messages"
        :streaming-index="streamingIndex"
        :input-value="inputValue"
        :is-sending="isSending"
        :quick-questions="quickQuestions"
        :messages-ref-setter="setMessagesRef"
        @update:input-value="inputValue = $event"
        @send="handleSend"
        @quick-question="handleQuickQuestion"
      />
    </a-modal>

    <!-- 移动端抽屉 (< 768px) -->
    <a-drawer
      v-else
      v-model:open="visible"
      placement="bottom"
      height="100vh"
      :closable="true"
      title="AI 数据助手"
      :destroyOnClose="true"
      class="ai-chat-drawer"
    >
      <template #closeIcon>
        <CloseOutlined />
      </template>
      <ChatContent
        :messages="messages"
        :streaming-index="streamingIndex"
        :input-value="inputValue"
        :is-sending="isSending"
        :quick-questions="quickQuestions"
        :messages-ref-setter="setMessagesRef"
        @update:input-value="inputValue = $event"
        @send="handleSend"
        @quick-question="handleQuickQuestion"
      />
    </a-drawer>
  </div>
</template>

<script setup lang="ts">
import { ref, nextTick, watch, defineComponent, h } from 'vue';
import { RobotOutlined, UserOutlined, CloseOutlined } from '@ant-design/icons-vue';
import { message } from 'ant-design-vue';
import { useMediaQuery } from '@vueuse/core';
import { sendChatMessage } from '/@/api/lab/ai';
import { streamNlqChat } from '/@/api/nlqAgent';
import type { ReasoningStep } from '/@/types/reasoning-protocol';
import ReasoningChain from '/@/components/ReasoningChain.vue';
import Showdown from 'showdown';
import { useNlqSession } from './useNlqSession';

// Props
interface Props {
  reportData?: any;
}

const props = withDefaults(defineProps<Props>(), {
  reportData: null,
});

// 状态 — messages 来源于持久化的 session（跨刷新 / 多 tab 同步）
const { messages, appendMessage, updateLastMessage, broadcastTail } = useNlqSession();

const visible = ref(false);
const inputValue = ref('');
const isSending = ref(false);
const streamingIndex = ref<number>(-1);
const messagesRef = ref<HTMLElement | null>(null);

// 响应式断点 — VueUse useMediaQuery
const isMobile = useMediaQuery('(max-width: 767px)');

// 未读消息数 — 真实计数
const unreadCount = ref(0);

// Markdown 渲染器 (使用 showdown)
const converter = new Showdown.Converter({
  ghCodeBlocks: true,
  simplifiedAutoLink: true,
  strikethrough: true,
  tables: true,
  tasklists: true,
  requireSpaceBeforeHeadingText: true,
});

// 快捷提问
const quickQuestions = [
  '本月合格率是多少？',
  'A类产品产量有多少？',
  '哪个班次的合格率最高？',
  '不合格原因主要是什么？',
];

// 渲染 Markdown (使用 showdown)
function renderMarkdown(content: string): string {
  return converter.makeHtml(content);
}

// 设置消息列表的 DOM ref（由 ChatContent 回调）
function setMessagesRef(el: HTMLElement | null) {
  messagesRef.value = el;
}

// 切换聊天窗口
function toggleChat() {
  visible.value = !visible.value;
}

// 发送消息
async function handleSend() {
  const text = inputValue.value.trim();
  if (!text || isSending.value) return;

  // 用户消息：appendMessage → 持久化 + broadcast
  appendMessage({ role: 'user', content: text });
  inputValue.value = '';

  await scrollToBottom();

  // 占位 AI 消息（流式填充期间内存更新，完成后 broadcast 完整消息）
  appendMessage({ role: 'assistant', content: '', reasoningSteps: [] });
  streamingIndex.value = messages.value.length - 1;

  isSending.value = true;

  try {
    let hasStreamed = false;
    let accumulated = '';
    let accumulatedSteps: ReasoningStep[] = [];

    await streamNlqChat(
      {
        messages: [
          { role: 'system', content: buildSystemPrompt() },
          { role: 'user', content: text },
        ],
      },
      {
        onReasoningStep(step: ReasoningStep) {
          accumulatedSteps = [...accumulatedSteps, step];
          updateLastMessage({ reasoningSteps: accumulatedSteps });
        },
        onText(chunk: string) {
          hasStreamed = true;
          accumulated += chunk;
          updateLastMessage({ content: accumulated });
          // 收到 AI 文本且窗口未打开 → 增加未读计数
          if (!visible.value) {
            unreadCount.value++;
          }
          scrollToBottom();
        },
        onDone() {
          if (!accumulated) {
            updateLastMessage({ content: '抱歉，我无法生成回复。' });
            // 未打开时也计一次未读
            if (!visible.value) {
              unreadCount.value++;
            }
          }
        },
        onError(err: Error) {
          console.warn('nlqAgent SSE error, falling back to REST:', err);
          if (!hasStreamed) {
            sendChatMessage({ message: text, systemPrompt: buildSystemPrompt() })
              .then((response) => {
                updateLastMessage({ content: response.response || '抱歉，我无法生成回复。' });
                if (!visible.value) {
                  unreadCount.value++;
                }
              })
              .catch((restErr) => {
                console.error('AI 接口调用失败:', restErr);
                updateLastMessage({ content: '抱歉，处理您的请求时出现错误。' });
                message.error('AI 助手暂时无法响应，请稍后重试');
              })
              .finally(() => {
                isSending.value = false;
                streamingIndex.value = -1;
                broadcastTail();
                scrollToBottom();
              });
          }
        },
      },
    );
  } catch (error) {
    console.error('AI 接口调用失败:', error);
    updateLastMessage({ content: '抱歉，处理您的请求时出现错误。' });
    message.error('AI 助手暂时无法响应，请稍后重试');
  } finally {
    isSending.value = false;
    streamingIndex.value = -1;
    // 流结束后,广播一次完整 assistant 消息让其他 tab 同步最终状态
    broadcastTail();
    await scrollToBottom();
  }
}

// 快捷提问
function handleQuickQuestion(question: string) {
  inputValue.value = question;
  handleSend();
}

// 滚动到底部
async function scrollToBottom() {
  await nextTick();
  if (messagesRef.value) {
    messagesRef.value.scrollTop = messagesRef.value.scrollHeight;
  }
}

// 构建 AI 系统提示词（包含当前报表数据）
function buildSystemPrompt(): string {
  if (!props.reportData?.summary) {
    return `你是一个实验室数据分析助手，帮助用户理解质量报表数据。请保持回答简洁准确。`;
  }

  const s = props.reportData.summary;
  const unqualifiedStats = props.reportData.unqualifiedCategoryStats || [];
  const shiftComparisons = props.reportData.shiftComparisons || [];

  let prompt = `你是一个实验室数据分析助手，帮助用户理解月度质量报表数据。

【当前数据上下文】
检验总重：${(s.totalWeight || 0).toLocaleString()} kg
合格率：${(s.qualifiedRate || 0).toFixed(2)}%
不合格率：${(s.unqualifiedRate || 0).toFixed(2)}%
`;

  // A类 B类数据
  if (s.classAWeight || s.classBWeight) {
    prompt += `- A类：${(s.classAWeight || 0).toLocaleString()} kg (${(s.classARate || 0).toFixed(2)}%)
- B类：${(s.classBWeight || 0).toLocaleString()} kg (${(s.classBRate || 0).toFixed(2)}%)
`;
  }

  // 不合格分类详情
  if (unqualifiedStats.length > 0) {
    prompt += `\n【不合格分类详情】\n`;
    unqualifiedStats.slice(0, 5).forEach((item: any) => {
      prompt += `- ${item.categoryName}: ${(item.weight || 0).toLocaleString()} kg (${(item.rate || 0).toFixed(2)}%)\n`;
    });
  }

  // 班次对比
  if (shiftComparisons.length > 0) {
    prompt += `\n【班次对比】\n`;
    shiftComparisons.forEach((item: any) => {
      prompt += `- ${item.shift}: 合格率 ${(item.qualifiedRate || 0).toFixed(2)}%, A类占比 ${(item.classARate || 0).toFixed(2)}%\n`;
    });
  }

  prompt += `
请根据以上数据回答用户问题，保持回答简洁准确。如果用户问的问题无法从当前数据中得出答案，请明确告知。
`;

  return prompt;
}

// 监听弹窗打开
watch(visible, (newVal) => {
  if (newVal) {
    // 打开时清零未读计数
    unreadCount.value = 0;

    if (messages.value.length === 0) {
      setTimeout(() => {
        // 再次检查 — 期间可能已有消息从 broadcast / storage 同步过来
        if (messages.value.length === 0) {
          appendMessage({
            role: 'assistant',
            content:
              '您好！我是 **AI 数据助手**。\n\n我可以帮您分析当前报表数据，回答关于合格率、产量、班次对比等问题。请问有什么可以帮助您的？',
          });
        }
      }, 300);
    }
  }
});

// ─── 内联子组件：共享聊天内容区（消息列表 + 输入框）─────────────────────────
// 避免重复模板，Modal 和 Drawer 共用同一内容结构
const ChatContent = defineComponent({
  name: 'ChatContent',
  props: {
    messages: { type: Array as () => any[], required: true },
    streamingIndex: { type: Number, required: true },
    inputValue: { type: String, required: true },
    isSending: { type: Boolean, required: true },
    quickQuestions: { type: Array as () => string[], required: true },
    messagesRefSetter: { type: Function as unknown as () => (el: HTMLElement | null) => void, required: true },
  },
  emits: ['update:inputValue', 'send', 'quickQuestion'],
  setup(props, { emit }) {
    return () =>
      h('div', { class: 'chat-container' }, [
        // 消息列表
        h(
          'div',
          {
            class: 'messages',
            ref: (el: any) => props.messagesRefSetter(el as HTMLElement | null),
          },
          [
            ...props.messages.map((msg: any, idx: number) =>
              h(
                'div',
                {
                  key: idx,
                  class: ['message-item', msg.role === 'user' ? 'user-message' : 'ai-message'],
                },
                [
                  h('div', { class: 'message-avatar' }, [
                    msg.role === 'user' ? h(UserOutlined) : h(RobotOutlined),
                  ]),
                  h('div', { class: 'message-content' }, [
                    props.streamingIndex === idx && !msg.content
                      ? h('div', { class: 'loading-dots' }, [
                          h('span'),
                          h('span'),
                          h('span'),
                        ])
                      : [
                          msg.role === 'assistant' &&
                          msg.reasoningSteps &&
                          msg.reasoningSteps.length > 0
                            ? h(ReasoningChain, {
                                steps: msg.reasoningSteps,
                                defaultOpen: false,
                              })
                            : null,
                          h('div', {
                            class: 'message-text markdown-content',
                            innerHTML: renderMarkdown(msg.content),
                          }),
                        ],
                  ]),
                ],
              ),
            ),
            // 空状态
            props.messages.length === 0
              ? h('div', { class: 'empty-state' }, [
                  h('div', { class: 'empty-icon-wrapper' }, [
                    h(RobotOutlined, { class: 'empty-icon' }),
                  ]),
                  h('h3', '您好，我是 AI 数据助手'),
                  h('p', '我可以帮您分析当前报表数据，回答相关问题'),
                ])
              : null,
          ],
        ),
        // 快捷提问
        props.messages.length === 0
          ? h('div', { class: 'suggestions' }, [
              h('div', { class: 'suggestion-title' }, '您可以问我：'),
              h(
                'div',
                { class: 'suggestion-chips' },
                props.quickQuestions.map((q: string) =>
                  h(
                    'button',
                    {
                      key: q,
                      class: 'suggestion-chip',
                      onClick: () => emit('quickQuestion', q),
                    },
                    q,
                  ),
                ),
              ),
            ])
          : null,
        // 输入框
        h('div', { class: 'input-area' }, [
          h('div', { class: 'input-inner' }, [
            h('input', {
              class: 'chat-input',
              value: props.inputValue,
              placeholder: '请输入您的问题...',
              disabled: props.isSending,
              onInput: (e: Event) => emit('update:inputValue', (e.target as HTMLInputElement).value),
              onKeydown: (e: KeyboardEvent) => {
                if (e.key === 'Enter' && !e.shiftKey) {
                  e.preventDefault();
                  emit('send');
                }
              },
            }),
            h(
              'button',
              {
                class: ['send-btn', props.isSending ? 'send-btn--loading' : ''],
                disabled: props.isSending,
                onClick: () => emit('send'),
              },
              props.isSending ? '发送中...' : '发送',
            ),
          ]),
        ]),
      ]);
  },
});
</script>

<style lang="less">
/* ── 浮动按钮 ────────────────────────────────────────── */
.chat-float-btn-wrapper {
  position: fixed;
  right: 32px;
  bottom: 32px;
  z-index: 1000;
}

.chat-trigger-btn {
  width: 56px;
  height: 56px;
  font-size: 24px;
  display: flex !important;
  align-items: center;
  justify-content: center;
  box-shadow: 0 4px 12px rgba(24, 144, 255, 0.3);

  .anticon {
    font-size: 24px;
    line-height: 1;
  }
}

/* ── 桌面 Modal ──────────────────────────────────────── */
.ai-chat-modal {
  .ant-modal-content {
    border-radius: 16px;
    overflow: hidden;
    box-shadow: 0 10px 40px rgba(0, 0, 0, 0.15);
  }

  .ant-modal-header {
    border-bottom: 1px solid #f0f0f0;
    padding: 16px 24px;
  }

  .ant-modal-body {
    padding: 0;
  }

  /* W3.3 — modal 关闭按钮触摸目标 */
  .ant-modal-close {
    min-width: 44px;
    min-height: 44px;
    display: flex;
    align-items: center;
    justify-content: center;
  }

  .ant-modal-close-x {
    width: 44px;
    height: 44px;
    line-height: 44px;
    display: flex;
    align-items: center;
    justify-content: center;
  }
}

/* ── 移动端 Drawer ────────────────────────────────────── */
.ai-chat-drawer {
  .ant-drawer-content-wrapper {
    border-radius: 16px 16px 0 0;
    overflow: hidden;
  }

  .ant-drawer-header {
    border-bottom: 1px solid #f0f0f0;
    padding: 16px 24px;
  }

  .ant-drawer-body {
    padding: 0;
    display: flex;
    flex-direction: column;
    overflow: hidden;
  }

  /* W3.3 — drawer 关闭按钮触摸目标 */
  .ant-drawer-close {
    min-width: 44px;
    min-height: 44px;
    display: flex;
    align-items: center;
    justify-content: center;
  }
}

/* ── 共享聊天内容 ─────────────────────────────────────── */
.chat-container {
  display: flex;
  flex-direction: column;
  height: 600px;
  background: #fcfcfc;

  /* Drawer 全屏时撑满 */
  .ai-chat-drawer & {
    height: 100%;
    min-height: 0;
  }
}

.messages {
  flex: 1;
  overflow-y: auto;
  padding: 20px;
  scroll-behavior: smooth;
  min-height: 0;
}

.message-item {
  display: flex;
  margin-bottom: 24px;
  animation: slideIn 0.3s cubic-bezier(0.16, 1, 0.3, 1);
  gap: 12px;

  &.user-message {
    flex-direction: row-reverse;

    .message-content {
      background: linear-gradient(135deg, #1890ff 0%, #0050b3 100%);
      color: #fff;
      border-radius: 12px 0 12px 12px;
      box-shadow: 0 4px 12px rgba(24, 144, 255, 0.2);

      .markdown-content {
        color: #fff;

        h1,
        h2,
        h3,
        h4,
        h5,
        h6 {
          color: #fff;
        }

        code {
          background: rgba(255, 255, 255, 0.2);
          color: #fff;
        }

        a {
          color: #fff;
          text-decoration: underline;
        }
      }
    }

    .message-avatar {
      background: #e6f7ff;
      color: #1890ff;
    }
  }

  &.ai-message {
    .message-content {
      background: #fff;
      color: #262626;
      border-radius: 0 12px 12px 12px;
      box-shadow: 0 2px 8px rgba(0, 0, 0, 0.04);
      border: 1px solid rgba(0, 0, 0, 0.03);
    }

    .message-avatar {
      background: linear-gradient(135deg, #52c41a 0%, #389e0d 100%);
      color: #fff;
      box-shadow: 0 2px 8px rgba(82, 196, 26, 0.3);
    }
  }
}

.message-avatar {
  width: 40px;
  height: 40px;
  border-radius: 50%;
  display: flex;
  align-items: center;
  justify-content: center;
  flex-shrink: 0;
  font-size: 20px;
}

.message-content {
  max-width: 80%;
  padding: 12px 16px;
  line-height: 1.6;
  position: relative;
}

.message-text {
  word-break: break-word;
}

.loading-dots {
  display: flex;
  align-items: center;
  gap: 4px;
  height: 24px;
  padding: 0 8px;

  span {
    display: block;
    width: 6px;
    height: 6px;
    background-color: #8c8c8c;
    border-radius: 50%;
    animation: typing 1.4s infinite ease-in-out both;

    &:nth-child(1) {
      animation-delay: -0.32s;
    }

    &:nth-child(2) {
      animation-delay: -0.16s;
    }
  }
}

@keyframes typing {

  0%,
  80%,
  100% {
    transform: scale(0);
  }

  40% {
    transform: scale(1);
  }
}

.markdown-content {
  font-size: 14px;

  h1,
  h2,
  h3,
  h4,
  h5,
  h6 {
    margin: 8px 0;
    font-weight: 600;
    line-height: 1.4;
  }

  p {
    margin: 8px 0;
  }

  ul,
  ol {
    margin: 8px 0;
    padding-left: 20px;
  }

  code {
    background: rgba(0, 0, 0, 0.05);
    padding: 2px 6px;
    border-radius: 4px;
    font-family: monospace;
    color: #f5222d;
  }

  pre {
    background: #f5f5f5;
    padding: 12px;
    border-radius: 8px;
    overflow-x: auto;
    margin: 8px 0;

    code {
      background: none;
      color: inherit;
      padding: 0;
    }
  }
}

.empty-state {
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  height: 100%;
  text-align: center;
  color: #8c8c8c;
  padding: 20px;

  .empty-icon-wrapper {
    width: 80px;
    height: 80px;
    background: #f0f5ff;
    border-radius: 50%;
    display: flex;
    align-items: center;
    justify-content: center;
    margin-bottom: 24px;
  }

  .empty-icon {
    font-size: 40px;
    color: #1890ff;
  }

  h3 {
    font-size: 18px;
    margin-bottom: 8px;
    color: #262626;
    font-weight: 600;
  }

  p {
    font-size: 14px;
    margin: 0;
    max-width: 240px;
  }
}

.suggestions {
  padding: 0 20px 16px;

  .suggestion-title {
    font-size: 12px;
    color: #999;
    margin-bottom: 8px;
  }

  .suggestion-chips {
    display: flex;
    flex-wrap: wrap;
    gap: 8px;
  }

  /* W3.3 — 快捷提问触摸目标 */
  .suggestion-chip {
    min-height: 44px;
    min-width: 44px;
    padding: 0 16px;
    border-radius: 22px;
    background: #fff;
    border: 1px solid #e8e8e8;
    color: #666;
    cursor: pointer;
    font-size: 13px;
    display: inline-flex;
    align-items: center;
    transition: color 0.2s, border-color 0.2s;

    &:hover {
      color: #1890ff;
      border-color: #1890ff;
    }
  }
}

/* ── 输入区域 ─────────────────────────────────────────── */
.input-area {
  padding: 16px 20px;
  background: #fff;
  border-top: 1px solid #f0f0f0;
  flex-shrink: 0;
}

.input-inner {
  display: flex;
  gap: 8px;
  align-items: center;
}

/* W3.3 — 输入框触摸目标 */
.chat-input {
  flex: 1;
  min-height: 44px;
  padding: 0 16px;
  border: 1px solid #d9d9d9;
  border-radius: 22px;
  font-size: 14px;
  outline: none;
  transition: border-color 0.2s;
  background: #fafafa;

  &:focus {
    border-color: #1890ff;
    background: #fff;
  }

  &:disabled {
    opacity: 0.6;
    cursor: not-allowed;
  }
}

/* W3.3 — 发送按钮触摸目标 */
.send-btn {
  min-height: 44px;
  min-width: 72px;
  padding: 0 20px;
  border-radius: 22px;
  background: #1890ff;
  color: #fff;
  border: none;
  font-size: 14px;
  cursor: pointer;
  transition: background 0.2s, opacity 0.2s;
  flex-shrink: 0;

  &:hover:not(:disabled) {
    background: #40a9ff;
  }

  &:disabled,
  &.send-btn--loading {
    opacity: 0.7;
    cursor: not-allowed;
  }
}

@keyframes slideIn {
  from {
    opacity: 0;
    transform: translateY(10px);
  }

  to {
    opacity: 1;
    transform: translateY(0);
  }
}
</style>
