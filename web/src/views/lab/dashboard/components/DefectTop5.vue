<template>
  <div class="chart-card">
    <div class="chart-header">
      <h3 class="chart-title">缺陷Top5</h3>
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

  // 缺陷数据
  const defectData = [
    { name: '划痕', value: 320 },
    { name: '麻点', value: 256 },
    { name: '毛边', value: 180 },
    { name: '亮线', value: 110 },
    { name: '网眼', value: 74 },
  ];

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
          type: 'shadow',
        },
        formatter: (params: any[]) => {
          const data = params[0];
          return `${data.name}<br/>缺陷次数: ${data.value}`;
        },
      },
      grid: {
        left: '3%',
        right: '12%',
        bottom: '3%',
        top: '5%',
        containLabel: true,
      },
      xAxis: {
        type: 'value',
        axisLine: { show: false },
        axisTick: { show: false },
        splitLine: {
          lineStyle: {
            color: '#f0f0f0',
          },
        },
        axisLabel: {
          color: '#999',
          fontSize: 11,
        },
      },
      yAxis: {
        type: 'category',
        data: defectData.map(item => item.name).reverse(),
        axisLine: { show: false },
        axisTick: { show: false },
        axisLabel: {
          color: '#666',
          fontSize: 13,
        },
      },
      series: [
        {
          name: '缺陷次数',
          type: 'bar',
          data: defectData.map(item => item.value).reverse(),
          barWidth: 16,
          itemStyle: {
            borderRadius: [0, 8, 8, 0],
            color: {
              type: 'linear',
              x: 0,
              y: 0,
              x2: 1,
              y2: 0,
              colorStops: [
                { offset: 0, color: '#91cc75' },
                { offset: 1, color: '#91cc75' },
              ],
            },
          },
          label: {
            show: true,
            position: 'right',
            formatter: '{c}',
            fontSize: 12,
            color: '#666',
          },
          emphasis: {
            itemStyle: {
              color: {
                type: 'linear',
                x: 0,
                y: 0,
                x2: 1,
                y2: 0,
                colorStops: [
                  { offset: 0, color: '#73b453' },
                  { offset: 1, color: '#73b453' },
                ],
              },
            },
          },
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
