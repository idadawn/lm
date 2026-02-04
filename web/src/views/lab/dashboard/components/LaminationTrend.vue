<template>
  <div class="chart-card">
    <div class="chart-header">
      <h3 class="chart-title">叠片系数趋势</h3>
    </div>
    <div class="chart-body">
      <div ref="chartRef" class="chart-container"></div>
    </div>
  </div>
</template>

<script lang="ts" setup>
  import { ref, onMounted, onUnmounted } from 'vue';
  import { useECharts } from '/@/hooks/web/useECharts';
  import { getLaminationTrend, type LaminationTrendData } from '/@/api/lab/dashboard';
  import dayjs from 'dayjs';

  // Props
  const props = defineProps<{
    startDate: string;
    endDate: string;
  }>();

  const chartRef = ref<HTMLDivElement | null>(null);

  // 趋势数据
  const dates = ref<string[]>([]);
  const avgValues = ref<number[]>([]);
  const upperBound = ref<number[]>([]);
  const lowerBound = ref<number[]>([]);

  let chartInstance: any = null;

  // 获取数据
  async function fetchData(start?: string, end?: string) {
    try {
      const startDate = start || props.startDate;
      const endDate = end || props.endDate;
      const data = await getLaminationTrend({ startDate, endDate });

      // 更新数据
      dates.value = data.map(item => {
        const date = dayjs(item.date);
        return date.format('M/D');
      });
      avgValues.value = data.map(item => Number(item.value));
      upperBound.value = data.map(item => Number(item.max));
      lowerBound.value = data.map(item => Number(item.min));

      // 更新图表
      if (chartInstance) {
        updateChart();
      }
    } catch (error) {
      console.error('获取叠片系数趋势数据失败:', error);
    }
  }

  // 暴露给父组件的方法
  defineExpose({ fetchData });

  onMounted(() => {
    initChart();
    fetchData();
  });

  onUnmounted(() => {
    if (chartInstance) {
      chartInstance.dispose();
    }
  });

  function initChart() {
    if (!chartRef.value) return;

    const { setOptions, echarts } = useECharts(chartRef);
    chartInstance = echarts;

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
          let result = params[0].name + '<br/>';
          params.forEach((param) => {
            if (param.seriesName !== '置信区间') {
              result += `${param.marker} ${param.seriesName}: ${param.value}%<br/>`;
            }
          });
          return result;
        },
      },
      grid: {
        left: '3%',
        right: '4%',
        bottom: '3%',
        top: '10%',
        containLabel: true,
      },
      xAxis: {
        type: 'category',
        boundaryGap: false,
        data: dates.value.length > 0 ? dates.value : [''],
        axisLine: {
          lineStyle: {
            color: '#e8e8e8',
          },
        },
        axisLabel: {
          color: '#666',
          fontSize: 12,
        },
      },
      yAxis: {
        type: 'value',
        axisLine: { show: false },
        axisTick: { show: false },
        splitLine: {
          lineStyle: {
            color: '#f0f0f0',
            type: 'dashed',
          },
        },
        axisLabel: {
          color: '#666',
          fontSize: 12,
          formatter: '{value}%',
        },
      },
      series: [
        // 置信区间背景
        {
          name: '置信区间',
          type: 'line',
          data: upperBound.value,
          lineStyle: { opacity: 0 },
          stack: 'confidence-band',
          symbol: 'none',
          areaStyle: {
            color: 'rgba(24, 144, 255, 0.1)',
          },
        },
        {
          name: '置信区间',
          type: 'line',
          data: lowerBound.value.map((val, idx) => upperBound.value[idx] - val),
          lineStyle: { opacity: 0 },
          stack: 'confidence-band',
          symbol: 'none',
          areaStyle: {
            color: 'rgba(24, 144, 255, 0.1)',
          },
        },
        // 平均值线
        {
          name: '叠片系数均值',
          type: 'line',
          data: avgValues.value,
          smooth: true,
          symbol: 'circle',
          symbolSize: 8,
          lineStyle: {
            color: '#1890ff',
            width: 3,
          },
          itemStyle: {
            color: '#1890ff',
            borderWidth: 2,
            borderColor: '#fff',
          },
          emphasis: {
            scale: 1.5,
          },
        },
        // 目标线
        {
          name: '目标值',
          type: 'line',
          data: dates.value.map(() => 90),
          lineStyle: {
            color: '#52c41a',
            width: 2,
            type: 'dashed',
          },
          symbol: 'none',
          silent: true,
        },
      ],
    };

    setOptions(option);
  }

  function updateChart() {
    if (!chartInstance) return;

    const option = {
      xAxis: {
        data: dates.value,
      },
      series: [
        { data: upperBound.value },
        {
          data: lowerBound.value.map((val, idx) => upperBound.value[idx] - val),
        },
        { data: avgValues.value },
        { data: dates.value.map(() => 90) },
      ],
    };

    chartInstance.setOption(option);
  }
</script>

<style lang="less" scoped>
  .chart-card {
    background: #fff;
    border-radius: 8px;
    padding: 16px;
    height: 100%;
    box-shadow: 0 2px 8px rgba(0, 0, 0, 0.06);
  }

  .chart-header {
    margin-bottom: 16px;

    .chart-title {
      font-size: 16px;
      font-weight: 600;
      color: #262626;
      margin: 0;
    }
  }

  .chart-body {
    height: calc(100% - 40px);
  }

  .chart-container {
    width: 100%;
    height: 100%;
    min-height: 250px;
  }
</style>
