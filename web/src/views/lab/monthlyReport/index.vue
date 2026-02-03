<template>
    <div class="monthly-report">
        <!-- 页面标题 -->
        <div class="report-header">
            <div class="header-left">
                <h1 class="page-title">月度质量统计报表</h1>
                <span class="page-subtitle">{{ currentDateRange }}</span>
            </div>
            <div class="header-right">
                <a-button type="primary" @click="handleRefresh">
                    <Icon icon="ant-design:reload-outlined" :size="14" />
                    刷新
                </a-button>
                <a-button @click="handleExport">
                    <Icon icon="ant-design:download-outlined" :size="14" />
                    导出
                </a-button>
            </div>
        </div>

        <!-- 顶部指标卡片 -->
        <SummaryCards :data="summaryData" :loading="loading" />

        <!-- 筛选条件 -->
        <FilterPanel v-model:dateRange="dateRange" v-model:shift="filterShift" v-model:shiftNo="filterShiftNo"
            v-model:productSpecCode="filterProductSpecCode" @search="handleSearch" @reset="handleReset" />

        <!-- 主体内容区域 -->
        <div class="report-content">
            <!-- 左侧明细表格 -->
            <div class="content-left">
                <DetailTable :data="detailData" :loading="loading" :unqualified-columns="unqualifiedColumns" />
            </div>

            <!-- 右侧班组统计和图表 -->
            <div class="content-right">
                <ShiftGroupPanel :data="shiftGroupData" :loading="loading" />

                <div class="charts-section">
                    <QualityTrendChart :data="qualityTrendData" :loading="loading" />
                    <UnqualifiedCategoryChart :data="unqualifiedCategoryData" :loading="loading" />
                    <ShiftComparisonChart :data="shiftComparisonData" :loading="loading" />
                </div>
            </div>
        </div>
    </div>
</template>

<script lang="ts" setup>
import { ref, computed, onMounted } from 'vue';
import dayjs, { Dayjs } from 'dayjs';
import { Icon } from '/@/components/Icon';
import { useMessage } from '/@/hooks/web/useMessage';
import {
    getMonthlyReport,
    type MonthlyReportQueryParams,
    type MonthlyReportResponse,
    type SummaryData,
    type DetailRow,
    type ShiftGroupRow,
    type QualityTrend,
    type UnqualifiedCategory,
    type ShiftComparison,
    type JudgmentLevelColumn,
} from '/@/api/lab/monthlyQualityReport';

// 导入组件
import SummaryCards from './components/SummaryCards.vue';
import FilterPanel from './components/FilterPanel.vue';
import DetailTable from './components/DetailTable.vue';
import ShiftGroupPanel from './components/ShiftGroupPanel.vue';
import QualityTrendChart from './components/QualityTrendChart.vue';
import UnqualifiedCategoryChart from './components/UnqualifiedCategoryChart.vue';
import ShiftComparisonChart from './components/ShiftComparisonChart.vue';

const { createMessage } = useMessage();

// 加载状态
const loading = ref(false);

// 筛选条件
type RangeValue = [Dayjs, Dayjs];
const dateRange = ref<RangeValue>([
    dayjs().startOf('month'),
    dayjs().endOf('month'),
]);
const filterShift = ref<string>('');
const filterShiftNo = ref<string>('');
const filterProductSpecCode = ref<string>('');

// 报表数据
const summaryData = ref<SummaryData>({
    totalWeight: 0,
    qualifiedRate: 0,
    classAWeight: 0,
    classARate: 0,
    classBWeight: 0,
    classBRate: 0,
    unqualifiedWeight: 0,
    unqualifiedRate: 0,
});
const detailData = ref<DetailRow[]>([]);
const shiftGroupData = ref<ShiftGroupRow[]>([]);
const qualityTrendData = ref<QualityTrend[]>([]);
const unqualifiedCategoryData = ref<UnqualifiedCategory[]>([]);
const shiftComparisonData = ref<ShiftComparison[]>([]);
const unqualifiedColumns = ref<JudgmentLevelColumn[]>([]);

