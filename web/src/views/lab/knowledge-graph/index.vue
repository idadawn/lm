<template>
  <div class="kg-page">
    <KgToolbar
      :loading="loading"
      :resyncing="resyncing"
      :search-text="searchText"
      :view-mode="viewMode"
      @refresh="loadData"
      @resync="handleResync"
      @search="handleSearch"
      @update:viewMode="viewMode = $event"
    />

    <!-- ====== 带材搜索栏 ====== -->
    <template v-if="!selectedRibbon && !questionMode">
      <div class="kg-search-panel">
        <div class="search-header">
          <a-input-search
            v-model:value="ribbonSearchText"
            placeholder="搜索炉号、规格、批次..."
            enter-button="搜索"
            size="large"
            @search="handleRibbonSearch"
            :loading="ribbonSearchLoading"
          />
          <div class="search-hint">
            输入炉号（如 <code>1丙20260110-1</code>）或规格代码搜索带材
          </div>
        </div>

        <div class="ribbon-list" v-if="ribbonResults.length">
          <div
            class="ribbon-card"
            v-for="r in ribbonResults"
            :key="r.id"
            @click="selectRibbon(r)"
          >
            <div class="ribbon-main">
              <span class="ribbon-no">{{ r.furnace_no }}</span>
              <a-tag :color="gradeColor(r.labeling)">{{ r.labeling || '未判定' }}</a-tag>
            </div>
            <div class="ribbon-meta">
              <span v-if="r.spec_name">规格: {{ r.spec_name }}</span>
              <span v-if="r.detection_date">检测: {{ r.detection_date }}</span>
              <span>状态: {{ r.detection_status }}</span>
            </div>
          </div>
        </div>

        <a-empty v-else-if="!ribbonSearchLoading && ribbonSearchText" description="未找到匹配的带材" />
      </div>
    </template>

    <!-- ====== 带子图标题栏 ====== -->
    <template v-if="selectedRibbon && !questionMode">
      <div class="kg-ribbon-bar">
        <a-button type="link" size="small" @click="clearRibbon">
          <ArrowLeftOutlined /> 返回全量图谱
        </a-button>
        <span class="ribbon-title">
          带材: {{ selectedRibbon.furnace_no }}
          <a-tag size="small" :color="gradeColor(selectedRibbon.labeling)">
            {{ selectedRibbon.labeling || '未判定' }}
          </a-tag>
        </span>
        <span class="ribbon-subtitle" v-if="selectedRibbon.spec_name">
          {{ selectedRibbon.spec_name }} · {{ selectedRibbon.detection_date }}
        </span>
      </div>
    </template>

    <!-- ====== 主图谱区域（始终显示，问答模式除外） ====== -->
    <div class="kg-main" v-if="!questionMode">
      <KgCanvas
        v-if="graphData.nodes.length > 0"
        :key="graphData.nodes.length"
        :data="graphData"
        :loading="loading"
        :empty="!graphData.nodes.length"
        :highlight-node-ids="highlightNodeIds"
        :highlight-edge-ids="highlightEdgeIds"
        @nodeClick="handleNodeClick"
        @comboClick="handleComboClick"
        @canvasClick="panel = null"
      />
      <div v-else class="kg-loading-mask" style="flex:1;display:flex;align-items:center;justify-content:center;">
        <a-spin tip="加载知识图谱..." />
      </div>
      <KgDetailPanel
        :panel="panel"
        @close="panel = null"
        @back="backToCombo"
        @selectRule="showRuleDetail"
        @action="handlePanelAction"
      />
    </div>

    <!-- ====== 问答模式 ====== -->
    <template v-if="questionMode">
      <div class="kg-answer-header">
        <a-button type="link" size="small" @click="exitQuestionMode">
          <ArrowLeftOutlined /> 返回浏览
        </a-button>
        <span class="question-text">{{ currentQuestion }}</span>
      </div>
      <div class="kg-main">
        <KgReasoningPanel
          :steps="reasoningSteps"
          :active-step-id="activeStepId"
          @selectStep="handleStepSelect"
          @highlightNode="handleHighlightNode"
        />
        <KgCanvas
          :data="graphData"
          :loading="explainLoading"
          :empty="false"
          :highlight-node-ids="highlightNodeIds"
          :highlight-edge-ids="highlightEdgeIds"
          @nodeClick="handleNodeClick"
          @comboClick="handleComboClick"
          @canvasClick="panel = null"
        />
        <KgEvidencePanel
          :answer="explainAnswer"
          :answer-card="explainAnswerCard"
          :evidence-table="explainEvidenceTable"
          :suggested-actions="explainSuggestedActions"
          @action="handleSuggestedAction"
        />
      </div>
    </template>

    <!-- 底部图例 -->
    <div class="kg-legend" v-if="!questionMode">
      <span class="leg"><i class="leg-dot" style="background:#3B82F6"></i> 带材</span>
      <span class="leg"><i class="leg-dot" style="background:#2563EB"></i> 产品规格</span>
      <span class="leg"><i class="leg-dot" style="background:#22C55E"></i> 叠片数据</span>
      <span class="leg"><i class="leg-dot" style="background:#F59E0B"></i> 单片性能</span>
      <span class="leg"><i class="leg-dot" style="background:#EF4444"></i> 外观特性</span>
      <span class="leg"><i class="leg-dot" style="background:#A855F7"></i> 判定规则</span>
    </div>
  </div>
