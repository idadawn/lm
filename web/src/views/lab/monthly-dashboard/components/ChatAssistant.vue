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
          <!--
            渲染逻辑（流式中的渐进展示）：
              1. 还在 streaming，且连 1 条 reasoning_step 都没来 → 显示通用"思考中"占位
              2. 一旦有 reasoning_step → 直接渲染 ReasoningChain（默认展开），步骤按节流队列一条条冒出来
              3. 一旦 narrative 流出来 → 在 ReasoningChain 下方显示 markdown 正文
          -->
          <div
            v-if="streamingIndex === idx && !msg.content && (!msg.reasoningSteps || msg.reasoningSteps.length === 0)"
            class="thinking-state"
          >
            <span class="thinking-avatar">💭</span>
            <span class="thinking-text">{{ currentThinkingLabel(msg) }}</span>
            <span class="thinking-dots">
              <i></i><i></i><i></i>
            </span>
          </div>
          <template v-else>
            <ReasoningChain
              v-if="msg.role === 'assistant' && msg.reasoningSteps && msg.reasoningSteps.length"
              :steps="msg.reasoningSteps"
              :default-open="streamingIndex === idx"
            />
            <div class="message-text markdown-content" v-html="renderMarkdown(msg.content)"></div>
            <ChatChartBubble
              v-if="msg.role === 'assistant' && msg.chartConfig"
              :chart-config="msg.chartConfig"
            />
            <!-- 📎 来源：当 LightRAG / KB 命中时显示，可折叠 -->
            <div
              v-if="msg.role === 'assistant' && msg.citations && msg.citations.length"
              class="citations-bar"
            >
              <div class="citations-toggle" @click="toggleCitations(idx)">
                <span class="citations-icon">📎</span>
                <span class="citations-label">来源 {{ msg.citations.length }}</span>
                <span v-if="msg.kbConfidence != null" class="citations-conf">
                  · 置信度 {{ Math.round(msg.kbConfidence * 100) }}%
                </span>
                <span class="citations-arrow">{{ citationsOpen[idx] ? '▾' : '▸' }}</span>
              </div>
              <div v-show="citationsOpen[idx]" class="citations-list">
                <div
                  v-for="(src, i) in friendlyCitations(msg.citations)"
                  :key="i"
                  class="citations-item"
                  :title="src.raw"
                >
                  <span class="citations-bullet">{{ i + 1 }}.</span>
                  <span class="citations-text">{{ src.label }}</span>
                </div>
              </div>
            </div>
          </template>
        </div>
      </div>
      <div v-if="messages.length === 0" class="empty-state">
        <div class="empty-icon-wrapper">
          <span class="empty-emoji">🤖</span>
        </div>
        <h3>您好，我是<span class="brand-name">小美</span>👋</h3>
        <p class="empty-lead">您的检测中心智能数据助理。</p>
        <p class="empty-desc">查 <b>合格率</b> · <b>产量</b> · <b>班次对比</b> · <b>不合格分类</b> · <b>炉号明细</b>，都可以问我。</p>
        <div class="capability-tags">
          <span class="cap-tag cap-tag--blue">📊 数据统计</span>
          <span class="cap-tag cap-tag--green">📈 趋势分析</span>
          <span class="cap-tag cap-tag--orange">⚙️ 班次对比</span>
          <span class="cap-tag cap-tag--purple">🔍 炉号溯源</span>
        </div>
      </div>
    </div>

    <div v-if="messages.length === 0" class="suggestions">
      <div class="suggestion-title">
        <span class="suggestion-title-text">💡 试试这些常见问题</span>
        <span class="suggestion-title-hint">点击直接发送</span>
      </div>
      <div class="suggestion-chips">
        <button
          v-for="(q, i) in quickQuestions"
          :key="q"
          class="suggestion-chip"
          :class="`suggestion-chip--${(i % 6) + 1}`"
          @click="handleQuickQuestion(q)"
        >
          <span class="suggestion-chip-icon">{{ ['📋','📦','⚖️','📈','⚠️','🔍'][i] || '💬' }}</span>
          <span class="suggestion-chip-text">{{ q }}</span>
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
import { getOntology, type OntologyReportConfig, type OntologyAppearanceFeature } from '/@/api/lab/knowledgeGraph';
import type { ReasoningStep } from '/@/types/reasoning-protocol';
import ChatChartBubble from './ChatChartBubble.vue';
import ReasoningChain from '/@/components/ReasoningChain.vue';
import Showdown from 'showdown';
import { useNlqSession } from './useNlqSession';

