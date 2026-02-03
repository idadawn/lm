<template>
    <div class="detail-table-container">
        <div class="table-header">
            <h3 class="table-title">
                <Icon icon="ant-design:table-outlined" :size="18" />
                质量检测明细
            </h3>
        </div>

        <a-table :columns="columns" :data-source="data" :loading="loading" :pagination="false"
            :scroll="{ x: scrollX, y: 600 }" size="small" bordered :row-class-name="getRowClassName">
            <!-- 生产日期 -->
            <template #prodDate="{ text, record }">
                <span v-if="!record.isSummaryRow">{{ formatDate(text) }}</span>
                <span v-else class="summary-label">{{ record.shift }}</span>
            </template>

            <!-- 数值列通用渲染 -->
            <template #bodyCell="{ column, text, record }">
                <template v-if="column.dataIndex === 'prodDate'">
                    <span v-if="!record.isSummaryRow">{{ formatDate(text) }}</span>
                    <span v-else class="summary-label">{{ record.shift }}</span>
                </template>
                <template v-else-if="column.dataIndex === 'qualifiedRate'">
                    <span :class="getQualifiedRateClass(text)">{{ text }}%</span>
                </template>
                <template v-else-if="column.key?.startsWith('unq_')">
                    <span v-if="getUnqualifiedValue(record, column.key) > 0" class="unqualified-value">
                        {{ getUnqualifiedValue(record, column.key).toFixed(1) }}
                    </span>
                    <span v-else class="zero-value">-</span>
                </template>
                <template v-else-if="isRateColumn(column.dataIndex)">
                    {{ text }}%
                </template>
                <template v-else-if="isWeightColumn(column.dataIndex)">
                    {{ typeof text === 'number' ? text.toFixed(1) : text }}
                </template>
            </template>
        </a-table>
    </div>
</template>

<script lang="ts" setup>
import { computed } from 'vue';
import { Icon } from '/@/components/Icon';
import type { DetailRow, JudgmentLevelColumn } from '/@/api/lab/monthlyQualityReport';
import dayjs from 'dayjs';

interface Props {
    data: DetailRow[];
    loading?: boolean;
    unqualifiedColumns: JudgmentLevelColumn[];
}

const props = withDefaults(defineProps<Props>(), {
    loading: false,
    data: () => [],
    unqualifiedColumns: () => [],
});

// 基础列
const baseColumns = [
    {
        title: '生产日期',
        dataIndex: 'prodDate',
        key: 'prodDate',
        width: 100,
        fixed: 'left',
    },
    {
        title: '班次',
        dataIndex: 'shift',
        key: 'shift',
        width: 60,
        fixed: 'left',
    },
    {
        title: '炉号',
        dataIndex: 'shiftNo',
        key: 'shiftNo',
        width: 80,
    },
    {
        title: '带宽',
        dataIndex: 'productSpecCode',
        key: 'productSpecCode',
        width: 70,
    },
    {
        title: '检测明细（kg）',
        children: [
            {
                title: '检测量',
                dataIndex: 'detectionWeight',
                key: 'detectionWeight',
                width: 90,
                align: 'right',
            },
            {
                title: 'A',
                dataIndex: 'classAWeight',
                key: 'classAWeight',
                width: 80,
                align: 'right',
            },
            {
                title: 'A类占比',
                dataIndex: 'classARate',
                key: 'classARate',
                width: 70,
                align: 'right',
            },
            {
                title: 'B',
                dataIndex: 'classBWeight',
                key: 'classBWeight',
                width: 80,
                align: 'right',
            },
            {
                title: 'B类占比',
                dataIndex: 'classBRate',
                key: 'classBRate',
                width: 70,
                align: 'right',
            },
            {
                title: '不合格',
                dataIndex: 'unqualifiedWeight',
                key: 'unqualifiedWeight',
                width: 80,
                align: 'right',
            },
            {
                title: '合格率',
                dataIndex: 'qualifiedRate',
                key: 'qualifiedRate',
                width: 70,
                align: 'right',
            },
        ]
    }
];

// 动态生成不合格分类列
const unqualifiedCategoryColumns = computed(() => {
    const categories = props.unqualifiedColumns?.map((col) => ({
        title: col.name,
        key: `unq_${col.name}`,
        dataIndex: `unqualifiedCategories.${col.name}`,
        width: 80,
        align: 'right',
    })) ?? [];

    if (categories.length > 0) {
        return [{
            title: '不合格分类',
            children: categories
        }];
    }
    return [];
});

// 合并所有列
const columns = computed(() => {
    return [...baseColumns, ...unqualifiedCategoryColumns.value];
});

// 计算表格横向滚动宽度
const scrollX = computed(() => {
    const baseWidth = 910; // 基础列宽度总和
    const dynamicWidth = props.unqualifiedColumns.length * 80;
    return baseWidth + dynamicWidth;
});

// 格式化日期
function formatDate(date: string | null) {
    if (!date) return '-';
    return dayjs(date).format('MM-DD');
}

// 获取行样式类名
function getRowClassName(record: DetailRow) {
    if (record.isSummaryRow) {
        if (record.summaryType === 'MonthlyTotal') {
            return 'monthly-total-row';
        }
        return 'subtotal-row';
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

// 判断是否为比率列
function isRateColumn(dataIndex: string) {
    return dataIndex?.includes('Rate');
}

// 判断是否为重量列
function isWeightColumn(dataIndex: string) {
    return dataIndex?.includes('Weight');
}

// 获取不合格分类值
function getUnqualifiedValue(record: DetailRow, key: string) {
    const categoryName = key.replace('unq_', '');
    return record.unqualifiedCategories?.[categoryName] || 0;
}
</script>

<style lang="less" scoped>
.detail-table-container {
    background: #fff;
    border-radius: 12px;
    padding: 16px;
    box-shadow: 0 2px 12px rgba(0, 0, 0, 0.04);
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

// 汇总行样式
:deep(.subtotal-row) {
    background: #fafafa !important;
    font-weight: 500;

    td {
        background: #fafafa !important;
    }
}

:deep(.monthly-total-row) {
    background: linear-gradient(135deg, #f0f5ff 0%, #e6f7ff 100%) !important;
    font-weight: 600;

    td {
        background: transparent !important;
    }
}

.summary-label {
    font-weight: 600;
    color: #1890ff;
}

// 合格率样式
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

// 不合格值
.unqualified-value {
    color: #ff4d4f;
}

.zero-value {
    color: #bfbfbf;
}

// 表格样式优化
:deep(.ant-table) {
    font-size: 12px;

    .ant-table-thead>tr>th {
        background: #fafafa;
        font-weight: 600;
        color: #595959;
        padding: 8px 6px;
    }

    .ant-table-tbody>tr>td {
        padding: 6px;
    }

    .ant-table-cell-fix-left,
    .ant-table-cell-fix-right {
        background: #fff;
    }
}
</style>
