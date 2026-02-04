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
  import { ref, onMounted, onUnmounted } from 'vue';
  import { useECharts } from '/@/hooks/web/useECharts';
  import { getThicknessCorrelation, type ScatterData } from '/@/api/lab/dashboard';

  // Props
  const props = defineProps<{
    startDate: string;
    endDate: string;
  }>();

  const chartRef = ref<HTMLDivElement | null>(null);

  // 质量等级颜色配置
  const qualityColors: Record<string, string> = {
    'A级': '#52c41a',
    'B级': '#fadb14',
    '性能不合': '#fa8c16',
    '其他不合': '#f5222d',
    '未判定': '#8c8c8c',
  };

  // 散点数据
  const scatterData = ref<Record<string, [number, number][]>>({
    'A级': [],
    'B级': [],
    '性能不合': [],
    '其他不合': [],
    '未判定': [],
  });

  let chartInstance: any = null;

  // 获取数据
  async function fetchData(start?: string, end?: string) {
    try {
      const startDate = start || props.startDate;
      const endDate = end || props.endDate;
      const data = await getThicknessCorrelation({ startDate, endDate });

      // 按质量等级分组
      const grouped: Record<string, [number, number][]> = {
        'A级': [],
        'B级': [],
        '性能不合': [],
        '其他不合': [],
        '未判定': [],
      };

      data.forEach(item => {
        const level = item.qualityLevel || '未判定';
        if (!grouped[level]) {
          grouped[level] = [];
        }
        grouped[level].push([Number(item.thickness), Number(item.laminationFactor)]);
      });

      scatterData.value = grouped;

      // 更新图表
      if (chartInstance) {
        updateChart();
      }
    } catch (error) {
      console.error('获取厚度-叠片系数关联数据失败:', error);
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

    const series = Object.keys(scatterData.value)
      .filter(key => scatterData.value[key].length > 0)
      .map((key) => ({
        name: key,
        type: 'scatter',
        data: scatterData.value[key],
        symbolSize: 8,
        itemStyle: {
          color: qualityColors[key] || '#8c8c8c',
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

    // 计算数据范围
    const allData = Object.values(scatterData.value).flat();
    const xData = allData.map(d => d[0]);
    const yData = allData.map(d => d[1]);
    const xMin = xData.length > 0 ? Math.min(...xData) : 5.15;
    const xMax = xData.length > 0 ? Math.max(...xData) : 5.28;
    const yMin = yData.length > 0 ? Math.min(...yData) : 88;
    const yMax = yData.length > 0 ? Math.max(...yData) : 92;

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
        min: Math.floor(xMin * 100) / 100 - 0.01,
        max: Math.ceil(xMax * 100) / 100 + 0.01,
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
        min: Math.floor(yMin) - 1,
        max: Math.ceil(yMax) + 1,
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

  function updateChart() {
    if (!chartInstance) return;

    const series = Object.keys(scatterData.value)
      .filter(key => scatterData.value[key].length > 0)
      .map((key) => ({
        name: key,
        type: 'scatter',
        data: scatterData.value[key],
        symbolSize: 8,
        itemStyle: {
          color: qualityColors[key] || '#8c8c8c',
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

    // 计算数据范围
    const allData = Object.values(scatterData.value).flat();
    const xData = allData.map(d => d[0]);
    const yData = allData.map(d => d[1]);
    const xMin = xData.length > 0 ? Math.min(...xData) : 5.15;
    const xMax = xData.length > 0 ? Math.max(...xData) : 5.28;
    const yMin = yData.length > 0 ? Math.min(...yData) : 88;
    const yMax = yData.length > 0 ? Math.max(...yData) : 92;

    const option = {
      xAxis: {
        min: Math.floor(xMin * 100) / 100 - 0.01,
        max: Math.ceil(xMax * 100) / 100 + 0.01,
      },
      yAxis: {
        min: Math.floor(yMin) - 1,
        max: Math.ceil(yMax) + 1,
      },
      series,
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
