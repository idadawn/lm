<template>
  <div class="kg-canvas" ref="canvasRef">
    <div class="kg-loading-mask" v-if="loading">
      <a-spin tip="加载本体数据..." />
    </div>
    <div class="kg-empty" v-if="!loading && empty">
      <a-empty description="暂无数据，点击全量重建初始化" />
    </div>
  </div>
</template>

<script lang="ts" setup>
import { ref, onMounted, onBeforeUnmount, watch, nextTick } from 'vue';
import * as echarts from 'echarts';
import type { G6GraphData } from '../composables/useGraphData';

const props = defineProps<{
  data: G6GraphData;
  loading: boolean;
  empty: boolean;
  highlightNodeIds?: string[];
  highlightEdgeIds?: string[];
}>();

const emit = defineEmits<{
  (e: 'nodeClick', model: any): void;
  (e: 'comboClick', model: any): void;
  (e: 'canvasClick'): void;
}>();

const canvasRef = ref<HTMLElement | null>(null);
let chart: echarts.ECharts | null = null;

// ------------------------------------------------------------------
// 配色
// ------------------------------------------------------------------
// 带材中心本体配色
const NODE_COLORS: Record<string, { fill: string; stroke: string; text: string; shadow: string }> = {
  Ribbon: { fill: '#3B82F6', stroke: '#2563EB', text: '#fff', shadow: 'rgba(59,130,246,0.5)' },
  ProductSpec: { fill: '#1D4ED8', stroke: '#1E40AF', text: '#fff', shadow: 'rgba(29,78,216,0.5)' },
  LaminationData: { fill: '#22C55E', stroke: '#16A34A', text: '#fff', shadow: 'rgba(34,197,94,0.5)' },
  SingleSheetPerformance: { fill: '#F59E0B', stroke: '#D97706', text: '#fff', shadow: 'rgba(245,158,11,0.5)' },
  AppearanceFeature: { fill: '#EF4444', stroke: '#DC2626', text: '#fff', shadow: 'rgba(239,68,68,0.5)' },
  JudgementRule: { fill: '#A855F7', stroke: '#9333EA', text: '#fff', shadow: 'rgba(168,85,247,0.5)' },
  // 兼容旧类型
  spec: { fill: '#3B82F6', stroke: '#2563EB', text: '#fff', shadow: 'rgba(59,130,246,0.5)' },
  rule: { fill: '#22C55E', stroke: '#16A34A', text: '#fff', shadow: 'rgba(34,197,94,0.5)' },
  formula: { fill: '#A855F7', stroke: '#9333EA', text: '#fff', shadow: 'rgba(168,85,247,0.5)' },
};

function getNodeColor(dataType: string, rawData?: any) {
  const c = NODE_COLORS[dataType];
  if (c) return c;
  // fallback for old types
  if (dataType === 'spec') return NODE_COLORS.spec;
  if (dataType === 'rule') {
    const isQualified = rawData?.quality_status === '合格';
    return isQualified ? NODE_COLORS.LaminationData : NODE_COLORS.AppearanceFeature;
  }
  if (dataType === 'formula') return NODE_COLORS.formula;
  return NODE_COLORS.Ribbon;
}

function getNodeSize(dataType: string) {
  if (dataType === 'Ribbon') return 88;
  if (dataType === 'ProductSpec') return 72;
  if (dataType === 'LaminationData') return 56;
  if (dataType === 'SingleSheetPerformance') return 56;
  if (dataType === 'AppearanceFeature') return 48;
  if (dataType === 'JudgementRule') return 56;
  // fallback
  if (dataType === 'spec') return 72;
  if (dataType === 'rule') return 44;
  if (dataType === 'formula') return 56;
  return 50;
}

