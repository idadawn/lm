<template>
  <div class="chart-card">
    <div class="chart-header">
      <h3 class="chart-title">班次-日期热力图</h3>
    </div>
    <div class="chart-body">
      <div ref="chartRef" class="chart-container"></div>
    </div>
  </div>
</template>

<script lang="ts" setup>
import { ref, watch, onMounted, onUnmounted } from 'vue';
import { useECharts } from '/@/hooks/web/useECharts';
import type { DetailRow } from '/@/api/lab/monthlyQualityReport';
import dayjs from 'dayjs';

interface Props {
  details?: DetailRow[] | null;
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
  if (!setChartOptions || !props.details) return;

  // Group by date and shift, calculate qualified rate
  const dataMap = new Map<string, { date: string; shift: string; rate: number; count: number }>();

  const shifts = ['甲', '乙', '丙'];
  const dates = [...new Set(props.details.map((d) => d.prodDate).filter(Boolean))].sort();

  for (const detail of props.details) {
    if (!detail.prodDate || detail.isSummaryRow) continue;

    const dateStr = dayjs(detail.prodDate).format('MM-DD');
    const key = `${dateStr}-${detail.shift}`;

    if (!dataMap.has(key)) {
      dataMap.set(key, { date: dateStr, shift: detail.shift, rate: 0, count: 0 });
    }

    const entry = dataMap.get(key)!;
    entry.rate += detail.qualifiedRate || 0;
    entry.count++;
  }

  // Calculate average rates
  const heatmapData: [number, number, number][] = [];
  const xAxisData = dates.map((d) => dayjs(d).format('MM-DD'));

  dataMap.forEach((value) => {
    const avgRate = value.count > 0 ? value.rate / value.count : 0;
    const xIndex = xAxisData.indexOf(value.date);
    const yIndex = shifts.indexOf(value.shift);

    if (xIndex >= 0 && yIndex >= 0) {
      heatmapData.push([xIndex, yIndex, Number(avgRate.toFixed(2))]);
    }
  });

  const option = {
    tooltip: {
      position: 'top',
      formatter: (params: any) => {
        const [x, y, value] = params.data;
        return `${xAxisData[x]}<br/>${shifts[y]}班<br/>合格率: ${value}%`;
      },
    },
    grid: {
      height: '70%',
      top: '10%',
    },
    xAxis: {
      type: 'category',
      data: xAxisData,
      splitArea: { show: true },
      axisLabel: {
        fontSize: 11,
        color: '#666',
      },
    },
    yAxis: {
      type: 'category',
      data: shifts,
      splitArea: { show: true },
      axisLabel: {
        fontSize: 12,
        color: '#262626',
      },
    },
    visualMap: {
      min: 0,
      max: 100,
      calculable: true,
      orient: 'horizontal',
      left: 'center',
      bottom: '0%',
      inRange: {
        color: ['#ff4d4f', '#ffec3d', '#52c41a'],
      },
      textStyle: { fontSize: 11 },
    },
    series: [
      {
        type: 'heatmap',
        data: heatmapData,
        label: {
          show: true,
          fontSize: 11,
          color: '#fff',
          formatter: (params: any) => {
            return params.data[2] > 0 ? params.data[2] + '%' : '';
          },
        },
        emphasis: {
          itemStyle: {
            shadowBlur: 10,
            shadowColor: 'rgba(0, 0, 0, 0.5)',
          },
        },
      },
    ],
  };

  setChartOptions(option);
}

watch(
  () => props.details,
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
  min-height: 300px;
}

.chart-container {
  width: 100%;
  height: 100%;
  min-height: 300px;
}
</style>
