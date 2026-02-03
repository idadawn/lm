<template>
  <div class="step4-container">
    <!-- 如果还没有导入会话ID，显示提示 -->
    <div v-if="!hasValidSessionId" class="waiting-section">
      <a-empty description="请先完成前面的步骤" />
    </div>

    <!-- 加载中状态 / 导入进度 -->
    <div v-else-if="loading || importing" class="loading-section">
      <a-spin size="large" :tip="importProgressTip" />
      <div v-if="importing" class="import-progress" style="margin-top: 20px; width: 60%; max-width: 400px;">
        <a-progress :percent="importProgress" :status="importProgress < 100 ? 'active' : 'success'" />
        <div style="text-align: center; margin-top: 8px; color: #666;">{{ importProgressMessage }}</div>
      </div>
    </div>

    <!-- 导入结果 -->
    <div v-else class="result-section">
      <!-- 提示状态 (无新数据) -->
      <a-result v-if="importSuccess && noChanges" status="info" title="数据无变化"
        :sub-title="noChangesMessage || 'Excel中有效数据全部已存在于数据库，未导入新数据'">
        <template #extra>
          <a-alert type="info" show-icon style="margin-bottom: 24px; text-align: left" message="提示"
            description="本次导入的Excel文件中所有有效数据在系统中已存在，因此未执行新增或更新操作。您可以直接关闭窗口或返回。" />

          <div class="result-stats">
            <a-row :gutter="24" style="margin-bottom: 24px">
              <a-col :span="12">
                <a-statistic title="总读取行数" :value="stats.totalRows" />
              </a-col>
              <a-col :span="12">
                <a-statistic title="重复/已存在行数" :value="stats.totalRows" :value-style="{ color: '#1890ff' }" />
              </a-col>
            </a-row>
          </div>
        </template>
      </a-result>

      <!-- 成功状态 -->
      <a-result v-else-if="importSuccess" status="success" title="数据导入成功"
        :sub-title="`成功导入 ${stats.validDataRows} 条数据`">
        <template #extra>
          <div class="result-stats">
            <a-row :gutter="24" style="margin-bottom: 24px">
              <a-col :span="6">
                <a-statistic title="总数据行" :value="stats.totalRows" />
              </a-col>
              <a-col :span="6">
                <a-statistic title="有效数据" :value="stats.validDataRows" :value-style="{ color: '#52c41a' }" />
              </a-col>
              <a-col :span="6">
                <a-statistic title="产品规格已匹配" :value="stats.productSpecMatchedRows"
                  :value-style="{ color: '#722ed1' }" />
              </a-col>
              <a-col :span="6">
                <a-statistic title="特性已匹配" :value="stats.featureMatchedRows" :value-style="{ color: '#13c2c2' }" />
              </a-col>
            </a-row>
          </div>
        </template>
      </a-result>

      <!-- 失败状态 -->
      <a-result v-else-if="importError" status="error" title="数据导入失败" :sub-title="importError">
        <template #extra>
          <a-button type="primary" @click="handleRetry">重试</a-button>
        </template>
      </a-result>

      <!-- 核对数据状态（默认显示） -->
      <div v-else class="review-section">
        <a-alert message="第五步：数据核对与完成导入" description="请核对以下数据统计信息，确认无误后点击完成导入。" type="info" show-icon
          style="margin-bottom: 24px" />

        <!-- 统计数据 -->
        <div class="review-stats">
          <a-row :gutter="24">
            <a-col :span="6">
              <a-statistic title="总数据行" :value="stats.totalRows" />
            </a-col>
            <a-col :span="6">
              <a-statistic title="有效数据" :value="stats.validDataRows" :value-style="{ color: '#52c41a' }" />
            </a-col>
            <a-col :span="6">
              <a-statistic title="产品规格已匹配" :value="stats.productSpecMatchedRows" :value-style="{ color: '#722ed1' }" />
            </a-col>
            <a-col :span="6">
              <a-statistic title="特性已匹配" :value="stats.featureMatchedRows" :value-style="{ color: '#13c2c2' }" />
            </a-col>
          </a-row>
        </div>

        <!-- 错误提示 -->
        <div v-if="reviewData.errors && reviewData.errors.length > 0" class="error-alert" style="margin-top: 24px">
          <a-alert type="warning" show-icon>
            <template #message>数据核对发现问题</template>
            <template #description>
              <ul style="margin: 0; padding-left: 20px">
                <li v-for="(error, index) in reviewData.errors" :key="index">{{ error }}</li>
              </ul>
            </template>
          </a-alert>
        </div>

        <!-- 本次待导入数据 -->
        <div class="review-table">
          <div class="review-table-header">
            <div class="review-table-title">本次待导入数据</div>
            <div class="review-table-count">
              <span>待导入：{{ pagination.total }} 条</span>
            </div>
          </div>
          <a-table :columns="previewTableColumns" :data-source="previewTableData" :loading="loadingReview" :pagination="{
            current: pagination.pageIndex,
            pageSize: pagination.pageSize,
            total: pagination.total,
            showSizeChanger: true,
          }" :row-key="previewTableRowKey" size="small" bordered @change="handleTableChange" />
          <a-empty v-if="previewTableData.length === 0" description="暂无可导入数据" class="review-table-empty" />
        </div>

        <!-- 操作按钮 -->
        <div class="review-actions" style="margin-top: 24px; text-align: center">
          <a-space>
            <a-button @click="handleStartImport" type="primary" size="large" :loading="importing">
              <CheckCircleOutlined /> 完成导入
            </a-button>
            <a-button @click="handleRefreshReview" :loading="loadingReview">
              <ReloadOutlined /> 刷新核对数据
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
import { CheckCircleOutlined, ReloadOutlined } from '@ant-design/icons-vue';
import { getImportReview, getImportReviewData, completeImport } from '/@/api/lab/rawData';
import { formatToDate } from '/@/utils/dateUtil';

