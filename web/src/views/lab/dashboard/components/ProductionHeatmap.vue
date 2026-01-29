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
  import { ref, onMounted } from 'vue';
  import { useECharts } from '/@/hooks/web/useECharts';

  const chartRef = ref<HTMLDivElement | null>(null);

  // 时间标签
  const hours = ['0:00', '3:00', '6:00', '9:00', '11:00', '15:00', '18:00', '21:00', '23:00'];
  const days = ['Mon', 'Tue', 'Wed', 'Thu', 'Fri', 'Sat', 'Sun'];

  // 生成模拟热力图数据（合格率 85-98%）
  const generateHeatmapData = () => {
    const data: [number, number, number][] = [];
    for (let i = 0; i < days.length; i++) {
      for (let j = 0; j < hours.length; j++) {
        // 模拟不同时间段的质量波动
        let baseRate = 92;
        // 夜班质量略低
        if (j >= 6 || j <= 1) baseRate -= 3;
        // 周末质量略低
        if (i >= 5) baseRate -= 2;
        // 添加随机波动
        const rate = baseRate + Math.random() * 8 - 4;
        data.push([i, j, Math.max(85, Math.min(98, Math.round(rate * 10) / 10))]);
      }
    }
    return data;
  };

  const heatmapData = generateHeatmapData();

  onMounted(() => {
    initChart();
  });

  function initChart() {
    if (!chartRef.value) return;

    const { setOptions } = useECharts(chartRef);

    const option = {
      tooltip: {
        position: 'top',
        formatter: (params: any) => {
          return `${days[params.value[0]]} ${hours[params.value[1]]}<br/>合格率: ${params.value[2]}%`;
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
        min: 85,
        max: 98,
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
          data: heatmapData,
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
