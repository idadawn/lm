<template>
  <div class="monthly-dashboard-page">
    <div class="page-header animate-card">
      <div class="header-left">
        <h1 class="page-title">生产驾驶舱</h1>
        <span class="page-subtitle">月度统计数据</span>
      </div>
      <div class="header-right">
        <a-range-picker v-model:value="dateRange" :format="dateFormat" :placeholder="['开始日期', '结束日期']"
          @change="onDateChange" style="margin-right: 12px" />
        <a-button type="primary" :loading="loading" @click="handleRefresh">
          <template #icon>
            <ReloadOutlined />
          </template>
          刷新
        </a-button>
      </div>
    </div>

    <div class="animate-row delay-1">
      <KpiCards ref="kpiCardsRef" :summary="data?.summary" :report-configs="data?.reportConfigs" :loading="loading" />
    </div>

    <!-- 空数据提示 -->
    <a-alert v-if="data && data.summary?.totalWeight === 0 && !loading" message="当前日期范围内暂无数据，请选择其他日期范围" type="info"
      show-icon style="margin-bottom: 16px" />

    <div class="chart-row chart-row-2 animate-row delay-2">
      <div class="chart-col chart-col-3">
        <QualityTrendChart ref="trendChartRef" :data="qualityTrendData" :loading="loading"
          :qualified-columns="data?.qualifiedColumns" :unqualified-columns="data?.unqualifiedColumns"
          :report-configs="data?.reportConfigs" />
      </div>
      <div class="chart-col chart-col-2">
        <QualityDistributionPie ref="distributionPieRef" :summary="distributionSummary" :loading="loading"
          :qualified-columns="data?.qualifiedColumns" :report-configs="data?.reportConfigs" />
      </div>
    </div>

    <div class="chart-row chart-row-3 animate-row delay-3">
      <div class="chart-col chart-col-2">
        <ShiftComparisonRadar ref="radarChartRef" :data="shiftComparisonData" :loading="loading"
          :qualified-columns="data?.qualifiedColumns" :report-configs="data?.reportConfigs" />
      </div>
      <div class="chart-col chart-col-2">
        <UnqualifiedTop5 ref="top5ChartRef" :data="unqualifiedTop5Data" :loading="loading" />
      </div>
      <div class="chart-col chart-col-1">
        <ProductionShiftHeatmap ref="heatmapChartRef" :details="data?.details" :loading="loading" />
      </div>
    </div>

  </div>
</template>

<script setup lang="ts">
defineOptions({ name: 'LabMonthlyDashboard' });
import { ref, onMounted, computed } from 'vue';
import { ReloadOutlined } from '@ant-design/icons-vue';
import dayjs from 'dayjs';
import type { Dayjs } from 'dayjs';
import {
  getMonthlyReport,
  type MonthlyReportResponse,
  type QualityTrend,
  type ShiftComparison,
  type UnqualifiedCategory,
  type SummaryData,
} from '/@/api/lab/monthlyQualityReport';
import type { ReportConfig } from '/@/api/lab/reportConfig';
import { message } from 'ant-design-vue';

import KpiCards from './components/KpiCards.vue';
import QualityTrendChart from './components/QualityTrendChart.vue';
import QualityDistributionPie from './components/QualityDistributionPie.vue';
import ShiftComparisonRadar from './components/ShiftComparisonRadar.vue';
import UnqualifiedTop5 from './components/UnqualifiedTop5.vue';
import ProductionShiftHeatmap from './components/ProductionShiftHeatmap.vue';

const loading = ref(false);
const data = ref<MonthlyReportResponse | null>(null);
const dateFormat = 'YYYY-MM-DD';

const today = dayjs();
const startOfMonth = today.startOf('month');
const dateRange = ref<[Dayjs, Dayjs]>([startOfMonth, today]);

const validDetails = computed(() => {
  return (data.value?.details || []).filter((d: any) => !d?.isSummaryRow && !!d?.prodDate);
});

const reportDisplayConfigs = computed<ReportConfig[]>(() => {
  return (data.value?.reportConfigs || []).filter((c: any) => !!c?.isShowInReport);
});

