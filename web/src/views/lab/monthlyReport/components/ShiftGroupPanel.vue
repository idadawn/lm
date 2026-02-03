<template>
    <div class="shift-group-panel">
        <div class="panel-header">
            <h3 class="panel-title">
                <Icon icon="ant-design:team-outlined" :size="18" />
                班组统计
            </h3>
        </div>

        <a-table :columns="columns" :data-source="data" :loading="loading" :pagination="false" :scroll="{ y: 300 }"
            size="small" bordered :row-class-name="getRowClassName">
            <template #bodyCell="{ column, text, record }">
                <template v-if="column.dataIndex === 'shift'">
                    <span v-if="record.isSummaryRow && record.summaryType === 'MonthlyTotal'"
                        class="monthly-total-label">
                        {{ text }}
                    </span>
                    <span v-else-if="record.isSummaryRow" class="shift-subtotal-label">
                        {{ text }}班小计
                    </span>
                    <a-tag v-else :color="getShiftColor(text)">{{ text }}班</a-tag>
                </template>
                <template v-else-if="column.dataIndex === 'qualifiedRate'">
                    <span :class="getQualifiedRateClass(text)">{{ text }}%</span>
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
import { Icon } from '/@/components/Icon';
import type { ShiftGroupRow } from '/@/api/lab/monthlyQualityReport';

interface Props {
    data: ShiftGroupRow[];
    loading?: boolean;
}

const props = withDefaults(defineProps<Props>(), {
    loading: false,
    data: () => [],
});

const columns = [
    {
        title: '班次',
        dataIndex: 'shift',
        key: 'shift',
        width: 90,
    },
    {
        title: '带宽',
        dataIndex: 'productSpecCode',
        key: 'productSpecCode',
        width: 60,
    },
    {
        title: '检测量',
        dataIndex: 'detectionWeight',
        key: 'detectionWeight',
        width: 80,
        align: 'right',
    },
    {
        title: 'A',
        dataIndex: 'classAWeight',
        key: 'classAWeight',
        width: 70,
        align: 'right',
    },
    {
        title: 'A%',
        dataIndex: 'classARate',
        key: 'classARate',
        width: 50,
        align: 'right',
    },
    {
        title: '合格率',
        dataIndex: 'qualifiedRate',
        key: 'qualifiedRate',
        width: 65,
        align: 'right',
    },
];

// 获取行样式类名
function getRowClassName(record: ShiftGroupRow) {
    if (record.isSummaryRow) {
        if (record.summaryType === 'MonthlyTotal') {
            return 'monthly-total-row';
        }
        return 'subtotal-row';
    }
    return '';
}

// 获取班次颜色
function getShiftColor(shift: string) {
    switch (shift) {
        case '甲': return 'blue';
        case '乙': return 'green';
        case '丙': return 'orange';
        default: return 'default';
    }
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
</script>

<style lang="less" scoped>
.shift-group-panel {
    background: #fff;
    border-radius: 12px;
    padding: 16px;
    box-shadow: 0 2px 12px rgba(0, 0, 0, 0.04);
}

.panel-header {
    display: flex;
    align-items: center;
    justify-content: space-between;
    margin-bottom: 12px;
}

.panel-title {
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

.shift-subtotal-label {
    font-weight: 600;
    color: #595959;
}

.monthly-total-label {
    font-weight: 700;
    color: #1890ff;
    font-size: 13px;
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
}
</style>
