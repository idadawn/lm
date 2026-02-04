<template>
  <div class="monthly-dashboard-page">
    <div class="page-header">
      <div class="header-left">
        <h1 class="page-title">生产驾驶舱</h1>
        <span class="page-subtitle">月度统计数据</span>
      </div>
      <div class="header-right">
        <a-range-picker
          v-model:value="dateRange"
          :format="dateFormat"
          :placeholder="['开始日期', '结束日期']"
          @change="onDateChange"
          style="margin-right: 12px"
        />
        <a-button type="primary" :loading="loading" @click="handleRefresh">
          <template #icon>
            <ReloadOutlined />
          </template>
          刷新
        </a-button>
      </div>
    </div>

    <KpiCards ref="kpiCardsRef" :summary="data?.summary" :loading="loading" />

    <!-- 空数据提示 -->
    <a-alert
      v-if="data && data.summary?.totalWeight === 0 && !loading"
      message="当前日期范围内暂无数据，请选择其他日期范围"
      type="info"
      show-icon
      style="margin-bottom: 16px"
    />

    <div class="chart-row chart-row-2">
      <div class="chart-col chart-col-3">
        <QualityTrendChart ref="trendChartRef" :data="data?.qualityTrends" :loading="loading" />
      </div>
      <div class="chart-col chart-col-2">
        <QualityDistributionPie ref="distributionPieRef" :summary="data?.summary" :loading="loading" />
      </div>
    </div>

    <div class="chart-row chart-row-3">
      <div class="chart-col chart-col-2">
        <ShiftComparisonRadar ref="radarChartRef" :data="data?.shiftComparisons" :loading="loading" />
      </div>
      <div class="chart-col chart-col-2">
        <UnqualifiedTop5 ref="top5ChartRef" :data="data?.unqualifiedCategoryStats" :loading="loading" />
      </div>
      <div class="chart-col chart-col-1">
        <ProductionShiftHeatmap ref="heatmapChartRef" :details="data?.details" :loading="loading" />
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, onMounted } from 'vue';
import { ReloadOutlined } from '@ant-design/icons-vue';
import dayjs from 'dayjs';
import type { Dayjs } from 'dayjs';
import { getMonthlyReport, type MonthlyReportResponse } from '/@/api/lab/monthlyQualityReport';
import { message } from 'ant-design-vue';

import KpiCards from './components/KpiCards.vue';
import QualityTrendChart from './components/QualityTrendChart.vue';
import QualityDistributionPie from './components/QualityDistributionPie.vue';
import ShiftComparisonRadar from './components/ShiftComparisonRadar.vue';
import UnqualifiedTop5 from './components/UnqualifiedTop5.vue';
import ProductionShiftHeatmap from './components/ProductionShiftHeatmap.vue';

const kpiCardsRef = ref<InstanceType<typeof KpiCards>>();
const trendChartRef = ref<InstanceType<typeof QualityTrendChart>>();
const distributionPieRef = ref<InstanceType<typeof QualityDistributionPie>>();
const radarChartRef = ref<InstanceType<typeof ShiftComparisonRadar>>();
const top5ChartRef = ref<InstanceType<typeof UnqualifiedTop5>>();
const heatmapChartRef = ref<InstanceType<typeof ProductionShiftHeatmap>>();

const loading = ref(false);
const data = ref<MonthlyReportResponse | null>(null);
const dateFormat = 'YYYY-MM-DD';

const today = dayjs();
const startOfMonth = today.startOf('month');
const dateRange = ref<[Dayjs, Dayjs]>([startOfMonth, today]);

async function fetchData() {
  if (!dateRange.value || dateRange.value.length !== 2) {
    message.warning('请选择日期范围');
    return;
  }

  loading.value = true;
  try {
    const response = await getMonthlyReport({
      startDate: dateRange.value[0].format(dateFormat),
      endDate: dateRange.value[1].format(dateFormat),
    });
    data.value = response;
    console.log('API响应数据:', response);
  } catch (error) {
    console.error('获取月度报表数据失败:', error);
    message.error('获取数据失败，请稍后重试');
  } finally {
    loading.value = false;
  }
}

function onDateChange() {
  fetchData();
}

function handleRefresh() {
  fetchData();
}

onMounted(() => {
  fetchData();
});
</script>

<style lang="less" scoped>
.monthly-dashboard-page {
  padding: 16px;
  background: #f0f2f5;
  min-height: 100vh;
}

.page-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 16px;
  padding: 16px 20px;
  background: #fff;
  border-radius: 8px;
  box-shadow: 0 2px 8px rgba(0, 0, 0, 0.06);
}

.header-left {
  display: flex;
  flex-direction: column;
  gap: 4px;
}

.page-title {
  font-size: 20px;
  font-weight: 600;
  color: #262626;
  margin: 0;
}

.page-subtitle {
  font-size: 13px;
  color: #8c8c8c;
}

.header-right {
  display: flex;
  align-items: center;
}

.chart-row {
  display: grid;
  gap: 16px;
  margin-bottom: 16px;
  grid-template-columns: repeat(5, 1fr);
}

.chart-row-2 {
  grid-template-columns: 3fr 2fr;
}

.chart-row-3 {
  grid-template-columns: 1.8fr 1.8fr 1.4fr;
}

@media (max-width: 1600px) {
  .chart-row-3 {
    grid-template-columns: repeat(2, 1fr);
  }
  .chart-row-3 .chart-col:last-child {
    grid-column: span 2;
  }
}

@media (max-width: 1200px) {
  .chart-row-2 {
    grid-template-columns: 1fr;
  }
  .chart-row-3 {
    grid-template-columns: 1fr;
  }
  .chart-row-3 .chart-col:last-child {
    grid-column: span 1;
  }
}

@media (max-width: 768px) {
  .page-header {
    flex-direction: column;
    gap: 12px;
    align-items: stretch;
  }

  .header-right {
    flex-direction: column;
    gap: 8px;
  }

  .header-right :deep(.ant-picker) {
    width: 100%;
  }
}
</style>
