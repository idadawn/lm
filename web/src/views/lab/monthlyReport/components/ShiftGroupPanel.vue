<template>
    <div class="shift-group-panel">
        <div class="panel-header">
            <h3 class="panel-title">
                <Icon icon="ant-design:team-outlined" :size="18" />
                班组统计
            </h3>
        </div>

        <a-table :columns="columns" :data-source="data" :loading="loading" :pagination="false"
            :scroll="{ y: 'calc(100vh - 320px)' }" size="small" bordered :row-class-name="getRowClassName">
            <template #bodyCell="{ column, text, record }">
                <template v-if="column.dataIndex === 'shift'">
                    <span v-if="record.isSummaryRow" class="summary-label">合计</span>
                    <a-tag v-else :color="getShiftColor(text)">{{ text }}</a-tag>
                </template>
                <template v-else-if="column.dataIndex === 'qualifiedRate'">
                    <span :class="getQualifiedRateClass(text)">{{ text }}%</span>
                </template>
                <template v-else-if="column.key?.startsWith('weight_')">
                    <span>{{ getLevelWeight(record, column.key) }}</span>
                </template>
                <template v-else-if="column.key?.startsWith('rate_')">
                    <span>{{ getLevelRate(record, column.key) }}%</span>
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
import type { ShiftGroupRow, JudgmentLevelColumn } from '/@/api/lab/monthlyQualityReport';

interface Props {
    data: ShiftGroupRow[];
    loading?: boolean;
    qualifiedColumns?: JudgmentLevelColumn[];
}

const props = withDefaults(defineProps<Props>(), {
    loading: false,
    data: () => [],
    qualifiedColumns: () => [],
});

const columns = computed(() => {
    // 固定列
    const fixedCols: any[] = [
        {
            title: '班次',
            dataIndex: 'shift',
            key: 'shift',
            width: 80,
            fixed: 'left',
        },
        {
            title: '带宽',
            dataIndex: 'productSpecCode',
            key: 'productSpecCode',
            width: 60,
            fixed: 'left',
        },
    ];

    // 检测明细（kg）分组
    const detailChildren: any[] = [
        {
            title: '检测量',
            dataIndex: 'detectionWeight',
            key: 'detectionWeight',
            width: 80,
            align: 'right',
        },
    ];

    // 按优先级排序的合格等级
    const sortedLevels = [...(props.qualifiedColumns || [])]
        .sort((a, b) => (a.priority || 0) - (b.priority || 0));

    // 添加合格等级列（重量 + 占比）
    sortedLevels.forEach((level) => {
        detailChildren.push({
            title: level.name,
            key: `weight_${level.name}`,
            width: 70,
            align: 'right',
        });
        detailChildren.push({
            title: `${level.name}%`,
            key: `rate_${level.name}`,
            width: 50,
            align: 'right',
        });
    });

    // 合格率
    detailChildren.push({
        title: '合格率',
        dataIndex: 'qualifiedRate',
        key: 'qualifiedRate',
        width: 65,
        align: 'right',
    });

    // 返回分组列结构
    return [
        ...fixedCols,
        {
            title: '检测明细',
            children: detailChildren,
        },
    ];
});

// 获取等级重量
function getLevelWeight(record: ShiftGroupRow, key: string) {
    const levelName = key.replace('weight_', '');
    const stat = record.qualifiedCategories?.[levelName];
    return stat?.weight?.toFixed(1) ?? '0.0';
}

// 获取等级占比
function getLevelRate(record: ShiftGroupRow, key: string) {
    const levelName = key.replace('rate_', '');
    const stat = record.qualifiedCategories?.[levelName];
    return stat?.rate?.toFixed(0) ?? '0';
}

// 获取行样式类名
function getRowClassName(record: ShiftGroupRow) {
    if (record.isSummaryRow) {
        return 'monthly-total-row';
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

.summary-label {
    font-weight: 600;
    color: #1890ff;
}

// 汇总行样式
:deep(.monthly-total-row) {
    background: linear-gradient(135deg, #f0f5ff 0%, #e6f7ff 100%) !important;
    font-weight: 600;

    td {
        background: transparent !important;
    }
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
