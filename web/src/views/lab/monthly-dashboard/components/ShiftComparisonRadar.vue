<template>
  <div class="chart-card">
    <div class="chart-header">
      <h3 class="chart-title">班次对比</h3>
    </div>
    <div class="chart-body">
      <div ref="chartRef" class="chart-container"></div>
    </div>
  </div>
</template>

<script lang="ts" setup>
import { ref, watch, onMounted, onUnmounted } from 'vue';
import { useECharts } from '/@/hooks/web/useECharts';
import type { ShiftComparison } from '/@/api/lab/monthlyQualityReport';

interface Props {
  data?: ShiftComparison[] | null;
  loading?: boolean;
}

const props = withDefaults(defineProps<Props>(), {
  loading: false,
});

const chartRef = ref<HTMLDivElement | null>(null);
let setChartOptions: ((option: any) => void) | null = null;
let chartEchartsInst: any = null;

function initChart() {
  if (!chartRef.value) return;

  const { setOptions, echarts } = useECharts(chartRef.value);
  setChartOptions = setOptions;
  chartEchartsInst = echarts;

  updateChart();
}

function updateChart() {
  if (!setChartOptions || !props.data) return;

  const shifts = props.data;
  const indicators = [
    { name: '总产量', max: 0 },
    { name: '合格率', max: 100 },
    { name: 'A类占比', max: 100 },
    { name: 'B类占比', max: 100 },
  ];

  // Calculate max for production weight
  const maxWeight = Math.max(...shifts.map((s) => s.totalWeight || 0)) * 1.2;
  indicators[0].max = maxWeight;

  const series = shifts.map((shift) => ({
    value: [
      shift.totalWeight || 0,
      shift.qualifiedRate || 0,
      shift.classARate || 0,
      shift.classBRate || 0,
    ],
    name: shift.shift,
  }));

  const option = {
    tooltip: {
      trigger: 'item',
    },
    legend: {
      data: shifts.map((s) => s.shift),
      bottom: 10,
    },
    radar: {
      indicator: indicators,
      radius: '65%',
      center: ['50%', '50%'],
      splitNumber: 4,
      axisName: {
        color: '#666',
        fontSize: 12,
      },
      splitLine: {
        lineStyle: { color: '#e8e8e8' },
      },
      splitArea: {
        show: true,
        areaStyle: { color: ['rgba(24, 144, 255, 0.05)', 'rgba(24, 144, 255, 0.1)'] },
      },
      axisLine: {
        lineStyle: { color: '#e8e8e8' },
      },
    },
    series: [
      {
        type: 'radar',
        data: series,
        symbol: 'circle',
        symbolSize: 6,
        areaStyle: { opacity: 0.2 },
      },
    ],
  };

  setChartOptions(option);
}

watch(
  () => props.data,
  () => {
    if (setChartOptions) {
      updateChart();
    }
  },
  { deep: true }
);

onMounted(() => {
  initChart();
});

onUnmounted(() => {
  if (setChartOptions) {
    chartEchartsInst.dispose();
  }
});
</script>

<style lang="less" scoped>
.chart-card {
  background: #fff;
  border-radius: 8px;
  padding: 16px;
  box-shadow: 0 2px 8px rgba(0, 0, 0, 0.06);
  height: 100%;
  display: flex;
  flex-direction: column;
}

.chart-header {
  margin-bottom: 12px;
}

.chart-title {
  font-size: 16px;
  font-weight: 600;
  color: #262626;
  margin: 0;
}

.chart-body {
  flex: 1;
  min-height: 320px;
}

.chart-container {
  width: 100%;
  height: 100%;
  min-height: 320px;
}
</style>