const props = defineProps({
  importSessionId: {
    type: String,
    required: true,
  },
  noChanges: {
    type: Boolean,
    default: false,
  },
  noChangesMessage: {
    type: String,
    default: '',
  },
  noChangesStats: {
    type: Object as () => { totalRows: number; validDataRows: number },
    default: () => ({ totalRows: 0, validDataRows: 0 }),
  },
});

const emit = defineEmits(['complete', 'prev', 'cancel']);

// 状态
const loading = ref(true); // 初始加载状态
const loadingReview = ref(false); // 加载核对数据状态
const importing = ref(false); // 导入执行状态
const importProgress = ref(0); // 导入进度 0-100
const importProgressMessage = ref(''); // 导入进度消息
const importProgressTip = computed(() => importing.value ? '正在导入数据...' : '正在加载...');
const importSuccess = ref(false);
const importError = ref('');
const reviewData = ref<any>({});
const hasLoadedReview = ref(false); // 标记是否已加载过核对数据
const reviewTableRows = ref<any[]>([]);
const pagination = ref({
  pageIndex: 1,
  pageSize: 10,
  total: 0,
});

const previewTableColumns = [
  { title: '序号', dataIndex: 'index', width: 70 },
  { title: '生产日期', dataIndex: 'prodDate', width: 120 },
  { title: '炉号', dataIndex: 'furnaceNo', width: 160 },
  { title: '产线', dataIndex: 'lineNo', width: 80 },
  { title: '班次', dataIndex: 'shift', width: 80 },
  { title: '宽度', dataIndex: 'width', width: 100 },
  { title: '带材重量', dataIndex: 'coilWeight', width: 110 },
  { title: '产品规格', dataIndex: 'productSpecName', width: 160 },
  { title: '特征后缀', dataIndex: 'featureSuffix', width: 120 },
];

const previewTableRowKey = (record: any) => record?.id || record?.key || record?.index;


