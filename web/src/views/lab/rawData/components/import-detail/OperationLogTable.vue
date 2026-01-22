<template>
  <div class="operation-log-table">
    <a-table
      :columns="columns"
      :data-source="dataSource"
      :pagination="pagination"
      :loading="loading"
      @change="handleTableChange"
    >
      <template #bodyCell="{ column, record }">
        <template v-if="column.key === 'operation'">
          <a-tag :color="getOperationColor(record.operation)">{{ record.operation }}</a-tag>
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
  { title: '操作时间', dataIndex: 'time', key: 'time', width: 180 },
  { title: '操作类型', dataIndex: 'operation', key: 'operation', width: 120 },
  { title: '操作人', dataIndex: 'operator', key: 'operator', width: 120 },
  { title: '说明', dataIndex: 'description', key: 'description', ellipsis: true },
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

const getOperationColor = (operation: string) => {
  const colorMap: Record<string, string> = {
    '导入': 'blue',
    '校验': 'orange',
    '确认': 'green',
    '删除': 'red',
  };
  return colorMap[operation] || 'default';
};
</script>

<style lang="less" scoped>
.operation-log-table {
  padding: 16px 0;
}
</style>
