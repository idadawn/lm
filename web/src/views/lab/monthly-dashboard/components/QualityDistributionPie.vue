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
  unqualifiedColumns?: JudgmentLevelColumn[];
  selectedLevel?: string | null;
}

const props = withDefaults(defineProps<Props>(), {
  loading: false,
  qualifiedColumns: () => [],
  unqualifiedColumns: () => [],
  selectedLevel: null,
});

const emit = defineEmits<{
  (e: 'select', levelName: string | null): void;
}>();

// 每个等级分类图例的默认初始化颜色（按顺序对应等级，与后端 color 无关）
const defaultLevelColors = [
  '#52c41a', '#1890ff', '#13c2c2', '#722ed1', '#eb2f96',
  '#fa8c16', '#faad14', '#a0d911', '#2f54eb', '#597ef7',
  '#ff4d4f', '#ff7a45', '#ffa940', '#ffc069', '#b37feb',
  '#9254de', '#69c0ff', '#5cdbd3', '#95de64', '#bae637',
  '#ff85c0', '#d3adf7', '#91d5ff', '#87d068', '#e6f7ff',
];
const noDataColor = '#f0f0f0';

const chartRef = ref<HTMLDivElement | null>(null);
let setChartOptions: ((option: any) => void) | null = null;
let getChartInstance: (() => any) | null = null;
// 记录所有图例名称，用于判断选中状态
let allLegendNames: string[] = [];

function initChart() {
  if (!chartRef.value) return;
  const { setOptions, getInstance } = useECharts(chartRef as Ref<HTMLDivElement>);
  setChartOptions = setOptions;
  getChartInstance = getInstance;

  // 绑定图例选中变化事件
  const bindEvents = () => {
    const instance = getInstance();
    if (!instance) {
      // 延迟重试
      setTimeout(bindEvents, 100);
      return;
    }
    instance.on('legendselectchanged', (params: any) => {
      const selected: Record<string, boolean> = params.selected || {};
      const selectedNames = Object.entries(selected)
        .filter(([, v]) => v)
        .map(([k]) => k);

      if (selectedNames.length === 1) {
        // 只有一个等级被选中，触发下钻
        emit('select', selectedNames[0]);
      } else if (selectedNames.length === allLegendNames.length) {
        // 全部选中，清除下钻
        emit('select', null);
      }
    });

    // 点击饼图扇区也触发下钻
    instance.on('click', 'series.pie', (params: any) => {
      if (params.name) {
        if (props.selectedLevel === params.name) {
          // 再次点击已选中的等级，清除筛选
          emit('select', null);
        } else {
          emit('select', params.name);
        }
      }
    });
  };

  updateChart();
  bindEvents();
}

