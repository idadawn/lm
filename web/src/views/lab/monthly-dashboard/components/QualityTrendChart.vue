<template>
  <div class="chart-card">
    <div class="chart-header">
      <h3 class="chart-title">质量趋势</h3>
    </div>
    <div class="chart-body">
      <div ref="chartRef" class="chart-container"></div>
    </div>
  </div>
</template>

<script lang="ts" setup>
import { ref, watch, onMounted, onUnmounted } from 'vue';
import { useECharts } from '/@/hooks/web/useECharts';
import type { QualityTrend } from '/@/api/lab/monthlyQualityReport';

interface Props {
  data?: QualityTrend[] | null;
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

  const dates = props.data.map((item) => item.date);
  const qualifiedRates = props.data.map((item) => Number(item.qualifiedRate) || 0);
  const classARates = props.data.map((item) => Number(item.classARate) || 0);
  const classBRates = props.data.map((item) => Number(item.classBRate) || 0);

  const option = {
    tooltip: {
      trigger: 'axis',
      axisPointer: {
        type: 'cross',
        label: {
          backgroundColor: '#6a7985',
        },
      },
      formatter: (params: any[]) => {
        let result = `${params[0].name}<br/>`;
        params.forEach((param) => {
          result += `${param.marker} ${param.seriesName}: ${param.value}%<br/>`;
        });
        return result;
      },
    },
    legend: {
      data: ['合格率', 'A类占比', 'B类占比'],
      top: 10,
      right: 20,
    },
    grid: {
      left: '3%',
      right: '4%',
      bottom: '3%',
      top: 60,
      containLabel: true,
    },
    xAxis: {
      type: 'category',
      boundaryGap: false,
      data: dates.length > 0 ? dates : [''],
      axisLine: {
        lineStyle: { color: '#e8e8e8' },
      },
      axisLabel: {
        color: '#666',
        fontSize: 12,
      },
    },
    yAxis: {
      type: 'value',
      max: 100,
      axisLine: { show: false },
      axisTick: { show: false },
      splitLine: {
        lineStyle: { color: '#f0f0f0', type: 'dashed' },
      },
      axisLabel: {
        color: '#666',
        fontSize: 12,
        formatter: '{value}%',
      },
    },
    series: [
      {
        name: '合格率',
        type: 'line',
        data: qualifiedRates,
        smooth: true,
        symbol: 'circle',
        symbolSize: 6,
        lineStyle: { color: '#1890ff', width: 3 },
        itemStyle: { color: '#1890ff' },
        areaStyle: {
          color: {
            type: 'linear',
            x: 0,
            y: 0,
            x2: 0,
            y2: 1,
            colorStops: [
              { offset: 0, color: 'rgba(24, 144, 255, 0.3)' },
              { offset: 1, color: 'rgba(24, 144, 255, 0.05)' },
            ],
          },
        },
      },
      {
        name: 'A类占比',
        type: 'line',
        data: classARates,
        smooth: true,
        symbol: 'circle',
        symbolSize: 6,
        lineStyle: { color: '#52c41a', width: 2, type: 'dashed' },
        itemStyle: { color: '#52c41a' },
      },
      {
        name: 'B类占比',
        type: 'line',
        data: classBRates,
        smooth: true,
        symbol: 'circle',
        symbolSize: 6,
        lineStyle: { color: '#1890ff', width: 2, type: 'dashed' },
        itemStyle: { color: '#1890ff' },
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
  if (chartEchartsInst) {
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
