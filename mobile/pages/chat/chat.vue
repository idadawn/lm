<template>
  <view class="chat-page">
    <!-- 消息列表 -->
    <scroll-view
      class="chat-list"
      scroll-y
      :scroll-into-view="scrollToView"
      :scroll-with-animation="true"
      :upper-threshold="50"
      :lower-threshold="80"
      @scroll="onChatScroll"
      @scrolltolower="onScrollToLower"
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
        <view class="msg-bubble assistant-card" v-if="msg.role === 'assistant'" @longpress="copyMessage(msg)">
          <!-- 长按 0.5s 复制：手机原生交互习惯，统一应用于 user / assistant 两侧 -->
          <!-- 推理过程：流式期间默认展开，流式结束自动收起 -->
          <KgReasoningChain
            v-if="msg.reasoningSteps && msg.reasoningSteps.length > 0"
            :steps="msg.reasoningSteps"
            :default-open="loading && index === messages.length - 1"
          />

          <!-- 工具调用卡片 -->
          <view v-if="msg.toolCalls && msg.toolCalls.length > 0" class="tool-cards">
            <view
              class="tool-card"
              v-for="(tool, tidx) in msg.toolCalls"
              :key="tidx"
              :class="tool.status"
            >
              <view class="tool-header">
                <text class="tool-dot" :class="tool.status"></text>
                <text class="tool-name">{{ tool.displayName || tool.name }}</text>
                <text v-if="tool.duration_ms" class="tool-duration">{{ tool.duration_ms }}ms</text>
              </view>
            </view>
          </view>

          <!-- Markdown 正文 -->
          <!-- #ifdef MP-WEIXIN || APP-PLUS -->
          <mp-html
            :content="renderMarkdown(msg.content)"
            :tag-style="mdTagStyle"
            class="msg-md"
          />
          <!-- #endif -->
          <!-- #ifdef H5 -->
          <view class="msg-md" v-html="sanitizeHtml(renderMarkdown(msg.content))"></view>
          <!-- #endif -->

          <!-- 图表（donut/bar/line）— 移动端简化为横向 bar 列表，最易读 -->
          <ChatChartBubble
            v-if="msg.chartConfig"
            :chart-config="msg.chartConfig"
          />

          <!-- 📎 来源：当 KB 命中时显示，可折叠列出 source -->
          <view v-if="msg.citations && msg.citations.length > 0" class="citations-bar">
            <view class="citations-toggle" @click="toggleCitations(index)">
              <text class="citations-icon">📎</text>
              <text class="citations-label">来源 {{ msg.citations.length }}</text>
              <text v-if="msg.kbConfidence != null" class="citations-conf">
                · 置信度 {{ Math.round(msg.kbConfidence * 100) }}%
              </text>
              <text class="citations-arrow">{{ msg.citationsOpen ? '▾' : '▸' }}</text>
            </view>
            <view v-if="msg.citationsOpen" class="citations-list">
              <view
                class="citations-item"
                v-for="(src, i) in friendlyCitations(msg.citations)"
                :key="i"
              >
                <text class="citations-bullet">{{ i + 1 }}.</text>
                <text class="citations-text" :selectable="true" :user-select="true">{{ src.label }}</text>
              </view>
            </view>
          </view>
        </view>

        <view class="msg-bubble" v-else @longpress="copyMessage(msg)">
          <text class="msg-text" :selectable="true" :user-select="true">{{ msg.content }}</text>
        </view>

        <view class="msg-avatar user-avatar" v-if="msg.role === 'user'">
          <text class="msg-avatar-text">我</text>
        </view>
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

    <!-- 浮动「一键到底」按钮：用户向上滚出底部区域后出现，点击平滑滚回最新消息 -->
    <view
      v-if="showScrollToBottomBtn"
      class="scroll-to-bottom-btn"
      :style="{ bottom: scrollBtnBottom + 'px' }"
      @click="jumpToBottom"
    >
      <text class="scroll-to-bottom-icon">↓</text>
      <text v-if="unreadCount > 0" class="scroll-to-bottom-badge">{{ unreadCount > 99 ? '99+' : unreadCount }}</text>
    </view>

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
        :class="{ active: inputText.trim() && !loading, stop: loading }"
        @click="loading ? stopGeneration() : sendMessage()"
      >
        <text class="send-btn-text">{{ loading ? '停止' : '发送' }}</text>
      </view>
    </view>
  </view>