// ------------------------------------------------------------------
// 数据转换：G6GraphData → ECharts graph
// ------------------------------------------------------------------
function convertToECharts(data: G6GraphData) {
  const nodeMap = new Map<string, any>();
  const echartsNodes: any[] = [];
  const echartsLinks: any[] = [];

  // 节点（combo 内的节点也扁平化）
  for (const n of data.nodes) {
    const dataType = n.type || n.dataType || 'Ribbon';
    const color = getNodeColor(dataType, n.raw);
    const size = getNodeSize(dataType);
    const node = {
      name: n.id as string,
      value: (n.metrics && Object.keys(n.metrics).length) || 0,
      symbolSize: size,
      symbol: 'circle',
      itemStyle: {
        color: color.fill,
        borderColor: color.stroke,
        borderWidth: 3,
        shadowBlur: 12,
        shadowColor: color.shadow,
      },
      label: {
        show: true,
        color: color.text,
        fontSize: dataType === 'Ribbon' ? 14 : dataType === 'ProductSpec' ? 13 : 10,
        fontWeight: dataType === 'Ribbon' || dataType === 'ProductSpec' ? 700 : 600,
        formatter: () => n.label as string,
      },
      // 自定义数据透传
      _raw: { ...n, dataType },
    };
    nodeMap.set(n.id as string, node);
    echartsNodes.push(node);
  }

  // combo 也作为大节点展示（用圆角矩形区分）
  for (const c of data.combos || []) {
    const comboNode = {
      name: c.id as string,
      value: (c.rules || []).length,
      symbolSize: 60 + ((c.rules || []).length * 2),
      symbol: 'roundRect',
      itemStyle: {
        color: 'rgba(255,255,255,0.85)',
        borderColor: '#94A3B8',
        borderWidth: 2,
        borderType: 'dashed',
        shadowBlur: 6,
        shadowColor: 'rgba(148,163,184,0.3)',
      },
      label: {
        show: true,
        color: '#475569',
        fontSize: 11,
        fontWeight: 600,
        formatter: () => c.label as string,
      },
      _raw: { ...c, dataType: 'combo' },
    };
    nodeMap.set(c.id as string, comboNode);
    echartsNodes.push(comboNode);
  }

  // 边
  for (const e of data.edges) {
    const source = e.source as string;
    const target = e.target as string;
    if (!nodeMap.has(source) || !nodeMap.has(target)) continue;

    const isSpecCombo = (e.dataType as string) === 'spec-combo';
    echartsLinks.push({
      source,
      target,
      relation: (e.label as string) || '',
      lineStyle: {
        color: isSpecCombo ? '#94A3B8' : '#A78BFA',
        width: isSpecCombo ? 1.5 : 2,
        curveness: 0.15,
      },
      label: {
        show: !!e.label,
        color: isSpecCombo ? '#64748B' : '#7C3AED',
        fontSize: 10,
        formatter: (p: any) => p.data.relation,
      },
      _raw: e,
    });
  }

  return { nodes: echartsNodes, links: echartsLinks };
}

// ------------------------------------------------------------------
// 初始化
// ------------------------------------------------------------------

function initChart() {
  if (!canvasRef.value) return;
  chart = echarts.init(canvasRef.value, undefined, { renderer: 'canvas' });

  chart.on('click', (params: any) => {
    if (params.dataType === 'node') {
      const raw = params.data?._raw;
      if (raw?.dataType === 'combo') {
        emit('comboClick', raw);
      } else {
        emit('nodeClick', raw);
      }
    } else {
      emit('canvasClick');
    }
  });

  chart.on('zr:click', (params: any) => {
    if (!params.target) {
      emit('canvasClick');
    }
  });

  window.addEventListener('resize', handleResize);
}

