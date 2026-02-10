<template>
  <div class="page-content-wrapper">
    <div class="page-content-wrapper-center">
      <div class="page-content-wrapper-content">
        <a-tabs v-model:activeKey="activeTab" type="card" @change="handleTabChange">
          <!-- 第一个页签：检测数据（只显示有效数据） -->
          <a-tab-pane key="data" tab="检测数据">
            <!-- 自定义排序控制 -->
            <div class="table-toolbar">
              <CustomSortControl v-model="sortRules" @change="handleSortChange" />
            </div>
            <BasicTable @register="registerTable">
              <template #bodyCell="{ column, record }">
                <template v-if="column.key === 'productSpecName'">
                  <a-button type="link" @click="handleViewProductSpec(record)" v-if="record.productSpecId">
                    {{ record.productSpecName || '-' }}
                  </a-button>
                  <span v-else>-</span>
                </template>
                <template v-else-if="column.key === 'detectionColumns'">
                  {{ record.detectionColumns || '-' }}
                </template>
                <template v-else-if="column.key === 'creatorTime'">
                  {{ record[column.key] ? formatToDateTime(record[column.key]) : '-' }}
                </template>
                <template v-else-if="column.key?.startsWith('detection')">
                  {{ record[column.key] ?? '-' }}
                </template>
              </template>
            </BasicTable>
          </a-tab-pane>

          <!-- 第二个页签：原始数据（按Excel顺序展示所有数据） -->
          <a-tab-pane key="rawData" tab="原始数据">
            <BasicTable @register="registerRawTable">
              <template #bodyCell="{ column, record }">
                <template v-if="column.dataIndex === 'isValidData'">
                  <a-tag :color="record.isValidData === 1 ? 'success' : 'error'">
                    {{ record.isValidData === 1 ? '有效' : '无效' }}
                  </a-tag>
                </template>
                <template v-else-if="column.dataIndex === 'creatorTime'">
                  {{ record[column.dataIndex] ? formatToDateTime(record[column.dataIndex]) : '-' }}
                </template>
                <template v-else-if="column.key?.startsWith('detection')">
                  {{ record[column.key] ?? '-' }}
                </template>
              </template>
            </BasicTable>
          </a-tab-pane>

          <!-- 第三个页签：导入与日志 -->
          <a-tab-pane key="import" tab="导入与日志">
            <div class="import-log-content">
              <!-- 导入方式选择 -->
              <div class="import-method-selector" style="margin-bottom: 16px">
                <a-row :gutter="16">
                  <a-col :span="24">
                    <a-card :hoverable="true" class="import-method-card" @click="handleStepImport">
                      <template #title>
                        <div class="method-title">
                          <SolutionOutlined style="color: #722ed1; margin-right: 8px" />
                          分步导入向导
                        </div>
                      </template>
                      <div class="method-content">
                        <div class="method-description">
                          4步向导式导入，每步都可查看和修改数据，适合复杂数据
                        </div>
                        <div class="method-features">
                          <div class="feature-item">
                            <CheckCircleOutlined style="color: #52c41a; margin-right: 4px" />
                            分步预览和修改数据
                          </div>
                          <div class="feature-item">
                            <CheckCircleOutlined style="color: #52c41a; margin-right: 4px" />
                            手动匹配产品规格和特性
                          </div>
                          <div class="feature-item">
                            <CheckCircleOutlined style="color: #52c41a; margin-right: 4px" />
                            支持导入会话恢复
                          </div>
                        </div>
                      </div>
                    </a-card>
                  </a-col>
                </a-row>
              </div>

              <!-- 导入日志表格 -->
              <div class="import-log-table" style="margin-top: 24px">
                <div style="margin-bottom: 16px; display: flex; align-items: center; gap: 8px">
                  <ClockCircleOutlined />
                  <span style="font-weight: 500; font-size: 16px">最近导入日志</span>
                </div>
                <BasicTable @register="registerLogTable">
                  <template #bodyCell="{ column, record }">
                    <template v-if="column.key === 'status'">
                      <a-tag :color="getStatusColor(record.status)">
                        <template #icon>
                          <CheckCircleOutlined v-if="record.status === 'success'" />
                          <ExclamationCircleOutlined v-else-if="record.status === 'partial'" />
                          <CloseCircleOutlined v-else />
                        </template>
                        {{ getStatusText(record.status) }}
                      </a-tag>
                    </template>
                    <template v-else-if="column.key === 'importTime'">
                      {{ record.importTime ? formatToDateTime(record.importTime) : '-' }}
                    </template>
                    <template v-else-if="column.key === 'totalRows'">
                      {{ formatTotalRows(record) }}
                    </template>
                  </template>
                </BasicTable>
              </div>
            </div>
          </a-tab-pane>
        </a-tabs>
      </div>
      <ProductSpecModal @register="registerProductSpecModal" />
      <DataReviewModal @register="registerDataReviewModal" @success="handleImportSuccess" />
      <StepImportWizard @register="registerStepImportModal" @reload="handleStepImportSuccess" @cancel="handleStepImportCancel" />
    </div>
  </div>
