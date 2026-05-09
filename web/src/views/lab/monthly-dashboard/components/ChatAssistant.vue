<template>
  <div class="chat-assistant-panel">
    <div ref="messagesEl" class="messages">
      <div
        v-for="(msg, idx) in messages"
        :key="idx"
        :class="['message-item', msg.role === 'user' ? 'user-message' : 'ai-message']"
        data-testid="message-bubble"
        :data-role="msg.role"
      >
        <div class="message-avatar">
          <UserOutlined v-if="msg.role === 'user'" />
          <RobotOutlined v-else />
        </div>
        <div class="message-content">
          <div v-if="streamingIndex === idx && !msg.content" class="loading-dots">
            <span></span><span></span><span></span>
          </div>
          <template v-else>
            <ReasoningChain
              v-if="msg.role === 'assistant' && msg.reasoningSteps && msg.reasoningSteps.length"
              :steps="msg.reasoningSteps"
              :default-open="streamingIndex === idx"
            />
            <div class="message-text markdown-content" v-html="renderMarkdown(msg.content)"></div>
          </template>
        </div>
      </div>
      <div v-if="messages.length === 0" class="empty-state">
        <div class="empty-icon-wrapper">
          <RobotOutlined class="empty-icon" />
        </div>
        <h3>您好，我是 AI 数据助手</h3>
        <p>我可以帮您分析当前报表数据，回答相关问题</p>
      </div>
    </div>

    <div v-if="messages.length === 0" class="suggestions">
      <div class="suggestion-title">您可以问我：</div>
      <div class="suggestion-chips">
        <button
          v-for="q in quickQuestions"
          :key="q"
          class="suggestion-chip"
          @click="handleQuickQuestion(q)"
        >
          {{ q }}
        </button>
      </div>
    </div>

    <div class="input-area">
      <div class="input-inner">
        <input
          v-model="inputValue"
          class="chat-input"
          data-testid="chat-input"
          placeholder="请输入您的问题..."
          :disabled="isSending"
          @keydown.enter.prevent="handleSend"
        />
        <button
          class="send-btn"
          :class="{ 'send-btn--loading': isSending }"
          data-testid="chat-send"
          :disabled="isSending"
          @click="handleSend"
        >
          {{ isSending ? '发送中...' : '发送' }}
        </button>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, nextTick, onMounted } from 'vue';
import { RobotOutlined, UserOutlined } from '@ant-design/icons-vue';
import { message } from 'ant-design-vue';
import { sendChatMessage } from '/@/api/lab/ai';
import { streamNlqChat } from '/@/api/nlqAgent';
import type { ReasoningStep } from '/@/types/reasoning-protocol';
import ReasoningChain from '/@/components/ReasoningChain.vue';
import Showdown from 'showdown';
import { useNlqSession } from './useNlqSession';

interface Props {
  reportData?: any;
}

const props = withDefaults(defineProps<Props>(), {
  reportData: null,
});

const { messages, appendMessage, updateLastMessage, broadcastTail } = useNlqSession();

const inputValue = ref('');
const isSending = ref(false);
const streamingIndex = ref<number>(-1);
const messagesEl = ref<HTMLElement | null>(null);

const converter = new Showdown.Converter({
  ghCodeBlocks: true,
  simplifiedAutoLink: true,
  strikethrough: true,
  tables: true,
  tasklists: true,
  requireSpaceBeforeHeadingText: true,
});

const quickQuestions = [
  '本月合格率是多少？',
  'A类产品产量有多少？',
  '哪个班次的合格率最高？',
  '不合格原因主要是什么？',
];

function renderMarkdown(content: string): string {
  return converter.makeHtml(content || '');
}

async function scrollToBottom() {
  await nextTick();
  if (messagesEl.value) {
    messagesEl.value.scrollTop = messagesEl.value.scrollHeight;
  }
}

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

  if (s.classAWeight || s.classBWeight) {
    prompt += `- A类：${(s.classAWeight || 0).toLocaleString()} kg (${(s.classARate || 0).toFixed(2)}%)
- B类：${(s.classBWeight || 0).toLocaleString()} kg (${(s.classBRate || 0).toFixed(2)}%)
`;
  }

  if (unqualifiedStats.length > 0) {
    prompt += `\n【不合格分类详情】\n`;
    unqualifiedStats.slice(0, 5).forEach((item: any) => {
      prompt += `- ${item.categoryName}: ${(item.weight || 0).toLocaleString()} kg (${(item.rate || 0).toFixed(2)}%)\n`;
    });
  }

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

