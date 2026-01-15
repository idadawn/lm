<template>
  <div class="import-history-container">
    <!-- 页面标题 -->
    <div class="page-header" style="margin-bottom: 24px">
      <a-page-header
        title="导入历史管理"
        sub-title="查看和管理原始数据导入历史记录">
        <template #extra>
          <a-space>
            <a-button @click="handleRefresh">
              <template #icon><ReloadOutlined /></template>
              刷新
            </a-button>
            <a-button type="primary" @click="handleNewImport">
              <template #icon><UploadOutlined /></template>
              新建导入
            </a-button>
          </a-space>
        </template>
      </a-page-header>
    </div>

    <!-- 筛选条件 -->
    <div class="filter-section" style="margin-bottom: 24px">
      <a-card title="筛选条件" :bordered="false" size="small">
        <a-form
          ref="filterFormRef"
          :model="filterForm"
          layout="inline"
          @finish="handleFilterSubmit">
          <a-form-item label="文件名">
            <a-input
              v-model:value="filterForm.fileName"
              placeholder="输入文件名"
              style="width: 200px" />
          </a-form-item>
          
          <a-form-item label="导入时间">
            <a-range-picker
              v-model:value="filterForm.importTimeRange"
              style="width: 300px" />
          </a-form-item>
          
          <a-form-item label="状态">
            <a-select
              v-model:value="filterForm.status"
              placeholder="选择状态"
              style="width: 150px"
              :options="statusOptions"
              allow-clear />
          </a-form-item>
          
          <a-form-item label="导入策略">
            <a-select
              v-model:value="filterForm.importStrategy"
              placeholder="选择策略"
              style="width: 150px"
              :options="strategyOptions"
              allow-clear />
          </a-form-item>
          
          <a-form-item>
            <a-space>
              <a-button type="primary" html-type="submit">
                <template #icon><SearchOutlined /></template>
                搜索
              </a-button>
              <a-button @click="handleResetFilter">
                <template #icon><RedoOutlined /></template>
                重置
              </a-button>
            </a-space>
          </a-form-item>
        </a-form>
      </a-card>
    </div>

    <!-- 导入历史表格 -->
    <div class="history-table">
      <a-card :bordered="false">
        <BasicTable
          @register="registerTable"
          :row-selection="rowSelection">
          
          <!-- 自定义列渲染 -->
          <template #bodyCell="{ column, record }">
            <!-- 状态列 -->
            <template v-if="column.key === 'status'">
              <a-tag :color="getStatusColor(record.status)">
                {{ getStatusText(record.status) }}
              </a-tag>
            </template>
            
            <!-- 导入策略列 -->
            <template v-else-if="column.key === 'importStrategy'">
              <a-tag :color="getStrategyColor(record.importStrategy)">
                {{ getStrategyText(record.importStrategy) }}
              </a-tag>
            </template>
            
            <!-- 统计信息列 -->
            <template v-else-if="column.key === 'stats'">
              <div class="stats-cell">
                <div class="stat-item">
                  <span class="stat-label">总行数：</span>
                  <span class="stat-value">{{ record.totalRows || 0 }}</span>
                </div>
                <div class="stat-item">
                  <span class="stat-label">有效数据：</span>
                  <span class="stat-value" style="color: #3f8600">
                    {{ record.validDataRows || 0 }}
                  </span>
                </div>
                <div class="stat-item">
                  <span class="stat-label">失败：</span>
                  <span class="stat-value" style="color: #cf1322">
                    {{ (record.totalRows || 0) - (record.validDataRows || 0) }}
                  </span>
                </div>
              </div>
            </template>
            
            <!-- 操作列 -->
            <template v-else-if="column.key === 'actions'">
              <a-space size="small">
                <a-button type="link" size="small" @click="handleViewDetail(record)">
                  <template #icon><EyeOutlined /></template>
                  详情
                </a-button>
                
                <a-button 
                  type="link" 
                  size="small" 
                  @click="handleDownloadSourceFile(record)"
                  :disabled="!record.sourceFileId">
                  <template #icon><DownloadOutlined /></template>
                  源文件
                </a-button>
                
                <a-button 
                  type="link" 
                  size="small" 
                  @click="handleDownloadErrorReport(record)"
                  :disabled="!hasErrorData(record)">
                  <template #icon><FileExcelOutlined /></template>
                  错误报告
                </a-button>
                
                <a-dropdown>
                  <a-button type="link" size="small">
                    更多
                    <DownOutlined />
                  </a-button>
                  <template #overlay>
                    <a-menu>
                      <a-menu-item 
                        @click="handleDelete(record)"
                        :disabled="!canDelete(record)">
                        <template #icon><DeleteOutlined /></template>
                        删除
                      </a-menu-item>
                      <a-menu-item 
                        @click="handleRestartImport(record)"
                        :disabled="!canRestart(record)">
                        <template #icon><RedoOutlined /></template>
                        重新导入
                      </a-menu-item>
                    </a-menu>
                  </template>
                </a-dropdown>
              </a-space>
            </template>
            
            <!-- 时间列 -->
            <template v-else-if="column.key === 'importTime'">
              {{ formatDateTime(record.importTime) }}
            </template>
            
            <template v-else-if="column.key === 'createTime'">
              {{ formatDateTime(record.createTime) }}
            </template>
          </template>
        </BasicTable>
      </a-card>
    </div>

    <!-- 批量操作 -->
    <div v-if="selectedRowKeys.length > 0" class="batch-actions" style="margin-top: 16px">
      <a-alert
        message="批量操作"
        :description="`已选择 ${selectedRowKeys.length} 条记录`"
        type="info"
        show-icon
        style="margin-bottom: 16px" />
      
      <a-space>
        <a-button @click="handleBatchDelete" :disabled="!canBatchDelete">
          <template #icon><DeleteOutlined /></template>
          批量删除
        </a-button>
        
        <a-button @click="handleBatchExport">
          <template #icon><ExportOutlined /></template>
          批量导出
        </a-button>
      </a-space>
    </div>

    <!-- 导入详情弹窗 -->
    <ImportDetailModal 
      @register="registerDetailModal"
      @reload="handleRefresh" />

    <!-- 删除确认弹窗 -->
    <a-modal
      v-model:open="deleteConfirmVisible"
      title="确认删除"
      @ok="handleDeleteConfirm"
      @cancel="handleDeleteCancel">
      <div class="delete-confirm-content">
        <a-alert
          message="警告"
          description="删除导入记录将同时删除相关的原始数据，此操作不可恢复！"
          type="warning"
          show-icon
          style="margin-bottom: 16px" />
        
        <div class="delete-info">
          <p>确定要删除以下导入记录吗？</p>
          <ul>
            <li v-for="record in deletingRecords" :key="record.id">
              {{ record.fileName }} ({{ formatDateTime(record.importTime) }})
            </li>
          </ul>
        </div>
      </div>
    </a-modal>

    <!-- 加载状态 -->
    <div v-if="loading" class="loading-overlay">
      <a-spin tip="正在加载数据..." />
    </div>
  </div>
