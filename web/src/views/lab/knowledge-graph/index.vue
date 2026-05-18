<template>
  <div class="kg-page">
    <KgToolbar
      :loading="loading"
      :resyncing="resyncing"
      :search-text="searchText"
      @refresh="loadData"
      @resync="handleResync"
      @search="handleSearch"
    />

    <!-- ====== 主图谱区域（始终显示，问答模式除外） ====== -->
    <div class="kg-main" v-if="!questionMode">
      <RgCanvas
        v-if="graphData.nodes.length > 0"
        ref="canvasRef"
        :data="graphData"
        :loading="loading"
        :empty="!graphData.nodes.length"
        :highlight-node-ids="highlightNodeIds"
        :highlight-edge-ids="highlightEdgeIds"
        @nodeClick="handleNodeClick"
        @canvasClick="panel = null; clearHighlights()"
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
        <RgCanvas
          :data="graphData"
          :loading="explainLoading"
          :empty="false"
          :highlight-node-ids="highlightNodeIds"
          :highlight-edge-ids="highlightEdgeIds"
          @nodeClick="handleNodeClick"
          @canvasClick="panel = null; clearHighlights()"
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

    <!-- 底部图例（只显示当前激活的节点类型） -->
    <div class="kg-legend" v-if="!questionMode">
      <span v-if="ACTIVE_NODE_TYPES.has('Ribbon')" class="leg"><i class="leg-dot" style="background:#3B82F6"></i> 带材</span>
      <span v-if="ACTIVE_NODE_TYPES.has('ProductSpec')" class="leg"><i class="leg-dot" style="background:#2563EB"></i> 产品规格</span>
      <span v-if="ACTIVE_NODE_TYPES.has('SpecAttribute')" class="leg"><i class="leg-dot" style="background:#EB2F96"></i> 产品属性</span>
      <span v-if="ACTIVE_NODE_TYPES.has('IntermediateData')" class="leg"><i class="leg-dot" style="background:#0EA5E9"></i> 叠片数据</span>
      <span v-if="ACTIVE_NODE_TYPES.has('RawDataImport')" class="leg"><i class="leg-dot" style="background:#10B981"></i> 原始叠片导入</span>
      <span v-if="ACTIVE_NODE_TYPES.has('MagneticDataImport')" class="leg"><i class="leg-dot" style="background:#F59E0B"></i> 单片性能</span>
      <span v-if="ACTIVE_NODE_TYPES.has('TemplateField')" class="leg"><i class="leg-dot" style="background:#9CA3AF"></i> 字段映射</span>
      <span v-if="ACTIVE_NODE_TYPES.has('FurnaceNoInput')" class="leg"><i class="leg-dot" style="background:#D97706"></i> 原始炉号</span>
      <span v-if="ACTIVE_NODE_TYPES.has('FurnaceNoParsed')" class="leg"><i class="leg-dot" style="background:#059669"></i> 炉号</span>
      <span v-if="ACTIVE_NODE_TYPES.has('FurnaceNoField')" class="leg"><i class="leg-dot" style="background:#3B82F6"></i> 炉号字段</span>
      <span v-if="ACTIVE_NODE_TYPES.has('AppearanceFeature')" class="leg"><i class="leg-dot" style="background:#FF4D4F"></i> 外观特性</span>
      <span v-if="ACTIVE_NODE_TYPES.has('AppearanceCategory')" class="leg"><i class="leg-dot" style="background:#FA8C16"></i> 特性大类</span>
      <span v-if="ACTIVE_NODE_TYPES.has('AppearanceLevel')" class="leg"><i class="leg-dot" style="background:#9254DE"></i> 特性等级</span>
      <span v-if="ACTIVE_NODE_TYPES.has('Formula')" class="leg"><i class="leg-dot" style="background:#13C2C2"></i> 公式</span>
      <span v-if="ACTIVE_NODE_TYPES.has('JudgmentRule')" class="leg"><i class="leg-dot" style="background:#722ED1"></i> 判定等级</span>
      <span v-if="ACTIVE_NODE_TYPES.has('ReportConfig')" class="leg"><i class="leg-dot" style="background:#FAAD14"></i> 指标</span>
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
  getOntology,
  type ExplainResponse,
  type OntologyData,
} from '/@/api/lab/knowledgeGraph';

