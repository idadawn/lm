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

    <!-- 对话弹窗 -->
    <a-modal v-model:open="visible" title="AI 数据助手" :footer="null" width="600px" centered :destroyOnClose="true"
      wrapClassName="chat-modal-wrapper" :maskClosable="false" class="ai-chat-modal">
      <template #closeIcon>
        <CloseOutlined />
      </template>

      <div class="chat-container">
        <!-- 消息列表 -->
        <div class="messages" ref="messagesRef">
          <div v-for="msg in messages" :key="msg.id"
            :class="['message-item', msg.role === 'user' ? 'user-message' : 'ai-message']">
            <div class="message-avatar">
              <UserOutlined v-if="msg.role === 'user'" />
              <RobotOutlined v-else />
            </div>
            <div class="message-content">
              <div v-if="msg.loading" class="loading-dots">
                <span></span><span></span><span></span>
              </div>
              <div v-else class="message-text markdown-content" v-html="renderMarkdown(msg.content)"></div>
            </div>
          </div>

          <!-- 空状态 -->
          <div v-if="messages.length === 0" class="empty-state">
            <div class="empty-icon-wrapper">
              <RobotOutlined class="empty-icon" />
            </div>
            <h3>您好，我是 AI 数据助手</h3>
            <p>我可以帮您分析当前报表数据，回答相关问题</p>
          </div>
        </div>

        <!-- 快捷提问 -->
        <div v-if="messages.length === 0" class="suggestions">
          <div class="suggestion-title">您可以问我：</div>
          <div class="suggestion-chips">
            <a-button v-for="q in quickQuestions" :key="q" size="small" class="suggestion-chip"
              @click="handleQuickQuestion(q)">
              {{ q }}
            </a-button>
          </div>
        </div>

        <!-- 输入框 -->
        <div class="input-area">
          <a-input-search v-model:value="inputValue" :loading="isSending" placeholder="请输入您的问题..." enter-button="发送"
            size="large" @search="handleSend" :disabled="isSending" />
        </div>
      </div>
    </a-modal>
  </div>
</template>

<script setup lang="ts">
import { ref, nextTick, watch, computed } from 'vue';
import { RobotOutlined, UserOutlined, CloseOutlined } from '@ant-design/icons-vue';
import { message } from 'ant-design-vue';
import { sendChatMessage, type ChatMessage } from '/@/api/lab/ai';
import Showdown from 'showdown';

// Props
interface Props {
  reportData?: any;
}

const props = withDefaults(defineProps<Props>(), {
  reportData: null,
});

// 状态
const visible = ref(false);
const inputValue = ref('');
const isSending = ref(false);
const messages = ref<ChatMessage[]>([]);
const messagesRef = ref<HTMLElement | null>(null);

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

// 未读消息数
const unreadCount = computed(() => 0);

// 渲染 Markdown (使用 showdown)
function renderMarkdown(content: string): string {
  return converter.makeHtml(content);
}

// 切换聊天窗口
function toggleChat() {
  visible.value = !visible.value;
}

// 发送消息
async function handleSend() {
  const text = inputValue.value.trim();
  if (!text || isSending.value) return;

  // 添加用户消息
  const userMsg: ChatMessage = {
    id: `user-${Date.now()}`,
    role: 'user',
    content: text,
    createdAt: new Date().toISOString(),
  };
  messages.value.push(userMsg);
  inputValue.value = '';

  // 滚动到底部
  await scrollToBottom();

  // 添加 AI 消息（loading 状态）
  const aiMsg: ChatMessage = {
    id: `ai-${Date.now()}`,
    role: 'assistant',
    content: '',
    createdAt: new Date().toISOString(),
    loading: true,
  };
  messages.value.push(aiMsg);

  isSending.value = true;

  try {
    // 构建 system prompt（包含当前数据上下文）
    const systemPrompt = buildSystemPrompt();

    // 调用 AI 接口
    const response = await sendChatMessage({
      message: text,
      systemPrompt,
    });

    // 更新 AI 消息
    aiMsg.content = response.response || '抱歉，我无法生成回复。';
    aiMsg.loading = false;
  } catch (error) {
    console.error('AI 接口调用失败:', error);
    aiMsg.content = `抱歉，处理您的请求时出现错误。`;
    aiMsg.loading = false;
    message.error('AI 助手暂时无法响应，请稍后重试');
  } finally {
    isSending.value = false;
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

// 监听弹窗打开，初始化欢迎消息
watch(visible, (newVal) => {
  if (newVal && messages.value.length === 0) {
    // delay slightly for effect
    setTimeout(() => {
      messages.value.push({
        id: 'welcome',
        role: 'assistant',
        content: '您好！我是 **AI 数据助手**。\n\n我可以帮您分析当前报表数据，回答关于合格率、产量、班次对比等问题。请问有什么可以帮助您的？',
        createdAt: new Date().toISOString(),
      });
    }, 300);
  }
});
</script>

<style lang="less">
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
}

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

.chat-container {
  display: flex;
  flex-direction: column;
  height: 600px;
  background: #fcfcfc;
}

.messages {
  flex: 1;
  overflow-y: auto;
  padding: 20px;
  scroll-behavior: smooth;
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

  .suggestion-chip {
    border-radius: 16px;
    background: #fff;
    border-color: #e8e8e8;
    color: #666;

    &:hover {
      color: #1890ff;
      border-color: #1890ff;
    }
  }
}

.input-area {
  padding: 16px 20px;
  background: #fff;
  border-top: 1px solid #f0f0f0;

  .ant-input-search {
    .ant-input-group {
      .ant-input {
        border-radius: 20px 0 0 20px;
        padding-left: 20px;
      }

      .ant-input-search-button {
        border-radius: 0 20px 20px 0;
      }
    }
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
