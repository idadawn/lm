<template>
    <div class="filter-panel">
        <div class="filter-content">
            <!-- 时间范围 -->
            <div class="filter-item">
                <span class="filter-label">时间范围</span>
                <a-range-picker v-model:value="dateRangeValue" :presets="rangePresets" format="YYYY-MM-DD"
                    value-format="YYYY-MM-DD" :allow-clear="false" style="width: 280px" @change="handleDateChange" />
            </div>

            <!-- 班次 -->
            <div class="filter-item">
                <span class="filter-label">班次</span>
                <a-select v-model:value="shiftValue" placeholder="全部班次" allow-clear style="width: 120px"
                    @change="handleShiftChange">
                    <a-select-option value="甲">甲班</a-select-option>
                    <a-select-option value="乙">乙班</a-select-option>
                    <a-select-option value="丙">丙班</a-select-option>
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
                    <a-select-option value="120">120mm</a-select-option>
                    <a-select-option value="142">142mm</a-select-option>
                    <a-select-option value="170">170mm</a-select-option>
                    <a-select-option value="213">213mm</a-select-option>
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
import { ref, watch } from 'vue';
import dayjs, { Dayjs } from 'dayjs';
import { Icon } from '/@/components/Icon';

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

// 本地值
const dateRangeValue = ref<RangeValue>(props.dateRange);
const shiftValue = ref<string>(props.shift);
const shiftNoValue = ref<string>(props.shiftNo);
const productSpecCodeValue = ref<string>(props.productSpecCode);

// 预设时间范围
const rangePresets = ref([
    { label: '本月', value: [dayjs().startOf('month'), dayjs().endOf('month')] },
    { label: '上月', value: [dayjs().subtract(1, 'month').startOf('month'), dayjs().subtract(1, 'month').endOf('month')] },
    { label: '本周', value: [dayjs().startOf('week'), dayjs().endOf('week')] },
    { label: '上周', value: [dayjs().subtract(1, 'week').startOf('week'), dayjs().subtract(1, 'week').endOf('week')] },
    { label: '本年', value: [dayjs().startOf('year'), dayjs().endOf('year')] },
    { label: '去年', value: [dayjs().subtract(1, 'year').startOf('year'), dayjs().subtract(1, 'year').endOf('year')] },
]);

// 监听外部值变化
watch(() => props.dateRange, (val) => { dateRangeValue.value = val; });
watch(() => props.shift, (val) => { shiftValue.value = val; });
watch(() => props.shiftNo, (val) => { shiftNoValue.value = val; });
watch(() => props.productSpecCode, (val) => { productSpecCodeValue.value = val; });

// 事件处理
function handleDateChange(dates: RangeValue) {
    emit('update:dateRange', dates);
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
    dateRangeValue.value = [dayjs().startOf('month'), dayjs().endOf('month')];
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
</style>