// 统计数据
const stats = computed(() => {
  if (props.noChanges) {
    return {
      totalRows: props.noChangesStats?.totalRows || 0,
      validDataRows: props.noChangesStats?.validDataRows || 0,
      productSpecMatchedRows: 0,
      featureMatchedRows: 0,
    };
  }
  return {
    totalRows: reviewData.value?.totalRows || 0,
    validDataRows: reviewData.value?.validDataRows || 0,
    productSpecMatchedRows: reviewData.value?.matchedSpecRows || 0,
    featureMatchedRows: reviewData.value?.matchedFeatureRows || 0,
  };
});

const previewTableData = computed(() => {
  const rows = reviewTableRows.value || [];
  if (!Array.isArray(rows)) return [];

  return rows.map((row: any, idx: number) => {
    const id = row.id || row.Id || row.rawDataId || row.RawDataId;
    let prodDate =
      row.prodDateStr ||
      row.ProdDateStr ||
      row.prodDate ||
      row.ProdDate ||
      row.detectionDateStr ||
      row.DetectionDateStr ||
      '';

    // 如果是非常大的数字（时间戳），进行格式化
    if (typeof prodDate === 'number' && prodDate > 0) {
      prodDate = formatToDate(prodDate);
    } else if (typeof prodDate === 'string' && /^\d{13}$/.test(prodDate)) {
      // 字符串类型的毫秒时间戳
      prodDate = formatToDate(parseInt(prodDate));
    }

    return {
      key: id || idx,
      id,
      index: idx + 1,
      prodDate,
      furnaceNo: row.furnaceNo || row.FurnaceNo || '',
      lineNo: row.lineNo || row.LineNo || '',
      shift: row.shift || row.Shift || '',
      width: row.width ?? row.Width ?? '',
      coilWeight: row.coilWeight ?? row.CoilWeight ?? '',
      productSpecName: row.productSpecName || row.ProductSpecName || '',
      featureSuffix: row.featureSuffix || row.FeatureSuffix || '',
    };
  });
});

async function loadReviewTable(pageIndex = 1, pageSize = pagination.value.pageSize) {
  if (!props.importSessionId || props.importSessionId.trim() === '') {
    reviewTableRows.value = [];
    pagination.value = { pageIndex: 1, pageSize, total: 0 };
    return;
  }

  loadingReview.value = true;
  try {
    const pageData = await getImportReviewData(props.importSessionId, pageIndex, pageSize);
    reviewTableRows.value = pageData.items || [];
    pagination.value = {
      pageIndex: pageData.pageIndex || pageIndex,
      pageSize: pageData.pageSize || pageSize,
      total: pageData.total || 0,
    };
  } catch (error: any) {
    reviewTableRows.value = [];
    pagination.value = { pageIndex: 1, pageSize, total: 0 };
    const errorMsg = error.message || error.msg || '加载分页数据失败，请重试';
    message.error(errorMsg);
  } finally {
    loadingReview.value = false;
  }
}

function handleTableChange(paginationInfo: any) {
  const nextPage = paginationInfo?.current || 1;
  const nextSize = paginationInfo?.pageSize || pagination.value.pageSize;
  loadReviewTable(nextPage, nextSize);
}


// 暴露给父组件的方法
const canGoNext = computed(() => importSuccess.value);

async function saveAndNext() {
  if (importSuccess.value) {
    emit('complete');
  }
}

