<template>
  <div class="kpi-cards-container">
    <div class="kpi-grid">
      <!-- 合格率卡片 -->
      <div class="kpi-card">
        <div class="kpi-content">
          <div class="gauge-wrapper">
            <div ref="gaugeChartRef" class="gauge-chart"></div>
            <div class="gauge-label">
              <div class="gauge-value">{{ qualificationRate }}%</div>
              <div class="gauge-title">合格率</div>
            </div>
          </div>
          <div class="kpi-trend">
            <div class="trend-up">
              <Icon icon="ant-design:rise-outlined" :size="20" color="#52c41a" />
              <span class="trend-value">+{{ trendValue }}%</span>
            </div>
          </div>
        </div>
      </div>

      <!-- 今日产量卡片 -->
      <div class="kpi-card">
        <div class="kpi-content">
          <div class="kpi-info">
            <div class="kpi-title">今日产量</div>
            <div class="kpi-main-value">
              <CountTo :startVal="0" :endVal="todayOutput" :duration="2000" separator="," />
              <span class="unit">卷</span>
            </div>
            <div class="kpi-sub-info">
              <Icon icon="ant-design:arrow-up-outlined" :size="14" color="#1890ff" />
              <span class="sub-text">较昨日</span>
              <span class="sub-value">+{{ outputGrowth }}%</span>
            </div>
          </div>
          <div class="kpi-icon blue">
            <Icon icon="ant-design:bar-chart-outlined" :size="40" color="#fff" />
          </div>
        </div>
      </div>

      <!-- 叠片系数均值卡片 -->
      <div class="kpi-card">
        <div class="kpi-content">
          <div class="kpi-info">
            <div class="kpi-title">叠片系数均值</div>
            <div class="kpi-main-value">
              <CountTo :startVal="0" :endVal="laminationAvg" :duration="2000" :decimals="1" />
              <span class="unit">%</span>
            </div>
            <div class="mini-chart">
              <div ref="miniTrendChartRef" class="mini-trend-chart"></div>
            </div>
            <div class="kpi-target">
              <span class="target-label">目标</span>
              <span class="target-value">{{ targetValue }}%</span>
            </div>
          </div>
        </div>
      </div>

      <!-- 预警数卡片 -->
      <div class="kpi-card warning">
        <div class="kpi-content">
          <div class="kpi-info">
            <div class="kpi-title warning-title">
              <span>预警数</span>
              <CountTo :startVal="0" :endVal="warningCount" :duration="1500" class="warning-count" />
            </div>
            <div class="warning-list">
              <div v-for="(item, index) in warningList" :key="index" class="warning-item">
                <span class="warning-dot" :class="item.level"></span>
                <span class="warning-text">{{ item.message }}</span>
              </div>
            </div>
          </div>
          <div class="kpi-icon orange">
            <Icon icon="ant-design:warning-outlined" :size="40" color="#fff" />
          </div>
        </div>
      </div>
    </div>
  </div>
</template>