</template>
<script lang="ts" setup>
import { ref, computed, onMounted, nextTick } from 'vue';
import { getRawDataList, getImportLogList } from '/@/api/lab/rawData';
import { BasicTable, useTable, BasicColumn } from '/@/components/Table';
import { useMessage } from '/@/hooks/web/useMessage';
import { useI18n } from '/@/hooks/web/useI18n';
import { useModal } from '/@/components/Modal';
import { usePopup } from '/@/components/Popup';
import { formatToDateTime, dateUtil } from '/@/utils/dateUtil';
import ProductSpecModal from './components/ProductSpecModal.vue';
import DataReviewModal from './components/DataReviewModal.vue';
import CustomSortControl from './components/CustomSortControl.vue';
import StepImportWizard from './StepImportWizard.vue';
import {
  ClockCircleOutlined,
  CheckCircleOutlined,
  ExclamationCircleOutlined,
  CloseCircleOutlined,
  SolutionOutlined,
} from '@ant-design/icons-vue';

defineOptions({ name: 'labRawData' });

const { createMessage } = useMessage();
const { t } = useI18n();
const [registerProductSpecModal, { openModal: openProductSpecModal }] = useModal();
const [registerDataReviewModal, { openModal: openDataReviewModal }] = useModal();

// 页签状态
const activeTab = ref('data');

// 排序配置（支持多字段排序）
const sortRules = ref([
  { field: 'prodDate', order: 'asc' as 'asc' | 'desc' },
  { field: 'furnaceBatchNo', order: 'asc' as 'asc' | 'desc' },
  { field: 'coilNo', order: 'asc' as 'asc' | 'desc' },
  { field: 'subcoilNo', order: 'asc' as 'asc' | 'desc' },
  { field: 'lineNo', order: 'asc' as 'asc' | 'desc' }
]);

// 检测数据基础列配置（按指定顺序：检测日期、生产日期、炉号、产品规格、特性描述、宽度、带材重量）
const baseColumnsBeforeDetection: BasicColumn[] = [
  { title: '检测日期', dataIndex: 'detectionDateStr', width: 90, align: 'center', fixed: 'left' },
  // { title: '生产日期', dataIndex: 'prodDateStr', width: 100, align: 'center', fixed: 'left' },
  { title: '炉号', dataIndex: 'furnaceNo', width: 130, align: 'center', fixed: 'left' },
  { title: '产品规格', dataIndex: 'productSpecName', align: 'center', key: 'productSpecName', width: 80, fixed: 'left' },
  { title: '宽度', dataIndex: 'width', width: 80, align: 'center', fixed: 'left' },
  { title: '带材重量', dataIndex: 'coilWeight', width: 80, align: 'center', fixed: 'left' },
];

// 检测数据列（固定显示1-22列）
const detectionColumns: BasicColumn[] = Array.from({ length: 22 }, (_, i) => ({
  title: `检测${i + 1}`,
  dataIndex: `detection${i + 1}`,
  key: `detection${i + 1}`,
  width: 60,
  align: 'center' as const,
}));

