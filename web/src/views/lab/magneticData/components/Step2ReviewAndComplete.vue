<template>
  <div class="step2-container">
    <!-- 如果还没有导入会话ID，显示提示 -->
    <div v-if="!hasValidSessionId" class="waiting-section">
      <a-empty description="请先完成前面的步骤" />
    </div>

    <!-- 加载中状态 -->
    <div v-else-if="loading" class="loading-section">
      <a-spin size="large" tip="正在加载核对数据..." />
    </div>

    <!-- 导入结果 -->
    <div v-else class="result-section">
      <!-- 成功状态 -->
      <a-result
        v-if="importSuccess"
        status="success"
        title="数据导入成功"
        :sub-title="`成功更新 ${stats.updatedRows} 条数据`">
        <template #extra>
          <div class="result-stats">
            <a-row :gutter="24" style="margin-bottom: 24px">
              <a-col :span="8">
                <a-statistic title="总数据行" :value="stats.totalRows" />
              </a-col>
              <a-col :span="8">
                <a-statistic title="有效数据" :value="stats.validDataRows" :value-style="{ color: '#52c41a' }" />
              </a-col>
              <a-col :span="8">
                <a-statistic title="已更新" :value="stats.updatedRows" :value-style="{ color: '#1890ff' }" />
              </a-col>
            </a-row>
            <a-row :gutter="24" v-if="stats.skippedRows > 0">
              <a-col :span="24">
                <a-statistic title="跳过数据" :value="stats.skippedRows" :value-style="{ color: '#faad14' }" />
              </a-col>
            </a-row>
          </div>
        </template>
      </a-result>

      <!-- 失败状态 -->
      <a-result
        v-else-if="importError"
        status="error"
        title="数据导入失败"
        :sub-title="importError">
        <template #extra>
          <a-button type="primary" @click="handleRetry">重试</a-button>
        </template>
      </a-result>

      <!-- 核对数据状态（默认显示） -->
      <div v-else class="review-section">
        <div class="review-header">
          <a-alert
            type="info"
            show-icon
            style="margin-bottom: 16px">
            <template #message>
              <div class="alert-message">
                <span>第二步：核对最优数据并确认导入</span>
                <a-tooltip placement="bottomRight">
                  <template #title>
                    <div class="rules-tooltip">
                      <p><strong>最优数据选择规则：</strong></p>
                      <ol>
                        <li>优先级1：H列(Ps铁损)值最小</li>
                        <li>优先级2：I列(Ss激磁功率)值最小</li>
                        <li>优先级3：F列(Hc)值最小</li>
                        <li>优先级4：P列(检测时间)最新</li>
                      </ol>
                      <p><strong>保存规则：</strong>带K保存到刻痕后，不带K保存到正常性能指标。</p>
                    </div>
                  </template>
                  <a-button type="link" size="small"><InfoCircleOutlined /> 匹配规则说明</a-button>
                </a-tooltip>
              </div>
            </template>
          </a-alert>

          <!-- 顶部精简统计 -->
          <div class="mini-stats">
            <a-space :size="24">
              <div class="stat-item">
                <span class="label">解析总行数:</span>
                <span class="value">{{ stats.totalRows }}</span>
              </div>
              <div class="stat-item primary">
                <span class="label">拟导入最优数:</span>
                <span class="value">{{ bestDataCount }}</span>
              </div>
              <div class="stat-item warning" v-if="reviewData.errors && reviewData.errors.length > 0">
                <span class="label">解析错误数:</span>
                <span class="value">{{ reviewData.errors.length }}</span>
              </div>
            </a-space>
          </div>
        </div>

        <a-tabs v-model:activeKey="activeTab" class="review-tabs">
          <!-- 标签页 1：拟导入最优数据 -->
          <a-tab-pane key="best" tab="拟导入最优数据">
            <div class="tab-content">
              <a-table
                :columns="bestDataColumns"
                :data-source="bestDataList"
                :pagination="{ pageSize: 10, size: 'small' }"
                size="small"
                :scroll="{ y: 400 }"
                bordered>
                <template #bodyCell="{ column, record }">
                  <template v-if="column.key === 'isScratched'">
                    <a-tag :color="(record.isScratched || record.IsScratched) ? 'orange' : 'blue'">
                      {{ (record.isScratched || record.IsScratched) ? '刻痕后' : '正常' }}
                    </a-tag>
                  </template>
                  <template v-else-if="column.key === 'psLoss'">
                    <span class="highlight-value">{{ record.psLoss ?? record.PsLoss }}</span>
                  </template>
                  <template v-else-if="column.key === 'detectionTime'">
                    {{ (record.detectionTime || record.DetectionTime) ? formatToDateTime(record.detectionTime || record.DetectionTime) : '-' }}
                  </template>

                </template>
              </a-table>
            </div>
          </a-tab-pane>

          <!-- 标签页 2：完整核对明细 (全部有效数据) -->
          <a-tab-pane key="all" tab="完整核对明细">
            <div class="tab-content">
              <a-table
                :columns="validDataColumns"
                :data-source="getValidData"
                :pagination="{ pageSize: 15, size: 'small' }"
                size="small"
                :scroll="{ y: 400 }"
                :row-class-name="getRowClassName">

                <template #bodyCell="{ column, record }">
                  <template v-if="column.key === 'isScratched'">
                    <a-tag :color="(record.isScratched || record.IsScratched) ? 'orange' : 'blue'">
                      {{ (record.isScratched || record.IsScratched) ? '是' : '否' }}
                    </a-tag>
                  </template>
                  <template v-else-if="column.key === 'isBest'">
                    <a-tag v-if="record.isBest || record.IsBest" color="success">
                      <CheckCircleOutlined /> 最优
                    </a-tag>
                    <span v-else>-</span>
                  </template>
                  <template v-else-if="column.key === 'detectionTime'">
                    {{ (record.detectionTime || record.DetectionTime) ? formatToDateTime(record.detectionTime || record.DetectionTime) : '-' }}
                  </template>

                </template>
              </a-table>
            </div>
          </a-tab-pane>

          <!-- 标签页 3：解析异常 (如果有) -->
          <a-tab-pane key="error" v-if="reviewData.errors && reviewData.errors.length > 0">
            <template #tab>
              <span style="color: #ff4d4f">
                解析异常 ({{ reviewData.errors.length }})
              </span>
            </template>
            <div class="tab-content">
              <div class="error-list">
                <a-list size="small" bordered :data-source="reviewData.errors">
                  <template #renderItem="{ item }">
                    <a-list-item>
                      <a-typography-text type="danger">
                        <CloseCircleOutlined /> {{ item }}
                      </a-typography-text>
                    </a-list-item>
                  </template>
                </a-list>
              </div>
            </div>
          </a-tab-pane>
        </a-tabs>

        <!-- 底部操作栏 -->
        <div class="review-actions">
          <a-space>
            <a-button @click="handleRefreshReview" :loading="loadingReview">
              <ReloadOutlined /> 刷新数据
            </a-button>
            <a-button @click="handleStartImport" type="primary" size="large" :loading="importing">
              <CloudUploadOutlined /> 确认并完成最终导入 ({{ bestDataCount }} 条)
            </a-button>
          </a-space>
        </div>
      </div>

    </div>
  </div>