<script lang="ts" setup>
  import { ref, onMounted, onUnmounted } from 'vue';
  import { useECharts } from '/@/hooks/web/useECharts';
  import { Icon } from '/@/components/Icon';
  import { CountTo } from '/@/components/CountTo';
  import { getDashboardKpi, type DashboardKpiDto } from '/@/api/lab/dashboard';

  // Props
  const props = defineProps<{
    startDate: string;
    endDate: string;
  }>();

  // KPI数据
  const qualificationRate = ref(0);
  const trendValue = ref(0);
  const todayOutput = ref(0);
  const outputGrowth = ref(0);
  const laminationAvg = ref(0);
  const targetValue = ref(90.0);
  const warningCount = ref(0);

  // 预警列表
  const warningList = ref<Array<{ level: string; message: string }>>([]);

  // 图表引用
  const gaugeChartRef = ref<HTMLDivElement | null>(null);
  const miniTrendChartRef = ref<HTMLDivElement | null>(null);

  let gaugeChart: any = null;
  let miniTrendChart: any = null;
  let laminationTrendData: number[] = [];

  // 获取数据
  async function fetchData(start?: string, end?: string) {
    try {
      const startDate = start || props.startDate;
      const endDate = end || props.endDate;
      const data = await getDashboardKpi({ startDate, endDate });

      // 更新KPI数据
      qualificationRate.value = data.qualifiedRate;
      todayOutput.value = Math.round(data.totalWeight / 1000); // 转换为吨
      laminationAvg.value = data.laminationFactorAvg;
      laminationTrendData = data.laminationFactorTrend || [];
      warningCount.value = data.warnings.length;

      // 计算趋势
      if (laminationTrendData.length >= 2) {
        const lastValue = laminationTrendData[laminationTrendData.length - 1];
        const prevValue = laminationTrendData[laminationTrendData.length - 2];
        trendValue.value = Math.round((lastValue - prevValue) * 10) / 10;
      }

      // 计算产量增长率（模拟）
      outputGrowth.value = 0;

      // 转换预警数据格式
      warningList.value = data.warnings.slice(0, 3).map(w => ({
        level: w.level === 'error' ? 'high' : w.level === 'warning' ? 'medium' : 'low',
        message: w.message
      }));

      // 更新图表
      if (gaugeChart) updateGaugeChart();
      if (miniTrendChart) updateMiniTrendChart();
    } catch (error) {
      console.error('获取KPI数据失败:', error);
    }
  }

  // 暴露给父组件的方法
  defineExpose({ fetchData });

  onMounted(() => {
    initGaugeChart();
    initMiniTrendChart();
    fetchData();
  });

  onUnmounted(() => {
    if (gaugeChart) {
      gaugeChart.dispose();
    }
    if (miniTrendChart) {
      miniTrendChart.dispose();
    }
  });

  // 初始化仪表盘图表
  function initGaugeChart() {
    if (!gaugeChartRef.value) return;

    const { setOptions, echarts } = useECharts(gaugeChartRef);
    gaugeChart = echarts;

    const option = {
      series: [
        {
          type: 'gauge',
          startAngle: 90,
          endAngle: -270,
          radius: '85%',
          pointer: { show: false },
          progress: {
            show: true,
            overlap: false,
            roundCap: true,
            clip: false,
            itemStyle: {
              color: qualificationRate.value >= 90 ? '#52c41a' : '#faad14',
            },
          },
          axisLine: {
            lineStyle: {
              width: 12,
              color: [[1, '#e8e8e8']],
            },
          },
          splitLine: { show: false },
          axisTick: { show: false },
          axisLabel: { show: false },
          data: [
            {
              value: qualificationRate.value || 0,
              name: '合格率',
            },
          ],
          detail: { show: false },
        },
      ],
    };

    setOptions(option);
  }

  // 更新仪表盘图表
  function updateGaugeChart() {
    if (!gaugeChart) return;

    const option = {
      series: [
        {
          progress: {
            itemStyle: {
              color: qualificationRate.value >= 90 ? '#52c41a' : '#faad14',
            },
          },
          data: [
            {
              value: qualificationRate.value,
            },
          ],
        },
      ],
    };

    gaugeChart.setOption(option);
  }

  // 初始化迷你趋势图
  function initMiniTrendChart() {
    if (!miniTrendChartRef.value) return;

    const { setOptions, echarts } = useECharts(miniTrendChartRef);
    miniTrendChart = echarts;

    const option = {
      grid: {
        left: 0,
        right: 0,
        top: 5,
        bottom: 5,
      },
      xAxis: {
        type: 'category',
        show: false,
        data: laminationTrendData.length > 0
          ? laminationTrendData.map((_, i) => i)
          : ['Mon', 'Tue', 'Wed', 'Thu', 'Fri', 'Sat', 'Sun'],
      },
      yAxis: {
        type: 'value',
        show: false,
        min: (data) => {
          const min = Math.min(...data);
          return Math.floor(min - 1);
        },
        max: (data) => {
          const max = Math.max(...data);
          return Math.ceil(max + 1);
        },
      },
      series: [
        {
          data: laminationTrendData.length > 0 ? laminationTrendData : [89.5],
          type: 'line',
          smooth: true,
          symbol: 'none',
          lineStyle: {
            color: '#1890ff',
            width: 2,
          },
          areaStyle: {
            color: {
              type: 'linear',
              x: 0,
              y: 0,
              x2: 0,
              y2: 1,
              colorStops: [
                { offset: 0, color: 'rgba(24, 144, 255, 0.3)' },
                { offset: 1, color: 'rgba(24, 144, 255, 0.05)' },
              ],
            },
          },
        },
      ],
    };

    setOptions(option);
  }

  // 更新迷你趋势图
  function updateMiniTrendChart() {
    if (!miniTrendChart) return;

    const option = {
      xAxis: {
        data: laminationTrendData.length > 0
          ? laminationTrendData.map((_, i) => i)
          : ['Mon', 'Tue', 'Wed', 'Thu', 'Fri', 'Sat', 'Sun'],
      },
      yAxis: {
        min: (data) => {
          const min = Math.min(...laminationTrendData);
          return min > 0 ? Math.floor(min - 1) : 88;
        },
        max: (data) => {
          const max = Math.max(...laminationTrendData);
          return max > 0 ? Math.ceil(max + 1) : 92;
        },
      },
      series: [
        {
          data: laminationTrendData.length > 0 ? laminationTrendData : [89.5],
        },
      ],
    };

    miniTrendChart.setOption(option);
  }
