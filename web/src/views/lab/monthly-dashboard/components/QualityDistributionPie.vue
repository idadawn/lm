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
import { ref, watch, onMounted, type Ref } from 'vue';
import { useECharts } from '/@/hooks/web/useECharts';
import type { SummaryData, JudgmentLevelColumn } from '/@/api/lab/monthlyQualityReport';
import type { ReportConfig } from '/@/api/lab/reportConfig';

interface Props {
  summary?: SummaryData | null;
  loading?: boolean;
  qualifiedColumns?: JudgmentLevelColumn[];
  reportConfigs?: ReportConfig[];
}

const props = withDefaults(defineProps<Props>(), {
  loading: false,
  qualifiedColumns: () => [],
  reportConfigs: () => [],
});

const qualifiedColors = [
  '#42e695', '#4facfe', '#fa709a', '#8ec5fc',
  '#a8edea', '#fed6e3', '#c1dfc4', '#deecf9'
];
const unqualifiedColors = ['#ff9a9e', '#fecfef', '#ff7875', '#ff9c6e', '#ffc069'];

const chartRef = ref<HTMLDivElement | null>(null);
let setChartOptions: ((option: any) => void) | null = null;

function initChart() {
  if (!chartRef.value) return;
  const { setOptions } = useECharts(chartRef as Ref<HTMLDivElement>);
  setChartOptions = setOptions;
  updateChart();
}

function updateChart() {
  if (!setChartOptions || !props.summary) return;

  const s = props.summary;
  const pieData: { name: string; value: number; itemStyle?: { color: string } }[] = [];

  // 使用所有 isShowInReport 的 reportConfigs，从 dynamicStats 获取数据
  const visibleConfigs = (props.reportConfigs || []).filter((c) => c.isShowInReport);

  if (visibleConfigs.length > 0 && s.dynamicStats) {
    visibleConfigs.forEach((config, index) => {
      const stat = (s.dynamicStats as any)?.[config.id];
      const weight = Number(stat?.weight) || 0;
      if (weight <= 0) return;
      pieData.push({
        name: config.name,
        value: weight,
        itemStyle: { color: qualifiedColors[index % qualifiedColors.length] },
      });
    });
  }

  // 补充不合格分类数据
  if (s.unqualifiedCategories) {
    let unqualifiedColorIndex = 0;
    for (const [key, weight] of Object.entries(s.unqualifiedCategories)) {
      if (Number(weight) > 0) {
        pieData.push({
          name: key,
          value: Number(weight),
          itemStyle: { color: unqualifiedColors[unqualifiedColorIndex % unqualifiedColors.length] },
        });
        unqualifiedColorIndex++;
      }
    }
  }

  const totalWeight = s.totalWeight || 0;
  const totalWeightText = totalWeight > 0 ? `${(totalWeight / 1000).toFixed(1)}` : '0';

  const option = {
    tooltip: {
      trigger: 'item',
      backgroundColor: 'rgba(255, 255, 255, 0.95)',
      borderColor: '#f0f2f5',
      textStyle: { color: '#262626' },
      formatter: (params: any) => {
        return `
          <div style="margin-bottom: 4px; font-weight: 600;">${params.name}</div>
          <div style="display: flex; justify-content: space-between; gap: 12px;">
            <span>重量:</span>
            <span style="font-weight: 500;">${params.value} kg</span>
          </div>
          <div style="display: flex; justify-content: space-between; gap: 12px;">
            <span>占比:</span>
            <span style="font-weight: 500;">${params.percent}%</span>
          </div>
        `;
      }
    },
    legend: {
      type: 'plain',
      orient: 'vertical',
      right: 10,
      top: 'middle',
      align: 'left',
      itemGap: 15,
      itemWidth: 18,
      itemHeight: 18,
      icon: 'rect',
      textStyle: {
        color: '#555',
        fontSize: 13,
        fontWeight: 500
      },
      formatter: (name: string) => name,
    },
    series: [
      {
        type: 'pie',
        radius: ['50%', '75%'],
        center: ['30%', '50%'],
        avoidLabelOverlap: false,
        itemStyle: {
          borderRadius: 8,
          borderColor: '#fff',
          borderWidth: 2
        },
        label: { show: false },
        emphasis: {
          label: {
            show: true,
            fontSize: 16,
            fontWeight: 'bold',
            formatter: '{b}\n{d}%',
            color: '#262626'
          },
          scale: true,
          scaleSize: 10
        },
        labelLine: { show: false },
        data: pieData,
      },
    ],
    title: {
      show: true,
      text: totalWeightText,
      subtext: '总重量',
      left: '29%',
      top: 'center',
      textAlign: 'center',
      textStyle: {
        fontSize: 24,
        fontWeight: 'bold',
        color: '#2d3748',
        fontFamily: 'Inter, sans-serif'
      },
      subtextStyle: {
        fontSize: 12,
        color: '#a0aec0',
        marginTop: 4
      },
      itemGap: 4
    },
  };

  setChartOptions(option);
}

watch(
  [() => props.summary, () => props.reportConfigs],
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