</template>

<script lang="ts" setup>
import { ref, computed, watch, onMounted } from 'vue';
import { message } from 'ant-design-vue';
import { 
  CheckCircleOutlined, 
  ReloadOutlined, 
  InfoCircleOutlined, 
  CloudUploadOutlined,
  CloseCircleOutlined
} from '@ant-design/icons-vue';
import { getMagneticImportReview, completeMagneticImport } from '/@/api/lab/magneticData';
import { formatToDateTime } from '/@/utils/dateUtil';


const props = defineProps({
  importSessionId: {
    type: String,
    required: true,
  },
});

const emit = defineEmits(['complete', 'prev', 'cancel']);

// 状态
const activeTab = ref('best');
const loading = ref(true); // 初始加载状态
const loadingReview = ref(false); // 加载核对数据状态
const importing = ref(false); // 导入执行状态
const importSuccess = ref(false);

const importError = ref('');
const reviewData = ref<any>({});
const hasLoadedReview = ref(false); // 标记是否已加载过核对数据

// 统计数据
const stats = computed(() => {
  const data = reviewData.value || {};
  return {
    totalRows: data.totalRows ?? data.TotalRows ?? 0,
    validDataRows: data.validDataRows ?? data.ValidDataRows ?? 0,
    updatedRows: data.updatedRows ?? data.UpdatedRows ?? 0,
    skippedRows: data.skippedRows ?? data.SkippedRows ?? 0,
  };
});