// 加载核对数据
async function loadReviewData() {
  if (props.noChanges) {
    loading.value = false;
    importError.value = '';
    importSuccess.value = true;
    reviewTableRows.value = [];
    pagination.value = { pageIndex: 1, pageSize: pagination.value.pageSize, total: 0 };
    return;
  }
  // 验证导入会话ID
  if (!props.importSessionId || props.importSessionId.trim() === '') {
    loading.value = false;
    reviewTableRows.value = [];
    pagination.value = { pageIndex: 1, pageSize: pagination.value.pageSize, total: 0 };
    return;
  }

  loadingReview.value = true;
  importError.value = '';
  importSuccess.value = false;

  try {
    // 获取核对数据
    const review = await getImportReview(props.importSessionId);
    // 调试日志
    reviewData.value = review;

    // 检查是否有错误（例如：需要先完成前面的步骤）
    if (review.matchStatus === 'error') {
      const errorMsg = review.errors?.[0] || '请先完成前面的步骤：文件上传和数据解析';
      importError.value = errorMsg;
      message.warning(errorMsg);
      hasLoadedReview.value = false; // 允许重试
      return;
    }

    // 检查是否有有效数据
    const validCount = review.validDataRows || 0;
    const totalCount = review.totalRows || 0;
    // 调试日志

    if (validCount === 0) {
      if (totalCount > 0) {
        importError.value = `共解析 ${totalCount} 行数据，但没有有效数据。请检查炉号格式是否正确。`;
      } else {
        importError.value = '没有解析到任何数据，请返回第二步重新解析';
      }
      message.warning(importError.value);
      hasLoadedReview.value = false;
      return;
    }

    // 标记为已加载
    hasLoadedReview.value = true;
    await loadReviewTable(1, pagination.value.pageSize);
  } catch (error: any) {
    // 处理后端返回的特定错误消息
    const errorMsg = error.message || error.msg || '';
    if (errorMsg.includes('解析数据文件不存在') || errorMsg.includes('请重新解析')) {
      importError.value = '数据解析未完成或已过期，请返回第二步重新解析数据';
    } else if (errorMsg.includes('导入会话不存在')) {
      importError.value = '导入会话已过期，请重新开始导入流程';
    } else {
      importError.value = errorMsg || '加载核对数据失败，请重试';
    }
    message.error(importError.value);
    hasLoadedReview.value = false;
    reviewTableRows.value = [];
    pagination.value = { pageIndex: 1, pageSize: pagination.value.pageSize, total: 0 };
  } finally {
    loading.value = false;
    loadingReview.value = false;
  }
}

// 执行导入
async function handleStartImport() {
  if (!props.importSessionId || props.importSessionId.trim() === '') {
    message.error('导入会话ID缺失');
    return;
  }

  // 检查是否有有效数据
  const validCount = reviewData.value?.validDataRows || 0;
  if (validCount === 0) {
    message.warning('没有有效数据可以导入');
    return;
  }

  importing.value = true;
  importError.value = '';
  importSuccess.value = false;
  importProgress.value = 0;
  importProgressMessage.value = '准备导入...';

  // 模拟进度更新
  const progressInterval = setInterval(() => {
    if (importProgress.value < 95) {
      // 使用更平滑的进度增长，避免小数
      const increment = Math.floor(Math.random() * 8) + 3; // 3-10 的随机整数
      importProgress.value = Math.min(95, importProgress.value + increment);

      if (importProgress.value < 30) {
        importProgressMessage.value = '正在验证数据...';
      } else if (importProgress.value < 60) {
        importProgressMessage.value = '正在保存数据...';
      } else if (importProgress.value < 90) {
        importProgressMessage.value = '正在完成导入...';
      } else {
        importProgressMessage.value = '即将完成...';
      }
    }
  }, 500);

  try {
    // 执行导入
    await completeImport(props.importSessionId);

    clearInterval(progressInterval);
    importProgress.value = 100;
    importProgressMessage.value = '导入完成!';

    // 稍微延迟显示成功状态
    await new Promise(resolve => setTimeout(resolve, 500));

    importSuccess.value = true;
    message.success('数据导入成功');
    // 导入成功后自动通知父组件关闭弹窗
    emit('complete');
  } catch (error: any) {
    clearInterval(progressInterval);
    importProgress.value = 0;
    importProgressMessage.value = '';

    // 处理后端返回的特定错误消息
    const errorMsg = error.message || error.msg || '';
    if (errorMsg.includes('解析数据文件不存在') || errorMsg.includes('请重新解析')) {
      importError.value = '数据解析未完成或已过期，请返回第二步重新解析数据';
    } else if (errorMsg.includes('导入会话不存在')) {
      importError.value = '导入会话已过期，请重新开始导入流程';
    } else {
      importError.value = '导入失败，请重试';
    }
    message.error(importError.value);
  } finally {
    importing.value = false;
  }
}

