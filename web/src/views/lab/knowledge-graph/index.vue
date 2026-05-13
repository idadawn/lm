<template>
  <div class="knowledge-graph-page">
    <div class="kg-toolbar">
      <a-space>
        <a-button type="primary" :loading="resyncing" @click="handleResync">
          <template #icon><ReloadOutlined /></template>
          全量重建
        </a-button>
        <a-button :loading="loading" @click="loadOntology">
          <template #icon><SyncOutlined /></template>
          刷新
        </a-button>
        <a-input-search
          v-model:value="searchText"
          placeholder="搜索节点..."
          style="width: 220px"
          @search="handleFilter"
        />
        <a-radio-group v-model:value="filterType" size="small" @change="handleFilter">
          <a-radio-button value="all">全部</a-radio-button>
          <a-radio-button value="spec">规格</a-radio-button>
          <a-radio-button value="rule">规则</a-radio-button>
          <a-radio-button value="formula">公式</a-radio-button>
        </a-radio-group>
      </a-space>
    </div>

    <div class="kg-main">
      <div class="kg-canvas-wrap" ref="canvasRef">
        <div class="kg-loading" v-if="loading">
          <a-spin tip="加载本体数据..." />
        </div>
      </div>

      <transition name="slide">
        <div class="kg-detail" v-if="detail">
          <div class="detail-head">
            <span class="detail-title">{{ detail.label }}</span>
            <a-tag :color="detail.color">{{ detail.typeLabel }}</a-tag>
            <a-button type="text" size="small" @click="detail = null" style="margin-left: auto">
              <template #icon><CloseOutlined /></template>
            </a-button>
          </div>
          <a-divider style="margin: 8px 0" />

          <!-- 规格详情 -->
          <template v-if="detail.type === 'spec'">
            <div class="detail-row"><strong>代码:</strong> {{ detail.raw.code }}</div>
            <div class="detail-row"><strong>名称:</strong> {{ detail.raw.name }}</div>
            <div class="detail-row" v-if="detail.raw.description">
              <strong>描述:</strong> {{ detail.raw.description }}
            </div>
            <div class="detail-section">
              <strong>属性 ({{ detail.raw.attributes?.length || 0 }})</strong>
              <div class="attr-list">
                <div class="attr-item" v-for="attr in (detail.raw.attributes || [])" :key="attr.key">
                  <span class="attr-name">{{ attr.name }}</span>
                  <span class="attr-val">{{ attr.value }} {{ attr.unit }}</span>
                </div>
              </div>
            </div>
          </template>

          <!-- 规则详情 -->
          <template v-if="detail.type === 'rule'">
            <div class="detail-row"><strong>等级:</strong> {{ detail.raw.name }}</div>
            <div class="detail-row"><strong>代码:</strong> {{ detail.raw.code }}</div>
            <div class="detail-row">
              <strong>质量状态:</strong>
              <a-tag :color="detail.raw.quality_status === '合格' ? 'success' : detail.raw.quality_status === '不合格' ? 'error' : 'default'">
                {{ detail.raw.quality_status }}
              </a-tag>
            </div>
            <div class="detail-row"><strong>优先级:</strong> {{ detail.raw.priority }}</div>
            <div class="detail-row" v-if="detail.raw.product_spec_name">
              <strong>产品规格:</strong> {{ detail.raw.product_spec_name }}
            </div>
            <div class="detail-row" v-if="detail.raw.formula_name">
              <strong>所属公式:</strong> {{ detail.raw.formula_name }}
            </div>
            <div class="detail-row" v-if="detail.raw.description">
              <strong>说明:</strong> {{ detail.raw.description }}
            </div>
          </template>

          <!-- 公式详情 -->
          <template v-if="detail.type === 'formula'">
            <div class="detail-row"><strong>名称:</strong> {{ detail.raw.formula_name }}</div>
            <div class="detail-row"><strong>类型:</strong>
              <a-tag>{{ detail.raw.formula_type === 'CALC' ? '计算公式' : detail.raw.formula_type === 'JUDGE' ? '判定公式' : detail.raw.formula_type }}</a-tag>
            </div>
            <div class="detail-row"><strong>列名:</strong> {{ detail.raw.column_name }}</div>
            <div class="detail-row"><strong>单位:</strong> {{ detail.raw.unit_name || '-' }}</div>
            <div class="detail-section" v-if="detail.raw.formula">
              <strong>公式</strong>
              <pre class="detail-code">{{ detail.raw.formula }}</pre>
            </div>
            <div class="detail-section">
              <strong>关联规则 ({{ detail.ruleCount || 0 }})</strong>
            </div>
          </template>
        </div>
      </transition>
    </div>

    <div class="kg-legend">
      <span class="legend-item"><span class="legend-dot" style="background:#1890ff"></span> 产品规格</span>
      <span class="legend-item"><span class="legend-dot" style="background:#52c41a"></span> 判定规则 (合格)</span>
      <span class="legend-item"><span class="legend-dot" style="background:#ff4d4f"></span> 判定规则 (不合格)</span>
      <span class="legend-item"><span class="legend-dot" style="background:#722ed1"></span> 指标公式</span>
      <span class="legend-item"><span class="legend-dot" style="background:#13c2c2"></span> 规格属性</span>
    </div>
  </div>
