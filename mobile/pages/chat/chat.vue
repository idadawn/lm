<template>
  <view class="chat-page">
    <!-- 消息列表 -->
    <scroll-view
      class="chat-list"
      scroll-y
      :scroll-into-view="scrollToView"
      :scroll-with-animation="true"
    >
      <view class="chat-welcome" v-if="messages.length === 0">
        <view class="welcome-icon">
          <text class="welcome-icon-text">智</text>
        </view>
        <text class="welcome-title">智能对话助手</text>
        <text class="welcome-desc">支持自然语言查询检测数据、分析趋势、生成报表</text>
        <view class="quick-actions">
          <view
            class="quick-tag"
            v-for="(tag, idx) in quickTags"
            :key="idx"
            @click="sendQuick(tag)"
          >
            <text>{{ tag }}</text>
          </view>
        </view>
      </view>

      <view
        v-for="(msg, index) in messages"
        :key="index"
        :id="'msg-' + index"
        class="msg-row"
        :class="msg.role"
      >
        <view class="msg-avatar" v-if="msg.role === 'assistant'">
          <text class="msg-avatar-text">AI</text>
        </view>
        <view class="msg-bubble">
          <!-- user messages: plain text (no markdown injection vector) -->
          <text v-if="msg.role === 'user'" class="msg-text">{{ msg.content }}</text>
          <!-- assistant messages: markdown rendered per platform -->
          <template v-else>
            <!-- #ifdef MP-WEIXIN || APP-PLUS -->
            <mp-html :content="renderMarkdown(msg.content)" class="msg-md" />
            <!-- #endif -->
            <!-- #ifdef H5 -->
            <view class="msg-md" v-html="sanitizeHtml(renderMarkdown(msg.content))"></view>
            <!-- #endif -->
          </template>
        </view>
        <view class="msg-avatar user-avatar" v-if="msg.role === 'user'">
          <text class="msg-avatar-text">我</text>
        </view>
      </view>

      <!-- 推理链 -->
      <view v-if="reasoningSteps.length > 0" class="reasoning-wrap">
        <kg-reasoning-chain :steps="reasoningSteps" :default-open="false" />
      </view>

      <!-- 加载中 -->
      <view v-if="loading" class="msg-row assistant">
        <view class="msg-avatar">
          <text class="msg-avatar-text">AI</text>
        </view>
        <view class="msg-bubble loading-bubble">
          <view class="typing-dot"></view>
          <view class="typing-dot"></view>
          <view class="typing-dot"></view>
        </view>
      </view>

      <view id="chat-bottom"></view>
    </scroll-view>

    <!-- 输入区 -->
    <view class="input-bar" :style="keyboardHeight > 0 ? { paddingBottom: keyboardHeight + 'px' } : {}">
      <input
        class="chat-input"
        v-model="inputText"
        placeholder="输入问题..."
        :disabled="loading"
        confirm-type="send"
        @confirm="sendMessage"
      />
      <view
        class="send-btn"
        :class="{ active: inputText.trim() && !loading }"
        @click="sendMessage"
      >
        <text class="send-btn-text">发送</text>
      </view>
    </view>
  </view>
</template>

<script setup>
import { ref, computed, nextTick, onMounted, onUnmounted } from 'vue'
import { streamNlqChat } from '@/utils/sse-client.js'
import KgReasoningChain from '@/components/kg-reasoning-chain/kg-reasoning-chain.vue'
import { renderMarkdown, sanitizeHtml } from '@/utils/markdown.js'
// #ifdef MP-WEIXIN || APP-PLUS
import MpHtml from 'mp-html/dist/uni-app/components/mp-html/mp-html.vue'
// #endif

const messages = ref([])

// ── Session 持久化(plan v3 W1.5 mobile,与 web 端 schema 兼容) ──
const STORAGE_KEY = 'nlq-sessions'
const SCHEMA_VERSION = 1

function makeId() {
  return `mobile-${Date.now()}-${Math.random().toString(36).slice(2, 9)}`
}

const currentSessionId = ref('')

