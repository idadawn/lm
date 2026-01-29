<template>
  <div class="lab-dashboard">
    <!-- 页面标题 -->
    <div class="dashboard-header">
      <div class="header-left">
        <h1 class="page-title">生产驾驶舱</h1>
        <span class="page-subtitle">实时监控生产质量与设备状态</span>
      </div>
      <div class="header-right">
        <a-range-picker 
          v-model:value="dateRange" 
          :presets="rangePresets"
          @change="onDateRangeChange"
        />
        <a-button type="primary" class="refresh-btn" @click="refreshData">
          <Icon icon="ant-design:reload-outlined" :size="14" />
          刷新
        </a-button>
      </div>
    </div>

    <!-- KPI卡片区 -->
    <KpiCards ref="kpiCardsRef" />

    <!-- 图表区域 - 第一行 -->
    <div class="chart-row">
      <div class="chart-col-3">
        <QualityDistribution ref="qualityRef" />
      </div>
      <div class="chart-col-3">
        <LaminationTrend ref="laminationRef" />
      </div>
      <div class="chart-col-3">
        <DefectTop5 ref="defectRef" />
      </div>
    </div>

    <!-- 图表区域 - 第二行 -->
    <div class="chart-row">
      <div class="chart-col-2">
        <ProductionHeatmap ref="heatmapRef" />
      </div>
      <div class="chart-col-2">
        <ThicknessCorrelation ref="correlationRef" />
      </div>
    </div>

    <!-- AI助手 -->
    <AiAssistant />
  </div>
</template>

<script lang="ts" setup>
  import { ref } from 'vue';
  import dayjs, { Dayjs } from 'dayjs';
  import { Icon } from '/@/components/Icon';
  import { useMessage } from '/@/hooks/web/useMessage';
  
  // 导入组件
  import KpiCards from './components/KpiCards.vue';
  import QualityDistribution from './components/QualityDistribution.vue';
  import LaminationTrend from './components/LaminationTrend.vue';
  import DefectTop5 from './components/DefectTop5.vue';
  import ProductionHeatmap from './components/ProductionHeatmap.vue';
  import ThicknessCorrelation from './components/ThicknessCorrelation.vue';
  import AiAssistant from './components/AiAssistant.vue';

  const { createMessage } = useMessage();

  // 组件引用
  const kpiCardsRef = ref();
  const qualityRef = ref();
  const laminationRef = ref();
  const defectRef = ref();
  const heatmapRef = ref();
  const correlationRef = ref();

  // 日期范围
  type RangeValue = [Dayjs, Dayjs];
  const dateRange = ref<RangeValue>([dayjs().subtract(7, 'day'), dayjs()]);

  // 预设日期范围
  const rangePresets = ref([
    { label: '今天', value: [dayjs(), dayjs()] },
    { label: '近3天', value: [dayjs().add(-3, 'd'), dayjs()] },
    { label: '近7天', value: [dayjs().add(-7, 'd'), dayjs()] },
    { label: '近30天', value: [dayjs().add(-30, 'd'), dayjs()] },
  ]);

  // 日期范围变化
  function onDateRangeChange(dates: RangeValue | null) {
    if (dates) {
      createMessage.success(`日期范围: ${dates[0].format('YYYY-MM-DD')} 至 ${dates[1].format('YYYY-MM-DD')}`);
      refreshData();
    }
  }

  // 刷新数据
  function refreshData() {
    createMessage.loading('数据刷新中...', 1);
    
    // 模拟数据刷新
    setTimeout(() => {
      createMessage.success('数据已更新');
    }, 1000);
  }
</script>

<style lang="less" scoped>
  .lab-dashboard {
    padding: 16px;
    background: #f0f2f5;
    min-height: 100%;
  }

  // 页面头部
  .dashboard-header {
    display: flex;
    justify-content: space-between;
    align-items: center;
    margin-bottom: 16px;
    padding: 0 4px;

    @media (max-width: 768px) {
      flex-direction: column;
      align-items: flex-start;
      gap: 12px;
    }

    .header-left {
      .page-title {
        font-size: 20px;
        font-weight: 600;
        color: #262626;
        margin: 0 0 4px 0;
      }

      .page-subtitle {
        font-size: 13px;
        color: #8c8c8c;
      }
    }

    .header-right {
      display: flex;
      align-items: center;
      gap: 12px;

      .refresh-btn {
        display: flex;
        align-items: center;
        gap: 6px;
      }
    }
  }

  // 图表行布局
  .chart-row {
    display: grid;
    gap: 16px;
    margin-bottom: 16px;

    &:last-child {
      margin-bottom: 0;
    }
  }

  // 三列布局
  .chart-col-3 {
    min-height: 320px;
  }

  .chart-row {
    &:first-of-type {
      grid-template-columns: repeat(3, 1fr);

      @media (max-width: 1200px) {
        grid-template-columns: repeat(2, 1fr);
      }

      @media (max-width: 768px) {
        grid-template-columns: 1fr;
      }
    }

    &:last-of-type {
      grid-template-columns: repeat(2, 1fr);

      @media (max-width: 768px) {
        grid-template-columns: 1fr;
      }
    }
  }

  .chart-col-2 {
    min-height: 360px;
  }

  // 确保图表卡片高度一致
  :deep(.chart-card) {
    height: 100%;
    display: flex;
    flex-direction: column;

    .chart-body {
      flex: 1;
    }
  }
</style>