import KgToolbar from './components/KgToolbar.vue';
import RgCanvas from './components/RgCanvas.vue';
import KgDetailPanel from './components/KgDetailPanel.vue';
import KgReasoningPanel from './components/KgReasoningPanel.vue';
import KgEvidencePanel from './components/KgEvidencePanel.vue';

import type { PanelData, PanelType, ReasoningStep } from './types/ontology';
import {
  buildRelationGraphData,
  ACTIVE_NODE_TYPES,
  type RgGraphData,
} from './composables/useRelationGraphData';

// ── State ──────────────────────────────────────────────

const loading = ref(false);
const resyncing = ref(false);
const searchText = ref('');
const panel = ref<PanelData | null>(null);

// 全量图谱（Relation Graph 格式）
const fullGraphData = shallowRef<RgGraphData>({ rootId: 'domain:ribbon', nodes: [], lines: [] });

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

// RgCanvas 组件引用，用于父组件触发"归位/重新布局"等动作
const canvasRef = ref<any>(null);

// 点击节点时按"有向 BFS"计算子图：从点击节点出发顺着边找所有下游节点
// 用于点击产品规格 → 自动高亮它的所有判定规则 / 公式 / 属性
function computeSubgraphFrom(rootId: string): { nodes: Set<string>; edges: Set<string> } {
  const nodes = new Set<string>([rootId]);
  const edges = new Set<string>();
  const allLines: any[] = (fullGraphData.value as any)?.lines || [];
  // 建邻接表（按 from 索引）—— 同时也支持反向（用户点叶子节点想看它的来源）
  const outAdj: Record<string, any[]> = {};
  const inAdj: Record<string, any[]> = {};
  for (const line of allLines) {
    const f = line.from, t = line.to;
    if (!f || !t) continue;
    (outAdj[f] = outAdj[f] || []).push(line);
    (inAdj[t] = inAdj[t] || []).push(line);
  }
  // BFS 双向：先向下游（outAdj）展开 2 层，再把直接父节点（inAdj）补 1 层
  const queue: Array<{ id: string; depth: number; dir: 'out' | 'in' }> = [
    { id: rootId, depth: 0, dir: 'out' },
  ];
  const seen = new Set<string>([rootId]);
  while (queue.length) {
    const cur = queue.shift()!;
    if (cur.depth >= 3) continue; // 限制扩散深度 3 层
    const adj = cur.dir === 'out' ? outAdj[cur.id] : inAdj[cur.id];
    if (!adj) continue;
    for (const line of adj) {
      const next = cur.dir === 'out' ? line.to : line.from;
      const eid = `${line.from}-${line.text || line.relation || ''}-${line.to}`;
      edges.add(eid);
      // line.id 兜底
      if (line.id) edges.add(line.id);
      if (!seen.has(next)) {
        seen.add(next);
        nodes.add(next);
        queue.push({ id: next, depth: cur.depth + 1, dir: cur.dir });
      }
    }
  }
  // 再把根节点的 1 层父节点也带上，用户更容易看到"上下文"
  for (const line of inAdj[rootId] || []) {
    nodes.add(line.from);
    const eid = `${line.from}-${line.text || line.relation || ''}-${line.to}`;
    edges.add(eid);
    if (line.id) edges.add(line.id);
  }
  return { nodes, edges };
}

function clearHighlights() {
  highlightNodeIds.value = [];
  highlightEdgeIds.value = [];
}

// ── Computed ───────────────────────────────────────────

const graphData = computed(() => {
  return markRaw(fullGraphData.value);
});

const ontologyData = ref<OntologyData | null>(null);

