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

  // KPI数据
  const qualificationRate = ref(91.2);
  const trendValue = ref(1.5);
  const todayOutput = ref(1256);
  const outputGrowth = ref(3.4);
  const laminationAvg = ref(89.5);
  const targetValue = ref(90.0);
  const warningCount = ref(3);

  // 预警列表
  const warningList = ref([
    { level: 'high', message: '2号机组-厚度超标' },
    { level: 'medium', message: '4号机组-划痕检测' },
    { level: 'high', message: '系统异常-数据延迟' },
  ]);

  // 图表引用
  const gaugeChartRef = ref<HTMLDivElement | null>(null);
  const miniTrendChartRef = ref<HTMLDivElement | null>(null);
  
  let gaugeChart: any = null;
  let miniTrendChart: any = null;

  onMounted(() => {
    initGaugeChart();
    initMiniTrendChart();
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
              color: '#52c41a',
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
              value: qualificationRate.value,
              name: '合格率',
            },
          ],
          detail: { show: false },
        },
      ],
    };
    
    setOptions(option);
  }

  // 初始化迷你趋势图
  function initMiniTrendChart() {
    if (!miniTrendChartRef.value) return;
    
    const { setOptions } = useECharts(miniTrendChartRef);
    
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
        data: ['Mon', 'Tue', 'Wed', 'Thu', 'Fri', 'Sat', 'Sun'],
      },
      yAxis: {
        type: 'value',
        show: false,
        min: 88,
        max: 92,
      },
      series: [
        {
          data: [89.2, 89.8, 89.5, 89.7, 89.9, 89.6, 89.5],
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