function loadSessions() {
  let raw
  try {
    raw = uni.getStorageSync(STORAGE_KEY)
  } catch (_) {
    raw = ''
  }
  if (!raw) return
  let parsed
  try {
    parsed = JSON.parse(raw)
  } catch (_) {
    // 数据损坏:清空
    try { uni.removeStorageSync(STORAGE_KEY) } catch (_) {}
    return
  }
  if (parsed?.schema_version !== SCHEMA_VERSION) {
    // 老版本数据 wipe
    try { uni.removeStorageSync(STORAGE_KEY) } catch (_) {}
    return
  }
  const last = Array.isArray(parsed.sessions) ? parsed.sessions[parsed.sessions.length - 1] : null
  if (!last) return
  currentSessionId.value = last.id
  messages.value = Array.isArray(last.messages) ? last.messages.slice() : []
}

function saveSessions() {
  if (!currentSessionId.value) currentSessionId.value = makeId()
  const payload = {
    schema_version: SCHEMA_VERSION,
    sessions: [
      {
        id: currentSessionId.value,
        messages: messages.value.slice(),
        updated_at: Date.now(),
      },
    ],
  }
  try {
    uni.setStorageSync(STORAGE_KEY, JSON.stringify(payload))
  } catch (_) {
    // 写入失败(配额、storage disabled 等)— 静默,不破坏 UX
  }
}

// 启动时立即恢复
loadSessions()

const inputText = ref('')
const loading = ref(false)
const reasoningSteps = ref([])
const scrollToView = ref('')

// 键盘高度:键盘弹起时上移输入栏,避免输入框被遮挡。
// uni.onKeyboardHeightChange 在 MP-WEIXIN / APP-PLUS 有效;H5 键盘由浏览器自动处理。
const keyboardHeight = ref(0)

onMounted(() => {
  uni.onKeyboardHeightChange((res) => {
    keyboardHeight.value = res.height || 0
  })
})

onUnmounted(() => {
  uni.offKeyboardHeightChange()
})

const quickTags = [
  '本月合格率是多少？',
  '叠片系数趋势如何？',
  '最近有哪些不合格项？',
  '今天产量多少？'
]

function scrollToBottom() {
  nextTick(() => {
    scrollToView.value = 'chat-bottom'
    setTimeout(() => {
      scrollToView.value = ''
    }, 300)
  })
}

function sendQuick(text) {
  inputText.value = text
  sendMessage()
}

function sendMessage() {
  const text = inputText.value.trim()
  if (!text || loading.value) return

  messages.value.push({ role: 'user', content: text })
  saveSessions()
  inputText.value = ''
  loading.value = true
  reasoningSteps.value = []
  scrollToBottom()

  let assistantContent = ''
  const currentIndex = messages.value.length

  // 先占位
  messages.value.push({ role: 'assistant', content: '' })

  streamNlqChat(
    {
      messages: [{ role: 'user', content: text }],
      session_id: `mobile-${Date.now()}`
    },
    {
      onText: (chunk) => {
        assistantContent += chunk
        messages.value[currentIndex].content = assistantContent
        scrollToBottom()
      },
      onReasoningStep: (step) => {
        reasoningSteps.value.push(step)
      },
      onResponseMetadata: (payload) => {
        if (Array.isArray(payload.reasoning_steps) && payload.reasoning_steps.length > 0) {
          reasoningSteps.value = payload.reasoning_steps.slice()
        }
      },
      onError: (err) => {
        messages.value[currentIndex].content = '请求出错：' + (err.message || '未知错误')
        loading.value = false
        scrollToBottom()
      },
      onDone: () => {
        loading.value = false
        saveSessions()
        scrollToBottom()
      }
    }
  )
}
</script>

<style lang="scss">
.chat-page {
  min-height: 100vh;
  background: #f7f9fc;
  display: flex;
  flex-direction: column;
  box-sizing: border-box;
}

.chat-list {
  flex: 1;
  overflow-y: auto;
  padding: 16px;
  box-sizing: border-box;
}

