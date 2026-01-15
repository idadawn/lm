<template>
  <div class="lamination-sub-table">
    <div
      v-for="item in data"
      :key="item.index"
      class="lamination-item"
      :title="`叠片${item.index}: ${item.value}`"
    >
      {{ item.value?.toFixed(2) || '-' }}
    </div>
  </div>
</template>

<script lang="ts" setup>
  interface LaminationDistItem {
    index: number;
    value: number | null;
  }

  defineProps<{
    data: LaminationDistItem[];
  }>();
</script>

<style scoped>
  .lamination-sub-table {
    display: flex;
    flex-wrap: wrap;
    gap: 2px;
  }

  .lamination-item {
    min-width: 36px;
    padding: 2px 4px;
    text-align: center;
    font-size: 12px;
    background: #e6f7ff;
    border-radius: 2px;
    cursor: default;
  }
</style>
