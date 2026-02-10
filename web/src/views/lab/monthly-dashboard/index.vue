<template>
  <div class="monthly-dashboard-page">
    <div class="page-header animate-card">
      <div class="header-left">
        <h1 class="page-title">生产驾驶舱</h1>
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
      <KpiCards ref="kpiCardsRef" :summary="data?.summary" :report-configs="data?.reportConfigs"
        :daily-production="dailyProduction" :loading="loading" />
    </div>

    <!-- 空数据提示 -->
    <a-alert v-if="data && data.summary?.totalWeight === 0 && !loading" message="当前日期范围内暂无数据，请选择其他日期范围" type="info"
      show-icon style="margin-bottom: 16px" />

    <div class="chart-row chart-row-2 animate-row delay-2">
      <div class="chart-col chart-col-lamination">
        <LaminationTrendChart ref="laminationChartRef" :data="laminationTrendData" :loading="loading" />
      </div>
      <div class="chart-col chart-col-pie">
        <QualityDistributionPie ref="distributionPieRef" :summary="distributionSummary" :loading="loading"
          :qualified-columns="data?.qualifiedColumns" :unqualified-columns="data?.unqualifiedColumns" />
      </div>
    </div>

    <div class="chart-row chart-row-3 animate-row delay-3">
      <div class="chart-col chart-col-2">
        <ShiftComparisonChart :data="shiftComparisonData" :loading="loading" :report-configs="data?.reportConfigs" />
      </div>
      <div class="chart-col chart-col-2">
        <QualityTrendChart :data="(data?.qualityTrends ?? [])" :loading="loading" :report-configs="data?.reportConfigs" />
      </div>
    </div>

    <div class="chart-row chart-row-bottom animate-row delay-4">
      <div class="chart-col chart-col-lamination">
        <ThicknessCorrelationScatter ref="thicknessCorrelationRef" :data="thicknessCorrelationData" :loading="loading" />
      </div>
      <div class="chart-col chart-col-top5">
        <UnqualifiedTop5 ref="top5ChartRef" :data="unqualifiedTop5Data" :loading="loading" />
      </div>
      <div class="chart-col chart-col-radar">
        <ShiftComparisonRadar ref="radarChartRef" :data="shiftComparisonData" :loading="loading"
          :qualified-columns="data?.qualifiedColumns" :report-configs="data?.reportConfigs" />
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
defineOptions({ name: 'LabMonthlyDashboard' });
import { ref, onMounted, computed } from 'vue';
import { ReloadOutlined, CloseCircleOutlined } from '@ant-design/icons-vue';
import dayjs from 'dayjs';
import type { Dayjs } from 'dayjs';
import {
  getMonthlyReport,
  type MonthlyReportResponse,
  type ShiftComparison,
  type UnqualifiedCategory,
  type SummaryData,
} from '/@/api/lab/monthlyQualityReport';
import type { ReportConfig } from '/@/api/lab/reportConfig';
import {
  getLaminationTrend,
  getThicknessCorrelation,
  getDailyProduction,
  type LaminationTrendData,
  type ScatterData,
  type DailyProductionDto,
} from '/@/api/lab/dashboard';
import { message } from 'ant-design-vue';

import KpiCards from './components/KpiCards.vue';
import LaminationTrendChart from './components/LaminationTrendChart.vue';
import QualityDistributionPie from './components/QualityDistributionPie.vue';
import ShiftComparisonRadar from './components/ShiftComparisonRadar.vue';
import UnqualifiedTop5 from './components/UnqualifiedTop5.vue';
import ShiftComparisonChart from '../monthlyReport/components/ShiftComparisonChart.vue';
import QualityTrendChart from '../monthlyReport/components/QualityTrendChart.vue';
import ThicknessCorrelationScatter from './components/ThicknessCorrelationScatter.vue';

const loading = ref(false);
const data = ref<MonthlyReportResponse | null>(null);
const laminationTrendData = ref<LaminationTrendData[]>([]);
const thicknessCorrelationData = ref<ScatterData[]>([]);
const dailyProduction = ref<DailyProductionDto | null>(null);
const selectedQualityLevel = ref<string | null>(null);
const dateFormat = 'YYYY-MM-DD';

const today = dayjs();
const startOfMonth = today.startOf('month');
const dateRange = ref<[Dayjs, Dayjs]>([startOfMonth, today]);

const validDetails = computed(() => {
  return (data.value?.details || []).filter((d: any) => !d?.isSummaryRow && !!d?.prodDate);
});

