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
import { ref, watch, onMounted, type Ref } from 'vue';
import { useECharts } from '/@/hooks/web/useECharts';
import type { ShiftComparison, JudgmentLevelColumn } from '/@/api/lab/monthlyQualityReport';

interface Props {
  data?: ShiftComparison[] | null;
  loading?: boolean;
  qualifiedColumns?: JudgmentLevelColumn[];
}

const props = withDefaults(defineProps<Props>(), {
  loading: false,
  qualifiedColumns: () => [],
});

const chartRef = ref<HTMLDivElement | null>(null);
let setChartOptions: ((option: any) => void) | null = null;

// 颜色池
const colors = ['#42e695', '#4facfe', '#ff9a9e', '#fa709a', '#8ec5fc'];

// 获取某个等级的占比数据
function getCategoryRate(shift: ShiftComparison, colCode: string): number {
  // 尝试从动态字段获取
  const dynamicField = `class${colCode}Rate`;
  const val = (shift as any)[dynamicField];
  if (val !== undefined && val !== null) {
    return Number(val) || 0;
  }
  // 兼容旧字段
  if (colCode === 'A') {
    const legacyVal = (shift as any).classARate;
    if (legacyVal !== undefined && legacyVal !== null) {
      return Number(legacyVal) || 0;
    }
  }
  if (colCode === 'B') {
    const legacyVal = (shift as any).classBRate;
    if (legacyVal !== undefined && legacyVal !== null) {
      return Number(legacyVal) || 0;
    }
  }
  return 0;
}

function initChart() {
  if (!chartRef.value) return;

  const { setOptions } = useECharts(chartRef as Ref<HTMLDivElement>);
  setChartOptions = setOptions;

  updateChart();
}

function updateChart() {
  if (!setChartOptions || !props.data) return;

  const shifts = props.data;
  console.log('ShiftComparisonRadar data:', shifts); // Debug logging

  // 动态构建指标：总产量、合格率，以及所有合格等级
  const indicators = [
    { name: '总产量', max: 0 },
    { name: '合格率', max: 100 },
  ];

  // 根据合格等级列动态添加指标
  for (const col of props.qualifiedColumns) {
    indicators.push({ name: `${col.name}占比`, max: 100 });
  }

  // 计算总产量的最大值
  const weights = shifts.map((s) => s.totalWeight || 0);
  const maxVal = weights.length > 0 ? Math.max(...weights) : 0;
  const maxWeight = maxVal > 0 ? maxVal * 1.2 : 100;
  indicators[0].max = maxWeight;

  // 构建系列数据
  const series = shifts.map((shift) => {
    const value = [
      shift.totalWeight || 0,
      shift.qualifiedRate || 0,
    ];

    // 动态添加各等级占比数据
    for (const col of props.qualifiedColumns) {
      value.push(getCategoryRate(shift, col.code));
    }

    return {
      value,
      name: shift.shift,
    };
  });

  const option = {
    tooltip: {
      trigger: 'item',
      backgroundColor: 'rgba(255, 255, 255, 0.95)',
      borderColor: '#f0f2f5',
      textStyle: { color: '#262626' },
    },
    legend: {
      data: shifts.map((s) => s.shift),
      bottom: 0,
      icon: 'circle',
      textStyle: { color: '#666' }
    },
    radar: {
      indicator: indicators,
      radius: '65%',
      center: ['50%', '50%'],
      splitNumber: 4,
      axisName: {
        color: '#666',
        fontSize: 12,
        fontWeight: 500
      },
      splitLine: {
        lineStyle: { color: '#e8e8e8' },
      },
      splitArea: {
        show: true,
        areaStyle: {
          color: ['rgba(245, 247, 250, 1)', 'rgba(255, 255, 255, 1)']
        },
      },
      axisLine: {
        lineStyle: { color: '#e8e8e8' },
      },
    },
    series: [
      {
        type: 'radar',
        data: series.map((item, index) => ({
          ...item,
          itemStyle: { color: colors[index % colors.length] },
          areaStyle: { opacity: 0.3 }
        })),
        symbol: 'circle',
        symbolSize: 6,
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
    background: #4facfe;
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