/* 欢迎页 */
.chat-welcome {
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  padding: 60px 24px;
  text-align: center;
}

.welcome-icon {
  width: 64px;
  height: 64px;
  border-radius: 50%;
  background: linear-gradient(135deg, #1890ff, #40a9ff);
  display: flex;
  align-items: center;
  justify-content: center;
  margin-bottom: 16px;
  box-shadow: 0 4px 12px rgba(24, 144, 255, 0.3);
}

.welcome-icon-text {
  font-size: 28px;
  font-weight: 700;
  color: #ffffff;
}

.welcome-title {
  font-size: 20px;
  font-weight: 700;
  color: #262626;
  margin-bottom: 8px;
}

.welcome-desc {
  font-size: 14px;
  color: #8c8c8c;
  line-height: 1.6;
  margin-bottom: 24px;
}

.quick-actions {
  display: flex;
  flex-wrap: wrap;
  justify-content: center;
  gap: 10px;
}

.quick-tag {
  background: #ffffff;
  border: 1px solid #e4e7ed;
  border-radius: 16px;
  padding: 8px 16px;
  font-size: 13px;
  color: #595959;
  transition: all 0.2s;
}

.quick-tag:active {
  background: #e6f7ff;
  border-color: #1890ff;
  color: #1890ff;
}

/* 消息 */
.msg-row {
  display: flex;
  align-items: flex-start;
  margin-bottom: 16px;
}

.msg-row.user {
  justify-content: flex-end;
}

.msg-avatar {
  width: 32px;
  height: 32px;
  border-radius: 50%;
  background: linear-gradient(135deg, #1890ff, #40a9ff);
  display: flex;
  align-items: center;
  justify-content: center;
  flex-shrink: 0;
  margin-right: 8px;
}

.user-avatar {
  background: linear-gradient(135deg, #52c41a, #73d13d);
  margin-right: 0;
  margin-left: 8px;
}

.msg-avatar-text {
  font-size: 12px;
  font-weight: 600;
  color: #ffffff;
}

.msg-bubble {
  max-width: 70%;
  padding: 10px 14px;
  border-radius: 12px;
  background: #ffffff;
  box-shadow: 0 2px 6px rgba(0, 0, 0, 0.04);
}

.msg-row.user .msg-bubble {
  background: #e6f7ff;
}

.msg-text {
  font-size: 14px;
  color: #262626;
  line-height: 1.6;
  word-break: break-word;
}

.loading-bubble {
  display: flex;
  align-items: center;
  gap: 6px;
  padding: 14px 16px;
}

.typing-dot {
  width: 8px;
  height: 8px;
  border-radius: 50%;
  background: #bfbfbf;
  animation: typing-bounce 1.4s infinite ease-in-out both;
}

.typing-dot:nth-child(1) {
  animation-delay: -0.32s;
}

.typing-dot:nth-child(2) {
  animation-delay: -0.16s;
}

@keyframes typing-bounce {
  0%, 80%, 100% {
    transform: scale(0.6);
    opacity: 0.4;
  }
  40% {
    transform: scale(1);
    opacity: 1;
  }
}

/* 推理链包裹 */
.reasoning-wrap {
  margin-bottom: 16px;
  padding: 0 40px;
}

/* 输入栏 */
.input-bar {
  display: flex;
  align-items: center;
  gap: 10px;
  padding: 10px 12px  calc(10px + env(safe-area-inset-bottom));
  background: #ffffff;
  border-top: 1px solid #f0f0f0;
}

.chat-input {
  flex: 1;
  height: 40px;
  background: #f5f7fa;
  border-radius: 20px;
  padding: 0 16px;
  font-size: 14px;
  color: #262626;
}

.send-btn {
  padding: 8px 18px;
  border-radius: 20px;
  background: #d9d9d9;
  transition: background 0.2s;
}

.send-btn.active {
  background: linear-gradient(135deg, #1890ff, #40a9ff);
}

.send-btn-text {
  font-size: 14px;
  font-weight: 600;
  color: #ffffff;
}
</style>
