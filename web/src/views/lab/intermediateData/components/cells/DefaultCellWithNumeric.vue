<template>
  <div :class="['cell-content', cellClass]" @click="handleClick">
    <NumericTableCell v-if="isNumericString" :value="record[column.key]" :field-name="column.key" />
    <span v-else>{{ formattedValue }}</span>
  </div>
</template>

<script lang="ts" setup>
import { computed } from 'vue';
import NumericTableCell from '../NumericTableCell.vue';

const props = defineProps<{
  record: any;
  column: any;
  cellClass: string;
  isNumericString?: (value: any) => boolean;
  formatValue?: (value: any, field?: string) => any;
}>();

const emit = defineEmits<{
  click: [];
}>();

const handleClick = () => emit('click');

const isNumericString = computed(() => {
  if (!props.isNumericString) return false;
  return props.isNumericString(props.record[props.column.key]);
});

const formattedValue = computed(() => {
  if (props.formatValue) {
    return props.formatValue(props.record[props.column.key], props.column.key);
  }
  return props.record[props.column.key] ?? '-';
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
</style>
