<template>
    <div class="detail-table-container" :class="{ 'detail-table-fullscreen': isFullscreen }">
        <div class="table-header">
            <h3 class="table-title">
                <Icon icon="ant-design:table-outlined" :size="18" />
                质量检测明细
            </h3>
            <a-button type="text" size="small" class="fullscreen-btn" @click="toggleFullscreen">
                <template #icon>
                    <FullscreenOutlined v-if="!isFullscreen" />
                    <FullscreenExitOutlined v-else />
                </template>
                {{ isFullscreen ? '退出全屏' : '全屏' }}
            </a-button>
        </div>

        <a-table :columns="columns" :data-source="data" :loading="loading" :pagination="false"
            :scroll="{ y: tableScrollY }" size="small" bordered :row-class-name="getRowClassName">
            <template #bodyCell="{ column, record }">
                <!-- 生产日期 -->
                <template v-if="column.key === 'prodDate'">
                    <span v-if="!record.isSummaryRow" :title="formatDate(record.prodDate)">{{ formatDate(record.prodDate) }}</span>
                    <span v-else class="summary-label">合计</span>
                </template>

                <!-- 带宽、班次、炉号：窄列 + 悬停显示全文 -->
                <template v-else-if="column.key === 'productSpecCode'">
                    <span :title="record.productSpecCode">{{ record.productSpecCode ?? '-' }}</span>
                </template>
                <template v-else-if="column.key === 'shift'">
                    <span :title="record.shift">{{ record.shift ?? '-' }}</span>
                </template>
                <template v-else-if="column.key === 'shiftNo'">
                    <span :title="record.shiftNo">{{ record.shiftNo ?? '-' }}</span>
                </template>

                <!-- 合格率 -->
                <template v-else-if="column.key === 'qualifiedRate'">
                    <span :class="getQualifiedRateClass(record.qualifiedRate)">{{ record.qualifiedRate }}%</span>
                </template>

                <!-- 动态统计列 -->
                <template v-else-if="column.key?.startsWith('dynamic_rate_')">
                    <span :class="getDynamicRateClass(record, column.key)">{{ getDynamicStatRate(record, column.key)
                    }}%</span>
                </template>
                <template v-else-if="column.key?.startsWith('dynamic_weight_')">
                    <span>{{ getDynamicStatWeight(record, column.key) }}</span>
                </template>

                <!-- 等级重量列（合格和不合格通用） -->
                <template v-else-if="column.key?.startsWith('weight_')">
                    <span :class="getLevelValueClass(record, column.key)">
                        {{ getLevelWeight(record, column.key) }}
                    </span>
                </template>

                <!-- 等级占比列（仅合格等级） -->
                <template v-else-if="column.key?.startsWith('rate_')">
                    <span>{{ getLevelRate(record, column.key) }}%</span>
                </template>

                <!-- 重量列（检测量等） -->
                <template v-else-if="isWeightColumn(column.key)">
                    {{ typeof record[column.key] === 'number' ? record[column.key].toFixed(1) : record[column.key] }}
                </template>
            </template>
        </a-table>
    </div>
</template>

<script lang="ts" setup>
import { computed, ref, onMounted, onUnmounted } from 'vue';
import { Icon } from '/@/components/Icon';
import { FullscreenOutlined, FullscreenExitOutlined } from '@ant-design/icons-vue';
import type { DetailRow, JudgmentLevelColumn } from '/@/api/lab/monthlyQualityReport';
import type { ReportConfig } from '/@/api/lab/reportConfig';
import dayjs from 'dayjs';

const isFullscreen = ref(false);

function toggleFullscreen() {
    isFullscreen.value = !isFullscreen.value;
}

function handleEscape(e: KeyboardEvent) {
    if (e.key === 'Escape') isFullscreen.value = false;
}

onMounted(() => {
    window.addEventListener('keydown', handleEscape);
});
onUnmounted(() => {
    window.removeEventListener('keydown', handleEscape);
});

const tableScrollY = computed(() =>
    isFullscreen.value ? 'calc(100vh - 120px)' : 'calc(100vh - 320px)'
);

interface Props {
    data: DetailRow[];
    loading?: boolean;
    qualifiedColumns: JudgmentLevelColumn[];
    unqualifiedColumns: JudgmentLevelColumn[];
    reportConfigs?: ReportConfig[];
}

const props = withDefaults(defineProps<Props>(), {
    loading: false,
    data: () => [],
    qualifiedColumns: () => [],
    unqualifiedColumns: () => [],
    reportConfigs: () => [],
});