function getConfigWeightFromRow(row: any, config: ReportConfig): number {
  // 优先从后端已计算的 dynamicStats 中获取
  const stat = row?.dynamicStats?.[config.id];
  if (stat) {
    return Number(stat?.weight) || 0;
  }
  // 回退：通过 levelNames 从 qualifiedCategories/unqualifiedCategories 中计算
  const levels = config?.levelNames || [];
  if (!levels.length) return 0;
  return levels.reduce((sum: number, levelName: string) => {
    const qualifiedWeight = Number(row?.qualifiedCategories?.[levelName]?.weight) || 0;
    const unqualifiedWeight = Number(row?.unqualifiedCategories?.[levelName]) || 0;
    return sum + qualifiedWeight + unqualifiedWeight;
  }, 0);
}

const qualityTrendData = computed<QualityTrend[]>(() => {
  const groups = new Map<string, any[]>();

  validDetails.value.forEach((row: any) => {
    const dateKey = dayjs(row.prodDate).format('YYYY-MM-DD');
    if (!groups.has(dateKey)) groups.set(dateKey, []);
    groups.get(dateKey)!.push(row);
  });

  return Array.from(groups.keys()).sort().map((dateKey) => {
    const rows = groups.get(dateKey) || [];
    const totalWeight = rows.reduce((sum, r) => sum + (Number(r.detectionWeight) || 0), 0);
    const qualifiedWeight = rows.reduce((sum, r) => sum + (Number(r.qualifiedWeight) || 0), 0);
    const qualifiedCategories: Record<string, number> = {};
    const unqualifiedCategories: Record<string, number> = {};
    const dynamicStats: Record<string, number> = {};

    reportDisplayConfigs.value.forEach((config: ReportConfig) => {
      const configWeight = rows.reduce((sum, r) => sum + getConfigWeightFromRow(r, config), 0);
      dynamicStats[config.id] = totalWeight > 0 ? Number(((configWeight / totalWeight) * 100).toFixed(2)) : 0;
    });

    return {
      date: dateKey,
      qualifiedRate: totalWeight > 0 ? Number(((qualifiedWeight / totalWeight) * 100).toFixed(2)) : 0,
      qualifiedCategories,
      unqualifiedCategories,
      classARate: 0,
      classBRate: 0,
      dynamicStats,
    };
  });
});

const shiftComparisonData = computed<ShiftComparison[]>(() => {
  const shiftMap = new Map<string, any[]>();

  validDetails.value.forEach((row: any) => {
    const shiftKey = row.shift || '未知';
    if (!shiftMap.has(shiftKey)) shiftMap.set(shiftKey, []);
    shiftMap.get(shiftKey)!.push(row);
  });

  return Array.from(shiftMap.entries()).map(([shift, rows]) => {
    const totalWeight = rows.reduce((sum, r) => sum + (Number(r.detectionWeight) || 0), 0);
    const qualifiedWeight = rows.reduce((sum, r) => sum + (Number(r.qualifiedWeight) || 0), 0);

    const dynamicStats: Record<string, number> = {};
    reportDisplayConfigs.value.forEach((config: ReportConfig) => {
      const configWeight = rows.reduce((sum, r) => sum + getConfigWeightFromRow(r, config), 0);
      dynamicStats[config.id] = totalWeight > 0 ? Number(((configWeight / totalWeight) * 100).toFixed(2)) : 0;
    });

    return {
      shift,
      totalWeight,
      qualifiedRate: totalWeight > 0 ? Number(((qualifiedWeight / totalWeight) * 100).toFixed(2)) : 0,
      classARate: 0,
      classBRate: 0,
      dynamicStats,
    };
  });
});

