<template>
  <div :class="['cell-content', cellClass]" @click="handleClick">
    <span :class="{ 'text-danger': isNegative }">{{ formattedValue }}</span>
  </div>
</template>

<script lang="ts" setup>
import { computed } from 'vue';

const props = defineProps<{
  record: any;
  column: any;
  text: any;
  cellClass: string;
  formatNumericValue?: (value: any, field?: string) => any;
}>();

const emit = defineEmits<{
  click: [];
}>();

const handleClick = () => emit('click');

const isNegative = computed(() => {
  const value = props.text;
  if (value === null || value === undefined) return false;
  const num = typeof value === 'number' ? value : parseFloat(value);
  return !isNaN(num) && num < 0;
});

const formattedValue = computed(() => {
  if (props.formatNumericValue) {
    return props.formatNumericValue(props.text, props.column.key);
  }
  return props.text ?? '-';
});
</script>

<style scoped>
.cell-content {
  width: 100%;
  height: 100%;
  padding: 4px;
  cursor: pointer;
  transition: background-color 0.3s;
}

.cell-content:hover {
  opacity: 0.8;
}

.text-danger {
  color: #f5222d;
}
</style>
