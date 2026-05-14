<template>
  <div class="kg-canvas" ref="canvasRef">
    <div class="kg-loading-mask" v-if="loading">
      <a-spin tip="加载图谱数据..." />
    </div>
    <div class="kg-empty" v-if="!loading && empty">
      <a-empty description="暂无数据，请先搜索带材或重建知识图谱" />
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

const NODE_STYLES: Record<string, { fill: string; stroke: string; text: string; size: number; symbol: string }> = {
  Ribbon: { fill: '#E6F4FF', stroke: '#1677FF', text: '#0958D9', size: 88, symbol: 'roundRect' },
  ProductSpec: { fill: '#F0F5FF', stroke: '#2F54EB', text: '#1D39C4', size: 74, symbol: 'roundRect' },
  spec: { fill: '#F0F5FF', stroke: '#2F54EB', text: '#1D39C4', size: 74, symbol: 'roundRect' },
  LaminationData: { fill: '#F6FFED', stroke: '#52C41A', text: '#237804', size: 68, symbol: 'roundRect' },
  SingleSheetPerformance: { fill: '#FFF7E6', stroke: '#FA8C16', text: '#AD4E00', size: 68, symbol: 'roundRect' },
  AppearanceFeature: { fill: '#FFF1F0', stroke: '#FF4D4F', text: '#A8071A', size: 64, symbol: 'roundRect' },
  JudgementRule: { fill: '#F9F0FF', stroke: '#722ED1', text: '#391085', size: 58, symbol: 'roundRect' },
  rule: { fill: '#F9F0FF', stroke: '#722ED1', text: '#391085', size: 58, symbol: 'roundRect' },
  Formula: { fill: '#E6FFFB', stroke: '#13C2C2', text: '#006D75', size: 62, symbol: 'roundRect' },
  formula: { fill: '#E6FFFB', stroke: '#13C2C2', text: '#006D75', size: 62, symbol: 'roundRect' },
  MetricJudgement: { fill: '#FFFBE6', stroke: '#FAAD14', text: '#AD6800', size: 68, symbol: 'roundRect' },
};

const EDGE_COLORS: Record<string, string> = {
  BELONGS_TO_SPEC: '#1677FF',
  HAS_LAMINATION_DATA: '#52C41A',
  HAS_SINGLE_SHEET_PERF: '#FA8C16',
  HAS_APPEARANCE: '#FF4D4F',
  USES_RULE: '#722ED1',
  USES_FORMULA: '#13C2C2',
  PRODUCES_RESULT: '#FAAD14',
  FEEDS_METRIC: '#8C8C8C',
};

function styleFor(dataType: string) {
  return NODE_STYLES[dataType] || NODE_STYLES.Ribbon;
}

function truncateLabel(label: string, max = 14) {
  const text = String(label || '');
  return text.length > max ? `${text.slice(0, max)}…` : text;
}

function getNodePosition(node: any, index: number, total: number) {
  if (typeof node.x === 'number' && typeof node.y === 'number') return [node.x, node.y];
  const angle = (Math.PI * 2 * index) / Math.max(total, 1);
  const radius = 260;
  return [460 + Math.cos(angle) * radius, 320 + Math.sin(angle) * radius];
}

function convertToECharts(data: G6GraphData) {
  const nodeMap = new Map<string, any>();
  const echartsNodes: any[] = [];
  const echartsLinks: any[] = [];
  const total = data.nodes.length;

  data.nodes.forEach((n, index) => {
    const dataType = n.dataType || n.type || 'Ribbon';
    const style = styleFor(dataType);
    const [x, y] = getNodePosition(n, index, total);
    const node = {
      id: n.id,
      name: n.id as string,
      value: n.label as string,
      x,
      y,
      fixed: true,
      symbol: style.symbol,
      symbolSize: [Math.max(style.size + truncateLabel(n.label, 18).length * 4, 116), style.size],
      itemStyle: {
        color: style.fill,
        borderColor: n.statusColor || style.stroke,
        borderWidth: n.isDomainNode ? 1.8 : 2,
        shadowBlur: 0,
      },
      label: {
        show: true,
        color: style.text,
        fontSize: dataType === 'Ribbon' ? 15 : 12,
        fontWeight: dataType === 'Ribbon' || dataType === 'ProductSpec' || dataType === 'spec' ? 700 : 600,
        lineHeight: 16,
        formatter: () => truncateLabel(n.label, dataType === 'JudgementRule' || dataType === 'rule' ? 12 : 16),
      },
      _raw: { ...n, dataType },
    };
    nodeMap.set(n.id as string, node);
    echartsNodes.push(node);
  });

  for (const e of data.edges || []) {
    const source = e.source as string;
    const target = e.target as string;
    if (!nodeMap.has(source) || !nodeMap.has(target)) continue;
    const color = EDGE_COLORS[e.relation || e.dataType] || '#BFBFBF';
    echartsLinks.push({
      source,
      target,
      relation: e.label || '',
      lineStyle: {
        color,
        width: e.relation === 'USES_FORMULA' ? 1.8 : 2,
        opacity: 0.76,
        curveness: 0.06,
      },
      label: {
        show: false,
        color: '#595959',
        fontSize: 10,
        formatter: (p: any) => p.data.relation,
      },
      emphasis: {
        label: { show: true },
        lineStyle: { width: 3, opacity: 1 },
      },
      _raw: e,
    });
  }

  return { nodes: echartsNodes, links: echartsLinks };
}

