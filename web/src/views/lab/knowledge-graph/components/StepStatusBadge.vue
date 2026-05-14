<template>
  <span class="status-badge" :class="statusClass">
    {{ statusText }}
  </span>
</template>

<script lang="ts" setup>
import { computed } from 'vue';

const props = defineProps<{
  status: string;
}>();

const statusClass = computed(() => {
  switch (props.status) {
    case 'success': return 'status-success';
    case 'warning': return 'status-warning';
    case 'failed': return 'status-failed';
    case 'running': return 'status-running';
    case 'pending': return 'status-pending';
    default: return 'status-success';
  }
});

const statusText = computed(() => {
  switch (props.status) {
    case 'success': return '成功';
    case 'warning': return '警告';
    case 'failed': return '失败';
    case 'running': return '执行中';
    case 'pending': return '等待';
    default: return '成功';
  }
});
</script>

<style lang="less" scoped>
.status-badge {
  display: inline-block;
  padding: 1px 6px;
  border-radius: 4px;
  font-size: 11px;
  font-weight: 500;
  line-height: 1.4;

  &.status-success {
    background: #DCFCE7;
    color: #166534;
  }
  &.status-warning {
    background: #FEF3C7;
    color: #92400E;
  }
  &.status-failed {
    background: #FEE2E2;
    color: #991B1B;
  }
  &.status-running {
    background: #DBEAFE;
    color: #1E40AF;
  }
  &.status-pending {
    background: #F1F5F9;
    color: #475569;
  }
}
</style>