</template>

<script setup>
import { ref, computed, nextTick, onMounted, onUnmounted } from 'vue'
import { streamNlqChat } from '@/utils/sse-client.js'
import KgReasoningChain from '@/components/kg-reasoning-chain/kg-reasoning-chain.vue'
import ChatChartBubble from '@/components/chat-chart-bubble/chat-chart-bubble.vue'
import { renderMarkdown, sanitizeHtml } from '@/utils/markdown.js'
// #ifdef MP-WEIXIN || APP-PLUS
import MpHtml from 'mp-html/dist/uni-app/components/mp-html/mp-html.vue'
// #endif

const messages = ref([])

// ── Session 持久化 ──
const STORAGE_KEY = 'nlq-sessions'
const SCHEMA_VERSION = 1

function makeId() {
  return `mobile-${Date.now()}-${Math.random().toString(36).slice(2, 9)}`
}

const currentSessionId = ref('')
let currentRequestTask = null

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
    try { uni.removeStorageSync(STORAGE_KEY) } catch (_) {}
    return
  }
  if (parsed?.schema_version !== SCHEMA_VERSION) {
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
  } catch (_) {}
}

// 启动时立即恢复
loadSessions()

const inputText = ref('')
const loading = ref(false)
const scrollToView = ref('')

// 键盘高度
const keyboardHeight = ref(0)

// ── 滚动状态 ──
// 当用户向上滚动离开底部时，showScrollToBottomBtn 变 true，显示「一键到底」浮动按钮。
// 流式更新时若用户在底部，自动跟随；用户离开底部后，新消息累计到 unreadCount。
const showScrollToBottomBtn = ref(false)
const unreadCount = ref(0)
const atBottom = ref(true)        // 当前是否在底部（容差 100px）
const scrollBtnBottom = computed(() => 64 + (keyboardHeight.value || 0))

onMounted(() => {
  uni.onKeyboardHeightChange((res) => {
    keyboardHeight.value = res.height || 0
  })
})

onUnmounted(() => {
  uni.offKeyboardHeightChange()
})

// 与后端已实现的快路径对齐，避免落到 chat2sql 撞墙
// mp-html 在 APP-PLUS / MP-WEIXIN 里不会被外部 CSS 命中——
// 必须用 :tag-style 把样式注入到 pre / code / table 等标签上。
const mdTagStyle = {
  pre: 'font-family:FiraCode,monospace;background:#1e293b;color:#f1f5f9;padding:24rpx 28rpx;border-radius:12rpx;margin:20rpx 0;font-size:24rpx;line-height:1.55;overflow-x:auto;',
  code: 'font-family:FiraCode,monospace;background:rgba(15,23,42,0.06);color:#d63384;padding:2rpx 10rpx;border-radius:6rpx;font-size:26rpx;',
  table: 'width:100%;border-collapse:collapse;border:1rpx solid #e2e8f0;border-radius:10rpx;font-size:24rpx;margin:16rpx 0;',
  th: 'background:#f1f5f9;font-weight:600;padding:10rpx 16rpx;border:1rpx solid #e2e8f0;text-align:left;',
  td: 'padding:10rpx 16rpx;border:1rpx solid #e2e8f0;text-align:left;',
}

// 与后端已实现的快路径对齐，避免落到 chat2sql 撞墙
const quickTags = [
  '本月一次交检合格率是多少？',
  '上个月质量等级分布',
  '一共有几种产品规格',
  '贴标的判断依据是什么'
]

function scrollToBottom() {
  nextTick(() => {
    scrollToView.value = 'chat-bottom'
    setTimeout(() => {
      scrollToView.value = ''
    }, 300)
  })
}