// 基础列（必须指定 width，否则 scroll.y 时表头与表体为两套表格会错位）
const colWidth = {
    prodDate: 90,
    productSpec: 56,
    shift: 48,
    shiftNo: 108,
    detection: 70,
    dynamic: 60,
    dynamicRate: 64, // A占比、B占比等
    unqualified: 56,
} as const;

const fixedColumns = [
    {
        title: '生产日期',
        dataIndex: 'prodDate',
        key: 'prodDate',
        width: colWidth.prodDate,
        align: 'center',
    },
    {
        title: '带宽',
        dataIndex: 'productSpecCode',
        key: 'productSpecCode',
        width: colWidth.productSpec,
        align: 'center',
    },
    {
        title: '班次',
        dataIndex: 'shift',
        key: 'shift',
        width: colWidth.shift,
        align: 'center',
    },
    {
        title: '炉号',
        dataIndex: 'shiftNo',
        key: 'shiftNo',
        width: colWidth.shiftNo,
        align: 'center',
    },
];

// 创建等级名称到合格/不合格状态的映射
const levelQualificationMap = computed(() => {
    const map = new Map<string, boolean>();

    // 合格等级标记为 true
    props.qualifiedColumns.forEach(col => {
        map.set(col.name, true);
    });

    // 不合格等级标记为 false
    props.unqualifiedColumns.forEach(col => {
        map.set(col.name, false);
    });

    return map;
});

// 动态生成表格列（指定 width 保证表头与表体列对齐）
const detailColumns = computed(() => {
    const qualifiedChildren: any[] = [
        {
            title: '检测量',
            dataIndex: 'detectionWeight',
            key: 'detectionWeight',
            width: colWidth.detection,
            align: 'center',
        },
    ];

    const reportVisibleConfigs = (props.reportConfigs || []).filter(c => c.isShowInReport);
    reportVisibleConfigs.forEach(config => {
        if (config.isPercentage) {
            qualifiedChildren.push({
                title: config.name,
                key: `dynamic_rate_${config.id}`,
                width: colWidth.dynamic,
                align: 'center',
            });
        } else {
            qualifiedChildren.push({
                title: `${config.name}`,
                key: `dynamic_weight_${config.id}`,
                width: colWidth.dynamic,
                align: 'center',
            });
            if (config.isShowRatio) {
                qualifiedChildren.push({
                    title: `${config.name}占比`,
                    key: `dynamic_rate_${config.id}`,
                    width: colWidth.dynamicRate,
                    align: 'center',
                });
            }
        }
    });

    const unqualifiedChildren: any[] = [];
    const unqualifiedConfig = (props.reportConfigs || []).find(c => c.name === '不合格');
    const unqualifiedLevelNames = unqualifiedConfig?.levelNames || [];
    unqualifiedLevelNames.forEach(levelName => {
        unqualifiedChildren.push({
            title: levelName,
            key: `weight_${levelName}`,
            width: colWidth.unqualified,
            align: 'center',
        });
    });

    // 返回两个分组列
    const result: any[] = [];
    if (qualifiedChildren.length > 0) {
        result.push({
            title: '检测明细（kg）',
            children: qualifiedChildren,
        });
    }
    if (unqualifiedChildren.length > 0) {
        result.push({
            title: '不合格分类',
            children: unqualifiedChildren,
        });
    }
    return result;
});

// 合并所有列
const columns = computed(() => {
    return [...fixedColumns, ...detailColumns.value];
});

// 获取等级重量（从 qualifiedCategories 或 unqualifiedCategories 中取）
function getLevelWeight(record: DetailRow, key: string) {
    const levelName = key.replace('weight_', '');

    // 先从合格分类中找
    const qualifiedStat = record.qualifiedCategories?.[levelName];
    if (qualifiedStat) {
        return qualifiedStat.weight?.toFixed(1) ?? '0.0';
    }

    // 再从不合格分类中找
    const unqualifiedWeight = record.unqualifiedCategories?.[levelName];
    if (unqualifiedWeight) {
        return unqualifiedWeight.toFixed(1);
    }

    return '0.0';
}

// 获取等级占比（仅合格等级有占比）
function getLevelRate(record: DetailRow, key: string) {
    const levelName = key.replace('rate_', '');
    const stat = record.qualifiedCategories?.[levelName];
    return stat?.rate?.toFixed(2) ?? '0.00';
}

// 获取等级值的样式类名
function getLevelValueClass(record: DetailRow, key: string) {
    const levelName = key.replace('weight_', '');

    // 使用映射判断该等级是否合格
    const isQualified = levelQualificationMap.value.get(levelName);

    // 如果是不合格等级，且有值，显示红色
    if (isQualified === false) {
        const value = record.unqualifiedCategories?.[levelName];
        if (value && value > 0) {
            return 'unqualified-value';
        }
        return 'zero-value';
    }

    return '';
}

