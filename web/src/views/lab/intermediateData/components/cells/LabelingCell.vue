<template>
  <div :class="['status-cell', { 'bg-red': isPerformanceFailed }, cellClass]" @click="handleClick">
    <span>{{ record.labeling || text }}</span>
  </div>
</template>

<script lang="ts" setup>
import { computed } from 'vue';

const props = defineProps<{
  record: any;
  text: any;
  cellClass: string;
}>();

const emit = defineEmits<{
  click: [];
}>();

const isPerformanceFailed = computed(() => {
  return (props.record.labeling || props.text) === '性能不合';
});

const handleClick = () => emit('click');
</script>

<style scoped>
.status-cell {
  width: 100%;
  height: 100%;
  padding: 4px;
  display: flex;
  align-items: center;
  justify-content: center;
  cursor: pointer;
}

.status-cell.bg-red {
  background-color: #ff0000 !important;
  color: white;
  font-weight: bold;
}
</style>
