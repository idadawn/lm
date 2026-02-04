<template>
  <div class="chart-card">
    <div class="chart-header">
      <h3 class="chart-title">质量判定分布</h3>
    </div>
    <div class="chart-body">
      <div ref="chartRef" class="chart-container"></div>
      <div class="chart-legend">
        <div class="total-info">
          <div class="total-label">总计</div>
          <div class="total-value">{{ totalCount }}卷</div>
        </div>
      </div>
    </div>
  </div>
</template>

<script lang="ts" setup>
  import { ref, onMounted, onUnmounted } from 'vue';
  import { useECharts } from '/@/hooks/web/useECharts';
  import { getQualityDistribution, type QualityDistributionDto } from '/@/api/lab/dashboard';

  // Props
  const props = defineProps<{
    startDate: string;
    endDate: string;
  }>();

  const chartRef = ref<HTMLDivElement | null>(null);
  const totalCount = ref(0);

  // 质量分布数据
  const qualityData = ref<Array<{ name: string; value: number; percentage: number; color: string }>>([]);
  let chartInstance: any = null;

  // 获取数据
  async function fetchData(start?: string, end?: string) {
    try {
      const startDate = start || props.startDate;
      const endDate = end || props.endDate;
      const data = await getQualityDistribution({ startDate, endDate });

      // 更新数据
      qualityData.value = data.map(item => ({
        name: item.category,
        value: item.count,
        percentage: item.rate,
        color: item.color || '#8c8c8c',
      }));

      totalCount.value = data.reduce((sum, item) => sum + item.count, 0);

      // 更新图表
      if (chartInstance) {
        updateChart();
      }
    } catch (error) {
      console.error('获取质量分布数据失败:', error);
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
        trigger: 'item',
        formatter: (params: any) => {
          return `${params.name}<br/>数量: ${params.value}卷<br/>占比: ${params.percent}%`;
        },
      },
      legend: {
        orient: 'vertical',
        right: '5%',
        top: 'center',
        itemGap: 16,
        textStyle: {
          fontSize: 13,
          color: '#666',
        },
        formatter: (name: string) => {
          const item = qualityData.value.find(d => d.name === name);
          return `{name|${name}} {percent|${item?.percentage || 0}%}`;
        },
        textStyle: {
          rich: {
            name: {
              fontSize: 13,
              color: '#666',
              width: 70,
            },
            percent: {
              fontSize: 13,
              color: '#999',
            },
          },
        },
      },
      series: [
        {
          name: '质量判定',
          type: 'pie',
          radius: ['55%', '75%'],
          center: ['35%', '50%'],
          avoidLabelOverlap: false,
          itemStyle: {
            borderRadius: 4,
            borderColor: '#fff',
            borderWidth: 2,
          },
          label: {
            show: true,
            position: 'outside',
            formatter: '{c}卷\n{d}%',
            fontSize: 11,
            color: '#666',
          },
          labelLine: {
            show: true,
            length: 10,
            length2: 10,
          },
          emphasis: {
            label: {
              show: true,
              fontSize: 14,
              fontWeight: 'bold',
            },
            itemStyle: {
              shadowBlur: 10,
              shadowOffsetX: 0,
              shadowColor: 'rgba(0, 0, 0, 0.2)',
            },
          },
          data: qualityData.value.map(item => ({
            name: item.name,
            value: item.value,
            itemStyle: { color: item.color },
          })),
        },
      ],
    };

    setOptions(option);
  }

  function updateChart() {
    if (!chartInstance) return;

    const option = {
      legend: {
        formatter: (name: string) => {
          const item = qualityData.value.find(d => d.name === name);
          return `{name|${name}} {percent|${item?.percentage || 0}%}`;
        },
      },
      series: [
        {
          data: qualityData.value.map(item => ({
            name: item.name,
            value: item.value,
            itemStyle: { color: item.color },
          })),
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
    margin-bottom: 16px;

    .chart-title {
      font-size: 16px;
      font-weight: 600;
      color: #262626;
      margin: 0;
    }
  }

  .chart-body {
    display: flex;
    align-items: center;
    height: calc(100% - 40px);
    position: relative;
  }

  .chart-container {
    flex: 1;
    height: 100%;
    min-height: 250px;
  }

  .chart-legend {
    position: absolute;
    left: 35%;
    top: 50%;
    transform: translate(-50%, -50%);
    text-align: center;
    pointer-events: none;

    .total-label {
      font-size: 13px;
      color: #999;
      margin-bottom: 4px;
    }

    .total-value {
      font-size: 18px;
      font-weight: 700;
      color: #262626;
    }
  }
</style>
