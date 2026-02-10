<template>
  <div class="chart-card">
    <div class="chart-header">
      <h3 class="chart-title">叠片系数关联</h3>
      <div class="legend-custom">
        <span
          v-for="item in legendItems"
          :key="item.level"
          class="legend-item"
        >
          <span class="legend-dot" :style="{ background: getLevelColor(item.level) }" />
          <span class="legend-text" :style="{ color: getLevelColor(item.level) }">{{ item.level }}</span>
        </span>
      </div>
    </div>
    <div class="chart-body">
      <div ref="chartRef" class="chart-container" />
      <div v-if="hasNoData" class="chart-empty">暂无厚度与叠片系数数据</div>
    </div>
  </div>
</template>

<script lang="ts" setup>
import { ref, computed, watch, onMounted, type Ref } from 'vue';
import { useECharts } from '/@/hooks/web/useECharts';
import type { ScatterData } from '/@/api/lab/dashboard';

interface Props {
  data?: ScatterData[] | null;
  loading?: boolean;
}

const props = withDefaults(defineProps<Props>(), {
  loading: false,
});

const chartRef = ref<HTMLDivElement | null>(null);
let setChartOptions: ((option: any) => void) | null = null;

const hasNoData = computed(() => !props.data || props.data.length === 0);

// 质量等级颜色：合格绿/黄，不合格橙/红，未判定灰（与质量等级分布色系一致，A/A级同色）
const LEVEL_COLORS: Record<string, string> = {
  A级: '#52c41a',
  A: '#52c41a',
  韧性: '#52c41a',
  B级: '#fadb14',
  B: '#fadb14',
  性能不合: '#fa8c16',
  极差不合: '#fa541c',
  劈裂: '#f5222d',
  其他不合: '#f5222d',
  未判定: '#8c8c8c',
};

const DEFAULT_COLOR = '#8c8c8c';

function getLevelColor(level: string): string {
  return LEVEL_COLORS[level] ?? DEFAULT_COLOR;
}

// 与质量等级分布图例顺序一致：合格(A/A级、B/B级、韧性) → 不合格(性能不合、极差不合、劈裂、其他不合) → 未判定
const LEGEND_PRIORITY_ORDER = [
  'A级', 'A', 'B级', 'B', '韧性',
  '性能不合', '极差不合', '劈裂', '其他不合',
  '未判定',
];

function levelSortIndex(name: string): number {
  const i = LEGEND_PRIORITY_ORDER.indexOf(name);
  return i >= 0 ? i : LEGEND_PRIORITY_ORDER.length;
}