</template>

<script lang="ts" setup>
  import { ref, computed, onMounted } from 'vue';
  import { useMessage } from '/@/hooks/web/useMessage';
  import { useI18n } from '/@/hooks/web/useI18n';
  import { useModal } from '/@/components/Modal';
  import { BasicTable, useTable } from '/@/components/Table';
  import { formatToDateTime } from '/@/utils/dateUtil';
  import { 
    getImportLogList,
    deleteImportLog,
    downloadSourceFile,
    downloadErrorReport
  } from '/@/api/lab/rawData';
  import ImportDetailModal from './components/ImportDetailModal.vue';
  import type { TableProps } from 'ant-design-vue';
  
  // 图标导入
  import {
    ReloadOutlined,
    UploadOutlined,
    SearchOutlined,
    RedoOutlined,
    EyeOutlined,
    DownloadOutlined,
    FileExcelOutlined,
    DeleteOutlined,
    DownOutlined,
    ExportOutlined,
  } from '@ant-design/icons-vue';

  const { createMessage } = useMessage();
  const { t } = useI18n();
  const [registerDetailModal, { openModal: openDetailModal }] = useModal();

  // 状态
  const loading = ref(false);
  const filterFormRef = ref();
  const deleteConfirmVisible = ref(false);
  
  // 筛选表单
  const filterForm = ref({
    fileName: '',
    importTimeRange: [],
    status: '',
    importStrategy: '',
  });

  // 选择状态
  const selectedRowKeys = ref<string[]>([]);
  const deletingRecords = ref<any[]>([]);

  // 选项配置
  const statusOptions = ref([
    { label: '成功', value: 'success' },
    { label: '部分失败', value: 'partial' },
    { label: '失败', value: 'failed' },
    { label: '进行中', value: 'in_progress' },
    { label: '已取消', value: 'cancelled' },
  ]);

  const strategyOptions = ref([
    { label: '增量导入', value: 'incremental' },
    { label: '全量导入', value: 'full' },
    { label: '覆盖导入', value: 'overwrite' },
    { label: '智能去重', value: 'deduplicate' },
  ]);

  // 表格列配置
  const columns = [
    {
      title: '选择',
      key: 'selection',
      width: 60,
    },
    {
      title: '文件名',
      dataIndex: 'fileName',
      key: 'fileName',
      width: 200,
      sorter: true,
    },
    {
      title: '导入时间',
      dataIndex: 'importTime',
      key: 'importTime',
      width: 180,
      sorter: true,
    },
    {
      title: '状态',
      dataIndex: 'status',
      key: 'status',
      width: 120,
    },
    {
      title: '导入策略',
      dataIndex: 'importStrategy',
      key: 'importStrategy',
      width: 120,
    },
    {
      title: '统计信息',
      key: 'stats',
      width: 200,
    },
    {
      title: '操作人',
      dataIndex: 'operatorName',
      key: 'operatorName',
      width: 120,
    },
    {
      title: '创建时间',
      dataIndex: 'createTime',
      key: 'createTime',
      width: 180,
    },
    {
      title: '操作',
      key: 'actions',
      width: 200,
      fixed: 'right',
    },
  ];

  // 表格配置
  const [registerTable, { reload, getDataSource }] = useTable({
    api: getImportLogList,
    columns,
    useSearchForm: false,
    pagination: {
      pageSize: 20,
      showSizeChanger: true,
      pageSizeOptions: ['10', '20', '50', '100'],
    },
    beforeFetch: (params) => {
      // 应用筛选条件
      if (filterForm.value.fileName) {
        params.fileName = filterForm.value.fileName;
      }
      if (filterForm.value.status) {
        params.status = filterForm.value.status;
      }
      if (filterForm.value.importStrategy) {
        params.importStrategy = filterForm.value.importStrategy;
      }
      if (filterForm.value.importTimeRange?.length === 2) {
        params.startTime = filterForm.value.importTimeRange[0];
        params.endTime = filterForm.value.importTimeRange[1];
      }
      return params;
    },
  });

  // 行选择配置
  const rowSelection = computed<TableProps['rowSelection']>(() => ({
    selectedRowKeys: selectedRowKeys.value,
    onChange: (keys: string[]) => {
      selectedRowKeys.value = keys;
    },
    getCheckboxProps: (record: any) => ({
      disabled: !canDelete(record), // 不能删除的记录不允许选择
    }),
  }));

  // 计算属性
  const canBatchDelete = computed(() => {
    const selectedRecords = getSelectedRecords();
    return selectedRecords.length > 0 && selectedRecords.every(record => canDelete(record));
  });

  // 辅助函数
  function getStatusColor(status: string): string {
    switch (status) {
      case 'success': return 'success';
      case 'partial': return 'warning';
      case 'failed': return 'error';
      case 'in_progress': return 'processing';
      case 'cancelled': return 'default';
      default: return 'default';
    }
  }

  function getStatusText(status: string): string {
    switch (status) {
      case 'success': return '成功';
      case 'partial': return '部分失败';
      case 'failed': return '失败';
      case 'in_progress': return '进行中';
      case 'cancelled': return '已取消';
      default: return status;
    }
  }

  function getStrategyColor(strategy: string): string {
    switch (strategy) {
      case 'incremental': return 'blue';
      case 'full': return 'green';
      case 'overwrite': return 'orange';
      case 'deduplicate': return 'purple';
      default: return 'default';
    }
  }

  function getStrategyText(strategy: string): string {
    switch (strategy) {
      case 'incremental': return '增量导入';
      case 'full': return '全量导入';
      case 'overwrite': return '覆盖导入';
      case 'deduplicate': return '智能去重';
      default: return strategy;
    }
  }

  function formatDateTime(date: string | Date): string {
    if (!date) return '-';
    return formatToDateTime(date);
  }

  function hasErrorData(record: any): boolean {
    return (record.totalRows || 0) > (record.validDataRows || 0);
  }

  function canDelete(record: any): boolean {
    // 只有成功、失败、已取消的记录可以删除
    return ['success', 'failed', 'cancelled'].includes(record.status);
  }

  function canRestart(record: any): boolean {
    // 只有失败或已取消的记录可以重新导入
    return ['failed', 'cancelled'].includes(record.status);
  }

  function getSelectedRecords(): any[] {
    const allData = getDataSource();
    return allData.filter(record => selectedRowKeys.value.includes(record.id));
  }

  // 生命周期
  onMounted(() => {
    loadData();
  });

  // 加载数据
  async function loadData() {
    try {
      loading.value = true;
      await reload();
    } catch (error) {
      console.error('加载导入历史失败:', error);
      createMessage.error('加载数据失败，请重试');
    } finally {
      loading.value = false;
    }
  }

  // 筛选提交
  function handleFilterSubmit() {
    reload();
  }

  // 重置筛选
  function handleResetFilter() {
    filterFormRef.value?.resetFields();
    filterForm.value = {
      fileName: '',
      importTimeRange: [],
      status: '',
      importStrategy: '',
    };
    reload();
  }

  // 刷新数据
  function handleRefresh() {
    selectedRowKeys.value = [];
    reload();
  }

  // 新建导入
  function handleNewImport() {
    // 跳转到原始数据页面
    // TODO: 实现页面跳转
    createMessage.info('请到原始数据页面进行导入');
  }

  // 查看详情
  function handleViewDetail(record: any) {
    openDetailModal(true, { importLogId: record.id });
  }

  // 下载源文件
  async function handleDownloadSourceFile(record: any) {
    if (!record.sourceFileId) {
      createMessage.warning('该记录没有源文件');
      return;
    }

    try {
      loading.value = true;
      await downloadSourceFile(record.sourceFileId, record.fileName);
      createMessage.success('文件下载成功');
    } catch (error) {
      console.error('下载源文件失败:', error);
      createMessage.error('下载失败，请重试');
    } finally {
      loading.value = false;
    }
  }

  // 下载错误报告
  async function handleDownloadErrorReport(record: any) {
    if (!hasErrorData(record)) {
      createMessage.warning('该记录没有错误数据');
      return;
    }

    try {
      loading.value = true;
      await downloadErrorReport(record.id);
      createMessage.success('错误报告下载成功');
    } catch (error) {
      console.error('下载错误报告失败:', error);
      createMessage.error('下载失败，请重试');
    } finally {
      loading.value = false;
    }
  }

  // 删除记录
  function handleDelete(record: any) {
    if (!canDelete(record)) {
      createMessage.warning('该记录不能删除');
      return;
    }
    
    deletingRecords.value = [record];
    deleteConfirmVisible.value = true;
  }

  // 批量删除
  function handleBatchDelete() {
    const selectedRecords = getSelectedRecords();
    if (selectedRecords.length === 0) {
      createMessage.warning('请先选择要删除的记录');
      return;
    }

    deletingRecords.value = selectedRecords;
    deleteConfirmVisible.value = true;
  }

  // 确认删除
  async function handleDeleteConfirm() {
    try {
      loading.value = true;
      
      // 批量删除
      const deletePromises = deletingRecords.value.map(record => 
        deleteImportLog(record.id)
      );
      
      await Promise.all(deletePromises);
      
      createMessage.success(`成功删除 ${deletingRecords.value.length} 条记录`);
      deleteConfirmVisible.value = false;
      deletingRecords.value = [];
      selectedRowKeys.value = [];
      
      // 重新加载数据
      await reload();
    } catch (error) {
      console.error('删除记录失败:', error);
      createMessage.error('删除失败，请重试');
    } finally {
      loading.value = false;
    }
  }

  // 取消删除
  function handleDeleteCancel() {
    deleteConfirmVisible.value = false;
    deletingRecords.value = [];
  }

  // 重新导入
  function handleRestartImport(record: any) {
    if (!canRestart(record)) {
      createMessage.warning('该记录不能重新导入');
      return;
    }
    
    // TODO: 实现重新导入逻辑
    createMessage.info('重新导入功能开发中...');
  }

  // 批量导出
  function handleBatchExport() {
    const selectedRecords = getSelectedRecords();
    if (selectedRecords.length === 0) {
      createMessage.warning('请先选择要导出的记录');
      return;
    }
    
    // TODO: 实现批量导出逻辑
    createMessage.info(`批量导出 ${selectedRecords.length} 条记录功能开发中...`);
  }
</script>

<style lang="less" scoped>
.import-history-container {
  position: relative;
  padding: 24px;
}

.filter-section {
  :deep(.ant-card-body) {
    padding: 16px;
  }
}

.history-table {
  :deep(.ant-card-body) {
    padding: 0;
  }
}

.stats-cell {
  .stat-item {
    display: flex;
    justify-content: space-between;
    margin-bottom: 4px;
    font-size: 12px;
    
    &:last-child {
      margin-bottom: 0;
    }
    
    .stat-label {
      color: #666;
    }
    
    .stat-value {
      font-weight: 500;
    }
  }
}

.delete-confirm-content {
  .delete-info {
    padding: 8px 0;
    
    p {
      margin-bottom: 8px;
      font-weight: 500;
    }
    
    ul {
      margin: 0;
      padding-left: 20px;
      
      li {
        margin-bottom: 4px;
        color: #666;
      }
    }
  }
}

.loading-overlay {
  position: absolute;
  top: 0;
  left: 0;
  right: 0;
  bottom: 0;
  background: rgba(255, 255, 255, 0.8);
  display: flex;
  justify-content: center;
  align-items: center;
  z-index: 10;
}
</style>