interface Props {
  reportData?: any;
}

const props = withDefaults(defineProps<Props>(), {
  reportData: null,
});

const { messages, sessionId, appendMessage, updateLastMessage, broadcastTail, clearAll, startNewSession } = useNlqSession();

// 暴露给父组件（XiaoMeiAssistant 的"新对话"按钮使用）
function resetConversation() {
  clearAll();
  startNewSession();
}

defineExpose({ resetConversation });

// ── 📎 来源 citations 折叠 / 复制 ───────────────────────────
// 每条 assistant 消息独立的展开状态；按 index 存
const citationsOpen = ref<Record<number, boolean>>({});

function toggleCitations(idx: number) {
  citationsOpen.value = { ...citationsOpen.value, [idx]: !citationsOpen.value[idx] };
}

// 把后端原始 citation（含文件路径 / # fragment）翻译成"业务依据"标签：
// - knowledge_base.json#xxx → 业务知识库
// - docs/development-guide.md#... → 系统使用手册
// - lab_report_config / dimensionsmeta.json → 配置/维度
// 同名标签自动去重，最终返回 [{ raw, label }]
function friendlyCitations(raw: string[] | undefined): Array<{ raw: string; label: string }> {
  if (!raw || !raw.length) return [];
  const result: Array<{ raw: string; label: string }> = [];
  const seen = new Set<string>();
  for (const src of raw) {
    if (!src) continue;
    const lower = String(src).toLowerCase();
    let label = '业务文档';
    if (lower.includes('knowledge_base.json') || lower.includes('knowledgebase.json')) {
      label = '业务术语知识库';
    } else if (lower.includes('judgmentrules') || lower.includes('judgment_rules')) {
      label = '判定规则';
    } else if (lower.includes('lab_report_config') || lower.includes('report-config') || lower.includes('reportconfig')) {
      label = '报表配置';
    } else if (lower.includes('dimensionsmeta') || lower.includes('dimensions_meta') || lower.includes('dimensions/')) {
      label = '业务维度库';
    } else if (lower.includes('kg-frontend-ontology') || lower.includes('ontology')) {
      label = '知识图谱本体';
    } else if (lower.includes('development-guide')) {
      label = '系统使用手册';
    } else if (lower.includes('lightrag') || lower.startsWith('lr#') || lower.includes('lightrag-index')) {
      label = '业务知识图谱';
    } else if (lower.startsWith('docs/')) {
      label = '业务文档';
    } else if (lower.startsWith('neo4j:') || lower.includes('neo4j')) {
      label = '知识图谱节点';
    }
    if (!seen.has(label)) {
      seen.add(label);
      result.push({ raw: String(src), label });
    }
  }
  return result;
}

async function copyCitation(text: string) {
  if (!text) return;
  try {
    await navigator.clipboard.writeText(text);
    message.success({ content: '已复制：' + text, duration: 1.2 });
  } catch {
    // 浏览器拒绝 clipboard API（非 HTTPS / Safari 旧版）退化到 prompt
    window.prompt('复制下面这段路径', text);
  }
}

const inputValue = ref('');
const isSending = ref(false);
const streamingIndex = ref<number>(-1);
const messagesEl = ref<HTMLElement | null>(null);

// 不再使用前端轮换的拟人化文案 — 完全依赖后端 dispatch_custom_event 推送的真实 reasoning_step
// 模板里读 msg.reasoningSteps 最后一条作为当前状态显示
function currentThinkingLabel(msg: { reasoningSteps?: ReasoningStep[] }): string {
  const steps = msg.reasoningSteps || [];
  if (steps.length === 0) return '小美正在连接服务';
  const last: any = steps[steps.length - 1];
  return last?.title || last?.label || '小美正在思考';
}

const converter = new Showdown.Converter({
  ghCodeBlocks: true,
  simplifiedAutoLink: true,
  strikethrough: true,
  tables: true,
  tasklists: true,
  requireSpaceBeforeHeadingText: true,
});