// 获取有效数据列表（处理大小写兼容）
const getValidData = computed(() => {
  return reviewData.value?.validData || reviewData.value?.ValidData || [];
});

// 最优数据数量
const bestDataCount = computed(() => {
  return getValidData.value.filter((item: any) => item.isBest || item.IsBest).length;
});

// 最优数据列表
const bestDataList = computed(() => {
  return getValidData.value.filter((item: any) => item.isBest || item.IsBest);
});


// 表格行类名（用于突出显示最优数据）
function getRowClassName(record: any) {
  return (record.isBest || record.IsBest) ? 'best-data-row' : '';
}


// 有效数据表格列定义
const validDataColumns = [
  { title: '行号', dataIndex: 'rowIndex', key: 'rowIndex', width: 70 },
  { title: '原始炉号', dataIndex: 'originalFurnaceNo', key: 'originalFurnaceNo', width: 150 },
  { title: '炉号', dataIndex: 'furnaceNo', key: 'furnaceNo', width: 120 },
  { title: '是否刻痕', key: 'isScratched', width: 100 },
  { title: 'Ps铁损(H)', dataIndex: 'psLoss', key: 'psLoss', width: 100 },
  { title: 'Ss激磁功率(I)', dataIndex: 'ssPower', key: 'ssPower', width: 120 },
  { title: 'Hc(F)', dataIndex: 'hc', key: 'hc', width: 100 },
  { title: '检测时间(P)', key: 'detectionTime', width: 150 },
  { title: '是否最优', key: 'isBest', width: 100, align: 'center' },
];

// 最优数据汇总表格列定义
const bestDataColumns = [
  { title: '行号', dataIndex: 'rowIndex', key: 'rowIndex', width: 70 },
  { title: '原始炉号', dataIndex: 'originalFurnaceNo', key: 'originalFurnaceNo', width: 150 },
  { title: '炉号', dataIndex: 'furnaceNo', key: 'furnaceNo', width: 120 },
  { title: '是否刻痕', key: 'isScratched', width: 150 },
  { title: 'Ps铁损(H)', dataIndex: 'psLoss', key: 'psLoss', width: 100, className: 'column-primary' },
  { title: 'Ss激磁功率(I)', dataIndex: 'ssPower', key: 'ssPower', width: 120 },
  { title: 'Hc(F)', dataIndex: 'hc', key: 'hc', width: 100 },
  { title: '检测时间(P)', key: 'detectionTime', width: 150 },
];


// 暴露给父组件的方法
const canGoNext = computed(() => importSuccess.value);

async function saveAndNext() {
  if (importSuccess.value) {
    emit('complete');
  }
}

// 验证会话ID
const hasValidSessionId = computed(() => {
  return props.importSessionId && props.importSessionId.trim() !== '';
});

// 加载核对数据
async function loadReviewData() {
  // 验证导入会话ID
  if (!props.importSessionId || props.importSessionId.trim() === '') {
    loading.value = false;
    return;
  }

  loadingReview.value = true;
  importError.value = '';
  importSuccess.value = false;

  try {
    // 获取核对数据
    const review = await getMagneticImportReview(props.importSessionId);
    console.log('[Step2] Review data:', review); // 调试日志
    reviewData.value = review;

    // 检查是否有有效数据
    const validCount = review.validDataRows || 0;
    const totalCount = review.totalRows || 0;
    console.log(`[Step2] Total: ${totalCount}, Valid: ${validCount}`); // 调试日志
    
    if (validCount === 0) {
      if (totalCount > 0) {
        importError.value = `共解析 ${totalCount} 行数据，但没有有效数据。请检查数据格式是否正确。`;
      } else {
        importError.value = '没有解析到任何数据，请返回第一步重新解析';
      }
      message.warning(importError.value);
      hasLoadedReview.value = false;
      return;
    }

    // 标记为已加载
    hasLoadedReview.value = true;
  } catch (error: any) {
    // 处理后端返回的特定错误消息
    const errorMsg = error.message || error.msg || '';
    if (errorMsg.includes('解析数据文件不存在') || errorMsg.includes('请重新解析')) {
      importError.value = '数据解析未完成或已过期，请返回第一步重新解析数据';
    } else if (errorMsg.includes('导入会话不存在')) {
      importError.value = '导入会话不存在，请重新开始导入';
    } else {
      importError.value = errorMsg || '加载核对数据失败';
    }
    message.error(importError.value);
    hasLoadedReview.value = false;
  } finally {
    loading.value = false;
    loadingReview.value = false;
  }
}

