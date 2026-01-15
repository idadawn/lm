<template>
  <a-select
    v-model:value="selectedUnitId"
    :placeholder="placeholder"
    :allowClear="allowClear"
    :disabled="disabled"
    :loading="loading"
    show-search
    :filter-option="filterOption"
    @change="handleChange"
    style="width: 100%"
  >
    <a-select-option
      v-for="unit in unitOptions"
      :key="unit.id"
      :value="unit.id"
    >
      {{ unit.displayName }}
    </a-select-option>
  </a-select>
</template>

<script setup lang="ts">
import { ref, watch, onMounted, computed } from 'vue';
import { getUnitsByCategory, getAllUnitsGroupedByCategory } from '/@/api/lab/unit';

interface UnitDefinition {
  id: string;
  categoryId: string;
  name: string;
  symbol: string;
  isBase: boolean;
  scaleToBase: number;
  offset: number;
  precision: number;
  displayName: string;
}

interface Props {
  modelValue?: string; // 选中的单位ID
  categoryId?: string; // 指定维度ID，如果提供则只显示该维度的单位
  placeholder?: string;
  allowClear?: boolean;
  disabled?: boolean;
}

const props = withDefaults(defineProps<Props>(), {
  placeholder: '请选择单位',
  allowClear: true,
  disabled: false,
});

const emit = defineEmits<{
  'update:modelValue': [value: string | undefined];
  'change': [value: string | undefined, unit: UnitDefinition | undefined];
}>();

const selectedUnitId = ref<string | undefined>(props.modelValue);
const unitOptions = ref<UnitDefinition[]>([]);
const loading = ref(false);

// 过滤选项
const filterOption = (input: string, option: any) => {
  const text = option.children || '';
  return text.toLowerCase().includes(input.toLowerCase());
};

// 加载单位列表
const loadUnits = async () => {
  loading.value = true;
  try {
    if (props.categoryId) {
      // 如果指定了维度，只加载该维度的单位
      const response = await getUnitsByCategory(props.categoryId);
      // 处理响应格式：可能是 { data: [...] } 或直接是数组
      const data = (response as any)?.data || response;
      unitOptions.value = Array.isArray(data) ? data : [];
    } else {
      // 否则加载所有单位（按维度分组）
      const response = await getAllUnitsGroupedByCategory();
      // 处理响应格式：可能是 { data: {...} } 或直接是对象
      const grouped = (response as any)?.data || response || {};
      // 将所有维度的单位合并到一个数组
      unitOptions.value = Object.values(grouped).flat() as UnitDefinition[];
    }
  } catch (error) {
    console.error('加载单位列表失败:', error);
    unitOptions.value = [];
  } finally {
    loading.value = false;
  }
};

// 处理选择变化
const handleChange = (value: string | undefined) => {
  selectedUnitId.value = value;
  const selectedUnit = unitOptions.value.find(u => u.id === value);
  emit('update:modelValue', value);
  emit('change', value, selectedUnit);
};

// 监听外部值变化
watch(
  () => props.modelValue,
  (newValue) => {
    selectedUnitId.value = newValue;
  }
);

// 监听维度变化，重新加载单位
watch(
  () => props.categoryId,
  () => {
    loadUnits();
  }
);

onMounted(() => {
  loadUnits();
});
</script>