// 当前日期范围显示
const currentDateRange = computed(() => {
    const [start, end] = dateRange.value;
    return `${start.format('YYYY年MM月DD日')} - ${end.format('YYYY年MM月DD日')}`;
});

// 加载数据
async function loadData() {
    loading.value = true;
    try {
        const params: MonthlyReportQueryParams = {
            startDate: dateRange.value[0].format('YYYY-MM-DD'),
            endDate: dateRange.value[1].format('YYYY-MM-DD'),
            shift: filterShift.value || undefined,
            shiftNo: filterShiftNo.value || undefined,
            productSpecCode: filterProductSpecCode.value || undefined,
        };

        const response: MonthlyReportResponse = await getMonthlyReport(params);

        summaryData.value = response.summary || {
            totalWeight: 0,
            qualifiedRate: 0,
            classAWeight: 0,
            classARate: 0,
            classBWeight: 0,
            classBRate: 0,
            unqualifiedWeight: 0,
            unqualifiedRate: 0,
        };
        detailData.value = response.details || [];
        shiftGroupData.value = response.shiftGroups || [];
        qualityTrendData.value = response.qualityTrends || [];
        unqualifiedCategoryData.value = response.unqualifiedCategories || [];
        shiftComparisonData.value = response.shiftComparisons || [];
        unqualifiedColumns.value = response.unqualifiedColumns || [];
    } catch (error) {
        console.error('加载报表数据失败:', error);
        createMessage.error('加载报表数据失败');
    } finally {
        loading.value = false;
    }
}

// 刷新
function handleRefresh() {
    loadData();
}

// 搜索
function handleSearch() {
    loadData();
}

// 重置筛选条件
function handleReset() {
    dateRange.value = [dayjs().startOf('month'), dayjs().endOf('month')];
    filterShift.value = '';
    filterShiftNo.value = '';
    filterProductSpecCode.value = '';
    loadData();
}

// 导出
function handleExport() {
    createMessage.info('导出功能开发中...');
}

onMounted(() => {
    loadData();
});
</script>

<style lang="less" scoped>
.monthly-report {
    padding: 16px;
    background: linear-gradient(135deg, #f5f7fa 0%, #e8ecf1 100%);
    min-height: 100%;
}

// 页面头部
.report-header {
    display: flex;
    justify-content: space-between;
    align-items: center;
    margin-bottom: 16px;
    padding: 16px 20px;
    background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
    border-radius: 12px;
    box-shadow: 0 4px 20px rgba(102, 126, 234, 0.3);

    .header-left {
        .page-title {
            font-size: 22px;
            font-weight: 600;
            color: #fff;
            margin: 0 0 4px 0;
            text-shadow: 0 2px 4px rgba(0, 0, 0, 0.1);
        }

        .page-subtitle {
            font-size: 13px;
            color: rgba(255, 255, 255, 0.85);
        }
    }

    .header-right {
        display: flex;
        gap: 12px;

        :deep(.ant-btn) {
            display: flex;
            align-items: center;
            gap: 6px;
            border-radius: 8px;

            &.ant-btn-primary {
                background: rgba(255, 255, 255, 0.2);
                border: 1px solid rgba(255, 255, 255, 0.3);
                color: #fff;

                &:hover {
                    background: rgba(255, 255, 255, 0.3);
                }
            }

            &:not(.ant-btn-primary) {
                background: rgba(255, 255, 255, 0.9);
                border: none;
                color: #667eea;

                &:hover {
                    background: #fff;
                }
            }
        }
    }
}

// 主体内容
.report-content {
    display: flex;
    gap: 16px;

    @media (max-width: 1400px) {
        flex-direction: column;
    }
}

// 左侧明细表格
.content-left {
    flex: 1;
    min-width: 0;
}

// 右侧班组统计和图表
.content-right {
    width: 500px;
    flex-shrink: 0;
    display: flex;
    flex-direction: column;
    gap: 16px;

    @media (max-width: 1400px) {
        width: 100%;
    }
}

// 图表区域
.charts-section {
    display: flex;
    flex-direction: column;
    gap: 16px;
}
</style>
