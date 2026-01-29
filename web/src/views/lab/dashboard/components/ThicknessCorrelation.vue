<template>
  <div class="chart-card">
    <div class="chart-header">
      <h3 class="chart-title">厚度-叠片系数关联</h3>
      <div class="legend-custom">
        <div class="legend-item">
          <span class="legend-dot" style="background: #52c41a;"></span>
          <span>A级</span>
        </div>
        <div class="legend-item">
          <span class="legend-dot" style="background: #fadb14;"></span>
          <span>B级</span>
        </div>
        <div class="legend-item">
          <span class="legend-dot" style="background: #fa8c16;"></span>
          <span>性能不合</span>
        </div>
        <div class="legend-item">
          <span class="legend-dot" style="background: #f5222d;"></span>
          <span>其他不合</span>
        </div>
      </div>
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

  // 质量等级颜色配置
  const qualityColors: Record<string, string> = {
    'A级': '#52c41a',
    'B级': '#fadb14',
    '性能不合': '#fa8c16',
    '其他不合': '#f5222d',
  };

  // 生成模拟散点数据
  const generateScatterData = () => {
    const data: Record<string, [number, number][]> = {
      'A级': [],
      'B级': [],
      '性能不合': [],
      '其他不合': [],
    };

    // A级品 - 集中在最佳工艺窗口
    for (let i = 0; i < 80; i++) {
      const thickness = 5.22 + Math.random() * 0.03;
      const lamination = 90 + Math.random() * 1.5;
      data['A级'].push([parseFloat(thickness.toFixed(3)), parseFloat(lamination.toFixed(2))]);
    }

    // B级品 - 分布稍广
    for (let i = 0; i < 20; i++) {
      const thickness = 5.20 + Math.random() * 0.06;
      const lamination = 89.5 + Math.random() * 1.5;
      data['B级'].push([parseFloat(thickness.toFixed(3)), parseFloat(lamination.toFixed(2))]);
    }

    // 性能不合 - 叠片系数较低
    for (let i = 0; i < 15; i++) {
      const thickness = 5.18 + Math.random() * 0.08;
      const lamination = 88.5 + Math.random() * 1.2;
      data['性能不合'].push([parseFloat(thickness.toFixed(3)), parseFloat(lamination.toFixed(2))]);
    }

    // 其他不合 - 厚度异常
    for (let i = 0; i < 10; i++) {
      const thickness = 5.15 + Math.random() * 0.12;
      const lamination = 88.8 + Math.random() * 1.5;
      data['其他不合'].push([parseFloat(thickness.toFixed(3)), parseFloat(lamination.toFixed(2))]);
    }

    return data;
  };

  const scatterData = generateScatterData();

  onMounted(() => {
    initChart();
  });

  function initChart() {
    if (!chartRef.value) return;

    const { setOptions } = useECharts(chartRef);

    const series = Object.keys(scatterData).map((key) => ({
      name: key,
      type: 'scatter',
      data: scatterData[key],
      symbolSize: 8,
      itemStyle: {
        color: qualityColors[key],
        opacity: 0.8,
      },
      emphasis: {
        focus: 'series',
        itemStyle: {
          opacity: 1,
          borderWidth: 2,
          borderColor: '#fff',
        },
      },
    }));

    const option = {
      tooltip: {
        trigger: 'item',
        formatter: (params: any) => {
          return `${params.seriesName}<br/>厚度: ${params.value[0]}<br/>叠片系数: ${params.value[1]}%`;
        },
      },
      grid: {
        left: '8%',
        right: '5%',
        top: '10%',
        bottom: '15%',
      },
      xAxis: {
        type: 'value',
        name: '厚度',
        nameLocation: 'middle',
        nameGap: 30,
        min: 5.15,
        max: 5.28,
        nameTextStyle: {
          color: '#666',
          fontSize: 12,
        },
        splitLine: {
          lineStyle: {
            color: '#f0f0f0',
            type: 'dashed',
          },
        },
        axisLabel: {
          color: '#666',
          fontSize: 11,
          formatter: (value: number) => value.toFixed(2),
        },
      },
      yAxis: {
        type: 'value',
        name: '叠片系数',
        nameLocation: 'middle',
        nameGap: 35,
        min: 88,
        max: 91.5,
        nameTextStyle: {
          color: '#666',
          fontSize: 12,
        },
        splitLine: {
          lineStyle: {
            color: '#f0f0f0',
            type: 'dashed',
          },
        },
        axisLabel: {
          color: '#666',
          fontSize: 11,
          formatter: '{value}%',
        },
      },
      series,
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
    display: flex;
    justify-content: space-between;
    align-items: center;
    margin-bottom: 16px;
    flex-wrap: wrap;
    gap: 8px;

    .chart-title {
      font-size: 16px;
      font-weight: 600;
      color: #262626;
      margin: 0;
    }
  }

  .legend-custom {
    display: flex;
    gap: 16px;
    flex-wrap: wrap;

    .legend-item {
      display: flex;
      align-items: center;
      gap: 6px;
      font-size: 12px;
      color: #666;

      .legend-dot {
        width: 10px;
        height: 10px;
        border-radius: 50%;
      }
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