</template>

<script lang="ts" setup>
import { ref, onMounted, onBeforeUnmount, nextTick } from 'vue';
import { message } from 'ant-design-vue';
import { ReloadOutlined, SyncOutlined, CloseOutlined } from '@ant-design/icons-vue';
import G6, { IGraph, IG6GraphEvent } from '@antv/g6';
import {
  getOntology,
  resyncNow,
  type OntologyData,
  type OntologySpec,
  type OntologyRule,
  type OntologyFormula,
} from '/@/api/lab/knowledgeGraph';

interface DetailInfo {
  id: string;
  label: string;
  type: 'spec' | 'rule' | 'formula' | 'attribute';
  typeLabel: string;
  color: string;
  raw: any;
  ruleCount?: number;
}

const canvasRef = ref<HTMLElement | null>(null);
const loading = ref(false);
const resyncing = ref(false);
const searchText = ref('');
const filterType = ref('all');
const detail = ref<DetailInfo | null>(null);

let graph: IGraph | null = null;
let ontologyData: OntologyData | null = null;

function buildNodes(data: OntologyData, filter: string, search: string) {
  const nodes: any[] = [];
  const edges: any[] = [];

  const searchLower = search.toLowerCase();
  const matchSearch = (text: string) => !searchLower || text.toLowerCase().includes(searchLower);

  // --- Product Spec nodes ---
  for (const spec of data.specs) {
    if (filter !== 'all' && filter !== 'spec') continue;
    if (searchLower && !matchSearch(spec.name) && !matchSearch(spec.code)) continue;

    nodes.push({
      id: `spec:${spec.id}`,
      label: spec.name || spec.code,
      nodeType: 'spec',
      type: 'rect',
      size: [120, 40],
      style: {
        fill: '#e6f7ff',
        stroke: '#1890ff',
        lineWidth: 2,
        radius: 6,
      },
      labelCfg: { style: { fill: '#1890ff', fontSize: 12, fontWeight: 600 } },
      rawData: spec,
    });

    // Attribute nodes
    for (const attr of spec.attributes || []) {
      if (searchLower && !matchSearch(attr.name) && !matchSearch(attr.value)) continue;
      const attrId = `attr:${spec.id}:${attr.key}`;
      nodes.push({
        id: attrId,
        label: `${attr.name}: ${attr.value}${attr.unit ? ' ' + attr.unit : ''}`,
        nodeType: 'attribute',
        type: 'rect',
        size: [100, 24],
        style: {
          fill: '#e6fffb',
          stroke: '#13c2c2',
          lineWidth: 1,
          radius: 4,
        },
        labelCfg: { style: { fill: '#13c2c2', fontSize: 10 } },
      });
      edges.push({
        source: `spec:${spec.id}`,
        target: attrId,
        label: '属性',
        style: { stroke: '#b5f5ec', lineWidth: 1, lineDash: [4, 2] },
        labelCfg: { style: { fill: '#999', fontSize: 8 } },
      });
    }
  }

  // --- Formula nodes ---
  for (const f of data.formulas) {
    if (filter !== 'all' && filter !== 'formula') continue;
    if (searchLower && !matchSearch(f.formula_name) && !matchSearch(f.column_name)) continue;

    nodes.push({
      id: `formula:${f.id}`,
      label: f.formula_name || f.column_name,
      nodeType: 'formula',
      type: 'diamond',
      size: [80, 50],
      style: {
        fill: '#f9f0ff',
        stroke: '#722ed1',
        lineWidth: 2,
      },
      labelCfg: { style: { fill: '#722ed1', fontSize: 11 } },
      rawData: f,
    });
  }

  // --- Rule nodes ---
  const specIds = new Set(data.specs.map((s) => s.id));
  for (const r of data.rules) {
    if (filter !== 'all' && filter !== 'rule') continue;
    if (searchLower && !matchSearch(r.name) && !matchSearch(r.code) && !matchSearch(r.quality_status)) continue;

    const isQualified = r.quality_status === '合格';
    const fillColor = isQualified ? '#f6ffed' : '#fff2f0';
    const strokeColor = isQualified ? '#52c41a' : '#ff4d4f';

    nodes.push({
      id: `rule:${r.id}`,
      label: r.name || r.code,
      nodeType: 'rule',
      type: 'circle',
      size: 28,
      style: { fill: fillColor, stroke: strokeColor, lineWidth: 2 },
      labelCfg: { style: { fill: strokeColor, fontSize: 10 } },
      rawData: r,
    });

    // Rule → Spec edge
    if (r.product_spec_id && specIds.has(r.product_spec_id)) {
      edges.push({
        source: `rule:${r.id}`,
        target: `spec:${r.product_spec_id}`,
        style: { stroke: '#91d5ff', lineWidth: 1 },
      });
    }

    // Rule → Formula edge
    if (r.formula_id) {
      edges.push({
        source: `rule:${r.id}`,
        target: `formula:${r.formula_id}`,
        label: '判定依据',
        style: { stroke: '#d3adf7', lineWidth: 1, endArrow: true },
        labelCfg: { style: { fill: '#999', fontSize: 8 } },
      });
    }
  }

  return { nodes, edges };
}

