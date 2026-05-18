<template>
  <div class="rg-canvas" ref="containerRef">
    <div class="rg-loading-mask" v-if="loading">
      <a-spin tip="加载图谱数据..." />
    </div>
    <div class="rg-empty" v-if="!loading && empty">
      <a-empty description="暂无数据，请先搜索带材或重建知识图谱" />
    </div>
    <RelationGraph
      v-show="!loading && !empty"
      ref="graphRef"
      :options="graphOptions"
      @node-click="onNodeClick"
      @line-click="onLineClick"
      @canvas-click="onCanvasClick"
    />
  </div>
</template>

<script lang="ts" setup>
import { ref, onMounted, watch, nextTick, onBeforeUnmount } from 'vue';
import RelationGraph from 'relation-graph-vue3';
import type { RGJsonData, RGOptions, RGNode, RGLine, RGLink, RelationGraphInstance, RGUserEvent } from 'relation-graph-vue3';

const props = defineProps<{
  data: RGJsonData;
  loading: boolean;
  empty: boolean;
  highlightNodeIds?: string[];
  highlightEdgeIds?: string[];
}>();

const emit = defineEmits<{
  (e: 'nodeClick', model: any): void;
  (e: 'canvasClick'): void;
}>();

const containerRef = ref<HTMLElement | null>(null);
const graphRef = ref<any>(null);
let graphInstance: RelationGraphInstance | null = null;

const graphOptions: RGOptions = {
  defaultNodeWidth: 70,
  defaultNodeHeight: 70,
  defaultNodeShape: 0, // 0=圆形, 1=矩形
  // 曲线边（6=曲线）大幅减少多枢纽图里的边交叉，看起来更整洁
  defaultLineShape: 6,
  defaultJunctionPoint: 'border',
  disableDragNode: false,
  // ★ 不再每次刷新都自动布局/居中/缩放——用户拖动后保持位置，避免"卡顿/抖动"
  // 想重新布局走右上角"重新布局"按钮（手动触发）
  moveToCenterWhenRefresh: false,
  zoomToFitWhenRefresh: false,
  useAnimationWhenRefresh: false,
  layout: {
    layoutName: 'force',
    distance_coefficient: 2.2,
    centerOffset_x: 0,
    centerOffset_y: 0,
  } as any,
  // 边/节点更细更淡，降低视觉噪声
  defaultNodeBorderWidth: 2,
  defaultLineWidth: 1.4,
  defaultLineColor: 'rgba(99, 102, 241, 0.35)',
  defaultNodeFontColor: '#475569',
  graphOffset_x: 0,
  graphOffset_y: 0,
  allowSwitchLineShape: false,
  allowSwitchJunctionPoint: false,
  // 保留库自带 mini 工具栏（缩放/居中/重新布局都在这里，业务手动触发）
  // 关掉 allowAutoLayoutIfSupport，避免节点 invisible/opacity 变化时连带触发整个布局重算
  allowShowMiniToolBar: true,
  allowShowMiniNameFilter: false,
  allowAutoLayoutIfSupport: false,
};

function getInstance(): RelationGraphInstance | null {
  if (!graphRef.value) return null;
  // relation-graph-vue3 v2.x 通过 getInstance() 暴露实例
  return graphRef.value.getInstance ? graphRef.value.getInstance() : null;
}

// 让力导向跑约 1.5 秒收敛后冻结——避免节点持续轻微晃动
function freezeLayoutAfterStable() {
  if (!graphInstance) return;
  setTimeout(() => {
    try {
      (graphInstance as any).stopAutoLayout?.();
    } catch (_) { /* 忽略：某些版本可能没暴露 */ }
  }, 1500);
}

async function render(data: RGJsonData) {
  graphInstance = getInstance();
  if (!graphInstance) return;
  await graphInstance.setJsonData(data, true);
  graphInstance.moveToCenter();
  graphInstance.zoomToFit();
  applyHighlights();
  freezeLayoutAfterStable();
}

