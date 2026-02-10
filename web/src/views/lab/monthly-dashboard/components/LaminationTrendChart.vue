<template>
  <div class="chart-card">
    <div class="chart-header">
      <h3 class="chart-title">叠片系数趋势</h3>
      <span class="chart-desc">按产品规格统计：检验日期 × 每日叠片系数均值，浅色带为波动范围</span>
    </div>
    <div class="chart-body">
      <div ref="chartRef" class="chart-container"></div>
      <div v-if="hasNoData" class="chart-empty">暂无叠片系数数据</div>
    </div>
  </div>
</template>

<script lang="ts" setup>
import { ref, computed, watch, onMounted, type Ref } from 'vue';
import { useECharts } from '/@/hooks/web/useECharts';
import type { LaminationTrendData } from '/@/api/lab/dashboard';
import dayjs from 'dayjs';
import * as echarts from 'echarts';

interface Props {
  data?: LaminationTrendData[] | null;
  loading?: boolean;
}

const props = withDefaults(defineProps<Props>(), {
  loading: false,
});

const chartRef = ref<HTMLDivElement | null>(null);
let setChartOptions: ((option: any) => void) | null = null;

const hasNoData = computed(() => !props.data || props.data.length === 0);

// 按产品规格分组：key 为展示名（规格名或编码或「未指定规格」）
function groupByProductSpec(items: LaminationTrendData[]) {
  const map = new Map<string, LaminationTrendData[]>();
  for (const item of items) {
    const key = item.productSpecName?.trim() || item.productSpecCode?.trim() || '未指定规格';
    if (!map.has(key)) map.set(key, []);
    map.get(key)!.push(item);
  }
  return map;
}

// 多规格色板（与单规格主色 #4facfe 协调）
const SPEC_COLORS = [
  '#4facfe',
  '#43e97b',
  '#fa709a',
  '#fee140',
  '#a18cd1',
  '#ff9a9e',
  '#667eea',
];

function hexToRgba(hex: string, alpha: number): string {
  const r = parseInt(hex.slice(1, 3), 16);
  const g = parseInt(hex.slice(3, 5), 16);
  const b = parseInt(hex.slice(5, 7), 16);
  return `rgba(${r},${g},${b},${alpha})`;
}

function initChart() {
  if (!chartRef.value) return;
  const { setOptions } = useECharts(chartRef as Ref<HTMLDivElement>);
  setChartOptions = setOptions;
  updateChart();
}