const unqualifiedTop5Data = computed<UnqualifiedCategory[]>(() => {
  const totalWeight = validDetails.value.reduce((sum, r: any) => sum + (Number(r.detectionWeight) || 0), 0);
  const unqualifiedLevelNames = new Set((data.value?.unqualifiedColumns || []).map((c: any) => c.name));
  const map = new Map<string, number>();

  reportDisplayConfigs.value.forEach((config: ReportConfig) => {
    const levelNames = (config.levelNames || []).filter((name) => unqualifiedLevelNames.has(name));
    if (!levelNames.length) return;
    let configWeight = 0;
    validDetails.value.forEach((row: any) => {
      levelNames.forEach((levelName) => {
        configWeight += Number(row.unqualifiedCategories?.[levelName]) || 0;
      });
    });
    if (configWeight > 0) {
      map.set(config.name, configWeight);
    }
  });

  return Array.from(map.entries())
    .map(([categoryName, weight]) => ({
      categoryName,
      weight,
      rate: totalWeight > 0 ? Number(((weight / totalWeight) * 100).toFixed(2)) : 0,
    }))
    .sort((a, b) => b.weight - a.weight)
    .slice(0, 5);
});

const distributionSummary = computed<SummaryData>(() => {
  const totalWeight = validDetails.value.reduce((sum, r: any) => sum + (Number(r.detectionWeight) || 0), 0);
  const qualifiedWeight = validDetails.value.reduce((sum, r: any) => sum + (Number(r.qualifiedWeight) || 0), 0);

  const dynamicStats: Record<string, { weight: number; rate: number }> = {};
  reportDisplayConfigs.value.forEach((config: ReportConfig) => {
    const weight = validDetails.value.reduce((sum, r: any) => sum + getConfigWeightFromRow(r, config), 0);
    dynamicStats[config.id] = {
      weight,
      rate: totalWeight > 0 ? Number(((weight / totalWeight) * 100).toFixed(2)) : 0,
    };
  });

  const qualifiedCategories: Record<string, { weight: number; rate: number }> = {};
  const unqualifiedCategories: Record<string, number> = {};

  return {
    totalWeight,
    qualifiedRate: totalWeight > 0 ? Number(((qualifiedWeight / totalWeight) * 100).toFixed(2)) : 0,
    qualifiedCategories,
    qualifiedWeight,
    unqualifiedCategories,
    unqualifiedWeight: 0,
    unqualifiedRate: 0,
    classAWeight: 0,
    classARate: 0,
    classBWeight: 0,
    classBRate: 0,
    dynamicStats,
  };
});

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
    // defHttp 可能返回完整响应对象，需要手动提取 data 字段
    const actualData = (response as any)?.data || response;
    data.value = actualData;
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
  padding: 20px;
  background: #f7f9fc;
  min-height: 100vh;
}

.page-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 24px;
  padding: 16px 24px;
  background: #fff;
  border-radius: 12px;
  box-shadow: 0 4px 12px rgba(0, 0, 0, 0.03);
}

.header-left {
  display: flex;
  flex-direction: column;
  gap: 4px;
}

.page-title {
  font-size: 20px;
  font-weight: 700;
  color: #262626;
  margin: 0;
  letter-spacing: 0.5px;
}

.page-subtitle {
  font-size: 13px;
  color: #8c9eae;
}

.header-right {
  display: flex;
  align-items: center;
}

.chart-row {
  display: grid;
  gap: 20px;
  margin-bottom: 24px;
  grid-template-columns: repeat(5, 1fr);
}

.chart-row-2 {
  grid-template-columns: 3fr 2fr;
}

.chart-row-3 {
  grid-template-columns: 1.5fr 1.5fr 2fr;
}

/* Animations */
@keyframes fadeInUp {
  from {
    opacity: 0;
    transform: translateY(20px);
  }

  to {
    opacity: 1;
    transform: translateY(0);
  }
}

.animate-card,
.animate-row {
  animation: fadeInUp 0.6s cubic-bezier(0.16, 1, 0.3, 1) both;
}

.delay-1 {
  animation-delay: 0.1s;
}

.delay-2 {
  animation-delay: 0.2s;
}

.delay-3 {
  animation-delay: 0.3s;
}

@media (max-width: 1600px) {
  .chart-row-2 {
    grid-template-columns: 1fr 1fr;
  }

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