function updateChart() {
  if (!setChartOptions) return;

  const s = props.summary;
  const pieData: { name: string; value: number; itemStyle?: { color: string }; _weight?: number }[] = [];

  // 等级列表来源：FormulaId=FirstInspection 的判定等级（API 的 qualifiedColumns + unqualifiedColumns），按优先级排序后合并、按等级名称去重
  const qualifiedList = [...(props.qualifiedColumns || [])].sort((a, b) => (a.priority || 0) - (b.priority || 0));
  const unqualifiedList = [...(props.unqualifiedColumns || [])].sort((a, b) => (a.priority || 0) - (b.priority || 0));
  const seenNames = new Set<string>();
  const levelList: { name: string; color?: string; isQualified: boolean; priority: number }[] = [];
  qualifiedList.forEach((c) => {
    if (seenNames.has(c.name)) return;
    seenNames.add(c.name);
    levelList.push({ name: c.name, color: c.color, isQualified: true, priority: c.priority ?? 0 });
  });
  unqualifiedList.forEach((c) => {
    if (seenNames.has(c.name)) return;
    seenNames.add(c.name);
    levelList.push({ name: c.name, color: c.color, isQualified: false, priority: c.priority ?? 0 });
  });
  levelList.sort((a, b) => a.priority - b.priority);

  if (s) {
    // 根据等级列表从 summary 计算各等级重量；每个等级图例始终使用默认色板颜色（不因无数据变灰）
    levelList.forEach((level, index) => {
      const weight = level.isQualified
        ? Number(s.qualifiedCategories?.[level.name]?.weight) || 0
        : Number(s.unqualifiedCategories?.[level.name]) || 0;
      const color = defaultLevelColors[index % defaultLevelColors.length];
      const displayValue = weight > 0 ? weight : 0.001;
      pieData.push({
        name: level.name,
        value: displayValue,
        itemStyle: {
          color, // 始终用默认色，图例与扇区一致
        },
        _weight: weight,
      });
    });
  }

  // 无数据时显示占位，避免空白
  if (pieData.length === 0) {
    pieData.push({
      name: '暂无数据',
      value: 1,
      itemStyle: { color: noDataColor },
      _weight: 0,
    });
  }

  // 记录所有图例名称
  allLegendNames = pieData.map(d => d.name);

  const totalWeight = s?.totalWeight ?? 0;
  const totalWeightText = totalWeight > 0 ? `${(totalWeight / 1000).toFixed(1)}` : '0';

  const option = {
    tooltip: {
      trigger: 'item',
      backgroundColor: 'rgba(255, 255, 255, 0.98)',
      borderColor: '#e8e8e8',
      borderWidth: 1,
      padding: [10, 14],
      textStyle: { color: '#262626', fontSize: 13 },
      formatter: (params: any) => {
        const realWeight = params.data?._weight ?? params.value;
        const percent = totalWeight > 0 && realWeight >= 0 ? ((realWeight / totalWeight) * 100).toFixed(2) : '0.00';
        return `
          <div style="font-weight: 600; margin-bottom: 8px; color: #1f2937;">${params.name}</div>
          <div style="display: flex; justify-content: space-between; gap: 20px; font-size: 12px;">
            <span style="color: #6b7280;">重量</span>
            <span style="font-weight: 500; color: #374151;">${Number(realWeight).toFixed(1)} kg</span>
          </div>
          <div style="display: flex; justify-content: space-between; gap: 20px; font-size: 12px; margin-top: 4px;">
            <span style="color: #6b7280;">占比</span>
            <span style="font-weight: 500; color: #374151;">${percent}%</span>
          </div>
        `;
      }
    },
    legend: {
      type: 'plain',
      orient: 'vertical',
      right: 8,
      top: 'middle',
      align: 'left',
      itemGap: 12,
      itemWidth: 12,
      itemHeight: 12,
      icon: 'circle',
      borderRadius: 6,
      textStyle: {
        color: '#4b5563',
        fontSize: 12,
        fontWeight: 500
      },
      formatter: (name: string) => name,
    },
    series: [
      {
        type: 'pie',
        radius: ['48%', '72%'],
        center: ['32%', '50%'],
        avoidLabelOverlap: false,
        itemStyle: {
          borderRadius: 6,
          borderColor: '#fff',
          borderWidth: 2
        },
        label: { show: false },
        emphasis: {
          label: {
            show: true,
            fontSize: 14,
            fontWeight: 600,
            formatter: '{b}\n{d}%',
            color: '#1f2937'
          },
          scale: true,
          scaleSize: 6,
          itemStyle: { shadowBlur: 12, shadowOffsetX: 0, shadowColor: 'rgba(0,0,0,0.15)' }
        },
        labelLine: { show: false },
        data: pieData,
      },
    ],
    title: {
      show: true,
      text: totalWeightText,
      subtext: '总重量 (t)',
      left: '31%',
      top: 'center',
      textAlign: 'center',
      textStyle: {
        fontSize: 22,
        fontWeight: 700,
        color: '#1f2937',
        fontFamily: 'Inter, system-ui, sans-serif'
      },
      subtextStyle: {
        fontSize: 11,
        color: '#9ca3af',
        marginTop: 2
      },
      itemGap: 2
    },
  };

  setChartOptions(option);
}

watch(
  [() => props.summary, () => props.qualifiedColumns, () => props.unqualifiedColumns, () => props.selectedLevel],
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
  padding: 20px 24px 24px;
  box-shadow: 0 1px 3px rgba(0, 0, 0, 0.06);
  height: 100%;
  display: flex;
  flex-direction: column;
  transition: box-shadow 0.2s;
  border: 1px solid rgba(0, 0, 0, 0.04);

  &:hover {
    box-shadow: 0 4px 12px rgba(0, 0, 0, 0.08);
  }
}

.chart-header {
  margin-bottom: 16px;
  padding-bottom: 12px;
  border-bottom: 1px solid #f3f4f6;
}

.chart-title {
  font-size: 15px;
  font-weight: 600;
  color: #1f2937;
  margin: 0;
  display: flex;
  align-items: center;
  letter-spacing: 0.02em;

  &::before {
    content: '';
    width: 4px;
    height: 14px;
    background: linear-gradient(180deg, #3b82f6 0%, #60a5fa 100%);
    border-radius: 2px;
    margin-right: 10px;
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
