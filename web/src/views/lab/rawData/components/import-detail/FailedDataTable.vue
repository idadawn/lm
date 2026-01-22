<template>
  <div class="failed-data-table">
    <a-table
      :columns="columns"
      :data-source="dataSource"
      :pagination="pagination"
      :loading="loading"
      @change="handleTableChange"
    >
      <template #bodyCell="{ column, record }">
        <template v-if="column.key === 'status'">
          <a-tag color="error">失败</a-tag>
        </template>
        <template v-if="column.key === 'error'">
          <a-tooltip :title="record.error">
            <span class="error-message">{{ record.error }}</span>
          </a-tooltip>
        </template>
      </template>
    </a-table>
  </div>
</template>

<script lang="ts" setup>
import { ref, computed } from 'vue';
import type { TableProps } from 'ant-design-vue';

interface Props {
  dataSource?: any[];
  loading?: boolean;
}

const props = withDefaults(defineProps<Props>(), {
  dataSource: () => [],
  loading: false,
});

const emit = defineEmits(['change']);

const columns = ref([
  { title: 'ID', dataIndex: 'id', key: 'id', width: 80 },
  { title: '名称', dataIndex: 'name', key: 'name' },
  { title: '状态', dataIndex: 'status', key: 'status', width: 100 },
  { title: '错误信息', dataIndex: 'error', key: 'error', ellipsis: true },
  { title: '创建时间', dataIndex: 'createTime', key: 'createTime', width: 180 },
]);

const pagination = computed(() => ({
  total: props.dataSource.length,
  pageSize: 10,
  current: 1,
  showSizeChanger: true,
  showTotal: (total: number) => `共 ${total} 条`,
}));

const handleTableChange: TableProps['onChange'] = (pag, filters, sorter) => {
  emit('change', { pag, filters, sorter });
};
</script>

<style lang="less" scoped>
.failed-data-table {
  padding: 16px 0;
}
.error-message {
  color: #ff4d4f;
}
</style>
