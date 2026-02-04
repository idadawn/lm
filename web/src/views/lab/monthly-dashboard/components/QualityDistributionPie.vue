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

interface Props {
  summary?: SummaryData | null;
  loading?: boolean;
  qualifiedColumns?: JudgmentLevelColumn[];
}

const props = withDefaults(defineProps<Props>(), {
  loading: false,
  qualifiedColumns: () => [],
});

// 预定义颜色池
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

  // 动态生成合格分类数据（按 qualifiedColumns 顺序）
  let qualifiedColorIndex = 0;
  for (const col of props.qualifiedColumns || []) {
    if (s.qualifiedCategories?.[col.code]) {
      pieData.push({
        name: col.name,
        value: s.qualifiedCategories[col.code].weight,
        itemStyle: {
          color: col.color || qualifiedColors[qualifiedColorIndex % qualifiedColors.length]
        }
      });
      qualifiedColorIndex++;
    }
  }

  // 添加不在 qualifiedColumns 中的其他合格分类（兜底）
  for (const [key, stat] of Object.entries(s.qualifiedCategories || {})) {
    const isInColumns = props.qualifiedColumns?.some(col => col.code === key);
    if (!isInColumns) {
      pieData.push({
        name: key,
        value: stat.weight,
        itemStyle: { color: qualifiedColors[qualifiedColorIndex % qualifiedColors.length] }
      });
      qualifiedColorIndex++;
    }
  }

  // 添加不合格分类
  let unqualifiedColorIndex = 0;
  for (const [key, weight] of Object.entries(s.unqualifiedCategories || {})) {
    pieData.push({
      name: key,
      value: weight,
      itemStyle: { color: unqualifiedColors[unqualifiedColorIndex % unqualifiedColors.length] }
    });
    unqualifiedColorIndex++;
  }

  const totalWeight = s.totalWeight || 0;

  // Format total weight for display
  const totalWeightText = totalWeight > 0
    ? `${(totalWeight / 1000).toFixed(1)}`
    : '0';

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
      type: 'plain', // Ensure all items are shown without scrolling
      orient: 'vertical',
      right: 10,
      top: 'middle',
      align: 'left',
      itemGap: 15, // Increased gap
      itemWidth: 18, // Larger square
      itemHeight: 18, // Larger square
      icon: 'rect', 
      textStyle: {
        color: '#555', // Slightly darker text
        fontSize: 13, // Slightly larger text
        fontWeight: 500 // Bolder text
      },
      // Using standard formatter to avoid alignment issues
      formatter: (name: string) => {
        return name;
      }
    },
    series: [
      {
        type: 'pie',
        radius: ['50%', '75%'],
        center: ['30%', '50%'], // Moved left from 35%
        avoidLabelOverlap: false,
        itemStyle: {
          borderRadius: 8,
          borderColor: '#fff',
          borderWidth: 2
        },
        label: {
          show: false,
        },
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
        labelLine: {
          show: false,
        },
        data: pieData,
      },
    ],
    title: {
      show: true,
      text: totalWeightText,
      subtext: '总吨数',
      left: '29%', // Adjusted to match center (roughly center - half width estimate, or just use percentage alignment if textAlign is center)
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
