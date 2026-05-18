<template>
  <div v-if="chartConfig" class="chat-chart-bubble" data-testid="chat-chart-bubble">
    <div class="chart-title">{{ chartConfig.title || '数据可视化' }}</div>
    <div ref="chartEl" class="chart-canvas"></div>
  </div>
</template>

<script setup lang="ts">
import { ref, onMounted, onBeforeUnmount, watch, nextTick } from 'vue';
import * as echarts from 'echarts/core';
import { PieChart, BarChart, LineChart } from 'echarts/charts';
import {
  TitleComponent,
  TooltipComponent,
  LegendComponent,
  GridComponent,
} from 'echarts/components';
import { CanvasRenderer } from 'echarts/renderers';

echarts.use([
  PieChart, BarChart, LineChart,
  TitleComponent, TooltipComponent, LegendComponent, GridComponent,
  CanvasRenderer,
]);

interface Props {
  chartConfig: Record<string, any>;
}

const props = defineProps<Props>();
const chartEl = ref<HTMLElement | null>(null);
let instance: echarts.ECharts | null = null;
let resizeObs: ResizeObserver | null = null;

const PALETTE = [
  '#16a34a', '#3b82f6', '#14b8a6', '#a855f7', '#f59e0b',
  '#ef4444', '#8b5cf6', '#06b6d4', '#84cc16', '#ec4899',
  '#f97316', '#10b981', '#6366f1', '#f43f5e', '#0ea5e9',
  '#22c55e', '#eab308', '#7c3aed', '#d946ef', '#0891b2',
];

function buildDonutOption(cfg: Record<string, any>) {
  const data = (cfg.data || []) as Array<{ name: string; value: number; percent?: number; count?: number }>;
  // 把饼图放在画布左侧 35% 处（留 30% 给右侧 legend），中间文字与饼图圆心严格对齐。
  const CENTER_X = '35%';
  const CENTER_Y = '50%';
  return {
    color: PALETTE,
    tooltip: {
      trigger: 'item',
      formatter: (p: any) => {
        const d = p.data || {};
        const pct = d.percent != null ? `${d.percent}%` : `${p.percent}%`;
        const cnt = d.count != null ? `<br/>记录数：${d.count}` : '';
        return `${p.marker}${p.name}<br/>重量：${(p.value).toLocaleString()} kg<br/>占比：${pct}${cnt}`;
      },
    },
    legend: {
      type: 'scroll',
      orient: 'vertical',
      right: 10,
      top: 'middle',
      itemWidth: 10,
      itemHeight: 10,
      textStyle: { fontSize: 12, color: '#475569' },
      pageIconColor: '#94a3b8',
      pageTextStyle: { fontSize: 11 },
    },
    series: [
      {
        type: 'pie',
        radius: ['54%', '78%'],
        center: [CENTER_X, CENTER_Y],
        avoidLabelOverlap: true,
        label: { show: false },
        labelLine: { show: false },
        emphasis: { itemStyle: { shadowBlur: 8, shadowColor: 'rgba(0,0,0,0.12)' } },
        data,
      },
    ],
    // 中心文字：用 rich text 一行包两段（数值 + 标签），靠 textAlign+textVerticalAlign 整体居中到环心
    graphic: cfg.center_label
      ? [
          {
            type: 'text',
            left: CENTER_X,
            top: CENTER_Y,
            z: 10,
            cursor: 'default',
            style: {
              text: `{val|${cfg.center_value ?? ''}}\n{lbl|${cfg.center_label || ''}}`,
              textAlign: 'center',
              textVerticalAlign: 'middle',
              fontSize: 14,  // 仅作为 fallback；具体大小由 rich 段控制
              rich: {
                val: {
                  fontSize: 28,
                  fontWeight: 700,
                  color: '#0f172a',
                  lineHeight: 32,
                  align: 'center',
                },
                lbl: {
                  fontSize: 12,
                  color: '#64748b',
                  lineHeight: 18,
                  padding: [4, 0, 0, 0],
                  align: 'center',
                },
              },
            },
          },
        ]
      : undefined,
  };
}

function buildBarOption(cfg: Record<string, any>) {
  const data = (cfg.data || []) as Array<{ category?: string; name?: string; value: number }>;
  const xs = data.map((d) => d.category ?? d.name ?? '');
  const ys = data.map((d) => d.value);
  return {
    color: [PALETTE[0]],
    tooltip: { trigger: 'axis' },
    grid: { left: 40, right: 16, top: 24, bottom: 36, containLabel: true },
    xAxis: { type: 'category', data: xs, axisLabel: { fontSize: 11, rotate: xs.length > 6 ? 30 : 0 } },
    yAxis: { type: 'value', axisLabel: { fontSize: 11 } },
    series: [{ type: 'bar', data: ys, barMaxWidth: 36 }],
  };
}