// 下钻筛选: 当选中某个质量等级时，只展示包含该等级数据的行
const filteredDetails = computed(() => {
  const level = selectedQualityLevel.value;
  if (!level) return validDetails.value;

  return validDetails.value.filter((row: any) => {
    // 检查合格分类中是否有该等级
    const qualifiedWeight = Number(row.qualifiedCategories?.[level]?.weight) || 0;
    if (qualifiedWeight > 0) return true;
    // 检查不合格分类中是否有该等级
    const unqualifiedWeight = Number(row.unqualifiedCategories?.[level]) || 0;
    if (unqualifiedWeight > 0) return true;
    return false;
  });
});

// 下钻事件处理
function onQualityLevelSelect(levelName: string | null) {
  selectedQualityLevel.value = levelName;
}

function clearQualityFilter() {
  selectedQualityLevel.value = null;
}

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

// 不合格原因：从 reportConfig name="不合格" 的 levelNames 获取等级，再从明细 unqualifiedCategories 聚合，取 Top5
const unqualifiedTop5Data = computed<UnqualifiedCategory[]>(() => {
  const unqualifiedConfig = (data.value?.reportConfigs || []).find((c: ReportConfig) => c.name === '不合格');
  const levelNames = unqualifiedConfig?.levelNames ?? [];
  if (!levelNames.length) return [];

  const totalWeight = validDetails.value.reduce((sum, r: any) => sum + (Number(r.detectionWeight) || 0), 0);
  const totalUnqWeight = validDetails.value.reduce((sum, r: any) => sum + (Number(r.unqualifiedWeight) || 0), 0);

  const list = levelNames.map((categoryName) => {
    const weight = validDetails.value.reduce(
      (s, r: any) => s + (Number(r.unqualifiedCategories?.[categoryName]) || 0),
      0
    );
    return {
      categoryName,
      weight,
      rate: totalUnqWeight > 0 ? Number(((weight / totalUnqWeight) * 100).toFixed(2)) : 0,
    };
  });

  return list
    .filter((item) => item.weight > 0)
    .sort((a, b) => b.weight - a.weight)
    .slice(0, 5);
});

const distributionSummary = computed<SummaryData>(() => {
  const totalWeight = validDetails.value.reduce((sum, r: any) => sum + (Number(r.detectionWeight) || 0), 0);
  const qualifiedWeight = validDetails.value.reduce((sum, r: any) => sum + (Number(r.qualifiedWeight) || 0), 0);

  // 从所有明细行汇总各合格等级的重量
  const qualifiedCategories: Record<string, { weight: number; rate: number }> = {};
  const allQualifiedNames = new Set<string>();
  validDetails.value.forEach((row: any) => {
    if (row.qualifiedCategories) {
      Object.keys(row.qualifiedCategories).forEach(name => allQualifiedNames.add(name));
    }
  });
  allQualifiedNames.forEach(name => {
    const weight = validDetails.value.reduce((sum, r: any) => {
      return sum + (Number(r.qualifiedCategories?.[name]?.weight) || 0);
    }, 0);
    qualifiedCategories[name] = {
      weight,
      rate: totalWeight > 0 ? Number(((weight / totalWeight) * 100).toFixed(2)) : 0,
    };
  });

  // 从所有明细行汇总各不合格等级的重量
  const unqualifiedCategories: Record<string, number> = {};
  const allUnqualifiedNames = new Set<string>();
  validDetails.value.forEach((row: any) => {
    if (row.unqualifiedCategories) {
      Object.keys(row.unqualifiedCategories).forEach(name => allUnqualifiedNames.add(name));
    }
  });
  allUnqualifiedNames.forEach(name => {
    unqualifiedCategories[name] = validDetails.value.reduce((sum, r: any) => {
      return sum + (Number(r.unqualifiedCategories?.[name]) || 0);
    }, 0);
  });

  const unqualifiedWeight = Object.values(unqualifiedCategories).reduce((sum, w) => sum + w, 0);

  return {
    totalWeight,
    qualifiedRate: totalWeight > 0 ? Number(((qualifiedWeight / totalWeight) * 100).toFixed(2)) : 0,
    qualifiedCategories,
    qualifiedWeight,
    unqualifiedCategories,
    unqualifiedWeight,
    unqualifiedRate: totalWeight > 0 ? Number(((unqualifiedWeight / totalWeight) * 100).toFixed(2)) : 0,
  };
});