// 手动重新布局：再走一次 setJsonData（强制重算）+ 归位 + 缩放
async function relayout() {
  graphInstance = getInstance();
  if (!graphInstance) return;
  if (props.data && props.data.nodes) {
    await graphInstance.setJsonData(props.data, true);
    graphInstance.moveToCenter();
    graphInstance.zoomToFit();
    applyHighlights();
    freezeLayoutAfterStable();
  }
}

// 仅归位 + 缩放，不重算布局（更轻量）
function recenter() {
  graphInstance = getInstance();
  if (!graphInstance) return;
  graphInstance.moveToCenter();
  graphInstance.zoomToFit();
}

defineExpose({ relayout, recenter });

function onNodeClick(node: RGNode, e: RGUserEvent) {
  // 构造与旧 KgCanvas 兼容的 model
  const model = {
    id: node.id,
    label: node.text,
    name: node.text,
    dataType: node.data?.dataType || node.data?.type,
    type: node.data?.dataType || node.data?.type,
    rawData: node.data?.rawData || node.data?.raw || {},
    subtitle: node.data?.subtitle,
    ruleCount: node.data?.ruleCount,
    formulaCount: node.data?.formulaCount,
    statusColor: node.data?.statusColor,
    isDomainNode: node.data?.isDomainNode,
  };
  emit('nodeClick', model);
}

function onLineClick(line: RGLine, link: RGLink, e: RGUserEvent) {
  // 暂不暴露连线点击事件，可在此扩展
}

function onCanvasClick(e: RGUserEvent) {
  emit('canvasClick');
}

function applyHighlights() {
  if (!graphInstance) return;
  const nodeIds = new Set(props.highlightNodeIds || []);
  const edgeIds = new Set(props.highlightEdgeIds || []);

  const allNodes = graphInstance.getNodes ? graphInstance.getNodes() : [];
  const allLinks = graphInstance.getLinks ? graphInstance.getLinks() : [];

  const hasHighlight = nodeIds.size > 0 || edgeIds.size > 0;

  // 高亮模式：命中节点 opacity=1，其余 0.18 淡化（不彻底隐藏，保留全局轮廓感）
  allNodes.forEach((n: any) => {
    n.opacity = hasHighlight ? (nodeIds.has(n.id) ? 1 : 0.18) : 1;
  });
  allLinks.forEach((link: any) => {
    const line = link.relations?.[0] || link;
    if (!hasHighlight) {
      line.opacity = 1;
      return;
    }
    const fromHit = nodeIds.has(link.fromNode?.id || line.from);
    const toHit = nodeIds.has(link.toNode?.id || line.to);
    line.opacity = edgeIds.has(line.id) || (fromHit && toHit) ? 1 : 0.12;
  });

  graphInstance.refresh();
}

function handleResize() {
  if (!graphInstance) return;
  if (typeof (graphInstance as any).onGraphResize === 'function') {
    (graphInstance as any).onGraphResize();
  } else if (typeof graphInstance.refresh === 'function') {
    graphInstance.refresh();
  }
}

watch(() => props.data, (val) => {
  if (val && val.nodes) {
    nextTick(() => render(val));
  }
}, { flush: 'post', deep: true });

watch(() => props.highlightNodeIds, () => applyHighlights(), { deep: true });
watch(() => props.highlightEdgeIds, () => applyHighlights(), { deep: true });

onMounted(async () => {
  await nextTick();
  if (props.data && props.data.nodes) {
    await render(props.data);
  }
  window.addEventListener('resize', handleResize);
});

onBeforeUnmount(() => {
  window.removeEventListener('resize', handleResize);
});
</script>

<style lang="less" scoped>
.rg-canvas {
  flex: 1;
  background: linear-gradient(180deg, #ffffff 0%, #fafafa 100%);
  position: relative;
  min-height: 0;
  overflow: hidden;
}

/* 圆形节点覆盖 */
:deep(.relation-graph-node) {
  border-radius: 50% !important;
}
:deep(.relation-graph-node .node-shape) {
  border-radius: 50% !important;
}

.rg-loading-mask,
.rg-empty {
  position: absolute;
  inset: 0;
  display: flex;
  align-items: center;
  justify-content: center;
  z-index: 10;
  pointer-events: none;
}
</style>