function initGraph() {
  if (!canvasRef.value) return;
  const container = canvasRef.value;

  graph = new G6.Graph({
    container,
    width: container.offsetWidth,
    height: container.offsetHeight,
    fitView: true,
    fitViewPadding: 30,
    animate: true,
    minZoom: 0.3,
    maxZoom: 3,
    modes: {
      default: ['drag-canvas', 'zoom-canvas', 'drag-node'],
    },
    layout: {
      type: 'gForce',
      preventOverlap: true,
      nodeSize: 40,
      linkDistance: 100,
      nodeStrength: -200,
      edgeStrength: 0.1,
      gravity: 10,
    },
    defaultEdge: {
      style: { stroke: '#e8e8e8', lineWidth: 1 },
    },
  });

  graph.on('node:click', (evt: IG6GraphEvent) => {
    const model = evt.item?.getModel();
    if (!model) return;
    const raw = model.rawData;
    if (!raw) { detail.value = null; return; }

    const typeMap: Record<string, { type: DetailInfo['type']; label: string; color: string }> = {
      spec: { type: 'spec', label: '产品规格', color: '#1890ff' },
      rule: { type: 'rule', label: '判定规则', color: raw.quality_status === '合格' ? '#52c41a' : '#ff4d4f' },
      formula: { type: 'formula', label: '指标公式', color: '#722ed1' },
      attribute: { type: 'attribute', label: '属性', color: '#13c2c2' },
    };
    const info = typeMap[model.nodeType as string];
    if (!info) return;

    const ruleCount = model.nodeType === 'formula' && ontologyData
      ? ontologyData.rules.filter((r) => r.formula_id === raw.id).length
      : undefined;

    detail.value = {
      id: model.id as string,
      label: (model.label as string) || '',
      type: info.type,
      typeLabel: info.label,
      color: info.color,
      raw,
      ruleCount,
    };
  });

  graph.on('canvas:click', () => { detail.value = null; });
}

function renderGraph() {
  if (!graph || !ontologyData) return;
  const { nodes, edges } = buildNodes(ontologyData, filterType.value, searchText.value);
  graph.data({ nodes, edges });
  graph.render();
}