// 滚动事件：判断当前是否在底部（scrollHeight - scrollTop - clientHeight < 100 算在底部）
function onChatScroll(e) {
  const d = e.detail || {}
  const scrollTop = d.scrollTop || 0
  const scrollHeight = d.scrollHeight || 0
  // uni-app scroll-view 没直接给 clientHeight，用 scrollTop + clientHeight ≈ scrollHeight 判断
  // 兼容做法：deltaY 小于 100 时认为接近底部
  const nearBottom = (scrollHeight - scrollTop) < (uni.getSystemInfoSync().windowHeight + 100)
  atBottom.value = nearBottom
  // 不在底部就显示按钮
  showScrollToBottomBtn.value = !nearBottom
  // 用户滚到底部后，清空未读计数
  if (nearBottom && unreadCount.value > 0) {
    unreadCount.value = 0
  }
}

// scroll-view 触底事件（uni-app 默认 lower-threshold 后触发）：清零未读 + 隐藏按钮
function onScrollToLower() {
  atBottom.value = true
  showScrollToBottomBtn.value = false
  unreadCount.value = 0
}

// 用户点击「一键到底」按钮
function jumpToBottom() {
  showScrollToBottomBtn.value = false
  unreadCount.value = 0
  atBottom.value = true
  scrollToBottom()
}

function sendQuick(text) {
  inputText.value = text
  sendMessage()
}

// ── 来源 citations 折叠/复制 ─────────────────────────
function toggleCitations(index) {
  const msg = messages.value[index]
  if (!msg) return
  msg.citationsOpen = !msg.citationsOpen
}

// 把后端原始 citation（含文件路径 / # fragment）翻译成"业务依据"业务标签：
// - knowledge_base.json#xxx → 业务术语知识库
// - docs/development-guide.md → 系统使用手册
// 同名标签去重，最终返回 [{ raw, label }]
function friendlyCitations(raw) {
  if (!raw || !raw.length) return []
  const result = []
  const seen = new Set()
  for (const src of raw) {
    if (!src) continue
    const lower = String(src).toLowerCase()
    let label = '业务文档'
    if (lower.includes('knowledge_base.json') || lower.includes('knowledgebase.json')) {
      label = '业务术语知识库'
    } else if (lower.includes('judgmentrules') || lower.includes('judgment_rules')) {
      label = '判定规则'
    } else if (lower.includes('lab_report_config') || lower.includes('report-config') || lower.includes('reportconfig')) {
      label = '报表配置'
    } else if (lower.includes('dimensionsmeta') || lower.includes('dimensions_meta') || lower.includes('dimensions/')) {
      label = '业务维度库'
    } else if (lower.includes('kg-frontend-ontology') || lower.includes('ontology')) {
      label = '知识图谱本体'
    } else if (lower.includes('development-guide')) {
      label = '系统使用手册'
    } else if (lower.includes('lightrag') || lower.startsWith('lr#') || lower.includes('lightrag-index')) {
      label = '业务知识图谱'
    } else if (lower.startsWith('docs/')) {
      label = '业务文档'
    } else if (lower.startsWith('neo4j:') || lower.includes('neo4j')) {
      label = '知识图谱节点'
    }
    if (!seen.has(label)) {
      seen.add(label)
      result.push({ raw: String(src), label })
    }
  }
  return result
}

function copyText(text) {
  if (!text) return
  uni.setClipboardData({
    data: String(text),
    showToast: false,
    success: () => uni.showToast({ title: '已复制', icon: 'success', duration: 1000 }),
    fail: () => uni.showToast({ title: '复制失败', icon: 'none' }),
  })
}

// ── 长按复制消息 ────────────────────────────────────────
// 用户消息和助手消息都支持。复制助手消息时，把 markdown 原文复制（含表格 / SQL），
// 不复制 HTML，便于粘贴到其他地方查看。
function copyMessage(msg) {
  if (!msg || !msg.content) return
  const text = typeof msg.content === 'string' ? msg.content : String(msg.content)
  uni.setClipboardData({
    data: text,
    showToast: false,
    success: () => {
      uni.showToast({ title: '已复制', icon: 'success', duration: 1200 })
      // #ifdef APP-PLUS
      try { uni.vibrateShort && uni.vibrateShort({ type: 'light' }) } catch (_) {}
      // #endif
    },
    fail: () => uni.showToast({ title: '复制失败', icon: 'none' })
  })
}


