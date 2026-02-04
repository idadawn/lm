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
import { ref, watch, onMounted, type Ref } from 'vue';
import { useECharts } from '/@/hooks/web/useECharts';
import type { QualityTrend, JudgmentLevelColumn } from '/@/api/lab/monthlyQualityReport';
import dayjs from 'dayjs';
import * as echarts from 'echarts';

interface Props {
  data?: QualityTrend[] | null;
  loading?: boolean;
  qualifiedColumns?: JudgmentLevelColumn[];
}

const props = withDefaults(defineProps<Props>(), {
  loading: false,
  qualifiedColumns: () => [],
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

  // 颜色池
  const colors = ['#42e695', '#4facfe', '#ff9a9e', '#fa709a', '#8ec5fc'];

  // 格式化日期
  const dates = props.data.map((item) => {
    const date = typeof item.date === 'number' ? dayjs(item.date) : dayjs(String(item.date));
    return date.format('MM-DD');
  });

  const qualifiedRates = props.data.map((item) => Number(item.qualifiedRate) || 0);

  // 基础系列：合格率
  const legendData = ['合格率'];
  const series: any[] = [
    {
      name: '合格率',
      type: 'line',
      data: qualifiedRates,
      smooth: true,
      showSymbol: false,
      symbol: 'circle',
      symbolSize: 6,
      lineStyle: { color: '#4facfe', width: 3, shadowColor: 'rgba(79, 172, 254, 0.4)', shadowBlur: 10 },
      itemStyle: { color: '#4facfe', borderColor: '#fff', borderWidth: 2 },
      areaStyle: {
        color: new echarts.graphic.LinearGradient(0, 0, 0, 1, [
          { offset: 0, color: 'rgba(79, 172, 254, 0.4)' },
          { offset: 1, color: 'rgba(79, 172, 254, 0.05)' },
        ]),
      },
    },
  ];

  // 动态添加合格等级系列
  if (props.qualifiedColumns && props.qualifiedColumns.length > 0) {
    props.qualifiedColumns.forEach((col, index) => {
      legendData.push(col.name);
      
      const colRates = props.data!.map((item: any) => {
         // 尝试从动态字段获取
        const dynamicField = `class${col.code}Rate`;
        return Number(item[dynamicField]) || 0;
      });

      // 为每个系列分配不同的颜色
      // 跳过第一个颜色(留给合格率，虽然合格率用了固定的 #4facfe，但为了区分，我们从颜色池的其他颜色开始选)
      // 这里简单处理，循环使用颜色池
      const color = colors[index % colors.length];
      
      series.push({
        name: col.name,
        type: 'line',
        data: colRates,
        smooth: true,
        showSymbol: false,
        symbol: 'circle',
        symbolSize: 5,
        lineStyle: { width: 2 }, // 细一点，区分主线
        itemStyle: { color: color },
        // 不需要 areaStyle，避免遮挡
      });
    });
  }

  const option = {
    tooltip: {
      trigger: 'axis',
      backgroundColor: 'rgba(255, 255, 255, 0.95)',
      borderColor: '#f0f2f5',
      textStyle: {
        color: '#262626',
        fontSize: 13,
      },
      extraCssText: 'box-shadow: 0 4px 12px rgba(0, 0, 0, 0.1); border-radius: 8px;',
      axisPointer: {
        type: 'line',
        lineStyle: {
          color: 'rgba(0, 0, 0, 0.2)',
          width: 1,
          type: 'dashed',
        },
      },
      formatter: (params: any[]) => {
        let result = `<div style="margin-bottom: 8px; font-weight: 600; color: #1f1f1f;">${params[0].name}</div>`;
        params.forEach((param) => {
          const color = param.color;
          // 如果是 Series 的颜色是对象（比如渐变），可能需要处理，这里 simplified
          const colorStr = typeof color === 'string' ? color : (color?.colorStops?.[0]?.color || '#4facfe');
          
          result += `
            <div style="display: flex; align-items: center; justify-content: space-between; gap: 20px; margin-bottom: 4px;">
              <div style="display: flex; align-items: center;">
                <span style="display: inline-block; width: 8px; height: 8px; border-radius: 50%; background-color: ${colorStr}; margin-right: 8px;"></span>
                <span style="color: #666;">${param.seriesName}</span>
              </div>
              <span style="font-weight: 500; color: #262626;">${param.value}%</span>
            </div>
          `;
        });
        return result;
      },
    },
    legend: {
      data: legendData,
      top: 0,
      right: 0,
      itemWidth: 10,
      itemHeight: 10,
      textStyle: {
        color: '#666',
        fontSize: 12,
      },
    },
    grid: {
      left: '2%',
      right: '2%',
      bottom: '5%',
      top: '15%',
      containLabel: true,
    },
    xAxis: {
      type: 'category',
      boundaryGap: false,
      data: dates.length > 0 ? dates : [''],
      axisLine: { show: false },
      axisTick: { show: false },
      axisLabel: {
        color: '#8c9eae',
        fontSize: 12,
        margin: 16,
      },
    },
    yAxis: {
      type: 'value',
      max: 100,
      axisLine: { show: false },
      axisTick: { show: false },
      splitLine: {
        lineStyle: { color: '#f5f5f5', type: 'dashed' },
      },
      axisLabel: {
        color: '#8c9eae',
        fontSize: 12,
        formatter: '{value}',
      },
    },
    series,
  };

  setChartOptions(option);
}

watch(
  [() => props.data, () => props.qualifiedColumns],
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
  display: flex;
  align-items: center;
  justify-content: space-between;
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