function updateChart() {
  if (!setChartOptions) return;
  if (!props.data || props.data.length === 0) {
    setChartOptions({});
    return;
  }

  const groups = groupByProductSpec(props.data);
  const specKeys = Array.from(groups.keys());

  // 所有日期并排序（按 date 字符串即可 yyyy-MM-dd）
  const dateSet = new Set<string>();
  for (const list of groups.values()) {
    for (const item of list) dateSet.add(item.date);
  }
  const sortedDates = Array.from(dateSet).sort();
  const dates = sortedDates.map((d) => {
    const parsed = dayjs(d);
    return parsed.isValid() ? parsed.format('MM-DD') : d;
  });

  const isSingleSpec = specKeys.length <= 1;
  const tooltipFormatter = buildTooltipFormatter(isSingleSpec);

  if (isSingleSpec) {
    // 单规格：保持原样，一条均值线 + 置信带
    const list = groups.get(specKeys[0])!;
    const byDate = new Map<string, LaminationTrendData>();
    for (const item of list) byDate.set(item.date, item);
    const avgValues = sortedDates.map((d) => {
      const item = byDate.get(d);
      return item ? Number(item.value) || 0 : null;
    });
    const minValues = sortedDates.map((d) => {
      const item = byDate.get(d);
      return item ? Number(item.min) : null;
    });
    const maxValues = sortedDates.map((d) => {
      const item = byDate.get(d);
      return item ? Number(item.max) : null;
    });

    const series: any[] = [
      {
        name: '下限',
        type: 'line',
        data: minValues,
        lineStyle: { opacity: 0 },
        stack: 'confidence-band',
        symbol: 'none',
        silent: true,
      },
      {
        name: '区间带',
        type: 'line',
        data: maxValues.map((max, i) => {
          const min = minValues[i];
          if (min == null || max == null) return null;
          return Number((max - min).toFixed(4));
        }),
        lineStyle: { opacity: 0 },
        stack: 'confidence-band',
        symbol: 'none',
        areaStyle: {
          color: new echarts.graphic.LinearGradient(0, 0, 0, 1, [
            { offset: 0, color: 'rgba(79, 172, 254, 0.25)' },
            { offset: 1, color: 'rgba(79, 172, 254, 0.05)' },
          ]),
        },
        silent: true,
        legendHoverLink: false,
      },
      {
        name: '平均值',
        type: 'line',
        data: avgValues,
        smooth: true,
        showSymbol: false,
        symbol: 'circle',
        symbolSize: 6,
        lineStyle: {
          color: '#4facfe',
          width: 3,
          shadowColor: 'rgba(79, 172, 254, 0.4)',
          shadowBlur: 10,
        },
        itemStyle: { color: '#4facfe', borderColor: '#fff', borderWidth: 2 },
      },
    ];
    setChartOptions(buildOption(dates, series, ['平均值', '区间带'], tooltipFormatter));
    return;
  }

  // 多规格：每个规格一条线 + 一条浅色带，图例为规格名
  const series: any[] = [];
  const legendData: string[] = [];

  specKeys.forEach((specName, idx) => {
    const list = groups.get(specName)!;
    const byDate = new Map<string, LaminationTrendData>();
    for (const item of list) byDate.set(item.date, item);
    const color = SPEC_COLORS[idx % SPEC_COLORS.length];
    const stackId = `band-${idx}`;

    const minValues = sortedDates.map((d) => {
      const item = byDate.get(d);
      return item ? Number(item.min) : null;
    });
    const maxValues = sortedDates.map((d) => {
      const item = byDate.get(d);
      return item ? Number(item.max) : null;
    });
    const avgValues = sortedDates.map((d) => {
      const item = byDate.get(d);
      return item ? Number(item.value) || null : null;
    });

    series.push(
      {
        name: `${specName}-下限`,
        type: 'line',
        data: minValues,
        lineStyle: { opacity: 0 },
        stack: stackId,
        symbol: 'none',
        silent: true,
      },
      {
        name: `${specName}-区间`,
        type: 'line',
        data: maxValues.map((max, i) => {
          const min = minValues[i];
          if (min == null || max == null) return null;
          return Number((max - min).toFixed(4));
        }),
        lineStyle: { opacity: 0 },
        stack: stackId,
        symbol: 'none',
        areaStyle: {
          color: new echarts.graphic.LinearGradient(0, 0, 0, 1, [
            { offset: 0, color: hexToRgba(color, 0.25) },
            { offset: 1, color: hexToRgba(color, 0.05) },
          ]),
        },
        silent: true,
        legendHoverLink: false,
      },
      {
        name: specName,
        type: 'line',
        data: avgValues,
        smooth: true,
        showSymbol: false,
        symbol: 'circle',
        symbolSize: 6,
        lineStyle: { color, width: 2, shadowColor: `${color}40`, shadowBlur: 6 },
        itemStyle: { color, borderColor: '#fff', borderWidth: 2 },
      },
    );
    legendData.push(specName);
  });

  setChartOptions(buildOption(dates, series, legendData, tooltipFormatter));
}

function buildOption(
  dates: string[],
  series: any[],
  legendData: string[],
  tooltipFormatter: (params: any[]) => string,
) {
  return {
    tooltip: {
      trigger: 'axis',
      backgroundColor: 'rgba(255, 255, 255, 0.95)',
      borderColor: '#f0f2f5',
      textStyle: { color: '#262626', fontSize: 13 },
      extraCssText: 'box-shadow: 0 4px 12px rgba(0, 0, 0, 0.1); border-radius: 8px;',
      axisPointer: {
        type: 'line',
        lineStyle: { color: 'rgba(0, 0, 0, 0.2)', width: 1, type: 'dashed' },
      },
      formatter: tooltipFormatter,
    },
    legend: {
      data: legendData,
      top: 0,
      right: 0,
      itemWidth: 14,
      itemHeight: 10,
      textStyle: { color: '#666', fontSize: 12 },
    },
    grid: {
      left: '2%',
      right: '2%',
      bottom: '5%',
      top: '15%',
      containLabel: true,
    },
    xAxis: {
      type: 'category',
      boundaryGap: false,
      data: dates,
      axisLine: { show: false },
      axisTick: { show: false },
      axisLabel: { color: '#8c9eae', fontSize: 12, margin: 16 },
    },
    yAxis: {
      type: 'value',
      axisLine: { show: false },
      axisTick: { show: false },
      splitLine: { lineStyle: { color: '#f5f5f5', type: 'dashed' } },
      axisLabel: { color: '#8c9eae', fontSize: 12 },
      scale: true,
    },
    series,
  };
}