function stopGeneration() {
  if (currentRequestTask && currentRequestTask.abort) {
    currentRequestTask.abort()
    currentRequestTask = null
  }
  loading.value = false
  // 把当前 assistant 消息标记为 cancelled
  const lastMsg = messages.value[messages.value.length - 1]
  if (lastMsg && lastMsg.role === 'assistant') {
    lastMsg.status = 'cancelled'
    saveSessions()
  }
}

function sendMessage() {
  const text = inputText.value.trim()
  if (!text || loading.value) return

  messages.value.push({ role: 'user', content: text })
  saveSessions()
  inputText.value = ''
  loading.value = true
  scrollToBottom()

  let assistantContent = ''
  const currentIndex = messages.value.length

  // assistant 消息占位，包含结构化字段
  messages.value.push({
    role: 'assistant',
    content: '',
    reasoningSteps: [],
    toolCalls: [],
    chartConfig: null,
    status: 'running',
  })

  // ★ 推理步骤节流队列：后端合并 LLM 返回会瞬间推 4-5 条 reasoning_step，
  //   一次性 flush 到 UI 像"啪一下"全冒出来。节流为「新 step 间隔 250ms 才追加」，
  //   同 id 的 running→success 替换仍然瞬时（视觉上是同一行更新）。
  const reasoningQueue = []
  let queueProcessing = false
  const REASONING_STAGGER_MS = 250
  const processReasoningQueue = async () => {
    if (queueProcessing) return
    queueProcessing = true
    while (reasoningQueue.length > 0) {
      const step = reasoningQueue.shift()
      const msg = messages.value[currentIndex]
      if (!msg || !msg.reasoningSteps) {
        continue
      }
      let isReplacement = false
      const sid = step && step.id
      if (sid) {
        const idx = msg.reasoningSteps.findIndex((s) => s && s.id === sid)
        if (idx >= 0) {
          msg.reasoningSteps[idx] = Object.assign({}, msg.reasoningSteps[idx], step)
          isReplacement = true
        } else {
          msg.reasoningSteps.push(step)
        }
      } else {
        msg.reasoningSteps.push(step)
      }
      // 仅"新出现的 step"等待节流间隔，replace 立刻处理下一条
      if (!isReplacement && reasoningQueue.length > 0) {
        await new Promise((r) => setTimeout(r, REASONING_STAGGER_MS))
      }
    }
    queueProcessing = false
  }

  // 复用 session_id
  if (!currentSessionId.value) {
    currentSessionId.value = makeId()
  }

  // 把完整对话历史发给后端（排除最后一条空 assistant 占位）。
  // 关键：模型才能理解"上个月呢"、"再算一下"这类承接性问题。
  const fullHistory = messages.value
    .slice(0, -1)
    .filter((m) => m.role === 'user' || m.role === 'assistant')
    .filter((m) => m.content && m.content.trim().length > 0)
    .map((m) => ({ role: m.role, content: m.content }))

  try {
    currentRequestTask = streamNlqChat(
    {
      messages: fullHistory,
      session_id: currentSessionId.value
    },
    {
      onText: (chunk) => {
        assistantContent += chunk
        const msg = messages.value[currentIndex]
        if (msg) msg.content = assistantContent
        // 流式更新时：用户在底部就跟随，不在底部（在看历史）就只累加未读，不打断阅读
        if (atBottom.value) {
          scrollToBottom()
        } else {
          unreadCount.value += 1
          showScrollToBottomBtn.value = true
        }
      },
      onReasoningStep: (step) => {
        // 后端可能瞬间推一批（合并 LLM 返回时同时到 4-5 条），由队列节流出来，
        // 同 id 的替换仍是原地更新，看起来像「一条条往上冒」。
        reasoningQueue.push(step)
        processReasoningQueue()
      },
      onToolStart: (toolInput) => {
        const msg = messages.value[currentIndex]
        if (msg && msg.toolCalls) {
          msg.toolCalls.push({
            toolCallId: toolInput.tool_call_id || `${toolInput.name}-${Date.now()}`,
            name: toolInput.name,
            displayName: toolInput.display_name || toolInput.name,
            status: 'running',
            input: toolInput.input,
            summary: toolInput.summary || '',
          })
        }
      },
      onToolEnd: (toolOutput) => {
        const msg = messages.value[currentIndex]
        if (msg && msg.toolCalls) {
          const tc = msg.toolCalls.find(
            (t) => t.toolCallId === toolOutput.tool_call_id || t.name === toolOutput.name
          )
          if (tc) {
            tc.status = toolOutput.status === 'success' ? 'completed' : 'error'
            tc.output = toolOutput.output
            tc.duration_ms = toolOutput.duration_ms
            tc.summary = toolOutput.summary || tc.summary
          }
        }
      },
      onChart: (chartSpec) => {
        const msg = messages.value[currentIndex]
        if (msg) msg.chartConfig = chartSpec
      },
      onResponseMetadata: (payload) => {
        // ★ 接收 LightRAG / KB 命中的 citations，前端展示「📎 来源」按钮
        if (Array.isArray(payload.citations) && payload.citations.length > 0) {
          const msg = messages.value[currentIndex]
          if (msg) {
            msg.citations = payload.citations
            msg.kbConfidence = payload.kb_confidence
          }
        }
        if (payload.session_id) {
          currentSessionId.value = payload.session_id
        }
        const msg = messages.value[currentIndex]
        if (!msg) return
        // canonical reasoning steps（后端 ground truth）
        // 节流队列还在出步骤时不要立刻覆盖，等其完成再用 canonical list 兜底，
        // 否则会破坏渐进显示效果。
        if (Array.isArray(payload.reasoning_steps) && payload.reasoning_steps.length > 0) {
          const finalSteps = payload.reasoning_steps
          const apply = () => {
            const m = messages.value[currentIndex]
            if (m) m.reasoningSteps = finalSteps
          }
          if (queueProcessing || reasoningQueue.length > 0) {
            const wait = () => {
              if (!queueProcessing && reasoningQueue.length === 0) {
                apply()
              } else {
                setTimeout(wait, 80)
              }
            }
            wait()
          } else {
            apply()
          }
        }
        // 图表配置（chart 事件先到也行、这里兜底也行）
        if (payload.chart_config) {
          msg.chartConfig = payload.chart_config
        }
        // 后端给的全量 response —— 如果比已流式部分更长（grade_distribution 这种
        // 走 narrative 流式 + 后端 append markdown 表的场景），用全量覆盖一次。
        const fullResp = typeof payload.response === 'string' ? payload.response.trim() : ''
        if (!fullResp) return
        const current = (msg.content || '').trim()
        if (current.length === 0) {
          msg.content = fullResp
          assistantContent = fullResp
        } else if (fullResp.length > current.length) {
          const head = current.slice(0, Math.min(60, current.length))
          if (fullResp.indexOf(head) === 0) {
            msg.content = fullResp
            assistantContent = fullResp
          }
        }
      },
      onError: (err) => {
        const msg = messages.value[currentIndex]
        if (msg) {
          msg.content = '请求出错：' + (err.message || '未知错误')
          msg.status = 'error'
        }
        loading.value = false
        currentRequestTask = null
        saveSessions()
        scrollToBottom()
      },
      onDone: () => {
        loading.value = false
        currentRequestTask = null
        const msg = messages.value[currentIndex]
        if (msg && msg.status === 'running') {
          msg.status = 'completed'
        }
        saveSessions()
        scrollToBottom()
      }
    }
  )
  } catch (e) {
    // 例如 NLQ Agent 地址未配置时 streamNlqChat 会同步抛错
    const msg = messages.value[currentIndex]
    if (msg) {
      msg.content = e && e.message ? e.message : '智能助手未就绪'
      msg.status = 'error'
    }
    loading.value = false
    currentRequestTask = null
    saveSessions()
    uni.showToast({ title: e && e.message ? e.message : '配置错误', icon: 'none' })
    scrollToBottom()
  }
}
</script>