function initChart() {
  if (!canvasRef.value) return;
  chart = echarts.init(canvasRef.value, undefined, { renderer: 'canvas' });

  chart.on('click', (params: any) => {
    if (params.dataType === 'node') {
      const raw = params.data?._raw;
      if (raw?.dataType === 'combo') emit('comboClick', raw);
      else emit('nodeClick', raw);
    } else {
      emit('canvasClick');
    }
  });

  chart.on('zr:click', (params: any) => {
    if (!params.target) emit('canvasClick');
  });

  window.addEventListener('resize', handleResize);
}

function render(data: G6GraphData) {
  if (!chart) return;
  const { nodes, links } = convertToECharts(data);

  const option: echarts.EChartsOption = {
    backgroundColor: 'transparent',
    tooltip: {
      trigger: 'item',
      backgroundColor: 'rgba(255,255,255,0.98)',
      borderColor: '#D9D9D9',
      borderWidth: 1,
      textStyle: { color: '#262626', fontSize: 12 },
      extraCssText: 'box-shadow: 0 6px 16px rgba(0,0,0,0.08); border-radius: 8px; padding: 10px 14px;',
      formatter: (p: any) => {
        const raw = p.data?._raw;
        if (!raw) return '';
        if (p.dataType === 'edge') return raw.label || raw.relation || '';
        const dt = raw.dataType || raw.type;
        const payload = raw.rawData || raw.raw || {};
        const subtitle = raw.subtitle ? `<br/><span style="color:#8C8C8C">${raw.subtitle}</span>` : '';
        if (dt === 'Ribbon') return `<b style="color:#0958D9">${raw.label}</b>${subtitle}<br/>规格: ${raw.spec_name || payload.spec_name || '-'}<br/>等级: ${raw.labeling || payload.labeling || '未判定'}`;
        if (dt === 'ProductSpec' || dt === 'spec') return `<b style="color:#1D39C4">${raw.label}</b>${subtitle}<br/>规则数: ${raw.ruleCount || 0}<br/>公式数: ${raw.formulaCount || 0}`;
        if (dt === 'LaminationData') return `<b style="color:#237804">${raw.label}</b>${subtitle}<br/>宽度: ${payload.width ?? '-'}<br/>卷重: ${payload.coil_weight ?? '-'}`;
        if (dt === 'SingleSheetPerformance') return `<b style="color:#AD4E00">${raw.label}</b>${subtitle}<br/>Ps铁损: ${payload.ps_loss ?? '-'}<br/>Ss功率: ${payload.ss_power ?? '-'}<br/>Hc: ${payload.hc ?? '-'}`;
        if (dt === 'AppearanceFeature') return `<b style="color:#A8071A">${raw.label}</b>${subtitle}<br/>大类: ${payload.category || '-'}<br/>等级: ${payload.level || '-'}`;
        if (dt === 'JudgementRule' || dt === 'rule') return `<b style="color:#391085">${raw.label}</b>${subtitle}<br/>状态: ${payload.quality_status || '-'}<br/>公式: ${payload.formula_name || '-'}`;
        if (dt === 'Formula' || dt === 'formula') return `<b style="color:#006D75">${raw.label}</b>${subtitle}<br/>类型: ${raw.typeLabel || '-'}<br/>关联规则: ${raw.ruleCount || 0}`;
        return `<b>${raw.label || raw.name}</b>${subtitle}`;
      },
    },
    grid: { left: 24, right: 24, top: 24, bottom: 24 },
    animationDuration: 500,
    animationEasingUpdate: 'cubicOut',
    series: [
      {
        type: 'graph',
        layout: 'none',
        data: nodes,
        links,
        roam: true,
        draggable: true,
        scaleLimit: { min: 0.35, max: 2.2 },
        emphasis: {
          focus: 'adjacency',
          blurScope: 'coordinateSystem',
          label: { show: true },
          itemStyle: { borderWidth: 3 },
          lineStyle: { width: 3, opacity: 1 },
        },
        blur: {
          itemStyle: { opacity: 0.18 },
          lineStyle: { opacity: 0.08 },
          label: { opacity: 0.2 },
        },
        lineStyle: { opacity: 0.72, curveness: 0.06 },
        edgeSymbol: ['none', 'arrow'],
        edgeSymbolSize: [0, 8],
        edgeLabel: { show: false, fontSize: 10, color: '#595959', backgroundColor: '#fff' },
      },
    ] as any,
  };

  chart.setOption(option, true);
  window.setTimeout(() => chart?.dispatchAction({ type: 'restore' }), 0);
}

function applyHighlights() {
  if (!chart) return;
  const nodeIds = new Set(props.highlightNodeIds || []);
  chart.dispatchAction({ type: 'downplay', seriesIndex: 0 });
  for (const id of nodeIds) chart.dispatchAction({ type: 'highlight', seriesIndex: 0, name: id });
}

function handleResize() {
  chart?.resize();
}

watch(() => props.data, (val) => {
  if (val && chart) nextTick(() => render(val));
}, { flush: 'post' });

watch(() => props.highlightNodeIds, () => applyHighlights(), { deep: true });
watch(() => props.highlightEdgeIds, () => applyHighlights(), { deep: true });

onMounted(async () => {
  await nextTick();
  initChart();
  if (props.data && chart) render(props.data);
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
  background: linear-gradient(180deg, #ffffff 0%, #fafafa 100%);
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