function buildTooltipFormatter(isSingleSpec: boolean): (params: any[]) => string {
  return (params: any[]) => {
    const date = params[0]?.axisValue ?? params[0]?.name ?? '';
    let result = `<div style="margin-bottom: 8px; font-weight: 600; color: #1f1f1f;">${date}</div>`;
    const avgSeries = params.filter((p) => p.seriesName === '平均值' || (!p.seriesName?.endsWith('-下限') && !p.seriesName?.endsWith('-区间')));
    if (isSingleSpec) {
      const avg = params.find((p) => p.seriesName === '平均值');
      const minParam = params.find((p) => p.seriesName === '下限');
      const maxParam = params.find((p) => p.seriesName === '区间带');
      if (avg?.value != null) {
        result += `<div style="display: flex; align-items: center; justify-content: space-between; gap: 20px; margin-bottom: 4px;">
          <div style="display: flex; align-items: center;">
            <span style="display: inline-block; width: 8px; height: 8px; border-radius: 50%; background-color: #4facfe; margin-right: 8px;"></span>
            <span style="color: #666;">平均值</span>
          </div>
          <span style="font-weight: 500; color: #262626;">${avg.value}</span>
        </div>`;
      }
      const minVal = minParam?.value;
      const bandVal = maxParam?.value;
      if (minVal !== undefined && bandVal !== undefined) {
        const maxVal = (Number(minVal) + Number(bandVal)).toFixed(4);
        result += `<div style="display: flex; align-items: center; justify-content: space-between; gap: 20px; margin-bottom: 4px;">
          <div style="display: flex; align-items: center;">
            <span style="display: inline-block; width: 8px; height: 8px; border-radius: 50%; background-color: rgba(79, 172, 254, 0.3); margin-right: 8px;"></span>
            <span style="color: #666;">范围</span>
          </div>
          <span style="font-weight: 500; color: #262626;">${minVal} ~ ${maxVal}</span>
        </div>`;
      }
      return result;
    }
    // 多规格：只展示「规格名: 均值」及范围（若有）
    const bySpec = new Map<string, { value: number | null; min?: number; max?: number }>();
    for (const p of params) {
      if (p.seriesName?.endsWith('-下限') || p.seriesName?.endsWith('-区间')) continue;
      const specName = p.seriesName;
      if (!specName || specName === '平均值') continue;
      const val = p.value != null ? Number(p.value) : null;
      if (!bySpec.has(specName)) bySpec.set(specName, { value: val });
      else bySpec.get(specName)!.value = val;
    }
    for (const p of params) {
      if (p.seriesName?.endsWith('-下限') && p.value != null) {
        const specName = p.seriesName.replace('-下限', '');
        const cur = bySpec.get(specName);
        if (cur) cur.min = Number(p.value);
      }
      if (p.seriesName?.endsWith('-区间') && p.value != null) {
        const specName = p.seriesName.replace('-区间', '');
        const cur = bySpec.get(specName);
        if (cur) cur.max = cur.min != null ? cur.min + Number(p.value) : undefined;
      }
    }
    bySpec.forEach((v, specName) => {
      if (v.value == null) return;
      result += `<div style="display: flex; align-items: center; justify-content: space-between; gap: 20px; margin-bottom: 4px;">
        <span style="color: #666;">${specName}</span>
        <span style="font-weight: 500; color: #262626;">${v.value}${v.min != null && v.max != null ? `（${v.min.toFixed(4)} ~ ${v.max.toFixed(4)}）` : ''}</span>
      </div>`;
    });
    return result;
  };
}

watch(
  () => props.data,
  () => {
    if (setChartOptions) {
      updateChart();
    }
  },
  { deep: true },
);

onMounted(() => {
  initChart();
});
</script>

<style lang="less" scoped>
.chart-card {
  background: #ffffff;
  border-radius: 12px;
  padding: 24px;
  box-shadow: 0 4px 20px rgba(0, 0, 0, 0.05);
  height: 100%;
  display: flex;
  flex-direction: column;
  transition: all 0.3s;
  border: 1px solid rgba(0, 0, 0, 0.02);

  &:hover {
    box-shadow: 0 10px 25px rgba(0, 0, 0, 0.08);
  }
}

.chart-header {
  margin-bottom: 12px;
}

.chart-title {
  font-size: 16px;
  font-weight: 600;
  color: #2d3748;
  margin: 0;
  display: flex;
  align-items: center;

  &::before {
    content: '';
    width: 4px;
    height: 16px;
    background: #4facfe;
    border-radius: 2px;
    margin-right: 8px;
  }
}

.chart-desc {
  display: block;
  font-size: 12px;
  color: #718096;
  margin-top: 6px;
  line-height: 1.45;
}

.chart-body {
  flex: 1;
  min-height: 320px;
  position: relative;
}

.chart-empty {
  position: absolute;
  inset: 0;
  display: flex;
  align-items: center;
  justify-content: center;
  font-size: 13px;
  color: #a0aec0;
}

.chart-container {
  width: 100%;
  height: 100%;
  min-height: 320px;
}
</style>
