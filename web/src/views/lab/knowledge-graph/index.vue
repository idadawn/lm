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

    <!-- 问答输入栏（浏览模式下显示） -->
    <div v-if="!questionMode" class="kg-ask-bar">
      <a-input-search
        v-model:value="questionInput"
        placeholder="问一问，例如：为什么炉号 1丙20260110-1 是 C 级？"
        enter-button="提问"
        size="large"
        @search="handleAsk"
        :disabled="explainLoading"
      />
    </div>

    <!-- 问答结果头部 -->
    <div v-if="questionMode" class="kg-answer-header">
      <div class="answer-breadcrumb">
        <a-button type="link" size="small" @click="exitQuestionMode">
          <ArrowLeftOutlined /> 返回浏览
        </a-button>
        <span class="question-text">{{ currentQuestion }}</span>
      </div>
      <div v-if="explainLoading" class="answer-loading">
        <a-spin size="small" /> 正在分析...
      </div>
    </div>

    <div class="kg-main">
      <!-- ====== 浏览模式 ====== -->
      <template v-if="!questionMode">
        <SpecGrid
          v-if="viewMode === 'grid' && !loading"
          :specs="specCards"
          @select="enterSpecView"
          @viewAll="viewMode = 'graph'"
        />

        <KgCanvas
          v-else-if="viewMode === 'graph'"
          :data="graphData"
          :loading="loading"
          :empty="!ontology"
          :highlight-node-ids="highlightNodeIds"
          :highlight-edge-ids="highlightEdgeIds"
          @nodeClick="handleNodeClick"
          @comboClick="handleComboClick"
          @canvasClick="panel = null"
        />

        <KgDetailPanel
          :panel="panel"
          @close="panel = null"
          @back="backToCombo"
          @selectRule="showRuleDetail"
          @action="handlePanelAction"
        />
      </template>

      <!-- ====== 问答模式：三栏布局 ====== -->
      <template v-else>
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
      </template>
    </div>

    <div class="kg-legend" v-if="!questionMode">
      <span class="leg"><i class="leg-dot" style="background:#2563EB"></i> 产品规格</span>
      <span class="leg"><i class="leg-dot" style="background:#F97316"></i> 规则组</span>
      <span class="leg"><i class="leg-dot" style="background:#7C3AED"></i> 指标公式</span>
      <span class="leg"><i style="display:inline-block;width:24px;height:2px;background:#94A3B8;vertical-align:middle"></i> 关联关系</span>
      <span class="leg" style="margin-left:auto;color:#999">点击节点查看详情 | 搜索定位子图 | 拖拽/缩放画布</span>
    </div>
  </div>
</template>

<script lang="ts" setup>
import { ref, computed, onMounted } from 'vue';
import { message } from 'ant-design-vue';
import { ArrowLeftOutlined } from '@ant-design/icons-vue';
import {
  getOntology,
  resyncNow,
  explainQuestion,
  type ExplainResponse,
} from '/@/api/lab/knowledgeGraph';

import KgToolbar from './components/KgToolbar.vue';
import KgCanvas from './components/KgCanvas.vue';
import KgDetailPanel from './components/KgDetailPanel.vue';
import SpecGrid from './components/SpecGrid.vue';
import KgReasoningPanel from './components/KgReasoningPanel.vue';
import KgEvidencePanel from './components/KgEvidencePanel.vue';

import { useGraphData } from './composables/useGraphData';
import type { PanelData, ViewMode, ReasoningStep, OntologyRef, EdgeRef } from './types/ontology';
import type { OntologyRule } from './types/ontology';

// ── Browse Mode State ──────────────────────────────────

const loading = ref(false);
const resyncing = ref(false);
const searchText = ref('');
const viewMode = ref<ViewMode>('grid');
const panel = ref<PanelData | null>(null);
const selectedRuleId = ref('');

const {
  setData,
  buildGraphData,
  indexes,
} = useGraphData();

const ontology = ref<import('/@/api/lab/knowledgeGraph').OntologyData | null>(null);

// ── Question Mode State ────────────────────────────────

const questionMode = ref(false);
const questionInput = ref('');
const currentQuestion = ref('');
const explainLoading = ref(false);
const explainAnswer = ref('');
const explainAnswerCard = ref<Record<string, unknown> | null>(null);
const reasoningSteps = ref<ReasoningStep[]>([]);
const explainEvidenceTable = ref<Array<Record<string, unknown>>>([]);
const explainSuggestedActions = ref<Array<{ label: string; action: string; params?: Record<string, unknown> }>>([]);
const activeStepId = ref('');