// ── Helpers ────────────────────────────────────────────

// ── Toolbar handlers ───────────────────────────────────

async function loadData() {
  loading.value = true;
  try {
    const data = await getOntology();
    ontologyData.value = data as OntologyData;
    fullGraphData.value = markRaw(buildRelationGraphData(data as OntologyData, searchText.value));

    // 统计实际显示的节点（按类型）
    const counts: Record<string, number> = {};
    for (const n of fullGraphData.value.nodes) {
      const t = n.data?.dataType || 'Unknown';
      counts[t] = (counts[t] || 0) + 1;
    }
    const parts = [];
    if (counts.Ribbon) parts.push(`根节点: ${counts.Ribbon}`);
    if (counts.ProductSpec) parts.push(`规格: ${counts.ProductSpec}`);
    if (counts.JudgmentRule) parts.push(`规则: ${counts.JudgmentRule}`);
    if (counts.Formula) parts.push(`公式: ${counts.Formula}`);
    if (parts.length === 0) parts.push('无可见节点');
    message.success(`图谱加载完成！${parts.join(', ')}`);
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

  // ★ 点击任意节点 → 计算子图（含 3 层下游 + 1 层上游）→ 高亮：相关节点保持原色，其他节点半透明淡化
  if (model.id) {
    const { nodes: subNodes, edges: subEdges } = computeSubgraphFrom(model.id);
    highlightNodeIds.value = Array.from(subNodes);
    highlightEdgeIds.value = Array.from(subEdges);
  }

  const colorMap: Record<string, string> = {
    Ribbon: '#3B82F6',
    ProductSpec: '#2563EB',
    SpecAttribute: '#EB2F96',
    LaminationData: '#22C55E',
    SingleSheetPerf: '#F59E0B',
    AppearanceFeature: '#EF4444',
    AppearanceCategory: '#FA8C16',
    AppearanceLevel: '#9254DE',
    JudgmentRule: '#722ED1',
    Formula: '#13C2C2',
    formula: '#13C2C2',
    MetricJudge: '#FAAD14',
    ReportConfig: '#FAAD14',
    RawDataImport: '#10B981',
    MagneticDataImport: '#F59E0B',
    IntermediateData: '#0EA5E9',
    TemplateField: '#9CA3AF',
    FurnaceNoInput: '#D97706',
    FurnaceNoParsed: '#059669',
    FurnaceNoField: '#3B82F6',
  };

  // 点击带材根节点 → 展示产品规格 + 扩展属性 + 叠片数据 + 单片性能（折叠区域）
  if (type === 'Ribbon') {
    // 产品规格列表
    const products = (ontologyData.value?.specs || []).map((s) => ({
      id: s.id,
      name: s.name || s.code || '未命名规格',
      code: s.code,
      description: s.description || '',
    }));
    // 扩展属性名列表（去重）
    const uniqueAttrNames = new Set<string>();
    for (const attr of ontologyData.value?.spec_attributes || []) {
      uniqueAttrNames.add(attr.name || attr.attr_key || '属性');
    }
    const attrList = Array.from(uniqueAttrNames).map((name) => ({
      id: name,
      name,
      code: '',
      description: '',
    }));
    // 叠片数据表字段
    const rawDataFields = (ontologyData.value?.table_fields?.['lab_raw_data'] || []).map((tf) => ({
      id: tf.column_name,
      name: tf.column_comment || tf.column_name,
      code: tf.column_name,
      description: '',
    }));
    // 单片性能表字段
    const magneticDataFields = (ontologyData.value?.table_fields?.['lab_magnetic_raw_data'] || []).map((tf) => ({
      id: tf.column_name,
      name: tf.column_comment || tf.column_name,
      code: tf.column_name,
      description: '',
    }));
    panel.value = {
      type: 'ribbonRoot',
      typeLabel: '带材',
      label: '带材',
      color: '#3B82F6',
      raw: {
        ...raw,
        rawDataFields,
        magneticDataFields,
      },
      products,     // 产品规格（可点击）
      specs: attrList,  // 扩展属性（只读列表）
    };
    return;
  }

  // 产品规格聚合节点 → 展示所有规格条目
  if (type === 'ProductSpec' && (raw as any)?.isAggregate) {
    const specsAll = ((raw as any).specs as Array<any>) || ontologyData.value?.specs || [];
    panel.value = {
      type: 'productSpecList',
      typeLabel: '产品规格',
      label: '产品规格',
      color: colorMap.ProductSpec,
      raw,
      specs: specsAll.map((s) => ({
        id: s.id,
        name: s.name || s.code || '未命名规格',
        code: s.code || '',
        description: s.description || '',
      })),
    };
    return;
  }

  // 产品扩展信息聚合节点 → 展示去重后的扩展属性
  if (type === 'SpecAttribute' && (raw as any)?.isAggregate) {
    const attrsAll = ((raw as any).attributes as Array<any>) || [];
    panel.value = {
      type: 'specAttributeList',
      typeLabel: '产品扩展信息',
      label: '产品扩展信息',
      color: colorMap.SpecAttribute,
      raw,
      specs: attrsAll.map((a) => ({
        id: a.id || a.attr_key || a.name,
        name: a.name || a.attr_key || '属性',
        code: a.value_type || '',
        description: [a.unit ? `单位 ${a.unit}` : '', a.specCount ? `${a.specCount} 个规格` : '']
          .filter(Boolean)
          .join(' · '),
      })),
    };
    return;
  }

  // 单条扩展属性条目（聚合节点的子节点）
  if (type === 'SpecAttribute' && !(raw as any)?.isAggregate) {
    panel.value = {
      type: 'specAttributeItem',
      typeLabel: '扩展属性',
      label: (raw as any)?.name || (raw as any)?.attr_key || '扩展属性',
      color: colorMap.SpecAttribute,
      raw,
    };
    return;
  }

  // ── 外观特性体系：聚合根 / 大类聚合 / 特性聚合 / 修正聚合 / 各类条目 ──

  // 外观特性聚合根（带材的子节点）
  if (type === 'AppearanceFeature' && (raw as any)?.isAggregate && (raw as any)?.isRoot) {
    panel.value = {
      type: 'appearanceFeatureRoot',
      typeLabel: '外观特性',
      label: '外观特性',
      color: colorMap.AppearanceFeature,
      raw,
    };
    return;
  }

  // 单条特性
  if (type === 'AppearanceFeature' && !(raw as any)?.isAggregate) {
    panel.value = {
      type: 'appearanceFeatureItem',
      typeLabel: '外观特性',
      label: (raw as any)?.name || '特性',
      color: colorMap.AppearanceFeature,
      raw,
    };
    return;
  }

  // 单个大类
  if (type === 'AppearanceCategory' && !(raw as any)?.isAggregate) {
    // 同时把该大类下的所有特性带到面板里
    const featsUnder = (ontologyData.value?.appearance_features || []).filter(
      (f) => f.category_id === (raw as any)?.id,
    );
    panel.value = {
      type: 'appearanceCategoryItem',
      typeLabel: '特性大类',
      label: (raw as any)?.name || '大类',
      color: colorMap.AppearanceCategory,
      raw,
      specs: featsUnder.map((f) => ({
        id: f.id,
        name: f.name,
        code: f.level_name || '默认',
        description: (f.keywords || []).join(' / '),
      })),
    };
    return;
  }

  // 特性等级聚合
  if (type === 'AppearanceLevel' && (raw as any)?.isAggregate) {
    const lvs = ((raw as any).levels as Array<any>) || [];
    panel.value = {
      type: 'appearanceLevelList',
      typeLabel: '特性等级',
      label: '特性等级',
      color: colorMap.AppearanceLevel,
      raw,
      specs: lvs.map((lv) => ({
        id: lv.id,
        name: lv.name,
        code: lv.is_default ? '默认' : (lv.enabled ? '启用' : '停用'),
        description: lv.description || '',
      })),
    };
    return;
  }

  // ── 公式体系：聚合根 / 类型组 / 单条公式 ──

  // 公式聚合根
  if (type === 'Formula' && (raw as any)?.isAggregate && (raw as any)?.isRoot) {
    panel.value = {
      type: 'formulaRoot',
      typeLabel: '公式',
      label: '公式',
      color: colorMap.Formula,
      raw,
    };
    return;
  }

  // 公式类型组（计算 / 判定 / 只展示）
  if (type === 'Formula' && (raw as any)?.isTypeGroup) {
    const fs = ((raw as any).formulas as Array<any>) || [];
    panel.value = {
      type: 'formulaTypeGroup',
      typeLabel: (raw as any).label || '类型组',
      label: (raw as any).label || '类型组',
      color: colorMap.Formula,
      raw,
      specs: fs.map((f) => ({
        id: f.id,
        name: f.formula_name || f.column_name || '公式',
        code: f.column_name || '',
        description: [
          f.unit_name ? `单位 ${f.unit_name}` : '',
          f.source_type === 'CUSTOM' ? '自定义' : '系统',
          f.is_enabled === false ? '已禁用' : '',
        ].filter(Boolean).join(' · '),
      })),
    };
    return;
  }

  // 单条公式
  if (type === 'Formula' && !(raw as any)?.isAggregate && !(raw as any)?.isTypeGroup) {
    panel.value = {
      type: 'formulaItem',
      typeLabel: '公式',
      label: (raw as any)?.formula_name || (raw as any)?.column_name || '公式',
      color: colorMap.Formula,
      raw,
    };
    return;
  }

  // ── 判定等级体系：聚合根 / 规格判定集 / 单条等级 ──

  // 判定等级聚合根
  if (type === 'JudgmentRule' && (raw as any)?.isAggregate && (raw as any)?.isRoot) {
    panel.value = {
      type: 'judgmentLevelRoot',
      typeLabel: '判定等级',
      label: '判定等级',
      color: colorMap.JudgmentRule,
      raw,
    };
    return;
  }

  // 按规格分组的判定集
  if (type === 'JudgmentRule' && (raw as any)?.isSpecGroup) {
    const rulesInGrp = ((raw as any).rules as Array<any>) || [];
    panel.value = {
      type: 'judgmentLevelSpecGroup',
      typeLabel: '规格判定集',
      label: (raw as any).specName || '规格判定集',
      color: colorMap.JudgmentRule,
      raw,
      specs: rulesInGrp.map((r) => ({
        id: r.id,
        name: r.name || '等级',
        code: r.quality_status || '',
        description: r.formula_name ? `公式: ${r.formula_name}` : '',
      })),
    };
    return;
  }

  // 单条判定等级
  if (type === 'JudgmentRule' && !(raw as any)?.isAggregate && !(raw as any)?.isSpecGroup) {
    panel.value = {
      type: 'judgmentLevelItem',
      typeLabel: '判定等级',
      label: (raw as any)?.name || '等级',
      color: (raw as any)?.color || colorMap.JudgmentRule,
      raw,
    };
    return;
  }

  // ── 统计指标体系：聚合根 / 单条指标 ──

  // 指标聚合根
  if (type === 'ReportConfig' && (raw as any)?.isAggregate && (raw as any)?.isRoot) {
    const ms = ((raw as any).metrics as Array<any>) || [];
    const allTpls = (ontologyData.value as any)?.sql_templates || [];
    panel.value = {
      type: 'metricRoot',
      typeLabel: '指标',
      label: '指标',
      color: colorMap.ReportConfig,
      raw: { ...raw, sql_templates: allTpls },
      specs: ms.map((m) => ({
        id: m.id,
        name: m.name,
        code: m.is_percentage ? '占比' : '汇总',
        description: (m.level_names || []).length > 0 ? `等级: ${(m.level_names).join('、')}` : '',
      })),
    };
    return;
  }

  // 单条指标
  if (type === 'ReportConfig' && !(raw as any)?.isAggregate) {
    // 解析公式名（横向引用）
    const formula = (ontologyData.value?.formulas || []).find((f) => f.id === (raw as any)?.formula_id);
    // 抓 NLQ SQL 模板（按 applicable_metric_id 过滤，'*' = 通用）
    const allTpls = (ontologyData.value as any)?.sql_templates || [];
    const applicableTpls = allTpls.filter((t: any) =>
      !t.applicable_metric_id || t.applicable_metric_id === '*' || t.applicable_metric_id === (raw as any)?.id,
    );
    panel.value = {
      type: 'metricItem',
      typeLabel: '指标',
      label: (raw as any)?.name || '指标',
      color: colorMap.ReportConfig,
      raw: {
        ...raw,
        formula_name_resolved: formula?.formula_name || formula?.column_name || '',
        formula_column_name: formula?.column_name || '',
        sql_templates: applicableTpls,
      },
    };
    return;
  }

  // 叠片数据成品视图（合并原 IntermediateData + 原 RawDataImport 直挂语义）
  if (type === 'IntermediateData') {
    // 从 excel_templates 找到两个数据源模板（用于面板里展示来源链接）
    const tmpls = ontologyData.value?.excel_templates || [];
    const rawTmpl = tmpls.find((t) => t.template_code === 'RawDataImport');
    const magTmpl = tmpls.find((t) => t.template_code === 'MagneticDataImport');
    panel.value = {
      type: 'laminationDataView',
      typeLabel: '叠片数据',
      label: '叠片数据',
      color: colorMap.IntermediateData,
      raw: {
        ...raw,
        rawSourceId: rawTmpl ? `tmpl:${rawTmpl.id}` : '',
        rawSourceName: rawTmpl?.template_name || '原始叠片导入',
        magneticSourceId: magTmpl ? `tmpl:${magTmpl.id}` : '',
        magneticSourceName: magTmpl?.template_name || '单片性能',
        formulaCounts: {
          calc: (ontologyData.value?.formulas || []).filter((f) => f.formula_type === 'CALC').length,
          judge: (ontologyData.value?.formulas || []).filter((f) => f.formula_type === 'JUDGE').length,
          no: (ontologyData.value?.formulas || []).filter((f) => f.formula_type === 'NO').length,
        },
        // 维度元数据（NLQ 用）
        dimensions: (ontologyData.value as any)?.dimensions || null,
      },
    };
    return;
  }

  // 单条特性等级
  if (type === 'AppearanceLevel' && !(raw as any)?.isAggregate) {
    // 同时把该等级下的所有特性带到面板里
    const featsAtLevel = (ontologyData.value?.appearance_features || []).filter(
      (f) => f.severity_level_id === (raw as any)?.id,
    );
    panel.value = {
      type: 'appearanceLevelItem',
      typeLabel: '特性等级',
      label: (raw as any)?.name || '等级',
      color: colorMap.AppearanceLevel,
      raw,
      specs: featsAtLevel.map((f) => ({
        id: f.id,
        name: f.name,
        code: f.category_id ? '已分类' : '未分类',
        description: (f.keywords || []).join(' / '),
      })),
    };
    return;
  }

  const typeMap: Record<string, PanelType> = {
    ProductSpec: 'spec',
    LaminationData: 'lamination',
    SingleSheetPerf: 'singleSheet',
    AppearanceFeature: 'appearance',
    JudgmentRule: 'rule',
    Formula: 'formula',
    formula: 'formula',
    MetricJudge: 'spec',
    ReportConfig: 'reportConfig',
    RawDataImport: 'spec',
    MagneticDataImport: 'spec',
    IntermediateData: 'spec',
    TemplateField: 'spec',
    FurnaceNoInput: 'furnaceNo',
    FurnaceNoParsed: 'furnaceNo',
    FurnaceNoField: 'furnaceNo',
  };

  // 收集该规格的属性（来自 lab_product_spec_attribute）
  // 过滤条件：F_PRODUCT_SPEC_ID = spec.F_Id 且 F_VERSION = spec.F_VERSION（当前版本）
  const specId = raw?.id as string | undefined;
  const specVersion = (raw as any)?.version as string | undefined;
  const specAttrs = specId
    ? (ontologyData.value?.spec_attributes || []).filter((a) => {
        if (a.spec_id !== specId) return false;
        if (specVersion && a.version) return a.version === specVersion;
        return true;  // 没有版本信息时兜底，只按 spec_id 过滤
      })
    : [];

  // 收集 Excel 模板的字段（用于 RawDataImport / MagneticDataImport 面板）
  const tmplCode = raw?.template_code as string | undefined;
  const tmplItem = tmplCode
    ? (ontologyData.value?.excel_templates || []).find((t) => t.template_code === tmplCode)
    : undefined;
  const tmplFields = tmplItem?.field_mappings || [];

  // 目标表字段（lab_magnetic_raw_data / lab_raw_data）
  const targetTable = raw?.targetTable as string | undefined;
  const tableFieldList = targetTable
    ? (ontologyData.value?.table_fields?.[targetTable] || [])
    : [];

  panel.value = {
    type: typeMap[type] || 'spec',
    typeLabel: type,
    label: model.label || model.name || '',
    color: colorMap[type] || '#64748B',
    raw,
    ruleCount: model.ruleCount,
    formulaCount: model.formulaCount,
    specs: type === 'RawDataImport' || type === 'MagneticDataImport' || type === 'IntermediateData'
      ? tableFieldList.map((tf) => {
          const fieldNameLower = tf.column_name.replace(/^F_/, '').toLowerCase();
          const mapping = tmplItem?.field_mappings?.find(
            (m) => m.field.toLowerCase() === fieldNameLower
          );
          return {
            id: tf.column_name,
            name: tf.column_name,           // 数据库字段名
            code: mapping?.field || '',     // C# 属性名
            description: tf.column_comment || '', // 中文注释
          };
        })
      : specAttrs.map((a) => ({
          id: a.id,
          name: a.name || a.attr_key || '属性',
          code: a.value ? `${a.value}${a.unit || ''}` : a.attr_key,
          description: a.value_type || '',
        })),
    products: type === 'RawDataImport' || type === 'MagneticDataImport'
      ? tmplFields.map((f) => ({
          id: f.field,
          name: f.label || f.field,
          code: f.data_type || '',
          description: f.required ? '必填' : '选填',
        }))
      : undefined,
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
  if (actionType === 'selectSpec') {
    const spec = payload.spec as Record<string, unknown> | undefined;
    if (!spec) return;
    // 查找完整的规格数据
    const fullSpec = ontologyData.value?.specs?.find((s) => s.id === spec.id);
    const effective = (fullSpec || spec) as any;
    // 同步带出该规格的扩展信息（按 spec_id + version 过滤），与点节点路径一致
    const specVersion = effective?.version as string | undefined;
    const specAttrs = effective?.id
      ? (ontologyData.value?.spec_attributes || []).filter((a) => {
          if (a.spec_id !== effective.id) return false;
          if (specVersion && a.version) return a.version === specVersion;
          return true;
        })
      : [];
    panel.value = {
      type: 'spec',
      typeLabel: 'ProductSpec',
      label: (spec.name as string) || '产品规格',
      color: '#2563EB',
      raw: effective as Record<string, unknown>,
      ruleCount: 0,
      formulaCount: 0,
      specs: specAttrs.map((a) => ({
        id: a.id,
        name: a.name || a.attr_key || '属性',
        code: a.value ? `${a.value}${a.unit || ''}` : (a.attr_key || ''),
        description: a.value_type || '',
      })),
    };
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