// 刷新核对数据
async function handleRefreshReview() {
  await loadReviewData();
}

// 开始导入
async function handleStartImport() {
  if (!props.importSessionId || props.importSessionId.trim() === '') {
    message.error('导入会话ID缺失');
    return;
  }

  importing.value = true;
  importError.value = '';
  importSuccess.value = false;

  try {
    // 执行导入
    await completeMagneticImport(props.importSessionId);

    // 重新加载核对数据以获取更新后的统计信息
    await loadReviewData();

    // 标记为成功
    importSuccess.value = true;
    message.success('数据导入成功');
    emit('complete');
  } catch (error: any) {
    const errorMsg = error.message || error.msg || '导入失败';
    importError.value = errorMsg;
    message.error(errorMsg);
  } finally {
    importing.value = false;
  }
}

// 重试
function handleRetry() {
  importError.value = '';
  importSuccess.value = false;
  loadReviewData();
}

// 监听导入会话ID变化
watch(
  () => props.importSessionId,
  (newId) => {
    if (newId && newId.trim() !== '') {
      loadReviewData();
    }
  },
  { immediate: true }
);

// 组件挂载时加载数据
onMounted(() => {
  if (props.importSessionId && props.importSessionId.trim() !== '') {
    loadReviewData();
  } else {
    loading.value = false;
  }
});

// 暴露给父组件
defineExpose({
  canGoNext,
  saveAndNext,
});
</script>

<style lang="less" scoped>
.step2-container {
  padding: 24px;
  height: 100%;
  min-height: 500px;
}


.waiting-section,
.loading-section {
  display: flex;
  justify-content: center;
  align-items: center;
  min-height: 400px;
}

.result-section {
  .result-stats {
    margin-top: 24px;
  }
}

.review-section {
  display: flex;
  flex-direction: column;
  height: 100%;

  .alert-message {
    display: flex;
    justify-content: space-between;
    align-items: center;
    width: 100%;
  }

  .mini-stats {
    padding: 12px 16px;
    background: #f8f9fa;
    border-radius: 4px;
    margin-bottom: 16px;
    border: 1px solid #f0f0f0;

    .stat-item {
      .label {
        color: #8c8c8c;
        margin-right: 8px;
        font-size: 13px;
      }
      .value {
        font-weight: 600;
        font-size: 16px;
        color: #262626;
      }

      &.primary .value {
        color: #52c41a;
      }
      &.warning .value {
        color: #faad14;
      }
    }
  }

  .review-tabs {
    flex: 1;
    min-height: 0;

    :deep(.ant-tabs-content) {
      height: 100%;
    }

    .tab-content {
      padding: 8px 0;
    }
  }

  .highlight-value {
    color: #52c41a;
    font-weight: 600;
  }

  :deep(.column-primary) {
    background-color: #f6ffed;
  }

  .error-list {
    max-height: 400px;
    overflow-y: auto;
  }

  .valid-data-section {
    :deep(.ant-table-tbody tr.best-data-row) {
      background-color: #f6ffed;
      border-left: 3px solid #52c41a;
    }
    
    :deep(.ant-table-tbody tr.best-data-row:hover) {
      background-color: #f0f9e8;
    }
  }

  .review-actions {
    margin-top: 16px;
    padding-top: 16px;
    border-top: 1px solid #f0f0f0;
    text-align: right;
  }
}

.rules-tooltip {
  padding: 4px;
  p {
    margin-bottom: 8px;
  }
  ol {
    padding-left: 18px;
    margin-bottom: 8px;
    li {
      margin-bottom: 2px;
    }
  }
}
</style>