// 图例项：仅显示数据中出现的等级，按与质量等级分布相同的判定等级顺序排序
const legendItems = computed(() => {
  if (!props.data?.length) return [];
  const set = new Set<string>();
  props.data.forEach((d) => set.add(d.qualityLevel?.trim() || '未判定'));
  return Array.from(set)
    .sort((a, b) => {
      const ia = levelSortIndex(a);
      const ib = levelSortIndex(b);
      if (ia !== ib) return ia - ib;
      return a.localeCompare(b);
    })
    .map((level) => ({ level }));
});

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

  const grouped = new Map<string, [number, number][]>();
  props.data.forEach((item) => {
    const level = item.qualityLevel?.trim() || '未判定';
    if (!grouped.has(level)) grouped.set(level, []);
    grouped.get(level)!.push([Number(item.thickness), Number(item.laminationFactor)]);
  });

  const allPoints = props.data.map((d) => [Number(d.thickness), Number(d.laminationFactor)] as [number, number]);
  const xArr = allPoints.map((p) => p[0]);
  const yArr = allPoints.map((p) => p[1]);
  const xMin = Math.min(...xArr);
  const xMax = Math.max(...xArr);
  const yMin = Math.min(...yArr);
  const yMax = Math.max(...yArr);
  const xPadding = Math.max((xMax - xMin) * 0.05 || 0.01, 0.01);
  const yPadding = Math.max((yMax - yMin) * 0.05 || 0.5, 0.5);

  // 系列顺序与图例一致：按判定等级优先级
  const sortedLevels = Array.from(grouped.keys()).sort((a, b) => {
    const ia = levelSortIndex(a);
    const ib = levelSortIndex(b);
    if (ia !== ib) return ia - ib;
    return a.localeCompare(b);
  });

  const series = sortedLevels.map((level) => {
    const points = grouped.get(level)!;
    return {
      name: level,
      type: 'scatter',
      data: points,
      symbolSize: 10,
      itemStyle: {
        color: getLevelColor(level),
        opacity: 0.85,
        borderColor: '#fff',
        borderWidth: 1,
      },
      emphasis: {
        scale: true,
        itemStyle: {
          opacity: 1,
          borderWidth: 2,
          borderColor: '#fff',
        },
      },
    };
  });

  const option = {
    tooltip: {
      trigger: 'item',
      backgroundColor: 'rgba(255, 255, 255, 0.95)',
      borderColor: '#f0f2f5',
      textStyle: { color: '#262626', fontSize: 13 },
      extraCssText: 'box-shadow: 0 4px 12px rgba(0, 0, 0, 0.1); border-radius: 8px;',
      formatter: (params: any) => {
        const p = params;
        const name = p.seriesName || '';
        const v = p.value as number[];
        if (!v || v.length < 2) return '';
        return `<div style="font-weight: 600; color: #1f1f1f; margin-bottom: 6px;">${name}</div>
          <div style="color: #666;">平均厚度：${Number(v[0]).toFixed(4)}</div>
          <div style="color: #666;">叠片系数：${Number(v[1]).toFixed(4)}</div>`;
      },
    },
    legend: {
      show: false,
    },
    grid: {
      left: '10%',
      right: '6%',
      bottom: '12%',
      top: '8%',
      containLabel: true,
    },
    xAxis: {
      type: 'value',
      name: '平均厚度',
      nameLocation: 'middle',
      nameGap: 28,
      min: xMin - xPadding,
      max: xMax + xPadding,
      axisLine: { show: false },
      axisTick: { show: false },
      splitLine: { lineStyle: { color: '#f5f5f5', type: 'dashed' } },
      axisLabel: { color: '#8c9eae', fontSize: 12, formatter: (v: number) => Number(v).toFixed(3) },
      nameTextStyle: { color: '#8c9eae', fontSize: 12 },
    },
    yAxis: {
      type: 'value',
      name: '叠片系数',
      nameLocation: 'middle',
      nameGap: 40,
      min: yMin - yPadding,
      max: yMax + yPadding,
      minInterval: 1,
      axisLine: { show: false },
      axisTick: { show: false },
      splitLine: { lineStyle: { color: '#f5f5f5', type: 'dashed' } },
      axisLabel: { color: '#8c9eae', fontSize: 12, formatter: (v: number) => String(Math.round(v)) },
      nameTextStyle: { color: '#8c9eae', fontSize: 12 },
      scale: true,
    },
    series,
  };

  setChartOptions(option);
}

watch(
  () => props.data,
  () => {
    if (setChartOptions) updateChart();
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
    background: #43e97b;
    border-radius: 2px;
    margin-right: 8px;
  }
}

.legend-custom {
  display: flex;
  flex-wrap: wrap;
  gap: 14px;
  align-items: center;
  margin-top: 8px;

  .legend-item {
    display: flex;
    align-items: center;
    gap: 6px;
    font-size: 12px;
    font-weight: 500;

    .legend-dot {
      width: 10px;
      height: 10px;
      border-radius: 50%;
      flex-shrink: 0;
    }

    .legend-text {
      font-weight: 500;
    }
  }
}

.chart-body {
  flex: 1;
  min-height: 360px;
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
  min-height: 360px;
}
</style>