function buildLineOption(cfg: Record<string, any>) {
  const data = (cfg.data || []) as Array<{ date?: string; category?: string; value: number }>;
  const xs = data.map((d) => d.date ?? d.category ?? '');
  const ys = data.map((d) => d.value);
  return {
    color: [PALETTE[1]],
    tooltip: { trigger: 'axis' },
    grid: { left: 40, right: 16, top: 24, bottom: 36, containLabel: true },
    xAxis: { type: 'category', data: xs, axisLabel: { fontSize: 11 } },
    yAxis: { type: 'value', axisLabel: { fontSize: 11 } },
    series: [{ type: 'line', data: ys, smooth: true, areaStyle: { opacity: 0.15 } }],
  };
}

// 2 维：grouped/stacked bar — 行=groups，列=series
function buildGroupedBarOption(cfg: Record<string, any>) {
  const groups = (cfg.groups || []) as string[];
  const series = (cfg.series || []) as Array<{ name: string; data: number[] }>;
  const isStacked = String(cfg.type || '').toLowerCase() === 'stacked_bar';
  // 类目过多自动旋转，避免 x 轴堆叠
  const rotate = groups.length > 6 ? 30 : 0;
  return {
    color: PALETTE,
    tooltip: {
      trigger: 'axis',
      axisPointer: { type: 'shadow' },
      formatter: (params: any[]) => {
        if (!Array.isArray(params) || params.length === 0) return '';
        const lines = [`<b>${params[0].axisValueLabel}</b>`];
        let total = 0;
        params.forEach((p) => {
          const v = Number(p.value || 0);
          total += v;
          lines.push(`${p.marker}${p.seriesName}: ${v.toLocaleString()}`);
        });
        lines.push(`<span style="color:#64748b">合计: ${total.toLocaleString()}</span>`);
        return lines.join('<br/>');
      },
    },
    legend: {
      type: 'scroll',
      bottom: 0,
      itemWidth: 12,
      itemHeight: 12,
      textStyle: { fontSize: 11, color: '#475569' },
    },
    grid: { left: 40, right: 16, top: 24, bottom: 48, containLabel: true },
    xAxis: {
      type: 'category',
      data: groups,
      axisLabel: { fontSize: 11, rotate, interval: 0 },
    },
    yAxis: {
      type: 'value',
      axisLabel: { fontSize: 11, formatter: (v: number) => v >= 10000 ? (v / 1000).toFixed(0) + 'k' : String(v) },
    },
    series: series.map((s) => ({
      name: s.name,
      type: 'bar',
      stack: isStacked ? 'total' : undefined,
      data: s.data,
      barMaxWidth: 36,
      emphasis: { focus: 'series' },
    })),
  };
}

// 多折线：x = xCategories（通常是时间），每条线是一个 series
function buildMultiLineOption(cfg: Record<string, any>) {
  const xs = (cfg.xCategories || []) as string[];
  const series = (cfg.series || []) as Array<{ name: string; data: number[] }>;
  return {
    color: PALETTE,
    tooltip: { trigger: 'axis' },
    legend: {
      type: 'scroll',
      bottom: 0,
      itemWidth: 12,
      itemHeight: 12,
      textStyle: { fontSize: 11, color: '#475569' },
    },
    grid: { left: 40, right: 16, top: 24, bottom: 48, containLabel: true },
    xAxis: {
      type: 'category',
      data: xs,
      axisLabel: { fontSize: 11, rotate: xs.length > 10 ? 30 : 0 },
    },
    yAxis: { type: 'value', axisLabel: { fontSize: 11 } },
    series: series.map((s) => ({
      name: s.name,
      type: 'line',
      data: s.data,
      smooth: true,
      symbol: 'circle',
      symbolSize: 5,
      emphasis: { focus: 'series' },
    })),
  };
}

function buildOption(cfg: Record<string, any>) {
  const t = String(cfg.type || '').toLowerCase();
  if (t === 'donut' || t === 'pie') return buildDonutOption(cfg);
  if (t === 'bar') return buildBarOption(cfg);
  if (t === 'line') return buildLineOption(cfg);
  if (t === 'grouped_bar' || t === 'stacked_bar') return buildGroupedBarOption(cfg);
  if (t === 'multi_line') return buildMultiLineOption(cfg);
  return buildBarOption(cfg);
}

async function render() {
  await nextTick();
  if (!chartEl.value) return;
  if (!instance) {
    instance = echarts.init(chartEl.value);
    resizeObs = new ResizeObserver(() => instance?.resize());
    resizeObs.observe(chartEl.value);
  }
  instance.setOption(buildOption(props.chartConfig), true);
}

onMounted(render);
watch(() => props.chartConfig, render, { deep: true });
onBeforeUnmount(() => {
  resizeObs?.disconnect();
  instance?.dispose();
  instance = null;
});
</script>

<style lang="less" scoped>
.chat-chart-bubble {
  margin-top: 10px;
  padding: 12px 12px 8px;
  background: #fafbfc;
  border: 1px solid #e2e8f0;
  border-radius: 8px;
}

.chart-title {
  font-size: 12px;
  font-weight: 600;
  color: #475569;
  margin-bottom: 6px;
  padding-left: 4px;
}

.chart-canvas {
  width: 100%;
  height: 280px;
  min-height: 240px;
}
</style>
