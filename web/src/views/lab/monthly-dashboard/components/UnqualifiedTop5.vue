<template>
  <div class="chart-card">
    <div class="chart-header">
      <h3 class="chart-title">不合格原因Top5</h3>
    </div>
    <div class="chart-body">
      <div ref="chartRef" class="chart-container"></div>
    </div>
  </div>
</template>

<script lang="ts" setup>
import { ref, watch, onMounted, onUnmounted } from 'vue';
import { useECharts } from '/@/hooks/web/useECharts';
import type { UnqualifiedCategory } from '/@/api/lab/monthlyQualityReport';

interface Props {
  data?: UnqualifiedCategory[] | null;
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

  // Sort by weight and take top 5
  const top5 = [...props.data]
    .sort((a, b) => (b.weight || 0) - (a.weight || 0))
    .slice(0, 5);

  const categories = top5.map((item) => item.categoryName);
  const weights = top5.map((item) => item.weight || 0);

  const option = {
    tooltip: {
      trigger: 'axis',
      axisPointer: { type: 'shadow' },
      formatter: (params: any) => {
        const param = params[0];
        const item = top5[param.dataIndex];
        return `${param.name}<br/>重量: ${param.value}kg<br/>占比: ${item.rate?.toFixed(2) || 0}%`;
      },
    },
    grid: {
      left: '3%',
      right: '4%',
      bottom: '3%',
      top: '3%',
      containLabel: true,
    },
    xAxis: {
      type: 'value',
      axisLine: { show: false },
      axisTick: { show: false },
      splitLine: {
        lineStyle: { color: '#f0f0f0', type: 'dashed' },
      },
      axisLabel: {
        color: '#666',
        fontSize: 12,
      },
    },
    yAxis: {
      type: 'category',
      data: categories.length > 0 ? categories : ['暂无数据'],
      axisLine: { show: false },
      axisTick: { show: false },
      axisLabel: {
        color: '#262626',
        fontSize: 13,
      },
    },
    series: [
      {
        type: 'bar',
        data: weights.length > 0 ? weights : [0],
        barWidth: '60%',
        itemStyle: {
          color: {
            type: 'linear',
            x: 0,
            y: 0,
            x2: 1,
            y2: 0,
            colorStops: [
              { offset: 0, color: '#ff7875' },
              { offset: 1, color: '#ff4d4f' },
            ],
          },
          borderRadius: [0, 4, 4, 0],
        },
        label: {
          show: true,
          position: 'right',
          formatter: '{c}kg',
          color: '#666',
          fontSize: 12,
        },
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
  min-height: 250px;
}

.chart-container {
  width: 100%;
  height: 100%;
  min-height: 250px;
}
</style>