// 检测数据基础列配置（检测列之后的列）
const baseColumnsAfterDetection: BasicColumn[] = [
  { title: '特性描述', dataIndex: 'featureSuffix', width: 100, align: 'center' },
  { title: '断头数(个)', dataIndex: 'breakCount', width: 80, align: 'center' },
  { title: '单卷重量(kg)', dataIndex: 'singleCoilWeight', width: 80, align: 'center' },
  // 隐藏的列
  { title: '产线', dataIndex: 'lineNo', width: 80, align: 'center', defaultHidden: true },
  { title: '班次', dataIndex: 'shift', width: 80, align: 'center', defaultHidden: true },
  { title: '卷号', dataIndex: 'coilNo', width: 80, align: 'center', defaultHidden: true },
  { title: '分卷', dataIndex: 'subcoilNo', width: 80, align: 'center', defaultHidden: true },
  { title: '录入人', dataIndex: 'creatorUserName', key: 'creatorUserName', align: 'center', width: 120 },
  { title: '录入日期', dataIndex: 'creatorTime', key: 'creatorTime', align: 'center', width: 160 },
];

// 完整的检测数据列配置
const baseColumns: BasicColumn[] = [
  ...baseColumnsBeforeDetection,
  ...detectionColumns,
  ...baseColumnsAfterDetection,
];

const [registerTable, { reload, setColumns, getForm }] = useTable({
  api: getRawDataList,
  columns: baseColumns, // 初始列配置：已包含检测列1-22
  useSearchForm: true,
  immediate: false, // 先不立即加载，等设置默认值后再加载
  formConfig: {
    baseColProps: { span: 6 },
    labelWidth: 100,
    showAdvancedButton: false,
    schemas: [
      {
        field: 'keyword',
        label: t('common.keyword'),
        component: 'Input',
        colProps: { span: 6 },
        componentProps: {
          placeholder: '炉号、产线等',
          submitOnPressEnter: true,
        },
      },
      {
        field: 'prodDateRange',
        label: '检测日期',
        component: 'DateRange',
        colProps: { span: 6 },
        componentProps: {
          placeholder: ['开始日期', '结束日期'],
        },
      },
      {
        field: 'creatorTimeRange',
        label: '录入日期',
        component: 'DateRange',
        colProps: { span: 6 },
        componentProps: {
          placeholder: ['开始日期', '结束日期'],
        },
      },
    ],
    fieldMapToTime: [
      ['prodDateRange', ['startDate', 'endDate'], 'YYYY-MM-DD'],
      ['creatorTimeRange', ['creatorTimeStart', 'creatorTimeEnd'], 'YYYY-MM-DD'],
    ],
  },
  // 添加排序参数，检测数据固定只显示有效数据
  beforeFetch: (params) => {
    // 检测数据只显示有效数据
    params.isValidData = 1;
    // 添加多字段排序规则
    if (sortRules.value.length > 0) {
      params.sortRules = JSON.stringify(sortRules.value);
    }
    return params;
  },
  afterFetch: data => {
    // 确保sortCode字段存在（后端返回的是SortCode，前端使用sortCode）
    data.forEach((record: any) => {
      if (record.SortCode !== undefined && record.sortCode === undefined) {
        record.sortCode = record.SortCode;
      }
    });

    // 更新表格列配置（固定显示检测列1-22）
    setTimeout(() => {
      setColumns(baseColumns);
    }, 0);
    return data;
  },
});

// 初始化时加载数据
onMounted(() => {
  // 使用 nextTick 确保表单已注册完成
  nextTick(() => {
    reload();
  });
});

function handleViewProductSpec(record) {
  if (record.productSpecId) {
    openProductSpecModal(true, { productSpecId: record.productSpecId });
  }
}

// 处理排序变化（新的多字段排序）
function handleSortChange(newSortRules: any[]) {
  sortRules.value = newSortRules;
  // 重新加载表格数据
  reload();
}

// ========== 原始数据表格（按Excel顺序，显示所有数据） ==========
// 原始数据基础列配置
const rawDataBaseColumns: BasicColumn[] = [
  { title: '行号', dataIndex: 'sortCode', width: 80, fixed: 'left' },
  { title: '生产日期', dataIndex: 'prodDateStr', width: 120, defaultHidden: true },
  { title: '检测日期', dataIndex: 'detectionDateStr', width: 120, fixed: 'left' },
  { title: '原始炉号', dataIndex: 'furnaceNo', width: 200, fixed: 'left' },
  { title: '炉号', dataIndex: 'furnaceNoFormatted', width: 200, fixed: 'left' },
  { title: '产线', dataIndex: 'lineNo', width: 80, align: 'center', defaultHidden: true },
  { title: '班次', dataIndex: 'shift', width: 80, align: 'center', defaultHidden: true },
  { title: '卷号', dataIndex: 'coilNo', width: 80, align: 'center', defaultHidden: true },
  { title: '分卷', dataIndex: 'subcoilNo', width: 80, align: 'center', defaultHidden: true },
  { title: '宽度', dataIndex: 'width', width: 100, align: 'right', fixed: 'left' },
  { title: '带材重量', dataIndex: 'coilWeight', width: 120, align: 'right', fixed: 'left' },
  { title: '断头数(个)', dataIndex: 'breakCount', width: 100, align: 'right' },
  { title: '单卷重量(kg)', dataIndex: 'singleCoilWeight', width: 120, align: 'right' },
  { title: '特性描述', dataIndex: 'featureSuffix', width: 120 },
  { title: '数据状态', dataIndex: 'isValidData', key: 'isValidData', width: 100, align: 'center' },
  { title: '错误信息', dataIndex: 'importError', width: 200 },
  { title: '录入日期', dataIndex: 'creatorTime', key: 'creatorTime', width: 160 },
];