</template>

<script lang="ts" setup>
import { ref, computed, onMounted, shallowRef, markRaw } from 'vue';
import { message } from 'ant-design-vue';
import { ArrowLeftOutlined } from '@ant-design/icons-vue';
import {
  resyncNow,
  explainQuestion,
  searchRibbon,
  getRibbonSubgraph,
  getOntology,
  type ExplainResponse,
  type RibbonSearchResult,
  type RibbonSubgraphResponse,
  type OntologyData,
} from '/@/api/lab/knowledgeGraph';

import KgToolbar from './components/KgToolbar.vue';
import KgCanvas from './components/KgCanvas.vue';
import KgDetailPanel from './components/KgDetailPanel.vue';
import KgReasoningPanel from './components/KgReasoningPanel.vue';
import KgEvidencePanel from './components/KgEvidencePanel.vue';

import type { PanelData, ViewMode, ReasoningStep } from './types/ontology';
import { useGraphData, type G6GraphData } from './composables/useGraphData';

// ── State ──────────────────────────────────────────────

const loading = ref(false);
const resyncing = ref(false);
const searchText = ref('');
const viewMode = ref<ViewMode>('graph');
const panel = ref<PanelData | null>(null);

// 全量图谱
const { setData: setOntologyData, buildGraphData: buildFullGraph } = useGraphData();
const fullGraphData = shallowRef<G6GraphData>({ nodes: [], edges: [], combos: [] });

// 带材搜索
const ribbonSearchText = ref('');
const ribbonSearchLoading = ref(false);
const ribbonResults = ref<RibbonSearchResult[]>([]);
const selectedRibbon = ref<RibbonSearchResult | null>(null);
const ribbonSubgraph = ref<RibbonSubgraphResponse | null>(null);

// 问答模式
const questionMode = ref(false);
const currentQuestion = ref('');
const explainLoading = ref(false);
const explainAnswer = ref('');
const explainAnswerCard = ref<Record<string, unknown> | null>(null);
const reasoningSteps = ref<ReasoningStep[]>([]);
const explainEvidenceTable = ref<Array<Record<string, unknown>>>([]);
const explainSuggestedActions = ref<Array<{ label: string; action: string; params?: Record<string, unknown> }>>([]);
const activeStepId = ref('');

// 高亮
const highlightNodeIds = ref<string[]>([]);
const highlightEdgeIds = ref<string[]>([]);

// ── Computed ───────────────────────────────────────────

const graphData = computed(() => {
  if (ribbonSubgraph.value) {
    return markRaw({
      nodes: ribbonSubgraph.value.nodes || [],
      edges: ribbonSubgraph.value.edges || [],
      combos: ribbonSubgraph.value.combos || [],
    });
  }
  return markRaw(fullGraphData.value);
});

// ── Helpers ────────────────────────────────────────────

function gradeColor(label: string | null): string {
  if (!label) return 'default';
  const g = String(label).toUpperCase();
  if (g === 'A') return 'success';
  if (g === 'B') return 'processing';
  if (g === 'C') return 'error';
  return 'default';
}

// ── Ribbon Search ──────────────────────────────────────

async function handleRibbonSearch(q: string) {
  ribbonSearchLoading.value = true;
  try {
    ribbonResults.value = await searchRibbon(q, 20);
  } catch (e: unknown) {
    message.error('搜索失败: ' + (e instanceof Error ? e.message : String(e)));
  } finally {
    ribbonSearchLoading.value = false;
  }
}

async function selectRibbon(r: RibbonSearchResult) {
  selectedRibbon.value = r;
  loading.value = true;
  try {
    ribbonSubgraph.value = await getRibbonSubgraph(r.furnace_no, 2);
  } catch (e: unknown) {
    message.error('加载子图失败: ' + (e instanceof Error ? e.message : String(e)));
    selectedRibbon.value = null;
  } finally {
    loading.value = false;
  }
}

