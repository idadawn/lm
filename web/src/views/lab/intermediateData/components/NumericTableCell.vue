<template>
  <div class="numeric-table-cell" v-if="numbers.length > 0">
    <table class="numeric-table">
      <tr>
        <td v-for="(num, index) in numbers" :key="index" class="numeric-cell">
          {{ num }}
        </td>
      </tr>
    </table>
  </div>
  <span v-else class="empty-value">-</span>
</template>

<script lang="ts" setup>
  import { computed } from 'vue';

  const props = defineProps<{
    value: string | number | null | undefined;
  }>();

  const numbers = computed(() => {
    if (!props.value) return [];
    
    // 将值转换为字符串
    const str = String(props.value).trim();
    if (!str) return [];
    
    // 按空格分割
    const parts = str.split(/\s+/).filter(p => p);
    
    // 尝试转换为数字并格式化
    return parts.map(part => {
      const num = parseFloat(part);
      if (isNaN(num)) return part; // 如果不是数字，返回原值
      // 保留2位小数
      return num.toFixed(2);
    });
  });
</script>

<style scoped>
  .numeric-table-cell {
    display: inline-block;
    padding: 0;
    width: 100%;
  }

  .numeric-table {
    border-collapse: separate;
    border-spacing: 1px;
    width: 100%;
    table-layout: auto;
  }

  .numeric-cell {
    padding: 2px 6px;
    text-align: center;
    font-size: 12px;
    border: 1px solid #d9d9d9;
    background: #fafafa;
    white-space: nowrap;
    min-width: 50px;
  }

  .numeric-cell:hover {
    background: #e6f7ff;
  }

  .empty-value {
    color: #999;
  }
</style>