<template>
  <div class="chart-card">
    <div class="chart-header">
      <h3 class="chart-title">生产热力图</h3>
      <div class="chart-subtitle">合格率分布（按星期/小时）</div>
    </div>
    <div class="chart-body">
      <div ref="chartRef" class="chart-container"></div>
    </div>
  </div>
</template>

<script lang="ts" setup>
  import { ref, onMounted, onUnmounted } from 'vue';
  import { useECharts } from '/@/hooks/web/useECharts';
  import { getProductionHeatmap, type HeatmapData } from '/@/api/lab/dashboard';

  // Props
  const props = defineProps<{
    startDate: string;
    endDate: string;
  }>();

  const chartRef = ref<HTMLDivElement | null>(null);

  // 时间标签
  const hours = ['0:00', '1:00', '2:00', '3:00', '4:00', '5:00', '6:00', '7:00', '8:00', '9:00', '10:00', '11:00',
                 '12:00', '13:00', '14:00', '15:00', '16:00', '17:00', '18:00', '19:00', '20:00', '21:00', '22:00', '23:00'];
  const days = ['周一', '周二', '周三', '周四', '周五', '周六', '周日'];

  // 热力图数据
  const heatmapData = ref<[number, number, number][]>([]);
  let chartInstance: any = null;

  // 获取数据
  async function fetchData(start?: string, end?: string) {
    try {
      const startDate = start || props.startDate;
      const endDate = end || props.endDate;
      const data = await getProductionHeatmap({ startDate, endDate });

      // 转换数据格式为 [dayOfWeek, hour, value]
      heatmapData.value = data
        .filter(item => item.count > 0) // 只显示有数据的点
        .map(item => [item.dayOfWeek, item.hour, Number(item.value)]);

      // 更新图表
      if (chartInstance) {
        updateChart();
      }
    } catch (error) {
      console.error('获取生产热力图数据失败:', error);
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

    const data = heatmapData.value.length > 0 ? heatmapData.value : [[0, 0, 0]];

    const option = {
      tooltip: {
        position: 'top',
        formatter: (params: any) => {
          const dayIndex = params.value[0];
          const hourIndex = params.value[1];
          const value = params.value[2];
          return `${days[dayIndex]} ${hours[hourIndex]}<br/>合格率: ${value}%`;
        },
      },
      grid: {
        left: '12%',
        right: '5%',
        top: '5%',
        bottom: '15%',
      },
      xAxis: {
        type: 'category',
        data: days,
        splitArea: {
          show: true,
          areaStyle: {
            color: ['rgba(250,250,250,0.3)', 'rgba(250,250,250,0.3)'],
          },
        },
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
        type: 'category',
        data: hours,
        name: '小时',
        nameLocation: 'middle',
        nameGap: 35,
        nameTextStyle: {
          color: '#666',
          fontSize: 12,
        },
        splitArea: {
          show: true,
          areaStyle: {
            color: ['rgba(250,250,250,0.3)', 'rgba(250,250,250,0.3)'],
          },
        },
        axisLine: {
          lineStyle: {
            color: '#e8e8e8',
          },
        },
        axisLabel: {
          color: '#666',
          fontSize: 11,
        },
      },
      visualMap: {
        min: 80,
        max: 100,
        calculable: true,
        orient: 'horizontal',
        left: 'center',
        bottom: '2%',
        inRange: {
          color: ['#ff4d4f', '#faad14', '#fadb14', '#52c41a', '#237804'],
        },
        text: ['高', '低'],
        textStyle: {
          color: '#666',
        },
      },
      series: [
        {
          name: '合格率',
          type: 'heatmap',
          data: data,
          label: {
            show: false,
          },
          emphasis: {
            itemStyle: {
              shadowBlur: 10,
              shadowColor: 'rgba(0, 0, 0, 0.5)',
            },
          },
          itemStyle: {
            borderRadius: 2,
            borderWidth: 1,
            borderColor: '#fff',
          },
        },
      ],
    };

    setOptions(option);
  }

  function updateChart() {
    if (!chartInstance) return;

    const data = heatmapData.value.length > 0 ? heatmapData.value : [[0, 0, 0]];

    const option = {
      series: [
        {
          data: data,
        },
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
    margin-bottom: 12px;

    .chart-title {
      font-size: 16px;
      font-weight: 600;
      color: #262626;
      margin: 0 0 4px 0;
    }

    .chart-subtitle {
      font-size: 12px;
      color: #999;
    }
  }

  .chart-body {
    height: calc(100% - 60px);
  }

  .chart-container {
    width: 100%;
    height: 100%;
    min-height: 280px;
  }
</style>