/**
 * 格式化日期
 * 支持 ISO 字符串、Date 对象、时间戳等多种格式
 * 汇总行或无效日期返回 '-'
 */
function formatDate(date: string | Date | null | undefined) {
    // 空值检查
    if (!date) {
        return '-';
    }

    // 检查是否为 C# DateTime 的默认值（0001-01-01）
    try {
        const d = dayjs(date);
        if (!d.isValid()) {
            return '-';
        }
        // C# DateTime 默认最小值是 0001-01-01，这种情况下应该显示为 '-'
        if (d.year() === 1 || d.year() < 1900) {
            return '-';
        }
        return d.format('YYYYMMDD');
    } catch (error) {
        console.warn('日期格式化失败:', date, error);
        return '-';
    }
}

// 获取行样式类名
function getRowClassName(record: DetailRow) {
    if (record.isSummaryRow) {
        return 'monthly-total-row';
    }
    return '';
}

// 获取合格率样式类名
function getQualifiedRateClass(rate: number) {
    if (rate >= 95) return 'rate-excellent';
    if (rate >= 90) return 'rate-good';
    if (rate >= 80) return 'rate-warning';
    return 'rate-danger';
}

// 判断是否为重量列
function isWeightColumn(dataIndex: string) {
    return dataIndex?.includes('Weight');
}

// 获取动态统计占比
function getDynamicStatRate(record: DetailRow, key: string) {
    // key format: dynamic_rate_{id}
    const id = key.replace('dynamic_rate_', '');
    const config = props.reportConfigs?.find(c => c.id === id);
    if (!config) return '0.00';

    const stat = record.dynamicStats?.[config.id];
    return stat?.rate?.toFixed(2) ?? '0.00';
}

// 获取动态统计重量
function getDynamicStatWeight(record: DetailRow, key: string) {
    const id = key.replace('dynamic_weight_', '');
    const config = props.reportConfigs?.find(c => c.id === id);
    if (!config) return '0.0';

    const stat = record.dynamicStats?.[config.id];
    return stat?.weight?.toFixed(1) ?? '0.0';
}

function getDynamicRateClass(record: DetailRow, key: string) {
    const rateStr = getDynamicStatRate(record, key);
    const rate = parseFloat(rateStr);
    return getQualifiedRateClass(rate);
}
</script>

<style lang="less" scoped>
.detail-table-container {
    background: #fff;
    border-radius: 12px;
    padding: 16px;
    box-shadow: 0 2px 12px rgba(0, 0, 0, 0.04);
    overflow-x: auto; /* 列不设宽时表格按内容撑开，容器横向滚动 */

    &.detail-table-fullscreen {
        position: fixed;
        inset: 0;
        z-index: 1000;
        border-radius: 0;
        display: flex;
        flex-direction: column;
        padding: 16px 24px 24px;
        box-shadow: none;
        overflow: hidden;

        .table-header {
            flex-shrink: 0;
            margin-bottom: 12px;
        }

        .ant-table-wrapper {
            flex: 1;
            min-height: 0;
            overflow: auto;
        }
    }
}

.fullscreen-btn {
    color: #666;
    &:hover {
        color: #1890ff;
    }
}

.table-header {
    display: flex;
    align-items: center;
    justify-content: space-between;
    margin-bottom: 12px;
}

.table-title {
    display: flex;
    align-items: center;
    gap: 8px;
    font-size: 15px;
    font-weight: 600;
    color: #262626;
    margin: 0;
}

.summary-label {
    font-weight: 600;
    color: #1890ff;
}

:deep(.monthly-total-row) {
    background: linear-gradient(135deg, #f0f5ff 0%, #e6f7ff 100%) !important;
    font-weight: 600;

    td {
        background: transparent !important;
    }
}

.rate-excellent {
    color: #52c41a;
    font-weight: 600;
}

.rate-good {
    color: #1890ff;
    font-weight: 500;
}

.rate-warning {
    color: #faad14;
    font-weight: 500;
}

.rate-danger {
    color: #ff4d4f;
    font-weight: 600;
}

.unqualified-value {
    color: #ff4d4f;
}

.zero-value {
    color: #bfbfbf;
}

:deep(.ant-table) {
    font-size: 12px;

    /* 使用列配置的 width，表头与表体为同一套列宽，保证对齐 */
    .ant-table-thead > tr > th {
        background: #fafafa;
        font-weight: 600;
        color: #595959;
        padding: 8px 6px;
        white-space: nowrap;
    }

    .ant-table-tbody > tr > td {
        padding: 6px;
    }
}
</style>
