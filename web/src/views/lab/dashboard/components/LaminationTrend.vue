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
  import { ref, onMounted } from 'vue';
  import { useECharts } from '/@/hooks/web/useECharts';

  const chartRef = ref<HTMLDivElement | null>(null);

  // 模拟7天数据
  const dates = ['5/21', '5/22', '5/23', '5/24', '5/25', '5/26', '5/27'];
  const avgValues = [89.3, 89.8, 89.5, 89.7, 89.9, 89.6, 89.8];
  const upperBound = [90.0, 90.4, 90.2, 90.3, 90.5, 90.2, 90.4];
  const lowerBound = [88.6, 89.2, 88.8, 89.1, 89.3, 89.0, 89.2];

  onMounted(() => {
    initChart();
  });

  function initChart() {
    if (!chartRef.value) return;

    const { setOptions } = useECharts(chartRef);

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
        data: dates,
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
        min: 88,
        max: 91,
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
          data: upperBound,
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
          data: lowerBound.map((val, idx) => upperBound[idx] - val),
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
          data: avgValues,
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
          data: dates.map(() => 90),
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