function render(data: G6GraphData) {
  if (!chart) return;
  console.log('[KgCanvas] render called, nodes:', data.nodes.length, 'edges:', data.edges.length, 'combos:', (data.combos || []).length);
  const { nodes, links } = convertToECharts(data);

  const option: echarts.EChartsOption = {
    backgroundColor: 'transparent',
    tooltip: {
      trigger: 'item',
      backgroundColor: 'rgba(255,255,255,0.95)',
      borderColor: '#E2E8F0',
      borderWidth: 1,
      textStyle: { color: '#334155', fontSize: 12 },
      extraCssText: 'box-shadow: 0 4px 12px rgba(0,0,0,0.08); border-radius: 8px; padding: 10px 14px;',
      formatter: (p: any) => {
        const raw = p.data?._raw;
        if (!raw) return '';
        const dt = raw.dataType || raw.type;
        if (dt === 'Ribbon') {
          const r = raw.raw || {};
          return `<b style="color:#2563EB">🏭 ${raw.label}</b><br/>规格: ${raw.spec_name || '-'}<br/>日期: ${raw.detection_date || '-'}<br/>等级: ${raw.labeling || '未判定'}`;
        }
        if (dt === 'ProductSpec') {
          const r = raw.raw || {};
          return `<b style="color:#1D4ED8">📦 ${raw.label}</b><br/>代码: ${r.code || '-'}<br/>检测列: ${r.detection_columns || '-'}`;
        }
        if (dt === 'LaminationData') {
          const r = raw.raw || {};
          return `<b style="color:#16A34A">📊 叠片数据</b><br/>宽度: ${r.width || '-'}<br/>卷重: ${r.coil_weight || '-'}<br/>断头: ${r.break_count || '-'}`;
        }
        if (dt === 'SingleSheetPerformance') {
          const r = raw.raw || {};
          return `<b style="color:#D97706">⚡ 单片性能</b><br/>Ps铁损: ${r.ps_loss || '-'}<br/>Ss功率: ${r.ss_power || '-'}<br/>Hc: ${r.hc || '-'}`;
        }
        if (dt === 'AppearanceFeature') {
          const r = raw.raw || {};
          return `<b style="color:#DC2626">🔴 ${raw.label}</b><br/>大类: ${r.category || '-'}<br/>等级: ${r.level || '-'}`;
        }
        if (dt === 'JudgementRule') {
          const r = raw.raw || {};
          const c = r.quality_status === '合格' ? '#16A34A' : '#DC2626';
          return `<b style="color:${c}">⚖️ ${raw.label}</b><br/>状态: ${r.quality_status || '-'}<br/>公式: ${r.formula_name || '-'}`;
        }
        if (dt === 'combo') {
          return `<b style="color:#64748B">📁 ${raw.label}</b><br/>规则数: ${(raw.rules || []).length}`;
        }
        if (dt === 'spec') {
          return `<b style="color:#2563EB">📦 ${raw.rawData?.name || raw.rawData?.code}</b><br/>规则: ${raw.ruleCount || 0} | 公式: ${raw.formulaCount || 0}`;
        }
        if (dt === 'rule') {
          const c = raw.rawData?.quality_status === '合格' ? '#16A34A' : '#DC2626';
          return `<b style="color:${c}">⚖️ ${raw.rawData?.name}</b><br/>状态: ${raw.rawData?.quality_status} | 优先级: ${raw.rawData?.priority}`;
        }
        if (dt === 'formula') {
          return `<b style="color:#9333EA">🔧 ${raw.rawData?.formula_name || raw.rawData?.column_name}</b><br/>类型: ${raw.typeLabel || '-'} | 关联规则: ${raw.ruleCount || 0}`;
        }
        return '';
      },
    },
    animationDuration: 800,
    animationEasingUpdate: 'quinticInOut',
    series: [
      {
        type: 'graph',
        layout: 'force',
        data: nodes,
        links: links,
        roam: true,
        draggable: true,
        force: {
          repulsion: 500,
          gravity: 0.05,
          edgeLength: [100, 200],
          layoutAnimation: true,
        },
        emphasis: {
          focus: 'adjacency',
          lineStyle: { width: 4, opacity: 1 },
          itemStyle: {
            shadowBlur: 24,
            shadowColor: 'rgba(245,158,11,0.6)',
          },
        },
        lineStyle: {
          opacity: 0.7,
          curveness: 0.15,
        },
        edgeSymbol: ['none', 'arrow'],
        edgeSymbolSize: [0, 10],
        edgeLabel: {
          show: true,
          fontSize: 10,
          color: '#64748B',
        },
      },
    ],
  };

  chart.setOption(option, true);
}

function applyHighlights() {
  if (!chart) return;
  const nodeIds = new Set(props.highlightNodeIds || []);
  const edgeIds = new Set(props.highlightEdgeIds || []);

  // 通过 downplay / highlight 实现
  chart.dispatchAction({ type: 'downplay', seriesIndex: 0 });

  for (const id of nodeIds) {
    chart.dispatchAction({ type: 'highlight', seriesIndex: 0, name: id });
  }
}

function handleResize() {
  chart?.resize();
}

// ------------------------------------------------------------------
// 生命周期
// ------------------------------------------------------------------

watch(() => props.data, (val) => {
  console.log('[KgCanvas] data changed, nodes:', val?.nodes?.length);
  if (val && chart) {
    nextTick(() => render(val));
  }
}, { flush: 'post' });

watch(() => props.highlightNodeIds, () => {
  applyHighlights();
}, { deep: true });

watch(() => props.highlightEdgeIds, () => {
  applyHighlights();
}, { deep: true });

onMounted(async () => {
  await nextTick();
  initChart();
  console.log('[KgCanvas] mounted, data nodes:', props.data?.nodes?.length);
  if (props.data && chart) {
    render(props.data);
  }
});

onBeforeUnmount(() => {
  window.removeEventListener('resize', handleResize);
  if (chart) {
    chart.dispose();
    chart = null;
  }
});
</script>

<style lang="less" scoped>
.kg-canvas {
  flex: 1;
  background: #FAFBFC;
  position: relative;
  min-height: 0;
}

.kg-loading-mask,
.kg-empty {
  position: absolute;
  inset: 0;
  display: flex;
  align-items: center;
  justify-content: center;
  z-index: 10;
  pointer-events: none;
}
</style>