// ── Highlight State ────────────────────────────────────

const highlightNodeIds = ref<string[]>([]);
const highlightEdgeIds = ref<string[]>([]);

// ── Computed ───────────────────────────────────────────

const specCards = computed(() => {
  if (!ontology.value) return [];
  const { rulesBySpec } = indexes.value;
  return ontology.value.specs.map((spec) => {
    const rules = rulesBySpec[spec.id] || [];
    const formulaIds = new Set(rules.map((r) => r.formula_id).filter(Boolean));
    const qualifiedCount = rules.filter((r) => r.quality_status === '合格').length;
    const unqualifiedCount = rules.filter((r) => r.quality_status !== '合格').length;
    return {
      ...spec,
      ruleCount: rules.length,
      formulaCount: formulaIds.size,
      qualifiedCount,
      unqualifiedCount,
    };
  });
});

const graphData = computed(() => {
  if (!ontology.value) return { nodes: [], edges: [], combos: [] };
  if (searchText.value) {
    return buildGraphData(searchText.value);
  }
  return buildGraphData();
});

// ── Data loading ───────────────────────────────────────

async function loadData() {
  loading.value = true;
  try {
    const data = await getOntology();
    ontology.value = data;
    setData(data);
  } catch (e: unknown) {
    message.error('加载失败: ' + (e instanceof Error ? e.message : String(e)));
  } finally {
    loading.value = false;
  }
}

async function handleResync() {
  resyncing.value = true;
  try {
    const result = await resyncNow();
    message.success(`重建完成！规则: ${result.rules}, 规格: ${result.specs}`);
    await loadData();
  } catch (e: unknown) {
    message.error('重建失败: ' + (e instanceof Error ? e.message : String(e)));
  } finally {
    resyncing.value = false;
  }
}

// ── Browse Mode Handlers ───────────────────────────────

function enterSpecView(specId: string) {
  viewMode.value = 'graph';
}

function handleSearch(value: string) {
  searchText.value = value;
  if (value && viewMode.value === 'grid') {
    viewMode.value = 'graph';
  }
}

// ── Question Mode Handlers ─────────────────────────────

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

    // 从 reasoning_steps 提取高亮节点和边
    const nodeIds = new Set<string>();
    const edgeIds = new Set<string>();

    for (const step of result.reasoning_steps || []) {
      for (const ref of step.ontologyRefs || []) {
        nodeIds.add(ref.id);
      }
      for (const er of step.edgeRefs || []) {
        edgeIds.add(`${er.source}-${er.relation}-${er.target}`);
      }
    }

    highlightNodeIds.value = Array.from(nodeIds);
    highlightEdgeIds.value = Array.from(edgeIds);

    // 自动切换到图谱视图以便高亮
    if (viewMode.value === 'grid') {
      viewMode.value = 'graph';
    }
  } catch (e: unknown) {
    message.error('问答失败: ' + (e instanceof Error ? e.message : String(e)));
  } finally {
    explainLoading.value = false;
  }
}

function exitQuestionMode() {
  questionMode.value = false;
  currentQuestion.value = '';
  questionInput.value = '';
  reasoningSteps.value = [];
  explainEvidenceTable.value = [];
  highlightNodeIds.value = [];
  highlightEdgeIds.value = [];
  activeStepId.value = '';
}

function handleStepSelect(step: ReasoningStep) {
  activeStepId.value = step.id;

  // 高亮该步骤关联的节点和边
  const nodeIds = new Set<string>();
  const edgeIds = new Set<string>();

  for (const ref of step.ontologyRefs || []) {
    nodeIds.add(ref.id);
  }
  for (const er of step.edgeRefs || []) {
    edgeIds.add(`${er.source}-${er.relation}-${er.target}`);
  }

  highlightNodeIds.value = Array.from(nodeIds);
  highlightEdgeIds.value = Array.from(edgeIds);
}

function handleHighlightNode(nodeId: string) {
  highlightNodeIds.value = [nodeId];
  highlightEdgeIds.value = [];
}

function handleSuggestedAction(action: { label: string; action: string; params?: Record<string, unknown> }) {
  message.info(`动作「${action.label}」待实现`);
}

// ── Panel handlers ─────────────────────────────────────