function clearRibbon() {
  selectedRibbon.value = null;
  ribbonSubgraph.value = null;
  panel.value = null;
  highlightNodeIds.value = [];
  highlightEdgeIds.value = [];
}

// ── Toolbar handlers ───────────────────────────────────

async function loadData() {
  loading.value = true;
  try {
    const data = await getOntology();
    setOntologyData(data as OntologyData);
    fullGraphData.value = markRaw(buildFullGraph(searchText.value));
    message.success(`图谱加载完成！规格: ${data.specs.length}, 规则: ${data.rules.length}, 公式: ${data.formulas.length}`);
  } catch (e: unknown) {
    message.error('加载图谱失败: ' + (e instanceof Error ? e.message : String(e)));
  } finally {
    loading.value = false;
  }
}

async function handleResync() {
  resyncing.value = true;
  try {
    const result = await resyncNow();
    message.success(`重建完成！规则: ${result.rules}, 规格: ${result.specs}`);
  } catch (e: unknown) {
    message.error('重建失败: ' + (e instanceof Error ? e.message : String(e)));
  } finally {
    resyncing.value = false;
  }
}

function handleSearch(value: string) {
  searchText.value = value;
}

// ── Node interaction ───────────────────────────────────

function handleNodeClick(model: any) {
  const type = model.dataType || model.type;
  const raw = model.rawData || model.raw || {};
  const colorMap: Record<string, string> = {
    Ribbon: '#3B82F6',
    ProductSpec: '#2563EB',
    LaminationData: '#22C55E',
    SingleSheetPerformance: '#F59E0B',
    AppearanceFeature: '#EF4444',
    JudgementRule: '#A855F7',
  };
  const typeMap: Record<string, string> = {
    Ribbon: 'ribbon',
    ProductSpec: 'spec',
    LaminationData: 'lamination',
    SingleSheetPerformance: 'singleSheet',
    AppearanceFeature: 'appearance',
    JudgementRule: 'ruleCombo',
  };
  panel.value = {
    type: typeMap[type] || 'spec',
    typeLabel: type,
    label: model.label || model.name || '',
    color: colorMap[type] || '#64748B',
    raw,
    ruleCount: model.ruleCount,
    formulaCount: model.formulaCount,
  };
}

function handleComboClick(model: any) {
  panel.value = {
    type: 'ruleCombo',
    typeLabel: '规则组',
    label: model.label || '',
    color: model.qualityStatus === '合格' ? '#16A34A' : '#DC2626',
    specName: model.specName,
    qualityStatus: model.qualityStatus,
    rules: model.rules,
  };
}

function backToCombo() {
  // noop
}

function showRuleDetail(r: any) {
  panel.value = {
    type: 'rule',
    typeLabel: '判定规则',
    label: r.name || '',
    color: r.quality_status === '合格' ? '#16A34A' : '#DC2626',
    raw: r,
  };
}

function handlePanelAction(payload: Record<string, unknown>) {
  const actionType = payload.type as string;
  if (actionType === 'explore') {
    message.info('已在图谱中定位');
  }
}

// ── Question Mode ──────────────────────────────────────

function exitQuestionMode() {
  questionMode.value = false;
  currentQuestion.value = '';
  reasoningSteps.value = [];
  explainAnswer.value = '';
  explainAnswerCard.value = null;
  explainEvidenceTable.value = [];
  explainSuggestedActions.value = [];
  highlightNodeIds.value = [];
  highlightEdgeIds.value = [];
  activeStepId.value = '';
}

async function handleAsk(question: string) {
  const q = question.trim();
  if (!q) return;

  currentQuestion.value = q;
  questionMode.value = true;
  explainLoading.value = true;
  explainAnswer.value = '';
  explainAnswerCard.value = null;
  reasoningSteps.value = [];
  explainEvidenceTable.value = [];
  explainSuggestedActions.value = [];
  highlightNodeIds.value = [];
  highlightEdgeIds.value = [];
  activeStepId.value = '';

  try {
    const result: ExplainResponse = await explainQuestion({ question: q });
    explainAnswer.value = result.answer || '';
    explainAnswerCard.value = result.answer_card || null;
    reasoningSteps.value = result.reasoning_steps || [];
    explainEvidenceTable.value = result.evidence_table || [];
    explainSuggestedActions.value = result.suggested_actions || [];

    const nodeIds = new Set<string>();
    const edgeIds = new Set<string>();
    for (const step of result.reasoning_steps || []) {
      for (const ref of step.ontologyRefs || []) nodeIds.add(ref.id);
      for (const er of step.edgeRefs || []) edgeIds.add(`${er.source}-${er.relation}-${er.target}`);
    }
    highlightNodeIds.value = Array.from(nodeIds);
    highlightEdgeIds.value = Array.from(edgeIds);
  } catch (e: unknown) {
    message.error('分析失败: ' + (e instanceof Error ? e.message : String(e)));
  } finally {
    explainLoading.value = false;
  }
}