// 与 NLQ SQL 模板对齐的推荐问题（覆盖 6 个核心查询场景）
const quickQuestions = [
  '本月一次交检合格率是多少？',
  '本月 A 类产量有多少吨？',
  '三班这个月合格率对比',
  '近 7 天合格率走势',
  '本月不合格原因 Top 5',
  '本月所有 C 级炉号明细',
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

// Ontology 缓存（onMounted 拉一次）：指标 + 外观特性 + 等级，用于动态填充 system prompt
const ontologyMetrics = ref<OntologyReportConfig[]>([]);
const ontologyFeatures = ref<OntologyAppearanceFeature[]>([]);
const ontologyShifts = ref<string[]>([]);
const ontologyDateRange = ref<{ min: string; max: string }>({ min: '', max: '' });

async function loadOntologyContext() {
  try {
    const data: any = await getOntology();
    ontologyMetrics.value = data?.report_configs || [];
    ontologyFeatures.value = data?.appearance_features || [];
    ontologyShifts.value = data?.dimensions?.shift?.values_from_data || [];
    const t = data?.dimensions?.time;
    if (t?.min_date || t?.max_date) {
      ontologyDateRange.value = {
        min: (t.min_date || '').slice(0, 10),
        max: (t.max_date || '').slice(0, 10),
      };
    }
  } catch {
    // 静默：拉不到时用静态兜底
  }
}

// 检测中心通用 system prompt — 注入业务术语 + 真实指标列表 + 数据源 + 风格约束
function buildDefaultPrompt(): string {
  const metricsLine = ontologyMetrics.value.length > 0
    ? ontologyMetrics.value
        .map((m) => {
          const lvl = (m.level_names || []).length > 0 ? `（包含等级：${m.level_names.join('/')}）` : '';
          const pct = m.is_percentage ? '【占比】' : '【汇总】';
          return `  · ${m.name}${lvl} ${pct}`;
        })
        .join('\n')
    : '  · 一次交检合格率（占比）\n  · A、B（合格等级占比）\n  · 合格率、不合格（汇总+占比）';

  const featCount = ontologyFeatures.value.length;
  const shiftsLine = ontologyShifts.value.length > 0 ? ontologyShifts.value.join('/') : '甲/乙/丙';
  const dataWindow = ontologyDateRange.value.min
    ? `（当前数据范围：${ontologyDateRange.value.min} ~ ${ontologyDateRange.value.max}）`
    : '';

  return `你是【小美】，检测中心智能数据助理。请用中文专业、简洁、口语化地回答。

【硬性规则 — 禁止英文术语】
绝不能在回答中出现任何英文/驼峰命名（如 StripWidth、PsIronLoss、ThicknessRange、LaminationFactor、Hc 等）。
必须使用以下行业通用的中文术语：
  - 带宽（不是带钢宽度、不是 StripWidth）
  - 厚度极差
  - 铁损（口语也叫"比铁损"，绝不要写 PsIronLoss/Ps；如果一定要标注英文符号，写"Ps 铁损"）
  - 矫顽力（可用 Hc 作为符号但要写"矫顽力（Hc）"，不要单写 Hc）
  - 叠片系数
  - 密度、卷重
  - 班次、产线、产品规格、炉号

【可统计的指标（来自系统配置 lab_report_config）】
${metricsLine}
当用户问"X 是什么"或"X 的判断依据"，要解释：基于哪个判定公式 + 包含哪些等级 + 是占比还是汇总。

【数据维度】
- 时间：F_PROD_DATE，按生产日期${dataWindow}
- 班次：${shiftsLine}（已是中文，不要翻译）
- 产线：F_LINE_NO
- 产品规格：F_PRODUCT_SPEC_CODE
- 炉号：标准炉号 F_FURNACE_NO_FORMATTED

【外观缺陷】
系统定义了 ${featCount > 0 ? featCount + ' 种' : ''}外观特性（脆、麻点、划伤、白边、亮线、棱、棱裂纹、卷叶边 等），分大类挂在产品规格上。

【数据来源链路】
原始叠片检测数据经计算公式（CALC）和判定公式（JUDGE）合成
  → lab_intermediate_data（叠片数据成品视图，即用户日常看到的"叠片数据"）
统计口径：检验总重、合格重量、不合格重量等重量类指标，只使用 lab_intermediate_data.F_SINGLE_COIL_WEIGHT，不回查原始表。

【你的能力 — 实事求是】
我（小美）已经接入了 lab_intermediate_data 等表的实时查询能力，可以直接回答：
- 一次交检合格率（按重量加权，已接 SQL）
- 指定指标的均值/极值/趋势（厚度极差、叠片系数、Ps 铁损、矫顽力、带宽）
- 单炉号的根因分析、判定规则、外观特性
绝不要回答"我还没有 SQL 执行能力 / 数据查询功能在开发中"——后台一定会真的去查。
如果用户问的口径系统暂时算不出（极小众的自定义聚合），可以说"这条数据我现在还没法直接算，建议把指标名/时间范围/班次说清楚我再帮你查"，但不要笼统地说"我不能查数据"。

【回答风格】
1. 先给数字结论再说判断依据，不要长篇大论。
2. 用户问"上个月呢/再算一下/三班对比"等承接性问题，要结合上文语境理解（沿用上轮的指标、班次、规格等）。
3. 不要编数字、不要硬说 0%/0kg；不知道就说不知道。
4. 回答末尾不要写"如有疑问请联系管理员"这种官话。`;
}

function buildSystemPrompt(): string {
  if (!props.reportData?.summary) {
    return buildDefaultPrompt();
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
  // 新插入的"思考中"气泡需要立即滚到底，否则会被 input-area 半挡住
  await scrollToBottom();

  try {
    let hasStreamed = false;
    let accumulated = '';
    let accumulatedSteps: ReasoningStep[] = [];

    // ★ 推理步骤节流队列：后端在合并 LLM 返回后会瞬间推 4-5 条 reasoning_step，
    //   一次性 flush 到 UI 看起来像"啪一下"全冒出来。
    //   节流为「新 step 间隔 250ms 才追加」，同 id 的 running→success 替换仍然瞬时。
    const reasoningQueue: ReasoningStep[] = [];
    let queueProcessing = false;
    const REASONING_STAGGER_MS = 250;

    async function processReasoningQueue() {
      if (queueProcessing) return;
      queueProcessing = true;
      while (reasoningQueue.length > 0) {
        const step = reasoningQueue.shift()!;
        const sid = (step as any)?.id;
        let isReplacement = false;
        if (sid) {
          const idx = accumulatedSteps.findIndex((s: any) => s?.id === sid);
          if (idx >= 0) {
            const next = accumulatedSteps.slice();
            next[idx] = { ...accumulatedSteps[idx], ...step } as any;
            accumulatedSteps = next;
            isReplacement = true;
          } else {
            accumulatedSteps = [...accumulatedSteps, step];
          }
        } else {
          accumulatedSteps = [...accumulatedSteps, step];
        }
        updateLastMessage({ reasoningSteps: accumulatedSteps });
        scrollToBottom();
        // 仅"新出现的 step"等待节流间隔；replace 是同一行更新，立刻处理下一条
        if (!isReplacement && reasoningQueue.length > 0) {
          await new Promise((r) => setTimeout(r, REASONING_STAGGER_MS));
        }
      }
      queueProcessing = false;
    }

    // 关键：把完整对话历史一起传给后端（排除最后一条空 assistant 占位 + 任何 content 为空的）
    // 这样模型才能理解"上个月呢"、"再加上甲班"这类承接性问题
    const fullHistory = messages.value
      .slice(0, -1)
      .filter((m) => m.role === 'user' || m.role === 'assistant')
      .filter((m) => m.content && m.content.trim().length > 0)
      .map((m) => ({ role: m.role as 'user' | 'assistant', content: m.content }));

    await streamNlqChat(
      {
        messages: [
          { role: 'system', content: buildSystemPrompt() },
          ...fullHistory,
        ],
        // ★ 关键：把 useNlqSession 维护的 sessionId 透传给后端
        // 缺这一项后端每轮都会生成新 uuid，导致多轮承接（如"120 贴标 A 等级" → "A 级别铁损范围"）失效
        session_id: sessionId.value,
      },
      {
        onReasoningStep(step: ReasoningStep) {
          // 后端可能瞬间推一批（合并 LLM 返回时 5 条同时到），由 processReasoningQueue 节流
          // 出来，看起来像"一条条往上冒"。同 id 的替换不延迟。
          reasoningQueue.push(step);
          processReasoningQueue();
        },
        onText(chunk: string) {
          hasStreamed = true;
          accumulated += chunk;
          updateLastMessage({ content: accumulated });
          scrollToBottom();
        },
        // 后端通过 chart 事件下发的可视化配置（donut / bar / line）→ 渲染在 AI 气泡底部
        onChart(chartSpec) {
          updateLastMessage({ chartConfig: chartSpec });
          scrollToBottom();
        },
        // response_metadata 是后端的 canonical 全量答复（含 narrative + 表格 + 提示）。
        // 三种情况：
        //   1. 没流式过任何 text → 直接用 respText 填充（产品规格旧版、判定规则、根因等）
        //   2. 已流式 narrative，respText 比 accumulated 更长 → 末尾差值就是后端拼的表格/补充段，追加进来
        //   3. 已流式 narrative，respText == accumulated → 不动
        onResponseMetadata(payload) {
          const respText = String((payload as any)?.response || '').trim();
          if (respText) {
            if (!hasStreamed) {
              accumulated = respText;
              updateLastMessage({ content: respText });
            } else if (respText.length > accumulated.length) {
              // 检查后端全量是否以已流式部分开头；若开头一致，直接覆盖（保证表格追加显示）
              const head = accumulated.slice(0, Math.min(60, accumulated.length));
              if (respText.startsWith(head) || head === '') {
                accumulated = respText;
                updateLastMessage({ content: respText });
              }
            }
            scrollToBottom();
          }
          // chart 事件先到也行、后到也行，response_metadata 里也带一份 chart_config 兜底
          const cfg = (payload as any)?.chart_config;
          if (cfg && typeof cfg === 'object') {
            updateLastMessage({ chartConfig: cfg });
          }
          // 后端在 response_metadata 里还会带 reasoning_steps（canonical list）——
          // 这是相对 SSE 流的 ground truth，用它覆盖累积值避免推理链有遗漏。
          const finalSteps = (payload as any)?.reasoning_steps;
          if (Array.isArray(finalSteps) && finalSteps.length > 0) {
            // 节流队列还在出步骤时不要覆盖，否则用户视觉上会"一闪而至"破坏渐进效果。
            // 等队列空 + 处理器停了再用 canonical list 兜底（防止 SSE 漏 step）。
            const applyFinal = () => {
              accumulatedSteps = finalSteps as ReasoningStep[];
              updateLastMessage({ reasoningSteps: accumulatedSteps });
            };
            if (queueProcessing || reasoningQueue.length > 0) {
              const wait = () => {
                if (!queueProcessing && reasoningQueue.length === 0) {
                  applyFinal();
                } else {
                  setTimeout(wait, 80);
                }
              };
              wait();
            } else {
              applyFinal();
            }
          }
          // ★ LightRAG / KB 命中的 citations
          const cites = (payload as any)?.citations;
          if (Array.isArray(cites) && cites.length > 0) {
            updateLastMessage({
              citations: cites as string[],
              kbConfidence: (payload as any)?.kb_confidence,
            });
          }
        },
        onDone() {
          if (!accumulated) {
            updateLastMessage({ content: '本次没有生成正文回复，可参考左侧推理过程。' });
          }
        },
        onError(err: Error) {
          // 后端通过 SSE 推送的错误已经是中文友好提示（_humanize_error_zh），直接展示
          const rawMsg = (err?.message || '').trim();
          console.warn('nlqAgent SSE error:', rawMsg);

          // 把错误也追加成一条 fallback 推理步骤，保留所有已收到的步骤
          accumulatedSteps = [
            ...accumulatedSteps,
            {
              kind: 'fallback',
              label: '请求中断',
              detail: rawMsg || '未知错误',
            } as any,
          ];
          updateLastMessage({ reasoningSteps: accumulatedSteps });

          if (!hasStreamed) {
            const friendly = rawMsg && rawMsg !== 'unknown error'
              ? rawMsg
              : '抱歉，处理您的请求时出现错误，请稍后重试。';
            updateLastMessage({ content: friendly });
            message.error(rawMsg || 'AI 助手暂时无法响应，请稍后重试');
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

// 首次打开不再硬插欢迎消息，改为空态视图直接展示欢迎语 + 推荐问题
// 同时拉取 Ontology 元数据用于丰富 system prompt（指标名 / 班次 / 时间范围 / 外观特性）
onMounted(() => {
  loadOntologyContext();
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
  /* 底部多留 12px：避免最后一条 AI 气泡贴到 input-area 上方边框、被边框线压住一点点 */
  padding: 20px 20px 32px;
  scroll-behavior: smooth;
  min-height: 0;
  /* scroll-padding-bottom 让 scrollIntoView/scrollTop 滚动到底时给最后一行留缓冲 */
  scroll-padding-bottom: 16px;
}

/* 最后一条 message 额外加一点底部余量，防止思考气泡刚出现时与输入区紧贴 */
.chat-assistant-panel .messages .message-item:last-child {
  margin-bottom: 8px;
}

.chat-assistant-panel .message-item {
  display: flex;
  margin-bottom: 18px;
  animation: chat-slide-in 0.3s cubic-bezier(0.16, 1, 0.3, 1);
  gap: 10px;

  &.user-message {
    flex-direction: row-reverse;

    .message-content {
      background: linear-gradient(135deg, #4096FF 0%, #1677FF 100%);
      color: #fff;
      border-radius: 14px 4px 14px 14px;
      box-shadow: 0 2px 8px rgba(22, 119, 255, 0.18);

      .markdown-content {
        color: #fff;

        h1, h2, h3, h4, h5, h6 { color: #fff; }
        code { background: rgba(255, 255, 255, 0.22); color: #fff; }
        a { color: #fff; text-decoration: underline; }
      }
    }

    .message-avatar {
      background: linear-gradient(135deg, #BFDBFE 0%, #93C5FD 100%);
      color: #1D4ED8;
      box-shadow: 0 2px 6px rgba(59, 130, 246, 0.15);
    }
  }

  &.ai-message {
    .message-content {
      background: #FFFFFF;
      color: #1E293B;
      border-radius: 4px 14px 14px 14px;
      box-shadow: 0 1px 4px rgba(15, 23, 42, 0.05);
      border: 1px solid #E2E8F0;
    }

    .message-avatar {
      background: linear-gradient(135deg, #A78BFA 0%, #7C3AED 100%);
      color: #fff;
      box-shadow: 0 2px 6px rgba(124, 58, 237, 0.22);
    }
  }
}

.chat-assistant-panel .message-avatar {
  width: 34px;
  height: 34px;
  border-radius: 50%;
  display: flex;
  align-items: center;
  justify-content: center;
  flex-shrink: 0;
  font-size: 17px;
}

.chat-assistant-panel .message-content {
  max-width: 78%;
  padding: 10px 14px;
  line-height: 1.65;
  position: relative;
  font-size: 14px;
}

.chat-assistant-panel .message-text { word-break: break-word; }

/* 思考过程指示器 — 取代干瘪的 loading dots */
.chat-assistant-panel .thinking-state {
  display: inline-flex;
  align-items: center;
  gap: 8px;
  padding: 2px 4px;
  color: #475569;
  font-size: 13px;
  line-height: 1.4;

  .thinking-avatar {
    font-size: 16px;
    line-height: 1;
    animation: thinking-pulse 1.6s ease-in-out infinite;
  }

  .thinking-text {
    background: linear-gradient(90deg, #7C3AED, #1677FF, #7C3AED);
    background-size: 200% 100%;
    -webkit-background-clip: text;
    -webkit-text-fill-color: transparent;
    background-clip: text;
    font-weight: 500;
    animation: thinking-gradient 2.4s linear infinite;
  }

  .thinking-dots {
    display: inline-flex;
    align-items: center;
    gap: 3px;

    i {
      display: block;
      width: 4px;
      height: 4px;
      background-color: #7C3AED;
      border-radius: 50%;
      animation: chat-typing 1.4s infinite ease-in-out both;

      &:nth-child(1) { animation-delay: -0.32s; }
      &:nth-child(2) { animation-delay: -0.16s; }
    }
  }
}

@keyframes thinking-pulse {
  0%, 100% { transform: scale(1); }
  50% { transform: scale(1.18); }
}

@keyframes thinking-gradient {
  0% { background-position: 200% 0; }
  100% { background-position: -200% 0; }
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

  /* Markdown 表格 — 让产品规格清单 / 等级分布明细这种数据表渲染成真正的表格 */
  table {
    display: table;  // 覆盖 reset 里的 inline-block
    width: 100%;
    margin: 10px 0 4px;
    border-collapse: collapse;
    border-spacing: 0;
    font-size: 13px;
    line-height: 1.55;
    background: #fff;
    border: 1px solid #e2e8f0;
    border-radius: 8px;
    overflow: hidden;
  }

  thead tr {
    background: #f1f5f9;
    color: #334155;
  }

  th, td {
    padding: 8px 12px;
    border-bottom: 1px solid #e2e8f0;
    border-right: 1px solid #e2e8f0;
    text-align: left;
    vertical-align: middle;
    word-break: break-word;
  }

  th {
    font-weight: 600;
    white-space: nowrap;
  }

  th:last-child, td:last-child { border-right: none; }
  tbody tr:last-child td { border-bottom: none; }

  tbody tr:nth-child(even) { background: #fafbfc; }
  tbody tr:hover { background: #f8fafc; }

  /* 让数字列右对齐：判断"看起来是数字"的简单方式——直接给所有 td 默认右对齐数字
     不太合适，所以保持左对齐；专门给加粗的"合计"行高亮。 */
  tbody tr td strong {
    color: #0f172a;
  }

  /* 在窄宽容器里允许横向滚动 */
  // 由于 markdown-content 自身没有外层 wrapper，给 table 加一层伪滚动通过 overflow-x 在父类上做。

  /* SQL 折叠块（chat2sql 默认折叠，业务人员一般不需要看 SQL 本身） */
  details.sql-block {
    margin: 10px 0;
    padding: 6px 12px;
    background: #f8fafc;
    border: 1px solid #e2e8f0;
    border-radius: 6px;
    font-size: 12px;

    summary {
      cursor: pointer;
      color: #64748b;
      font-weight: 500;
      padding: 4px 0;
      list-style: none;
      user-select: none;
      transition: color 0.15s;

      &::-webkit-details-marker { display: none; }
      &::before {
        content: '▶';
        display: inline-block;
        margin-right: 6px;
        font-size: 9px;
        color: #94a3b8;
        transition: transform 0.15s;
      }

      &:hover { color: #334155; }
    }

    &[open] summary::before {
      transform: rotate(90deg);
    }

    pre {
      margin: 6px 0 4px;
      padding: 10px;
      background: #1e293b;
      color: #f1f5f9;
      border-radius: 4px;
      font-size: 11.5px;
      max-height: 280px;
      overflow: auto;

      code {
        background: none;
        color: inherit;
        padding: 0;
      }
    }
  }
}

.chat-assistant-panel .message-text {
  /* 当对话气泡比较窄时，宽表格允许横向滚动 */
  overflow-x: auto;
}

.chat-assistant-panel .empty-state {
  display: flex;
  flex-direction: column;
  align-items: center;
  text-align: center;
  color: #475569;
  padding: 32px 24px 16px;

  .empty-icon-wrapper {
    width: 72px;
    height: 72px;
    background: linear-gradient(135deg, #DBEAFE 0%, #BFDBFE 100%);
    border-radius: 50%;
    display: flex;
    align-items: center;
    justify-content: center;
    margin-bottom: 18px;
    box-shadow: 0 8px 24px rgba(59, 130, 246, 0.18);
  }

  .empty-emoji { font-size: 36px; line-height: 1; }

  h3 {
    font-size: 19px;
    margin-bottom: 10px;
    color: #0F172A;
    font-weight: 600;
    display: inline-flex;
    align-items: center;
    gap: 4px;
  }

  .brand-name {
    background: linear-gradient(135deg, #1890FF 0%, #722ED1 100%);
    -webkit-background-clip: text;
    -webkit-text-fill-color: transparent;
    background-clip: text;
    font-weight: 700;
    margin: 0 4px;
  }

  .empty-lead {
    font-size: 14px;
    margin: 0 0 6px;
    color: #334155;
    max-width: 360px;
  }

  .empty-desc {
    font-size: 13px;
    margin: 0 0 18px;
    color: #64748B;
    max-width: 360px;
    line-height: 1.7;
    b { color: #1890FF; font-weight: 600; }
  }

  .capability-tags {
    display: flex;
    flex-wrap: wrap;
    justify-content: center;
    gap: 8px;
    margin-top: 4px;
  }

  .cap-tag {
    font-size: 12px;
    padding: 4px 10px;
    border-radius: 12px;
    border: 1px solid transparent;
    font-weight: 500;
  }

  .cap-tag--blue   { color: #1D4ED8; background: #EFF6FF; border-color: #DBEAFE; }
  .cap-tag--green  { color: #047857; background: #ECFDF5; border-color: #D1FAE5; }
  .cap-tag--orange { color: #C2410C; background: #FFF7ED; border-color: #FFEDD5; }
  .cap-tag--purple { color: #6D28D9; background: #F5F3FF; border-color: #EDE9FE; }
}

.chat-assistant-panel .suggestions {
  padding: 12px 20px 18px;
  border-top: 1px dashed #E2E8F0;
  background: linear-gradient(180deg, #FAFBFC 0%, #F8FAFC 100%);

  .suggestion-title {
    display: flex;
    align-items: baseline;
    gap: 8px;
    margin-bottom: 12px;
  }

  .suggestion-title-text {
    font-size: 13px;
    color: #334155;
    font-weight: 600;
  }

  .suggestion-title-hint {
    font-size: 11px;
    color: #94A3B8;
  }

  .suggestion-chips {
    display: grid;
    grid-template-columns: 1fr 1fr;
    gap: 8px;
  }

  .suggestion-chip {
    display: flex;
    align-items: center;
    gap: 8px;
    min-height: 40px;
    padding: 8px 12px;
    border-radius: 10px;
    background: #FFFFFF;
    border: 1px solid #E2E8F0;
    color: #1E293B;
    cursor: pointer;
    font-size: 13px;
    line-height: 1.4;
    text-align: left;
    transition: all 0.18s ease;
    box-shadow: 0 1px 2px rgba(15, 23, 42, 0.03);

    &:hover {
      border-color: #93C5FD;
      background: #F8FAFF;
      color: #1D4ED8;
      transform: translateY(-1px);
      box-shadow: 0 4px 12px rgba(59, 130, 246, 0.08);
    }
  }

  .suggestion-chip-icon {
    font-size: 16px;
    line-height: 1;
    flex-shrink: 0;
  }

  .suggestion-chip-text {
    flex: 1;
    min-width: 0;
    overflow: hidden;
    text-overflow: ellipsis;
    white-space: nowrap;
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

/* ── 移动端紧凑断点（≤ 480px / 手机竖屏）──
   配合 XiaoMeiAssistant 在 ≤ 960px 已经全屏铺面板，这里再做内部细节优化 */
@media screen and (max-width: 480px) {
  .chat-assistant-panel .messages {
    padding: 12px;
    gap: 12px;
  }
  .chat-assistant-panel .empty-state {
    padding: 24px 12px;
    h3 { font-size: 17px; }
    p { font-size: 13px; }
  }
  .chat-assistant-panel .empty-icon-wrapper {
    width: 56px;
    height: 56px;
  }
  .chat-assistant-panel .suggestions {
    padding: 12px 12px 6px;
  }
  /* 推荐问题改单列竖排（网格 2 列 → 1 列） */
  .chat-assistant-panel .suggestion-chips {
    grid-template-columns: 1fr;
    gap: 8px;
  }
  .chat-assistant-panel .suggestion-chip {
    width: 100%;
    min-height: 44px;       /* iOS HIG 触摸标准 */
    font-size: 14px;
  }
  .chat-assistant-panel .input-area {
    padding: 10px 12px;
    /* 防 iOS Safari 底部安全区域遮挡 */
    padding-bottom: calc(10px + env(safe-area-inset-bottom));
  }
  .chat-assistant-panel .chat-input {
    min-height: 44px;
    font-size: 16px;        /* ≥16 防 iOS 自动放大 */
  }
  .chat-assistant-panel .send-btn {
    min-height: 44px;
    min-width: 64px;
    padding: 0 14px;
  }
}

/* ── 📎 来源（LightRAG / KB 命中时显示）─────────────────── */
.citations-bar {
  margin-top: 10px;
  padding: 8px 12px;
  background: linear-gradient(135deg, rgba(24, 144, 255, 0.06), rgba(82, 196, 26, 0.06));
  border: 1px solid #d6e4ff;
  border-radius: 8px;
  font-size: 12px;
  user-select: none;
}
.citations-toggle {
  display: flex;
  align-items: center;
  gap: 6px;
  cursor: pointer;
}
.citations-toggle:hover {
  opacity: 0.85;
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
  gap: 4px;
}
.citations-item {
  display: flex;
  align-items: flex-start;
  gap: 6px;
  padding: 4px 6px;
  border-radius: 4px;
  cursor: pointer;
  transition: background 0.15s;
}
.citations-item:hover {
  background: rgba(24, 144, 255, 0.08);
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
  text-align: right;
  padding-right: 6px;
}
</style>
