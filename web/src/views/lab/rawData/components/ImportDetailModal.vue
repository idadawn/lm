<template>
  <BasicModal
    v-bind="$attrs"
    @register="registerModal"
    :title="modalTitle"
    :width="1200"
    :footer="null"
    destroyOnClose>
    
    <div v-if="loading" class="loading-state">
      <a-spin tip="正在加载详情..." />
    </div>
    
    <div v-else-if="error" class="error-state">
      <a-result
        status="error"
        title="加载失败"
        :sub-title="error">
        <template #extra>
          <a-button type="primary" @click="loadData">
            重试
          </a-button>
        </template>
      </a-result>
    </div>
    
    <div v-else class="import-detail-content">
      <!-- 基本信息 -->
      <div class="basic-info-section" style="margin-bottom: 24px">
        <a-card title="基本信息" :bordered="false" size="small">
          <a-descriptions bordered :column="3" size="small">
            <a-descriptions-item label="文件名">{{ detailData.fileName || '-' }}</a-descriptions-item>
            <a-descriptions-item label="导入时间">{{ formatDateTime(detailData.importTime) }}</a-descriptions-item>
            <a-descriptions-item label="状态">
              <a-tag :color="getStatusColor(detailData.status)">
                {{ getStatusText(detailData.status) }}
              </a-tag>
            </a-descriptions-item>
            <a-descriptions-item label="导入策略">
              <a-tag :color="getStrategyColor(detailData.importStrategy)">
                {{ getStrategyText(detailData.importStrategy) }}
              </a-tag>
            </a-descriptions-item>
            <a-descriptions-item label="操作人">{{ detailData.operatorName || '-' }}</a-descriptions-item>
            <a-descriptions-item label="创建时间">{{ formatDateTime(detailData.createTime) }}</a-descriptions-item>
          </a-descriptions>
        </a-card>
      </div>

      <!-- 统计信息 -->
      <div class="stats-section" style="margin-bottom: 24px">
        <a-card title="统计信息" :bordered="false" size="small">
          <a-row :gutter="16">
            <a-col :span="6">
              <a-statistic
                title="总行数"
                :value="detailData.totalRows || 0"
                :value-style="{ color: '#1890ff' }" />
            </a-col>
            <a-col :span="6">
              <a-statistic
                title="有效数据"
                :value="detailData.validDataRows || 0"
                :value-style="{ color: '#3f8600' }" />
            </a-col>
            <a-col :span="6">
              <a-statistic
                title="失败数据"
                :value="(detailData.totalRows || 0) - (detailData.validDataRows || 0)"
                :value-style="{ color: '#cf1322' }" />
            </a-col>
            <a-col :span="6">
              <a-statistic
                title="成功率"
                :value="getSuccessRate()"
                suffix="%"
                :value-style="{ color: getSuccessRateColor() }" />
            </a-col>
          </a-row>
        </a-card>
      </div>

      <!-- 数据详情标签页 -->
      <div class="data-detail-section">
        <a-tabs v-model:activeKey="activeTab">
          <!-- 成功数据 -->
          <a-tab-pane key="successData" tab="成功数据">
            <SuccessDataTable 
              :import-log-id="importLogId"
              v-if="activeTab === 'successData'" />
          </a-tab-pane>
          
          <!-- 失败数据 -->
          <a-tab-pane key="failedData" tab="失败数据">
            <FailedDataTable 
              :import-log-id="importLogId"
              v-if="activeTab === 'failedData'" />
          </a-tab-pane>
          
          <!-- 有效数据 -->
          <a-tab-pane key="validData" tab="有效数据">
            <ValidDataTable 
              :import-log-id="importLogId"
              v-if="activeTab === 'validData'" />
          </a-tab-pane>
          
          <!-- 操作日志 -->
          <a-tab-pane key="operationLog" tab="操作日志">
            <OperationLogTable 
              :import-log-id="importLogId"
              v-if="activeTab === 'operationLog'" />
          </a-tab-pane>
        </a-tabs>
      </div>

      <!-- 操作按钮 -->
      <div class="action-buttons" style="margin-top: 24px; text-align: right">
        <a-space>
          <a-button 
            @click="handleDownloadSourceFile"
            :disabled="!detailData.sourceFileId">
            <template #icon><DownloadOutlined /></template>
            下载源文件
          </a-button>
          
          <a-button 
            @click="handleDownloadErrorReport"
            :disabled="!hasErrorData">
            <template #icon><FileExcelOutlined /></template>
            下载错误报告
          </a-button>
          
          <a-button 
            type="primary" 
            @click="handleClose">
            关闭
          </a-button>
        </a-space>
      </div>
    </div>
  </BasicModal>