<style lang="scss">
.chat-page {
  /* 关键：用 height 而非 min-height，确保整页严格 = 视口高度。
     这样 .chat-list (flex:1) 才能正确 overflow-y:auto，
     而输入栏始终钉在底部，不会被超长内容推到屏幕外。 */
  height: 100vh;
  background: #f7f9fc;
  display: flex;
  flex-direction: column;
  box-sizing: border-box;
  overflow: hidden;
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

.assistant-card {
  max-width: 82%;
  padding: 12px;
  background: #ffffff;
  border: 1px solid #f0f0f0;
}

.msg-text {
  font-size: 14px;
  color: #262626;
  line-height: 1.6;
  word-break: break-word;
}

/* 助手状态条 */
.assistant-status {
  margin-bottom: 8px;
  padding: 6px 10px;
  background: #f6ffed;
  border-radius: 6px;
  border-left: 3px solid #52c41a;
}

.status-text {
  font-size: 12px;
  color: #389e0d;
}

/* 工具卡片 */
.tool-cards {
  margin-bottom: 10px;
  display: flex;
  flex-direction: column;
  gap: 6px;
}

.tool-card {
  padding: 8px 10px;
  border-radius: 6px;
  background: #fafafa;
  border: 1px solid #f0f0f0;
}

.tool-card.running {
  background: #fffbe6;
  border-color: #ffd666;
}

.tool-card.completed {
  background: #f6ffed;
  border-color: #b7eb8f;
}

.tool-card.error {
  background: #fff2f0;
  border-color: #ffccc7;
}

.tool-header {
  display: flex;
  align-items: center;
  gap: 8px;
}

.tool-dot {
  width: 6px;
  height: 6px;
  border-radius: 50%;
  background: #bfbfbf;
}

.tool-dot.running {
  background: #faad14;
}

.tool-dot.completed {
  background: #52c41a;
}

.tool-dot.error {
  background: #f5222d;
}

.tool-name {
  font-size: 12px;
  color: #595959;
  flex: 1;
}

.tool-duration {
  font-size: 11px;
  color: #8c8c8c;
}

/* 加载动画 */
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

/* 浮动「一键到底」按钮 */
.scroll-to-bottom-btn {
  position: fixed;
  right: 16px;
  width: 40px;
  height: 40px;
  border-radius: 50%;
  background: rgba(255, 255, 255, 0.96);
  box-shadow: 0 4px 14px rgba(15, 23, 42, 0.12);
  display: flex;
  align-items: center;
  justify-content: center;
  border: 1px solid #e2e8f0;
  z-index: 50;
  transition: transform 0.18s ease, opacity 0.2s;
  animation: fadeInScale 0.2s ease;
}

.scroll-to-bottom-btn:active {
  transform: scale(0.9);
}

.scroll-to-bottom-icon {
  font-size: 18px;
  color: #1890ff;
  font-weight: 600;
  line-height: 1;
}

.scroll-to-bottom-badge {
  position: absolute;
  top: -4px;
  right: -4px;
  min-width: 16px;
  height: 16px;
  padding: 0 4px;
  border-radius: 8px;
  background: #f5222d;
  color: #ffffff;
  font-size: 10px;
  font-weight: 600;
  line-height: 16px;
  text-align: center;
  box-sizing: border-box;
}

@keyframes fadeInScale {
  from { opacity: 0; transform: scale(0.85); }
  to   { opacity: 1; transform: scale(1); }
}

/* 输入栏 */
.input-bar {
  display: flex;
  align-items: center;
  gap: 10px;
  padding: 10px 12px calc(10px + env(safe-area-inset-bottom));
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

.send-btn.stop {
  background: linear-gradient(135deg, #ff4d4f, #ff7875);
}

.send-btn-text {
  font-size: 14px;
  font-weight: 600;
  color: #ffffff;
}

/* ── Markdown 代码块 / 内联代码 / 表格统一样式 ──
   H5 通过 v-html 直接渲染原生 <pre>/<code>，下面 CSS 命中即可。
   APP-PLUS / MP-WEIXIN 走 mp-html 组件，靠 :tag-style 注入（见 template）。 */
.msg-md {
  font-size: 14px;
  line-height: 1.65;
  color: #262626;
  word-break: break-word;
}

.msg-md pre,
.msg-md code {
  font-family: 'FiraCode', 'JetBrains Mono', 'SFMono-Regular', Consolas, 'Liberation Mono', Menlo, monospace !important;
  font-feature-settings: 'calt' 1, 'liga' 1;  /* FiraCode 连字（=>、!==、->） */
}

.msg-md code {
  background: rgba(15, 23, 42, 0.06);
  padding: 2px 6px;
  border-radius: 4px;
  color: #d63384;
  font-size: 13px;
}

.msg-md pre {
  background: #1e293b;
  color: #f1f5f9;
  padding: 12px 14px;
  border-radius: 8px;
  overflow-x: auto;
  margin: 10px 0;
  font-size: 12.5px;
  line-height: 1.55;
}

.msg-md pre code {
  background: transparent !important;
  color: inherit;
  padding: 0;
  font-size: inherit;
}

.msg-md table {
  width: 100%;
  border-collapse: collapse;
  font-size: 13px;
  margin: 10px 0;
  border: 1px solid #e2e8f0;
  border-radius: 6px;
  overflow: hidden;
}

.msg-md th,
.msg-md td {
  padding: 6px 10px;
  border-bottom: 1px solid #e2e8f0;
  border-right: 1px solid #e2e8f0;
  text-align: left;
}

.msg-md th {
  background: #f1f5f9;
  font-weight: 600;
  white-space: nowrap;
}

.msg-md th:last-child,
.msg-md td:last-child { border-right: none; }
.msg-md tbody tr:last-child td { border-bottom: none; }
.msg-md tbody tr:nth-child(even) { background: #fafbfc; }

/* 📎 来源：可折叠的 citation 列表 */
.citations-bar {
  margin-top: 12px;
  padding: 8px 12px;
  background: linear-gradient(135deg, rgba(24, 144, 255, 0.05), rgba(82, 196, 26, 0.05));
  border: 1px solid #d6e4ff;
  border-radius: 8px;
  font-size: 12px;
}

.citations-toggle {
  display: flex;
  align-items: center;
  gap: 6px;
  cursor: pointer;
  user-select: none;
}

.citations-icon {
  font-size: 14px;
}

.citations-label {
  font-weight: 600;
  color: #1890ff;
  font-size: 12px;
}

.citations-conf {
  flex: 1;
  font-size: 11px;
  color: #8c8c8c;
}

.citations-arrow {
  font-size: 10px;
  color: #8c8c8c;
}

.citations-list {
  margin-top: 8px;
  padding-top: 8px;
  border-top: 1px dashed #d6e4ff;
  display: flex;
  flex-direction: column;
  gap: 6px;
}

.citations-item {
  display: flex;
  align-items: flex-start;
  gap: 6px;
  padding: 4px 0;
}

.citations-bullet {
  flex-shrink: 0;
  width: 20px;
  font-size: 11px;
  color: #1890ff;
  font-weight: 600;
}

.citations-text {
  flex: 1;
  font-size: 11px;
  color: #475569;
  word-break: break-all;
  line-height: 1.5;
  font-family: 'FiraCode', 'JetBrains Mono', 'SFMono-Regular', Consolas, monospace;
}

.citations-hint {
  margin-top: 4px;
  font-size: 10px;
  color: #94a3b8;
  text-align: center;
}

/* SQL 折叠块：默认收起，summary 作为可点击按钮 */
.msg-md details.sql-fold {
  margin: 10px 0;
  border: 1px solid #cbd5e1;
  border-radius: 8px;
  background: #f8fafc;
  overflow: hidden;
}
.msg-md details.sql-fold > summary {
  list-style: none;
  cursor: pointer;
  padding: 8px 12px;
  font-size: 13px;
  color: #475569;
  background: #f1f5f9;
  user-select: none;
  display: flex;
  align-items: center;
  gap: 6px;
}
.msg-md details.sql-fold > summary::-webkit-details-marker { display: none; }
.msg-md details.sql-fold > summary::before {
  content: '▶';
  font-size: 10px;
  transition: transform 0.15s;
}
.msg-md details.sql-fold[open] > summary::before {
  transform: rotate(90deg);
}
.msg-md details.sql-fold > pre {
  margin: 0;
  border-radius: 0;
}
</style>