async function handleSend() {
  const text = inputValue.value.trim();
  if (!text || isSending.value) return;

  appendMessage({ role: 'user', content: text });
  inputValue.value = '';
  await scrollToBottom();

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
          scrollToBottom();
        },
        onDone() {
          if (!accumulated) {
            updateLastMessage({ content: '抱歉，我无法生成回复。' });
          }
        },
        onError(err: Error) {
          console.warn('nlqAgent SSE error, falling back to REST:', err);
          if (!hasStreamed) {
            sendChatMessage({ message: text, systemPrompt: buildSystemPrompt() })
              .then((response) => {
                updateLastMessage({ content: response.response || '抱歉，我无法生成回复。' });
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
    broadcastTail();
    await scrollToBottom();
  }
}

function handleQuickQuestion(q: string) {
  inputValue.value = q;
  handleSend();
}

onMounted(() => {
  if (messages.value.length === 0) {
    setTimeout(() => {
      if (messages.value.length === 0) {
        appendMessage({
          role: 'assistant',
          content:
            '您好！我是 **AI 数据助手**。\n\n我可以帮您分析当前报表数据，回答关于合格率、产量、班次对比等问题。请问有什么可以帮助您的？',
        });
      }
    }, 200);
  }
});
</script>

<style lang="less">
.chat-assistant-panel {
  display: flex;
  flex-direction: column;
  height: 100%;
  min-height: 0;
  background: #fcfcfc;
}

.chat-assistant-panel .messages {
  flex: 1;
  overflow-y: auto;
  padding: 20px;
  scroll-behavior: smooth;
  min-height: 0;
}

.chat-assistant-panel .message-item {
  display: flex;
  margin-bottom: 24px;
  animation: chat-slide-in 0.3s cubic-bezier(0.16, 1, 0.3, 1);
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

        h1, h2, h3, h4, h5, h6 { color: #fff; }
        code { background: rgba(255, 255, 255, 0.2); color: #fff; }
        a { color: #fff; text-decoration: underline; }
      }
    }

    .message-avatar { background: #e6f7ff; color: #1890ff; }
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

.chat-assistant-panel .message-avatar {
  width: 40px;
  height: 40px;
  border-radius: 50%;
  display: flex;
  align-items: center;
  justify-content: center;
  flex-shrink: 0;
  font-size: 20px;
}

.chat-assistant-panel .message-content {
  max-width: 80%;
  padding: 12px 16px;
  line-height: 1.6;
  position: relative;
}

.chat-assistant-panel .message-text { word-break: break-word; }

.chat-assistant-panel .loading-dots {
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
    animation: chat-typing 1.4s infinite ease-in-out both;

    &:nth-child(1) { animation-delay: -0.32s; }
    &:nth-child(2) { animation-delay: -0.16s; }
  }
}

.chat-assistant-panel .markdown-content {
  font-size: 14px;

  h1, h2, h3, h4, h5, h6 {
    margin: 8px 0;
    font-weight: 600;
    line-height: 1.4;
  }

  p { margin: 8px 0; }

  ul, ol { margin: 8px 0; padding-left: 20px; }

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

    code { background: none; color: inherit; padding: 0; }
  }
}

.chat-assistant-panel .empty-state {
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

  .empty-icon { font-size: 40px; color: #1890ff; }

  h3 {
    font-size: 18px;
    margin-bottom: 8px;
    color: #262626;
    font-weight: 600;
  }

  p { font-size: 14px; margin: 0; max-width: 240px; }
}

.chat-assistant-panel .suggestions {
  padding: 0 20px 16px;

  .suggestion-title { font-size: 12px; color: #999; margin-bottom: 8px; }

  .suggestion-chips { display: flex; flex-wrap: wrap; gap: 8px; }

  .suggestion-chip {
    min-height: 36px;
    padding: 0 16px;
    border-radius: 18px;
    background: #fff;
    border: 1px solid #e8e8e8;
    color: #666;
    cursor: pointer;
    font-size: 13px;
    display: inline-flex;
    align-items: center;
    transition: color 0.2s, border-color 0.2s;

    &:hover { color: #1890ff; border-color: #1890ff; }
  }
}

.chat-assistant-panel .input-area {
  padding: 16px 20px;
  background: #fff;
  border-top: 1px solid #f0f0f0;
  flex-shrink: 0;
}

.chat-assistant-panel .input-inner {
  display: flex;
  gap: 8px;
  align-items: center;
}

.chat-assistant-panel .chat-input {
  flex: 1;
  min-height: 40px;
  padding: 0 16px;
  border: 1px solid #d9d9d9;
  border-radius: 20px;
  font-size: 14px;
  outline: none;
  transition: border-color 0.2s;
  background: #fafafa;

  &:focus { border-color: #1890ff; background: #fff; }

  &:disabled { opacity: 0.6; cursor: not-allowed; }
}

.chat-assistant-panel .send-btn {
  min-height: 40px;
  min-width: 72px;
  padding: 0 20px;
  border-radius: 20px;
  background: #1890ff;
  color: #fff;
  border: none;
  font-size: 14px;
  cursor: pointer;
  transition: background 0.2s, opacity 0.2s;
  flex-shrink: 0;

  &:hover:not(:disabled) { background: #40a9ff; }

  &:disabled,
  &.send-btn--loading { opacity: 0.7; cursor: not-allowed; }
}

@keyframes chat-typing {
  0%, 80%, 100% { transform: scale(0); }
  40% { transform: scale(1); }
}

@keyframes chat-slide-in {
  from { opacity: 0; transform: translateY(10px); }
  to { opacity: 1; transform: translateY(0); }
}
</style>