function handleNodeClick(model: any) {
  if (model.dataType === 'spec') {
    panel.value = {
      type: 'spec',
      typeLabel: '产品规格',
      label: model.rawData?.name || model.rawData?.code,
      color: '#2563EB',
      raw: model.rawData,
      ruleCount: model.ruleCount,
      formulaCount: model.formulaCount,
    };
  } else if (model.dataType === 'rule') {
    selectedRuleId.value = model.rawData?.id || '';
    const specName = model.rawData?.product_spec_name || '通用';
    const specId = model.rawData?.product_spec_id;
    const status = model.rawData?.quality_status;
    const comboRules = (specId && indexes.value.rulesBySpec[specId])
      ? indexes.value.rulesBySpec[specId].filter((r) => r.quality_status === status)
      : [];
    panel.value = {
      type: 'rule',
      typeLabel: '判定规则',
      label: model.rawData?.name || '',
      color: status === '合格' ? '#16A34A' : '#DC2626',
      raw: model.rawData,
      specName,
      qualityStatus: status,
      rules: comboRules,
    };
  } else if (model.dataType === 'formula') {
    const fId = model.rawData?.id;
    const linkedRules = fId ? (indexes.value.rulesByFormula[fId] || []) : [];
    panel.value = {
      type: 'formula',
      typeLabel: '指标公式',
      label: model.rawData?.formula_name || '',
      color: '#7C3AED',
      raw: model.rawData,
      ruleCount: linkedRules.length,
      linkedRules,
    };
  }
}

function handleComboClick(model: any) {
  panel.value = {
    type: 'ruleCombo',
    typeLabel: '规则组',
    label: `${model.specName} · ${model.qualityStatus}`,
    color: model.qualityStatus === '合格' ? '#16A34A' : '#DC2626',
    raw: null,
    specName: model.specName,
    qualityStatus: model.qualityStatus,
    rules: model.rules || [],
  };
}

function showRuleDetail(r: OntologyRule) {
  selectedRuleId.value = r.id;
  panel.value = {
    type: 'rule',
    typeLabel: '判定规则',
    label: r.name,
    color: r.quality_status === '合格' ? '#16A34A' : '#DC2626',
    raw: r,
    specName: r.product_spec_name,
    qualityStatus: r.quality_status,
    rules: (indexes.value.rulesBySpec[r.product_spec_id || ''] || []).filter(
      (x) => x.quality_status === r.quality_status
    ),
  };
}

function backToCombo() {
  if (!panel.value || panel.value.type !== 'rule') return;
  const r = panel.value.raw as OntologyRule;
  const specName = r.product_spec_name || '通用';
  const status = r.quality_status;
  const comboRules = (indexes.value.rulesBySpec[r.product_spec_id || ''] || []).filter(
    (x) => x.quality_status === status
  );
  panel.value = {
    type: 'ruleCombo',
    typeLabel: '规则组',
    label: `${specName} · ${status}`,
    color: status === '合格' ? '#16A34A' : '#DC2626',
    raw: null,
    specName,
    qualityStatus: status,
    rules: comboRules,
  };
}

function handlePanelAction(payload: Record<string, unknown>) {
  const actionType = payload.type as string;
  if (actionType === 'explore') {
    viewMode.value = 'graph';
    message.info('已在图谱中定位');
  } else if (actionType === 'records') {
    message.info('跳转检测记录（待实现）');
  }
}

// ── Lifecycle ──────────────────────────────────────────

onMounted(() => {
  loadData();
});
</script>

<style lang="less" scoped>
.kg-page {
  height: 100%;
  display: flex;
  flex-direction: column;
  overflow: hidden;
  background: #fff;
}

.kg-ask-bar {
  padding: 12px 16px;
  border-bottom: 1px solid #F1F5F9;
  background: #FAFBFC;
  flex-shrink: 0;
}

.kg-answer-header {
  padding: 10px 16px;
  border-bottom: 1px solid #F1F5F9;
  background: #FAFBFC;
  flex-shrink: 0;
  display: flex;
  align-items: center;
  justify-content: space-between;
}

.answer-breadcrumb {
  display: flex;
  align-items: center;
  gap: 12px;
}

.question-text {
  font-size: 14px;
  font-weight: 600;
  color: #1E293B;
}

.answer-loading {
  font-size: 13px;
  color: #64748B;
  display: flex;
  align-items: center;
  gap: 6px;
}

.kg-main {
  flex: 1;
  display: flex;
  overflow: hidden;
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
