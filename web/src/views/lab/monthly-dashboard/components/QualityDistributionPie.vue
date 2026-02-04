<template>
  <div class="chart-card">
    <div class="chart-header">
      <h3 class="chart-title">质量等级分布</h3>
    </div>
    <div class="chart-body">
      <div ref="chartRef" class="chart-container"></div>
    </div>
  </div>
</template>

<script lang="ts" setup>
import { ref, watch, onMounted, onUnmounted } from 'vue';
import { useECharts } from '/@/hooks/web/useECharts';
import type { SummaryData } from '/@/api/lab/monthlyQualityReport';

interface Props {
  summary?: SummaryData | null;
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
  if (!setChartOptions || !props.summary) return;

  const s = props.summary;

  // Build pie data from qualifiedCategories and unqualifiedCategories
  const pieData: { name: string; value: number; itemStyle: { color: string } }[] = [];

  // Add qualified categories
  if (s.qualifiedCategories?.['A']) {
    pieData.push({ name: 'A类', value: s.qualifiedCategories['A'].weight, itemStyle: { color: '#52c41a' } });
  }
  if (s.qualifiedCategories?.['B']) {
    pieData.push({ name: 'B类', value: s.qualifiedCategories['B'].weight, itemStyle: { color: '#1890ff' } });
  }
  for (const [key, stat] of Object.entries(s.qualifiedCategories || {})) {
    if (key !== 'A' && key !== 'B') {
      pieData.push({ name: key, value: stat.weight, itemStyle: { color: '#52c41a' } });
    }
  }

  // Add unqualified categories with different colors
  const unqualifiedColors = ['#ff4d4f', '#faad14', '#fa8c16', '#a0d911', '#722ed1'];
  let colorIndex = 0;
  for (const [key, weight] of Object.entries(s.unqualifiedCategories || {})) {
    pieData.push({
      name: key,
      value: weight,
      itemStyle: { color: unqualifiedColors[colorIndex % unqualifiedColors.length] }
    });
    colorIndex++;
  }

  const totalWeight = s.totalWeight || 0;

  // Format total weight for display
  const totalWeightText = totalWeight > 0
    ? `${(totalWeight / 1000).toFixed(1)}吨`
    : '0吨';

  const option = {
    tooltip: {
      trigger: 'item',
      formatter: '{b}: {c}kg ({d}%)',
    },
    legend: {
      orient: 'vertical',
      right: 10,
      top: 'center',
      textStyle: { fontSize: 12 },
    },
    series: [
      {
        type: 'pie',
        radius: ['40%', '65%'],
        center: ['38%', '50%'],
        avoidLabelOverlap: false,
        label: {
          show: false,
        },
        emphasis: {
          label: {
            show: true,
            fontSize: 14,
            fontWeight: 'bold',
          },
        },
        labelLine: {
          show: false,
        },
        data: pieData,
      },
    ],
    title: {
      show: true,
      text: totalWeightText,
      subtext: '总重量',
      left: '38%',
      top: '44%',
      textAlign: 'center',
      textStyle: {
        fontSize: 20,
        fontWeight: 'bold',
        color: '#262626',
      },
      subtextStyle: {
        fontSize: 13,
        color: '#8c8c8c',
      },
    },
  };

  setChartOptions(option);
}

watch(
  () => props.summary,
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
