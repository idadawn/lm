<template>
  <div :class="['cell-content', cellClass]" @click="handleClick">
    <slot />
  </div>
</template>

<script lang="ts" setup>
defineProps<{
  cellClass: string;
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
