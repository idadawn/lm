<template>
    <div class="detail-table-container">
        <div class="table-header">
            <h3 class="table-title">
                <Icon icon="ant-design:table-outlined" :size="18" />
                质量检测明细
            </h3>
        </div>

        <a-table :columns="columns" :data-source="data" :loading="loading" :pagination="false"
            :scroll="{ x: scrollX, y: 'calc(100vh - 320px)' }" size="small" bordered :row-class-name="getRowClassName">
            <template #bodyCell="{ column, record }">
                <!-- 生产日期 -->
                <template v-if="column.key === 'prodDate'">
                    <span v-if="!record.isSummaryRow">{{ formatDate(record.prodDate) }}</span>
                    <span v-else class="summary-label">合计</span>
                </template>

                <!-- 合格率 -->
                <template v-else-if="column.key === 'qualifiedRate'">
                    <span :class="getQualifiedRateClass(record.qualifiedRate)">{{ record.qualifiedRate }}%</span>
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
import { computed } from 'vue';
import { Icon } from '/@/components/Icon';
import type { DetailRow, JudgmentLevelColumn } from '/@/api/lab/monthlyQualityReport';
import dayjs from 'dayjs';

interface Props {
    data: DetailRow[];
    loading?: boolean;
    qualifiedColumns: JudgmentLevelColumn[];
    unqualifiedColumns: JudgmentLevelColumn[];
}

const props = withDefaults(defineProps<Props>(), {
    loading: false,
    data: () => [],
    qualifiedColumns: () => [],
    unqualifiedColumns: () => [],
});

// 固定列
const fixedColumns = [
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
        width: 100,
    },
    {
        title: '带宽',
        dataIndex: 'productSpecCode',
        key: 'productSpecCode',
        width: 70,
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

// 动态生成表格列
const detailColumns = computed(() => {
    // 检测明细（kg）组 - 合格等级
    const qualifiedChildren: any[] = [
        {
            title: '检测量',
            dataIndex: 'detectionWeight',
            key: 'detectionWeight',
            width: 90,
            align: 'right',
        },
    ];

    // 按优先级排序的合格等级
    const sortedQualifiedLevels = [...props.qualifiedColumns]
        .sort((a, b) => (a.priority || 0) - (b.priority || 0));

    // 添加合格等级列（重量 + 占比）
    sortedQualifiedLevels.forEach(level => {
        qualifiedChildren.push({
            title: level.name,
            key: `weight_${level.name}`,
            width: 80,
            align: 'right',
        });
        qualifiedChildren.push({
            title: `${level.name}占比`,
            key: `rate_${level.name}`,
            width: 70,
            align: 'right',
        });
    });

    // 合格 + 合格率
    qualifiedChildren.push({
        title: '合格',
        dataIndex: 'qualifiedWeight',
        key: 'qualifiedWeight',
        width: 80,
        align: 'right',
    });
    qualifiedChildren.push({
        title: '合格率',
        dataIndex: 'qualifiedRate',
        key: 'qualifiedRate',
        width: 70,
        align: 'right',
    });

    // 不合格分类组 - 不合格等级
    const unqualifiedChildren: any[] = [];

    // 按优先级排序的不合格等级
    const sortedUnqualifiedLevels = [...props.unqualifiedColumns]
        .sort((a, b) => (a.priority || 0) - (b.priority || 0));

    // 添加不合格等级列（只有重量）
    sortedUnqualifiedLevels.forEach(level => {
        unqualifiedChildren.push({
            title: level.name,
            key: `weight_${level.name}`,
            width: 80,
            align: 'right',
        });
    });

    // 返回两个分组列
    const result = [];
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

// 计算表格横向滚动宽度
const scrollX = computed(() => {
    const fixedWidth = 330; // 固定列宽度
    const qualifiedCount = props.qualifiedColumns.length;
    const unqualifiedCount = props.unqualifiedColumns.length;
    // 每个合格等级 2 列 (150)，每个不合格等级 1 列 (80)，加上检测量(90) + 合格合计(80) + 合格率(70)
    const detailWidth = 90 + (qualifiedCount * 150) + (unqualifiedCount * 80) + 80 + 70;
    return fixedWidth + detailWidth;
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
        return d.format('MM-DD');
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
