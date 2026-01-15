<template>
  <div class="thickness-sub-table">
    <div
      v-for="item in data"
      :key="item.index"
      class="thickness-item"
      :class="{ abnormal: item.isAbnormal }"
      :title="`带厚${item.index}: ${item.value}`"
    >
      {{ item.value?.toFixed(2) || '-' }}
    </div>
  </div>
</template>

<script lang="ts" setup>
  interface ThicknessDistItem {
    index: number;
    value: number | null;
    isAbnormal: boolean;
  }

  defineProps<{
    data: ThicknessDistItem[];
  }>();
</script>

<style scoped>
  .thickness-sub-table {
    display: flex;
    flex-wrap: wrap;
    gap: 2px;
  }

  .thickness-item {
    min-width: 32px;
    padding: 2px 4px;
    text-align: center;
    font-size: 12px;
    background: #f5f5f5;
    border-radius: 2px;
    cursor: default;
  }

  .thickness-item.abnormal {
    background: #fff1f0;
    color: #ff4d4f;
    font-weight: bold;
  }
</style>
