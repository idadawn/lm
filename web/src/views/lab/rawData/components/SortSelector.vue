<template>
  <a-space class="sort-selector">
    <span class="sort-label">排序：</span>
    <a-select
      v-model:value="selectedSort"
      style="width: 200px"
      placeholder="选择排序字段"
      @change="handleSortChange">
      <a-select-option
        v-for="option in sortOptions"
        :key="option.value"
        :value="option.value">
        <span class="sort-option">
          <span class="field-name">{{ option.label }}</span>
          <a-tag v-if="currentSort.value === option.value" size="small">
            {{ currentSort.order === 'asc' ? '↑' : '↓' }}
          </a-tag>
        </span>
      </a-select-option>
    </a-select>

    <a-button-group v-if="currentSort.value">      <a-button
        :type="currentSort.order === 'asc' ? 'primary' : 'default'"
        size="small"
        @click="handleOrderChange('asc')">
        <Icon icon="ant-design:sort-ascending-outlined" />
        正序
      </a-button>
      <a-button
        :type="currentSort.order === 'desc' ? 'primary' : 'default'"
        size="small"
        @click="handleOrderChange('desc')">
        <Icon icon="ant-design:sort-descending-outlined" />
        倒序
      </a-button>
    </a-button-group>

    <a-button
      v-if="currentSort.value"
      size="small"
      @click="handleReset">
      重置
    </a-button>
  </a-space>
</template>

<script lang="ts" setup>
  import { ref, reactive, watch } from 'vue';
  import { Icon } from '/@/components/Icon';

  interface SortOption {
    value: string;
    label: string;
  }

  interface SortConfig {
    value: string;
    order: 'asc' | 'desc';
  }

  interface Props {
    modelValue?: SortConfig;
  }

  interface Emits {
    (e: 'update:modelValue', value: SortConfig): void;
    (e: 'change', value: SortConfig): void;
  }

  const props = withDefaults(defineProps<Props>(), {
    modelValue: () => ({ value: '', order: 'asc' }),
  });

  const emit = defineEmits<Emits>();

  // 排序选项
  const sortOptions: SortOption[] = [
    { value: 'prodDate', label: '生产日期' },
    { value: 'furnaceBatchNo', label: '炉次号' },
    { value: 'coilNo', label: '卷号' },
    { value: 'subcoilNo', label: '分卷号' },
    { value: 'lineNo', label: '产线' },
    { value: 'productSpecName', label: '产品规格' },
    { value: 'creatorTime', label: '录入日期' },
  ];

  // 当前排序配置
  const currentSort = reactive<SortConfig>({
    value: props.modelValue?.value || '',
    order: props.modelValue?.order || 'asc',
  });

  // 选择的排序字段
  const selectedSort = ref(currentSort.value);

  // 监听外部值变化
  watch(
    () => props.modelValue,
    (newVal) => {
      if (newVal) {
        currentSort.value = newVal.value;
        currentSort.order = newVal.order;
        selectedSort.value = newVal.value;
      }
    },
    { immediate: true }
  );

  // 处理排序字段变化
  const handleSortChange = (value: string) => {
    if (!value) return;

    currentSort.value = value;
    emit('update:modelValue', { ...currentSort });
    emit('change', { ...currentSort });
  };

  // 处理排序方式变化
  const handleOrderChange = (order: 'asc' | 'desc') => {
    currentSort.order = order;
    emit('update:modelValue', { ...currentSort });
    emit('change', { ...currentSort });
  };

  // 处理重置
  const handleReset = () => {
    currentSort.value = '';
    currentSort.order = 'asc';
    selectedSort.value = '';
    emit('update:modelValue', { ...currentSort });
    emit('change', { ...currentSort });
  };
</script>

<style lang="less" scoped>
.sort-selector {
  padding: 8px 0;

  .sort-label {
    font-size: 14px;
    color: #666;
  }

  .sort-option {
    display: flex;
    justify-content: space-between;
    align-items: center;
    width: 100%;

    .field-name {
      flex: 1;
    }
  }
}
</style>