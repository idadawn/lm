<template>
  <div class="table-card">
    <div class="table-header">
      <h3 class="table-title">明细数据</h3>
      <a-space>
        <a-select
          v-model:value="shiftFilter"
          placeholder="筛选班次"
          style="width: 120px"
          allowClear
          @change="handleFilterChange"
        >
          <a-select-option value="甲">甲班</a-select-option>
          <a-select-option value="乙">乙班</a-select-option>
          <a-select-option value="丙">丙班</a-select-option>
        </a-select>
        <a-button size="small" @click="handleExport">
          <template #icon>
            <DownloadOutlined />
          </template>
          导出
        </a-button>
      </a-space>
    </div>
    <div class="table-body">
      <a-table
        :columns="columns"
        :data-source="filteredData"
        :loading="loading"
        :pagination="paginationConfig"
        :scroll="{ x: 1200 }"
        size="small"
        :row-class-name="getRowClassName"
        bordered
      >
        <template #bodyCell="{ column, record }">
          <template v-if="column.key === 'prodDate'">
            {{ record.prodDate ? dayjs(record.prodDate).format('MM-DD') : '-' }}
          </template>
          <template v-else-if="column.key === 'qualifiedRate'">
            <span :class="getRateClass(record.qualifiedRate)">
              {{ formatNumber(record.qualifiedRate) }}%
            </span>
          </template>
          <template v-else-if="column.key === 'classARate'">
            <span class="success-text">{{ formatNumber(record.classARate) }}%</span>
          </template>
          <template v-else-if="column.key === 'classBRate'">
            <span class="primary-text">{{ formatNumber(record.classBRate) }}%</span>
          </template>
          <template v-else-if="column.key === 'unqualifiedCategories'">
            <a-tag v-for="(value, key) in record.unqualifiedCategories" :key="key" color="red">
              {{ key }}: {{ value }}kg
            </a-tag>
            <span v-if="!record.unqualifiedCategories || Object.keys(record.unqualifiedCategories).length === 0">-</span>
          </template>
          <template v-else-if="column.key === 'detectionWeight'">
            {{ formatNumber(record.detectionWeight) }}
          </template>
          <template v-else>
            {{ record[column.dataIndex] }}
          </template>
        </template>
      </a-table>
    </div>
  </div>
</template>

<script lang="ts" setup>
import { ref, computed, watch } from 'vue';
import { DownloadOutlined } from '@ant-design/icons-vue';
import dayjs from 'dayjs';
import type { DetailRow, JudgmentLevelColumn } from '/@/api/lab/monthlyQualityReport';
import { exportMonthlyReport } from '/@/api/lab/monthlyQualityReport';
import { message } from 'ant-design-vue';

interface Props {
  details?: DetailRow[] | null;
  unqualifiedColumns?: JudgmentLevelColumn[] | null;
  loading?: boolean;
}

const props = withDefaults(defineProps<Props>(), {
  loading: false,
});

// State
const shiftFilter = ref<string | undefined>();

// Pagination
const paginationConfig = {
  pageSize: 20,
  showSizeChanger: true,
  showTotal: (total: number) => `共 ${total} 条`,
};

// Filter data
const filteredData = computed(() => {
  if (!props.details) return [];

  let data = [...props.details];

  if (shiftFilter.value) {
    data = data.filter((d) => d.shift === shiftFilter.value);
  }

  return data;
});

// Dynamic columns
const columns = computed(() => {
  const baseColumns = [
    { title: '生产日期', dataIndex: 'prodDate', key: 'prodDate', width: 100, fixed: 'left' },
    { title: '班次', dataIndex: 'shift', key: 'shift', width: 80 },
    { title: '炉号', dataIndex: 'shiftNo', key: 'shiftNo', width: 100 },
    { title: '产品规格', dataIndex: 'productSpecCode', key: 'productSpecCode', width: 120 },
    { title: '检测量(kg)', dataIndex: 'detectionWeight', key: 'detectionWeight', width: 120 },
    { title: '合格率(%)', dataIndex: 'qualifiedRate', key: 'qualifiedRate', width: 100 },
    { title: 'A类占比(%)', dataIndex: 'classARate', key: 'classARate', width: 100 },
    { title: 'B类占比(%)', dataIndex: 'classBRate', key: 'classBRate', width: 100 },
    { title: '不合格分类', dataIndex: 'unqualifiedCategories', key: 'unqualifiedCategories', width: 200 },
  ];

  return baseColumns;
});

// Format number
function formatNumber(value?: number): string {
  if (value === undefined || value === null) return '-';
  return value.toFixed(2);
}

// Get rate class
function getRateClass(rate?: number): string {
  if (rate === undefined || rate === null) return '';
  if (rate < 90) return 'low-rate';
  if (rate < 95) return 'mid-rate';
  return 'high-rate';
}

// Get row class name
function getRowClassName(record: DetailRow): string {
  if (record.isSummaryRow) return 'summary-row';
  if ((record.qualifiedRate ?? 0) < 90) return 'warning-row';
  return '';
}

// Handle filter change
function handleFilterChange() {
  // Filter is handled by computed property
}

// Handle export
async function handleExport() {
  try {
    await exportMonthlyReport({
      startDate: dayjs().startOf('month').format('YYYY-MM-DD'),
      endDate: dayjs().format('YYYY-MM-DD'),
    });
    message.success('导出成功');
  } catch (error) {
    console.error('导出失败:', error);
    message.error('导出失败');
  }
}
</script>

<style lang="less" scoped>
.table-card {
  background: #fff;
  border-radius: 8px;
  padding: 16px;
  box-shadow: 0 2px 8px rgba(0, 0, 0, 0.06);
}

.table-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 16px;
}

.table-title {
  font-size: 16px;
  font-weight: 600;
  color: #262626;
  margin: 0;
}

.table-body {
  :deep(.ant-table) {
    .summary-row {
      background-color: #fafafa;
      font-weight: 600;

      td {
        background-color: #fafafa !important;
      }
    }

    .warning-row {
      background-color: #fff1f0;
    }

    .low-rate {
      color: #ff4d4f;
      font-weight: 600;
    }

    .mid-rate {
      color: #faad14;
    }

    .high-rate {
      color: #52c41a;
    }

    .success-text {
      color: #52c41a;
    }

    .primary-text {
      color: #1890ff;
    }
  }
}
</style>
