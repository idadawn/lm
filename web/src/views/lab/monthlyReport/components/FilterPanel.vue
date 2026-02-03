<template>
    <div class="filter-panel">
        <div class="filter-content">
            <!-- 生产日期 -->
            <div class="filter-item">
                <span class="filter-label">生产日期</span>
                <a-range-picker v-model:value="dateRangeValue" format="YYYY-MM-DD"
                    :placeholder="['开始日期', '结束日期']" :allow-clear="false" style="width: 280px"
                    @change="handleDateChange" />
            </div>

            <!-- 班次 -->
            <div class="filter-item">
                <span class="filter-label">班次</span>
                <a-select v-model:value="shiftValue" placeholder="全部班次" allow-clear style="width: 120px"
                    @change="handleShiftChange">
                    <a-select-option value="甲">甲</a-select-option>
                    <a-select-option value="乙">乙</a-select-option>
                    <a-select-option value="丙">丙</a-select-option>
                </a-select>
            </div>

            <!-- 炉号 -->
            <div class="filter-item">
                <span class="filter-label">炉号</span>
                <a-input v-model:value="shiftNoValue" placeholder="请输入炉号" allow-clear style="width: 140px"
                    @press-enter="handleSearch" />
            </div>

            <!-- 产品规格/带宽 -->
            <div class="filter-item">
                <span class="filter-label">带宽</span>
                <a-select v-model:value="productSpecCodeValue" placeholder="全部规格" allow-clear style="width: 140px"
                    @change="handleProductSpecChange">
                    <a-select-option v-for="spec in productSpecOptions" :key="spec.id"
                        :value="spec.code || spec.name">{{ spec.name }}</a-select-option>
                </a-select>
            </div>

            <!-- 操作按钮 -->
            <div class="filter-actions">
                <a-button type="primary" @click="handleSearch">
                    <Icon icon="ant-design:search-outlined" :size="14" />
                    查询
                </a-button>
                <a-button @click="handleReset">
                    <Icon icon="ant-design:redo-outlined" :size="14" />
                    重置
                </a-button>
            </div>
        </div>
    </div>
</template>

<script lang="ts" setup>
import { ref, watch, onMounted } from 'vue';
import dayjs, { Dayjs } from 'dayjs';
import { Icon } from '/@/components/Icon';
import { getProductSpecOptions } from '/@/api/lab/intermediateData';

type RangeValue = [Dayjs, Dayjs];

interface Props {
    dateRange: RangeValue;
    shift: string;
    shiftNo: string;
    productSpecCode: string;
}

const props = defineProps<Props>();

const emit = defineEmits<{
    'update:dateRange': [value: RangeValue];
    'update:shift': [value: string];
    'update:shiftNo': [value: string];
    'update:productSpecCode': [value: string];
    'search': [];
    'reset': [];
}>();

// 本地值 - 简化初始化
const dateRangeValue = ref<RangeValue>([dayjs().startOf('month'), dayjs().endOf('month')]);
const shiftValue = ref<string>('');
const shiftNoValue = ref<string>('');
const productSpecCodeValue = ref<string>('');
const productSpecOptions = ref<any[]>([]);

// 初始化
onMounted(async () => {
    // 从 props 初始化
    if (props.dateRange && props.dateRange.length === 2) {
        dateRangeValue.value = [dayjs(props.dateRange[0]), dayjs(props.dateRange[1])];
    }
    shiftValue.value = props.shift || '';
    shiftNoValue.value = props.shiftNo || '';
    productSpecCodeValue.value = props.productSpecCode || '';

    // 加载产品规格选项
    try {
        const res = await getProductSpecOptions();
        productSpecOptions.value = res.data || res || [];
    } catch (e) {
        console.error(e);
    }
});

// 监听外部值变化
watch(() => props.dateRange, (val) => {
    if (val && val.length === 2 && val[0] && val[1]) {
        dateRangeValue.value = [dayjs(val[0]), dayjs(val[1])];
    }
});
watch(() => props.shift, (val) => { shiftValue.value = val || ''; });
watch(() => props.shiftNo, (val) => { shiftNoValue.value = val || ''; });
watch(() => props.productSpecCode, (val) => { productSpecCodeValue.value = val || ''; });

// 事件处理
function handleDateChange(dates: any) {
    if (dates && dates.length === 2) {
        emit('update:dateRange', dates);
    }
}

function handleShiftChange(value: string) {
    emit('update:shift', value || '');
}

function handleProductSpecChange(value: string) {
    emit('update:productSpecCode', value || '');
}

function handleSearch() {
    emit('update:dateRange', dateRangeValue.value);
    emit('update:shift', shiftValue.value || '');
    emit('update:shiftNo', shiftNoValue.value || '');
    emit('update:productSpecCode', productSpecCodeValue.value || '');
    emit('search');
}

function handleReset() {
    const newRange: RangeValue = [dayjs().startOf('month'), dayjs().endOf('month')];
    dateRangeValue.value = newRange;
    shiftValue.value = '';
    shiftNoValue.value = '';
    productSpecCodeValue.value = '';
    emit('reset');
}
</script>

<style lang="less" scoped>
.filter-panel {
    background: #fff;
    border-radius: 12px;
    padding: 16px 20px;
    margin-bottom: 16px;
    box-shadow: 0 2px 12px rgba(0, 0, 0, 0.04);
}

.filter-content {
    display: flex;
    flex-wrap: wrap;
    align-items: center;
    gap: 16px;
}

.filter-item {
    display: flex;
    align-items: center;
    gap: 8px;
}

.filter-label {
    font-size: 13px;
    color: #595959;
    white-space: nowrap;
}

.filter-actions {
    display: flex;
    gap: 8px;
    margin-left: auto;

    :deep(.ant-btn) {
        display: flex;
        align-items: center;
        gap: 4px;
        border-radius: 6px;
    }
}

// 美化下拉框和输入框
:deep(.ant-picker),
:deep(.ant-select-selector),
:deep(.ant-input) {
    border-radius: 6px !important;
}

:deep(.ant-select-selector) {
    height: 32px !important;
}

// 确保日期选择器可见性 - 覆盖全局样式
:deep(.ant-picker) {
    border: 1px solid #d9d9d9 !important;
    border-radius: 6px !important;
    transition: all 0.3s;

    &:hover,
    &:focus,
    &-focused {
        border-color: #1890ff !important;
        box-shadow: 0 0 0 2px rgba(24, 144, 255, 0.2) !important;
    }
}

// 确保下拉弹出层可见
:deep(.ant-picker-dropdown) {
    z-index: 1050 !important;
}
</style>