</template>

<script lang="ts" setup>
  import { ref, computed, watch } from 'vue';
  import { BasicModal, useModalInner } from '/@/components/Modal';
  import { useMessage } from '/@/hooks/web/useMessage';
  import { useI18n } from '/@/hooks/web/useI18n';
  import { formatToDateTime } from '/@/utils/dateUtil';
  import { 
    getImportLogDetail,
    downloadSourceFile,
    downloadErrorReport
  } from '/@/api/lab/rawData';
  import { DownloadOutlined, FileExcelOutlined } from '@ant-design/icons-vue';
  
  // 导入子组件
  import SuccessDataTable from './import-detail/SuccessDataTable.vue';
  import FailedDataTable from './import-detail/FailedDataTable.vue';
  import ValidDataTable from './import-detail/ValidDataTable.vue';
  import OperationLogTable from './import-detail/OperationLogTable.vue';

  const emit = defineEmits(['register', 'reload']);
  const [registerModal, { closeModal }] = useModalInner(init);
  const { createMessage } = useMessage();
  const { t } = useI18n();

  // 状态
  const loading = ref(false);
  const error = ref<string>('');
  const importLogId = ref<string>('');
  const detailData = ref<any>({});
  const activeTab = ref('successData');

  // 计算属性
  const modalTitle = computed(() => {
    return `导入详情 - ${detailData.value.fileName || ''}`;
  });

  const hasErrorData = computed(() => {
    return (detailData.value.totalRows || 0) > (detailData.value.validDataRows || 0);
  });

  // 初始化
  function init(data: any) {
    importLogId.value = data.importLogId || '';
    if (importLogId.value) {
      loadData();
    }
  }

  // 加载数据
  async function loadData() {
    if (!importLogId.value) {
      error.value = '导入记录ID不能为空';
      return;
    }

    try {
      loading.value = true;
      error.value = '';
      
      const response = await getImportLogDetail(importLogId.value);
      detailData.value = response.data || {};
      
    } catch (err: any) {
      console.error('加载导入详情失败:', err);
      error.value = err.message || '加载失败，请重试';
    } finally {
      loading.value = false;
    }
  }

  // 辅助函数
  function formatDateTime(date: string | Date): string {
    if (!date) return '-';
    return formatToDateTime(date);
  }

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

  function getSuccessRate(): number {
    const total = detailData.value.totalRows || 0;
    const valid = detailData.value.validDataRows || 0;
    if (total === 0) return 0;
    return Math.round((valid / total) * 100);
  }

  function getSuccessRateColor(): string {
    const rate = getSuccessRate();
    if (rate >= 90) return '#3f8600';
    if (rate >= 70) return '#faad14';
    return '#cf1322';
  }

  // 下载源文件
  async function handleDownloadSourceFile() {
    if (!detailData.value.sourceFileId) {
      createMessage.warning('该记录没有源文件');
      return;
    }

    try {
      loading.value = true;
      await downloadSourceFile(detailData.value.sourceFileId, detailData.value.fileName);
      createMessage.success('文件下载成功');
    } catch (err: any) {
      console.error('下载源文件失败:', err);
      createMessage.error('下载失败，请重试');
    } finally {
      loading.value = false;
    }
  }

  // 下载错误报告
  async function handleDownloadErrorReport() {
    if (!hasErrorData.value) {
      createMessage.warning('该记录没有错误数据');
      return;
    }

    try {
      loading.value = true;
      await downloadErrorReport(importLogId.value);
      createMessage.success('错误报告下载成功');
    } catch (err: any) {
      console.error('下载错误报告失败:', err);
      createMessage.error('下载失败，请重试');
    } finally {
      loading.value = false;
    }
  }

  // 关闭弹窗
  function handleClose() {
    closeModal();
    emit('reload');
  }

  // 监听导入记录ID变化
  watch(() => importLogId.value, (newVal) => {
    if (newVal) {
      loadData();
    }
  });
</script>

<style lang="less" scoped>
.import-detail-content {
  min-height: 600px;
}

.loading-state {
  display: flex;
  justify-content: center;
  align-items: center;
  min-height: 400px;
}

.error-state {
  min-height: 400px;
  display: flex;
  align-items: center;
  justify-content: center;
}

.basic-info-section {
  :deep(.ant-card-body) {
    padding: 16px;
  }
}

.stats-section {
  :deep(.ant-card-body) {
    padding: 24px;
  }
}

.data-detail-section {
  :deep(.ant-tabs-content) {
    padding-top: 16px;
  }
}
</style>