// 检测数据列（1-22列）
const rawDataDetectionColumns: BasicColumn[] = Array.from({ length: 22 }, (_, i) => ({
  title: `检测${i + 1}`,
  dataIndex: `detection${i + 1}`,
  key: `detection${i + 1}`,
  width: 100,
  align: 'right' as const,
}));

// 原始数据完整列配置（基础列 + 检测列，检测列放在带材重量后面）
const rawDataColumns: BasicColumn[] = [
  ...rawDataBaseColumns.slice(0, 10), // 行号到带材重量
  ...rawDataDetectionColumns, // 检测列1-22
  ...rawDataBaseColumns.slice(10), // 断头数及之后的列
];

const [registerRawTable, { reload: reloadRawTable }] = useTable({
  api: getRawDataList,
  columns: rawDataColumns,
  useSearchForm: true,
  immediate: false,
  formConfig: {
    baseColProps: { span: 6 },
    labelWidth: 100,
    showAdvancedButton: false,
    schemas: [
      {
        field: 'keyword',
        label: t('common.keyword'),
        component: 'Input',
        colProps: { span: 6 },
        componentProps: {
          placeholder: '炉号、产线等',
          submitOnPressEnter: true,
        },
      },
      {
        field: 'prodDateRange',
        label: '检测日期',
        component: 'DateRange',
        colProps: { span: 8 },
        componentProps: {
          placeholder: ['开始日期', '结束日期'],
        },
      },
      {
        field: 'isValidData',
        label: '数据类型',
        component: 'Select',
        colProps: { span: 4 },
        defaultValue: '',
        componentProps: {
          options: [
            { label: '全部', value: '' },
            { label: '有效数据', value: '1' },
            { label: '无效数据', value: '0' },
          ],
          fieldNames: { label: 'label', value: 'value' },
        },
      },
    ],
    fieldMapToTime: [
      ['prodDateRange', ['startDate', 'endDate'], 'YYYY-MM-DD'],
    ],
  },
  // 原始数据按Excel顺序（SortCode）排序
  beforeFetch: (params) => {
    // 按导入顺序排序（SortCode是Excel的行号）
    params.sortRules = JSON.stringify([{ field: 'sortCode', order: 'asc' }]);
    // 处理数据类型筛选：空字符串表示全部，不传该参数
    if (params.isValidData === '' || params.isValidData === null || params.isValidData === undefined) {
      delete params.isValidData; // 删除该参数，后端会返回全部数据
    } else {
      // 转换为数字类型
      params.isValidData = Number(params.isValidData);
    }
    return params;
  },
  afterFetch: data => {
    data.forEach((record: any) => {
      // 确保字段名映射正确
      if (record.SortCode !== undefined && record.sortCode === undefined) {
        record.sortCode = record.SortCode;
      }
      if (record.FurnaceBatchNo !== undefined && record.furnaceBatchNo === undefined) {
        record.furnaceBatchNo = record.FurnaceBatchNo;
      }
    });
    return data;
  },
});

// 页签切换处理
function handleTabChange(key: string) {
  if (key === 'rawData') {
    nextTick(() => {
      reloadRawTable();
    });
  } else if (key === 'data') {
    // 切换到检测数据页签时，刷新检测数据表格
    nextTick(() => {
      reload();
    });
  }
}

// ========== 导入与日志相关 ==========
// 分步导入向导相关
const [registerStepImportModal, { openPopup: openStepImportModal, closePopup: closeStepImportModal }] = usePopup();

