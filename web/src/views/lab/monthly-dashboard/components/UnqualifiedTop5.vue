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
import { ref, watch, onMounted, type Ref } from 'vue';
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

function initChart() {
  if (!chartRef.value) return;

  const { setOptions } = useECharts(chartRef as Ref<HTMLDivElement>);
  setChartOptions = setOptions;

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
      backgroundColor: 'rgba(255, 255, 255, 0.95)',
      borderColor: '#f0f2f5',
      textStyle: { color: '#262626' },
      axisPointer: { type: 'shadow' },
      formatter: (params: any) => {
        const param = params[0];
        const item = top5[param.dataIndex];
        if (!item) return '';
        return `
          <div style="font-weight: 600; margin-bottom: 4px;">${param.name}</div>
          <div>重量: ${param.value}kg</div>
          <div>占比: ${item.rate?.toFixed(2) || 0}%</div>
        `;
      },
    },
    grid: {
      left: '3%',
      right: '8%',
      bottom: '3%',
      top: '3%',
      containLabel: true,
    },
    xAxis: {
      type: 'value',
      axisLine: { show: false },
      axisTick: { show: false },
      splitLine: {
        lineStyle: { color: '#f5f5f5', type: 'dashed' },
      },
      axisLabel: {
        color: '#8c9eae',
        fontSize: 12,
      },
    },
    yAxis: {
      type: 'category',
      data: categories.length > 0 ? categories : ['暂无数据'],
      axisLine: { show: false },
      axisTick: { show: false },
      axisLabel: {
        color: '#2d3748',
        fontSize: 13,
        fontWeight: 500,
        width: 100,
        overflow: 'truncate'
      },
    },
    series: [
      {
        type: 'bar',
        data: weights.length > 0 ? weights : [0],
        barWidth: '24px',
        itemStyle: {
          color: {
            type: 'linear',
            x: 0,
            y: 0,
            x2: 1,
            y2: 0,
            colorStops: [
              { offset: 0, color: '#ff9a9e' },
              { offset: 1, color: '#ff7875' },
            ],
          },
          borderRadius: [0, 12, 12, 0],
          shadowColor: 'rgba(255, 120, 117, 0.2)',
          shadowBlur: 4,
          shadowOffsetX: 2
        },
        label: {
          show: true,
          position: 'right',
          formatter: '{c} kg',
          color: '#8c9eae',
          fontSize: 12,
          offset: [5, 0]
        },
        showBackground: true,
        backgroundStyle: {
          color: '#f5f7fa',
          borderRadius: [0, 12, 12, 0]
        }
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
  margin-bottom: 20px;
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
    background: #FF7875;
    border-radius: 2px;
    margin-right: 8px;
  }
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