// 刷新核对数据
async function handleRefreshReview() {
  hasLoadedReview.value = false;
  await loadReviewData();
}

// 重试（重新加载核对数据）
function handleRetry() {
  hasLoadedReview.value = false;
  loadReviewData();
}

// 计算属性：是否有有效的导入会话ID
const hasValidSessionId = computed(() => props.importSessionId && props.importSessionId.trim() !== '');

// 监听 importSessionId 变化，当有有效ID时加载核对数据
// 监听 importSessionId 变化
watch(
  () => props.importSessionId,
  (newId, oldId) => {
    // 这里的逻辑只负责重置状态，不再自动加载数据
    // 数据加载由父组件在切换到此步骤时调用 triggerLoad 触发
    if (newId !== oldId) {
      if (!newId) {
        // 如果 importSessionId 变为空，重置所有状态
        loading.value = false;
        loadingReview.value = false;
        importError.value = '';
        importSuccess.value = false;
        hasLoadedReview.value = false;
        reviewData.value = {};
        reviewTableRows.value = [];
        pagination.value = { pageIndex: 1, pageSize: pagination.value.pageSize, total: 0 };
      } else {
        // 如果变为新的ID，仅重置加载标记和数据
        hasLoadedReview.value = false;
        reviewData.value = {};
        importError.value = '';
        importSuccess.value = false;
        reviewTableRows.value = [];
        pagination.value = { pageIndex: 1, pageSize: pagination.value.pageSize, total: 0 };
      }
    }
  },
  { immediate: true }
);

// 监听 noChanges 变化
watch(
  () => props.noChanges,
  (newVal) => {
    if (newVal) {
      loading.value = false;
      importSuccess.value = true;
    }
  },
  { immediate: true }
);

// 组件挂载时不自动加载数据，等待父组件调用 triggerLoad
onMounted(() => {
  // 移除自动加载逻辑
});

// 添加一个方法来手动触发加载（供父组件调用）
function triggerLoad() {
  if (props.noChanges) {
    loading.value = false;
    importError.value = '';
    importSuccess.value = true;
    reviewTableRows.value = [];
    pagination.value = { pageIndex: 1, pageSize: pagination.value.pageSize, total: 0 };
    return;
  }
  if (props.importSessionId && props.importSessionId.trim() !== '' && !hasLoadedReview.value) {
    hasLoadedReview.value = false;
    loadReviewData();
  }
}

// 暴露给父组件
defineExpose({
  canGoNext,
  saveAndNext,
  triggerLoad, // 暴露触发加载的方法
});
</script>

<style lang="less" scoped>
.step4-container {
  padding: 20px;
  min-height: 400px;
  display: flex;
  align-items: center;
  justify-content: center;
}

.waiting-section {
  text-align: center;
  padding: 60px 0;
}

.loading-section {
  text-align: center;
  padding: 60px 0;
}

.result-section {
  width: 100%;
}

.result-stats {
  padding: 24px;
  background: #fafafa;
  border-radius: 8px;
  margin-top: 16px;
}

.review-section {
  width: 100%;
  max-width: 1200px;
  margin: 0 auto;
}

.review-stats {
  padding: 24px;
  background: #fafafa;
  border-radius: 8px;
  margin-bottom: 24px;
}

.review-actions {
  padding: 24px 0;
}

.error-alert {
  margin-top: 16px;
}

.review-table {
  margin-top: 24px;
}

.review-table-header {
  display: flex;
  align-items: center;
  justify-content: space-between;
  margin-bottom: 12px;
}

.review-table-title {
  font-weight: 600;
  color: #262626;
}

.review-table-count {
  display: flex;
  gap: 12px;
  color: #8c8c8c;
  font-size: 12px;
}

.review-table-empty {
  margin-top: 16px;
}
</style>