// 导入成功回调
function handleImportSuccess(result: any) {
  // 刷新日志表格和数据表格
  reloadLogTable();
  if (result.data.successCount > 0) {
    reload();
    reloadRawTable();
    // 如果当前不在检测数据页签，切换到检测数据页签以便用户看到新导入的数据
    if (activeTab.value !== 'data') {
      activeTab.value = 'data';
      // 切换到检测数据页签后，确保数据已刷新
      nextTick(() => {
        reload();
      });
    }
  }
}

// 导入日志表格
const logColumns: BasicColumn[] = [
  { title: '文件名', dataIndex: 'fileName', key: 'fileName', align: 'center', width: 200 },
  { title: '导入时间', dataIndex: 'importTime', key: 'importTime', align: 'center', width: 180 },
  { title: '操作人', dataIndex: 'operatorName', key: 'operatorName', align: 'center', width: 120 },
  { title: '总行数', dataIndex: 'totalRows', key: 'totalRows', align: 'center', width: 150 },
  { title: '状态', dataIndex: 'status', key: 'status', align: 'center', width: 120 },
];

const [registerLogTable, { reload: reloadLogTable }] = useTable({
  api: getImportLogList,
  columns: logColumns,
  useSearchForm: false,
  pagination: {
    pageSize: 10,
  },
});

// 获取状态颜色
function getStatusColor(status: string) {
  switch (status) {
    case 'success':
      return 'success';
    case 'partial':
      return 'warning';
    case 'failed':
      return 'error';
    default:
      return 'default';
  }
}

// 获取状态文本
function getStatusText(status: string) {
  switch (status) {
    case 'success':
      return '成功';
    case 'partial':
      return '部分失败';
    case 'failed':
      return '失败';
    default:
      return '未知';
  }
}

// 格式化总行数显示（如：125 / 124 ok）
function formatTotalRows(record: any) {
  const total = record.totalRows || 0;
  const success = record.successRows || record.totalRows || 0;
  if (total === 0) return '-';
  return `${total} / ${success} ok`;
}

// ========== 导入相关方法 ==========


// ========== 分步导入向导相关方法 ==========

// 处理分步导入
function handleStepImport() {
  openStepImportModal(true);
}

function handleStepImportCancel() {
  closeStepImportModal();
}

// 分步导入成功回调
function handleStepImportSuccess() {
  // 刷新日志表格
  reloadLogTable();
  // 刷新检测数据表格
  reload();
  // 刷新原始数据表格
  reloadRawTable();
  // 如果当前不在检测数据页签，切换到检测数据页签以便用户看到新导入的数据
  if (activeTab.value !== 'data') {
    activeTab.value = 'data';
    // 切换到检测数据页签后，确保数据已刷新
    nextTick(() => {
      reload();
    });
  }
  createMessage.success('分步导入完成！');
}
</script>

<style scoped>
.import-log-content {
  padding: 16px 0;
}

.import-log-table {
  padding: 16px 0;
}

.import-method-card {
  cursor: pointer;
  transition: all 0.3s;
  height: 100%;
}

.import-method-card :deep(.ant-card-body) {
  padding: 12px 16px;
}

.import-method-card:hover {
  border-color: #1890ff;
  box-shadow: 0 2px 8px rgba(0, 0, 0, 0.15);
}

.method-title {
  display: flex;
  align-items: center;
  font-size: 16px;
  font-weight: 500;
}

.method-content {
  padding: 4px 0;
}

.method-description {
  color: #666;
  margin-bottom: 8px;
  font-size: 14px;
}

.method-features {
  .feature-item {
    display: flex;
    align-items: center;
    color: #666;
    font-size: 12px;
    margin-bottom: 2px;
  }
}

/* 表格工具栏 */
.table-toolbar {
  margin-bottom: 1px;
  display: flex;
  align-items: center;
  gap: 16px;
}

/* 针对工业大表格的样式微调 */
:deep(.ant-table-thead > tr > th) {
  padding: 4px 8px;
  font-size: 12px;
  background-color: #fafafa;
  text-align: center !important;
}

/* 表格单元格紧凑布局 */
:deep(.ant-table-tbody > tr > td) {
  padding: 4px 8px;
  font-size: 12px;
}

/* 负数红色显示 */
.text-danger {
  color: #f5222d;
}
</style>