async function fetchData() {
  if (!dateRange.value || dateRange.value.length !== 2) {
    message.warning('请选择日期范围');
    return;
  }

  loading.value = true;
  try {
    const start = dateRange.value[0].format(dateFormat);
    const end = dateRange.value[1].format(dateFormat);
    const [reportRes, laminationRes, thicknessCorrelationRes] = await Promise.all([
      getMonthlyReport({ startDate: start, endDate: end }),
      getLaminationTrend({ startDate: start, endDate: end }).catch(() => []),
      getThicknessCorrelation({ startDate: start, endDate: end }).catch(() => []),
    ]);
    const actualData = (reportRes as any)?.data ?? reportRes;
    data.value = actualData;
    const laminationRaw = (laminationRes as any)?.data ?? laminationRes;
    laminationTrendData.value = Array.isArray(laminationRaw) ? laminationRaw : [];
    const thicknessRaw = (thicknessCorrelationRes as any)?.data ?? thicknessCorrelationRes;
    thicknessCorrelationData.value = Array.isArray(thicknessRaw) ? thicknessRaw : [];
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

async function fetchDailyProduction() {
  try {
    const res = (await getDailyProduction()) as any;
    dailyProduction.value = res?.data ?? res ?? null;
  } catch {
    dailyProduction.value = null;
  }
}

function handleRefresh() {
  fetchData();
  fetchDailyProduction();
}

onMounted(() => {
  fetchData();
  fetchDailyProduction();
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
  grid-template-columns: 2fr 2fr;
}

.chart-row-3 {
  grid-template-columns: 1fr 2fr;
  align-items: stretch;

  .chart-col {
    min-width: 0;
    display: flex;
  }

  :deep(.chart-card) {
    flex: 1;
    display: flex;
    flex-direction: column;
    min-height: 400px;
    padding: 20px;
    border-radius: 12px;
    box-shadow: 0 4px 20px rgba(0, 0, 0, 0.05);
    border: 1px solid rgba(0, 0, 0, 0.02);
    transition: box-shadow 0.2s;

    &:hover {
      box-shadow: 0 8px 24px rgba(0, 0, 0, 0.08);
    }
  }

  :deep(.chart-header) {
    flex-shrink: 0;
    margin-bottom: 12px;
  }

  :deep(.chart-title),
  :deep(.chart-title h4) {
    font-size: 15px;
    font-weight: 600;
    color: #2d3748;
    margin: 0;
  }

  :deep(.chart-body) {
    flex: 1;
    min-height: 320px;
    display: flex;
    flex-direction: column;
  }

  :deep(.chart-container) {
    flex: 1;
    min-height: 320px;
    height: 100% !important;
    width: 100%;
  }
}

.chart-row-bottom {
  grid-template-columns: 2fr 1fr 1fr;
  align-items: stretch;

  .chart-col {
    min-width: 0;
    display: flex;
  }

  :deep(.chart-card) {
    flex: 1;
    display: flex;
    flex-direction: column;
    min-height: 400px;
    padding: 20px;
    border-radius: 12px;
    box-shadow: 0 4px 20px rgba(0, 0, 0, 0.05);
    border: 1px solid rgba(0, 0, 0, 0.02);
    transition: box-shadow 0.2s;

    &:hover {
      box-shadow: 0 8px 24px rgba(0, 0, 0, 0.08);
    }
  }

  :deep(.chart-header) {
    flex-shrink: 0;
    margin-bottom: 12px;
  }

  :deep(.chart-body) {
    flex: 1;
    min-height: 320px;
    display: flex;
    flex-direction: column;
  }

  :deep(.chart-container) {
    flex: 1;
    min-height: 320px;
    height: 100% !important;
    width: 100%;
  }
}

.chart-col-lamination,
.chart-col-top5,
.chart-col-radar {
  min-width: 0;
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

.delay-4 {
  animation-delay: 0.4s;
}

@media (max-width: 1600px) {
  .chart-row-2 {
    grid-template-columns: 1fr 1fr;
  }

.chart-row-3 {
  grid-template-columns: 1fr 2fr;
}

  .chart-row-bottom {
    grid-template-columns: 2fr 1fr 1fr;
  }
}

@media (max-width: 1200px) {
  .chart-row-2 {
    grid-template-columns: 1fr;
  }

  .chart-row-3 {
    grid-template-columns: 1fr;
  }

  .chart-row-bottom {
    grid-template-columns: 1fr;
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