</script>

<style lang="less" scoped>
  .kpi-cards-container {
    width: 100%;
    margin-bottom: 16px;
  }

  .kpi-grid {
    display: grid;
    grid-template-columns: repeat(4, 1fr);
    gap: 16px;

    @media (max-width: 1200px) {
      grid-template-columns: repeat(2, 1fr);
    }

    @media (max-width: 768px) {
      grid-template-columns: 1fr;
    }
  }

  .kpi-card {
    background: #fff;
    border-radius: 8px;
    padding: 20px;
    box-shadow: 0 2px 8px rgba(0, 0, 0, 0.06);
    transition: all 0.3s;

    &:hover {
      box-shadow: 0 4px 16px rgba(0, 0, 0, 0.1);
    }

    &.warning {
      background: linear-gradient(135deg, #fff7e6 0%, #fff 100%);
      border: 1px solid #ffd591;
    }
  }

  .kpi-content {
    display: flex;
    align-items: center;
    justify-content: space-between;
    height: 100%;
  }

  // 合格率卡片样式
  .gauge-wrapper {
    position: relative;
    width: 120px;
    height: 120px;
    flex-shrink: 0;
  }

  .gauge-chart {
    width: 100%;
    height: 100%;
  }

  .gauge-label {
    position: absolute;
    top: 50%;
    left: 50%;
    transform: translate(-50%, -50%);
    text-align: center;
  }

  .gauge-value {
    font-size: 28px;
    font-weight: 700;
    color: #52c41a;
    line-height: 1;
  }

  .gauge-title {
    font-size: 14px;
    color: #666;
    margin-top: 4px;
  }

  .kpi-trend {
    flex: 1;
    display: flex;
    justify-content: center;
    align-items: center;
  }

  .trend-up {
    display: flex;
    align-items: center;
    gap: 4px;

    .trend-value {
      color: #52c41a;
      font-size: 16px;
      font-weight: 600;
    }
  }

  // KPI信息样式
  .kpi-info {
    flex: 1;
  }

  .kpi-title {
    font-size: 14px;
    color: #666;
    margin-bottom: 8px;

    &.warning-title {
      display: flex;
      align-items: center;
      gap: 8px;
      font-size: 16px;
      font-weight: 600;
      color: #fa8c16;

      .warning-count {
        font-size: 24px;
        font-weight: 700;
      }
    }
  }

  .kpi-main-value {
    font-size: 36px;
    font-weight: 700;
    color: #262626;
    line-height: 1.2;

    .unit {
      font-size: 16px;
      font-weight: 400;
      color: #999;
      margin-left: 4px;
    }
  }

  .kpi-sub-info {
    display: flex;
    align-items: center;
    gap: 4px;
    margin-top: 8px;
    font-size: 13px;

    .sub-text {
      color: #999;
    }

    .sub-value {
      color: #1890ff;
      font-weight: 500;
    }
  }

  .mini-chart {
    margin-top: 12px;
    height: 50px;
  }

  .mini-trend-chart {
    width: 100%;
    height: 100%;
  }

  .kpi-target {
    display: flex;
    justify-content: flex-end;
    align-items: center;
    gap: 8px;
    margin-top: 8px;
    font-size: 12px;

    .target-label {
      color: #999;
    }

    .target-value {
      color: #666;
    }
  }

  // KPI图标样式
  .kpi-icon {
    width: 60px;
    height: 60px;
    border-radius: 12px;
    display: flex;
    align-items: center;
    justify-content: center;
    flex-shrink: 0;

    &.blue {
      background: linear-gradient(135deg, #1890ff 0%, #36cfc9 100%);
    }

    &.orange {
      background: linear-gradient(135deg, #fa8c16 0%, #ffc53d 100%);
    }
  }

  // 预警列表样式
  .warning-list {
    margin-top: 12px;
  }

  .warning-item {
    display: flex;
    align-items: center;
    gap: 8px;
    padding: 6px 0;
    font-size: 13px;
  }

  .warning-dot {
    width: 8px;
    height: 8px;
    border-radius: 50%;
    flex-shrink: 0;

    &.high {
      background: #ff4d4f;
    }

    &.medium {
      background: #faad14;
    }

    &.low {
      background: #52c41a;
    }
  }

  .warning-text {
    color: #595959;
  }
</style>