function handleStepSelect(step: ReasoningStep) {
  activeStepId.value = step.id;
  const nodeIds = new Set<string>();
  const edgeIds = new Set<string>();
  for (const ref of step.ontologyRefs || []) nodeIds.add(ref.id);
  for (const er of step.edgeRefs || []) edgeIds.add(`${er.source}-${er.relation}-${er.target}`);
  highlightNodeIds.value = Array.from(nodeIds);
  highlightEdgeIds.value = Array.from(edgeIds);
}

function handleHighlightNode(nodeId: string) {
  highlightNodeIds.value = [nodeId];
}

function handleSuggestedAction(action: { label: string; action: string; params?: Record<string, unknown> }) {
  message.info(`执行: ${action.label}`);
}

// ── Lifecycle ──────────────────────────────────────────

onMounted(() => {
  loadData();
});
</script>

<style lang="less" scoped>
.kg-page {
  height: calc(100vh - 112px);
  display: flex;
  flex-direction: column;
  overflow: hidden;
  background: #fff;
}

// 搜索面板
.kg-search-panel {
  flex-shrink: 0;
  padding: 16px 32px;
  max-height: 280px;
  overflow-y: auto;
  border-bottom: 1px solid #F1F5F9;
}

.search-header {
  max-width: 600px;
  margin: 0 auto 24px;
}

.search-hint {
  margin-top: 8px;
  font-size: 12px;
  color: #94A3B8;
  text-align: center;
  code {
    background: #F1F5F9;
    padding: 1px 4px;
    border-radius: 3px;
    font-size: 11px;
  }
}

.ribbon-list {
  max-width: 800px;
  margin: 0 auto;
  display: grid;
  grid-template-columns: repeat(auto-fill, minmax(280px, 1fr));
  gap: 12px;
}

.ribbon-card {
  background: #fff;
  border: 1px solid #E2E8F0;
  border-radius: 10px;
  padding: 14px 16px;
  cursor: pointer;
  transition: all 0.2s ease;
  box-shadow: 0 1px 2px rgba(0,0,0,0.04);

  &:hover {
    border-color: #93C5FD;
    box-shadow: 0 4px 12px rgba(37, 99, 235, 0.08);
    transform: translateY(-1px);
  }
}

.ribbon-main {
  display: flex;
  align-items: center;
  justify-content: space-between;
  margin-bottom: 8px;
}

.ribbon-no {
  font-size: 15px;
  font-weight: 700;
  color: #1E293B;
}

.ribbon-meta {
  display: flex;
  flex-wrap: wrap;
  gap: 8px;
  font-size: 12px;
  color: #64748B;
}

// 带材子图模式
.kg-ribbon-bar {
  padding: 10px 16px;
  border-bottom: 1px solid #F1F5F9;
  background: #FAFBFC;
  flex-shrink: 0;
  display: flex;
  align-items: center;
  gap: 12px;
}

.ribbon-title {
  font-size: 14px;
  font-weight: 600;
  color: #1E293B;
}

.ribbon-subtitle {
  font-size: 12px;
  color: #64748B;
  margin-left: auto;
}

.kg-main {
  flex: 1;
  display: flex;
  overflow: hidden;
}

.kg-answer-header {
  padding: 10px 16px;
  border-bottom: 1px solid #F1F5F9;
  background: #FAFBFC;
  flex-shrink: 0;
  display: flex;
  align-items: center;
  gap: 12px;
}

.question-text {
  font-size: 14px;
  font-weight: 600;
  color: #1E293B;
}

.kg-legend {
  padding: 6px 16px;
  border-top: 1px solid #F1F5F9;
  display: flex;
  gap: 16px;
  flex-shrink: 0;
  font-size: 12px;
  color: #94A3B8;
}

.leg {
  display: flex;
  align-items: center;
  gap: 4px;
}

.leg-dot {
  width: 10px;
  height: 10px;
  border-radius: 3px;
  display: inline-block;
}
</style>
