<template>
  <div :class="['cell-content', cellClass]" @click="handleClick">
    <span>{{ text || record[dateField] || '-' }}</span>
  </div>
</template>

<script lang="ts" setup>
defineProps<{
  record: any;
  text: any;
  cellClass: string;
  dateField?: string;
}>();

const emit = defineEmits<{
  click: [];
}>();

const handleClick = () => emit('click');
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