async function loadOntology() {
  loading.value = true;
  try {
    ontologyData = await getOntology();
    if (!graph) {
      await nextTick();
      initGraph();
    }
    renderGraph();
  } catch (e: unknown) {
    message.error('加载本体数据失败: ' + (e instanceof Error ? e.message : String(e)));
  } finally {
    loading.value = false;
  }
}

function handleFilter() {
  renderGraph();
}

async function handleResync() {
  resyncing.value = true;
  try {
    const result = await resyncNow();
    message.success(`重建完成！规则: ${result.rules}, 规格: ${result.specs}`);
    await loadOntology();
  } catch (e: unknown) {
    message.error('重建失败: ' + (e instanceof Error ? e.message : String(e)));
  } finally {
    resyncing.value = false;
  }
}

function handleResize() {
  if (!graph || !canvasRef.value) return;
  graph.changeSize(canvasRef.value.offsetWidth, canvasRef.value.offsetHeight);
}

onMounted(() => {
  loadOntology();
  window.addEventListener('resize', handleResize);
});

onBeforeUnmount(() => {
  window.removeEventListener('resize', handleResize);
  if (graph) { graph.destroy(); graph = null; }
});
</script>

<style lang="less" scoped>
.knowledge-graph-page {
  height: 100%;
  display: flex;
  flex-direction: column;
  overflow: hidden;
}

.kg-toolbar {
  padding: 10px 16px;
  background: #fff;
  border-bottom: 1px solid #f0f0f0;
  flex-shrink: 0;
}

.kg-main {
  flex: 1;
  display: flex;
  overflow: hidden;
}

.kg-canvas-wrap {
  flex: 1;
  background: linear-gradient(135deg, #f5f7fa 0%, #f0f2f5 100%);
  position: relative;
}

.kg-loading {
  position: absolute;
  top: 50%;
  left: 50%;
  transform: translate(-50%, -50%);
  z-index: 10;
}

.kg-detail {
  width: 340px;
  background: #fff;
  border-left: 1px solid #f0f0f0;
  padding: 16px;
  overflow-y: auto;
  flex-shrink: 0;
}

.detail-head {
  display: flex;
  align-items: center;
  gap: 8px;
}

.detail-title {
  font-size: 15px;
  font-weight: 600;
  color: #333;
  overflow: hidden;
  text-overflow: ellipsis;
  white-space: nowrap;
}

.detail-row {
  font-size: 13px;
  color: #555;
  margin-bottom: 6px;
  line-height: 1.6;
}

.detail-section {
  margin-top: 12px;

  strong {
    display: block;
    margin-bottom: 6px;
    font-size: 13px;
    color: #333;
  }
}

.detail-code {
  background: #f5f5f5;
  border-radius: 6px;
  padding: 10px;
  font-size: 12px;
  font-family: 'SFMono-Regular', Consolas, monospace;
  color: #333;
  white-space: pre-wrap;
  word-break: break-all;
  max-height: 200px;
  overflow-y: auto;
  margin: 0;
}

.attr-list {
  display: flex;
  flex-direction: column;
  gap: 4px;
}

.attr-item {
  display: flex;
  justify-content: space-between;
  align-items: center;
  background: #fafafa;
  border-radius: 4px;
  padding: 4px 8px;
  font-size: 12px;
}

.attr-name {
  color: #666;
}

.attr-val {
  color: #333;
  font-weight: 500;
}

.kg-legend {
  padding: 6px 16px;
  background: #fff;
  border-top: 1px solid #f0f0f0;
  display: flex;
  gap: 16px;
  flex-shrink: 0;
}

.legend-item {
  font-size: 12px;
  color: #888;
  display: flex;
  align-items: center;
  gap: 4px;
}

.legend-dot {
  width: 10px;
  height: 10px;
  border-radius: 50%;
  display: inline-block;
}

.slide-enter-active,
.slide-leave-active {
  transition: transform 0.25s ease, opacity 0.25s ease;
}
.slide-enter-from {
  transform: translateX(100%);
  opacity: 0;
}
.slide-leave-to {
  transform: translateX(100%);
  opacity: 0;
}
</style>
