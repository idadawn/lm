<template>
  <div class="page-content-wrapper">
    <div class="page-content-wrapper-center">
      <div class="page-content-wrapper-content">
        <a-tabs v-model:activeKey="activeTab" type="card" @change="handleTabChange">
          <!-- 第一个页签：检测数据（只显示有效数据） -->
          <a-tab-pane key="data" tab="检测数据">
            <!-- 自定义排序控制 -->
            <div class="table-toolbar">
              <CustomSortControl
                v-model="sortRules"
                @change="handleSortChange"
              />
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
                    <a-card 
                      :hoverable="true" 
                      class="import-method-card"
                      @click="handleStepImport">
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
      <StepImportWizard @register="registerStepImportModal" @reload="handleStepImportSuccess" />
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

  defineOptions({ name: 'RawData' });

  const { createMessage } = useMessage();
  const { t } = useI18n();
  const [registerProductSpecModal, { openModal: openProductSpecModal }] = useModal();
  const [registerDataReviewModal, { openModal: openDataReviewModal }] = useModal();

  // 页签状态
  const activeTab = ref('data');

  // 排序配置（支持多字段排序）
  const sortRules = ref([
    { field: 'prodDate', order: 'asc' as 'asc' | 'desc' },
    { field: 'furnaceNoParsed', order: 'asc' as 'asc' | 'desc' },
    { field: 'coilNo', order: 'asc' as 'asc' | 'desc' },
    { field: 'subcoilNo', order: 'asc' as 'asc' | 'desc' },
    { field: 'lineNo', order: 'asc' as 'asc' | 'desc' }
  ]);

  // 从检测列字符串中解析出所有检测列号（如 "13,15,18,22" -> [13, 15, 18, 22]）
  function parseDetectionColumns(detectionColumnsStr: string): number[] {
    if (!detectionColumnsStr) return [];
    return detectionColumnsStr
      .split(',')
      .map(s => parseInt(s.trim()))
      .filter(n => !isNaN(n) && n > 0)
      .sort((a, b) => a - b);
  }

  // 基础列配置
  const baseColumns: BasicColumn[] = [
    { title: '检测日期', dataIndex: 'prodDateStr', width: 120, fixed: 'left' },
    { title: '原始炉号', dataIndex: 'furnaceNo', width: 150, fixed: 'left' },
    { title: '产线', dataIndex: 'lineNo', width: 80, align: 'center' },
    { title: '班次', dataIndex: 'shift', width: 80, align: 'center' },
    { title: '炉号', dataIndex: 'furnaceNoParsed', width: 100, align: 'center' },
    { title: '卷号', dataIndex: 'coilNo', width: 80, align: 'center' },
    { title: '分卷', dataIndex: 'subcoilNo', width: 80, align: 'center' },
    { title: '宽度', dataIndex: 'width', width: 100, align: 'right' },
    { title: '带材重量', dataIndex: 'coilWeight', width: 120, align: 'right' },
    { title: '产品规格', dataIndex: 'productSpecName', key: 'productSpecName', width: 150 },
    { title: '检测列', dataIndex: 'detectionColumns', key: 'detectionColumns', width: 120 },
    { title: '特性描述', dataIndex: 'featureSuffix', width: 120 },
    { title: '录入人', dataIndex: 'creatorUserName', key: 'creatorUserName', width: 120 },
    { title: '录入日期', dataIndex: 'creatorTime', key: 'creatorTime', width: 160 },
  ];

  // 动态检测列（根据数据中的检测列配置生成）
  const detectionColumnSet = ref<Set<number>>(new Set());
  const detectionColumns = computed(() => {
    return Array.from(detectionColumnSet.value)
      .sort((a, b) => a - b)
      .map(num => ({
        title: `检测${num}`,
        dataIndex: `detection${num}`,
        key: `detection${num}`,
        width: 100,
        align: 'right' as const,
      }));
  });

  const [registerTable, { reload, setColumns, getForm }] = useTable({
    api: getRawDataList,
    columns: baseColumns, // 初始只使用基础列
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
      // 数据加载后，分析所有数据中的检测列，动态生成检测列
      // 优先从实际数据中提取有值的检测列，如果没有则从detectionColumns配置中提取
      const columnSet = new Set<number>();
      data.forEach((record: any) => {
        // 首先检查实际数据中有值的检测列（Detection1-Detection22）
        for (let i = 1; i <= 22; i++) {
          const key = `detection${i}`;
          if (record[key] != null && record[key] !== '') {
            columnSet.add(i);
          }
        }
        // 如果从配置中解析出的列不在实际数据中，也添加（用于显示空列）
        if (record.detectionColumns) {
          const cols = parseDetectionColumns(record.detectionColumns);
          cols.forEach(col => columnSet.add(col));
        }
      });
      // 更新检测列集合
      detectionColumnSet.value = columnSet;
      // 更新表格列配置（使用setTimeout确保computed已更新）
      setTimeout(() => {
        const allColumns = [...baseColumns, ...detectionColumns.value];
        setColumns(allColumns);
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
  // 原始数据列配置
  const rawDataColumns: BasicColumn[] = [
    { title: '行号', dataIndex: 'sortCode', width: 80, fixed: 'left' },
    { title: '检测日期', dataIndex: 'prodDateStr', width: 120 },
    { title: '原始炉号', dataIndex: 'furnaceNo', width: 200 },
    { title: '产线', dataIndex: 'lineNo', width: 80, align: 'center' },
    { title: '班次', dataIndex: 'shift', width: 80, align: 'center' },
    { title: '炉号', dataIndex: 'furnaceNoParsed', width: 80, align: 'center' },
    { title: '卷号', dataIndex: 'coilNo', width: 80, align: 'center' },
    { title: '分卷', dataIndex: 'subcoilNo', width: 80, align: 'center' },
    { title: '宽度', dataIndex: 'width', width: 100, align: 'right' },
    { title: '带材重量', dataIndex: 'coilWeight', width: 120, align: 'right' },
    { title: '特性描述', dataIndex: 'featureSuffix', width: 120 },
    { title: '数据状态', dataIndex: 'isValidData', key: 'isValidData', width: 100, align: 'center' },
    { title: '错误信息', dataIndex: 'importError', width: 200 },
    { title: '录入日期', dataIndex: 'creatorTime', key: 'creatorTime', width: 160 },
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
  });

  // 页签切换处理
  function handleTabChange(key: string) {
    if (key === 'rawData') {
      nextTick(() => {
        reloadRawTable();
      });
    }
  }

  // ========== 导入与日志相关 ==========
  // 分步导入向导相关
  const [registerStepImportModal, { openPopup: openStepImportModal }] = usePopup();

  // 导入成功回调
  function handleImportSuccess(result: any) {
    // 刷新日志表格和数据表格
    reloadLogTable();
    if (result.data.successCount > 0) {
      reload();
      reloadRawTable();
    }
  }

  // 导入日志表格
  const logColumns: BasicColumn[] = [
    { title: '文件名', dataIndex: 'fileName', key: 'fileName', width: 200 },
    { title: '导入时间', dataIndex: 'importTime', key: 'importTime', width: 180 },
    { title: '操作人', dataIndex: 'operatorName', key: 'operatorName', width: 120 },
    { title: '总行数', dataIndex: 'totalRows', key: 'totalRows', width: 150 },
    { title: '状态', dataIndex: 'status', key: 'status', width: 120 },
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

  // ========== 分步导入向导相关方法 ==========

  // 处理分步导入
  function handleStepImport() {
    openStepImportModal(true);
  }

  // 分步导入成功回调
  function handleStepImportSuccess() {
    // 刷新检测数据表格
    reload();
    // 刷新原始数据表格
    reloadRawTable();
    // 刷新日志表格
    reloadLogTable();
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
</style>
