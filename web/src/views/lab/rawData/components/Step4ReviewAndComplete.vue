<template>
  <div class="step4-container">
    <!-- 如果还没有导入会话ID，显示提示 -->
    <div v-if="!hasValidSessionId" class="waiting-section">
      <a-empty description="请先完成前面的步骤" />
    </div>

    <!-- 加载中状态 -->
    <div v-else-if="loading" class="loading-section">
      <a-spin size="large" tip="正在导入数据..." />
    </div>

    <!-- 导入结果 -->
    <div v-else class="result-section">
      <!-- 成功状态 -->
      <a-result
        v-if="importSuccess"
        status="success"
        title="数据导入成功"
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
                <a-statistic title="产品规格已匹配" :value="stats.productSpecMatchedRows" :value-style="{ color: '#722ed1' }" />
              </a-col>
              <a-col :span="6">
                <a-statistic title="特性已匹配" :value="stats.featureMatchedRows" :value-style="{ color: '#13c2c2' }" />
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
        <a-alert
          message="第五步：数据核对与完成导入"
          description="请核对以下数据统计信息，确认无误后点击完成导入。"
          type="info"
          show-icon
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
          <a-alert
            type="warning"
            show-icon>
            <template #message>数据核对发现问题</template>
            <template #description>
              <ul style="margin: 0; padding-left: 20px">
                <li v-for="(error, index) in reviewData.errors" :key="index">{{ error }}</li>
              </ul>
            </template>
          </a-alert>
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
import { getImportReview, completeImport } from '/@/api/lab/rawData';

const props = defineProps({
  importSessionId: {
    type: String,
    required: true,
  },
});

const emit = defineEmits(['complete', 'prev', 'cancel']);

// 状态
const loading = ref(true); // 初始加载状态
const loadingReview = ref(false); // 加载核对数据状态
const importing = ref(false); // 导入执行状态
const importSuccess = ref(false);
const importError = ref('');
const reviewData = ref<any>({});
const hasLoadedReview = ref(false); // 标记是否已加载过核对数据

// 统计数据
const stats = computed(() => ({
  totalRows: reviewData.value?.totalRows || 0,
  validDataRows: reviewData.value?.validDataRows || 0,
  productSpecMatchedRows: reviewData.value?.matchedSpecRows || 0,
  featureMatchedRows: reviewData.value?.matchedFeatureRows || 0,
}));

// 暴露给父组件的方法
const canGoNext = computed(() => importSuccess.value);

async function saveAndNext() {
  if (importSuccess.value) {
    emit('complete');
  }
}

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
    const review = await getImportReview(props.importSessionId);
    console.log('[Step4] Review data:', review); // 调试日志
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
    console.log(`[Step4] Total: ${totalCount}, Valid: ${validCount}`); // 调试日志
    
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

  try {
    // 执行导入
    await completeImport(props.importSessionId);
    importSuccess.value = true;
    message.success('数据导入成功');
  } catch (error: any) {
    // 处理后端返回的特定错误消息
    const errorMsg = error.message || error.msg || '';
    if (errorMsg.includes('解析数据文件不存在') || errorMsg.includes('请重新解析')) {
      importError.value = '数据解析未完成或已过期，请返回第二步重新解析数据';
    } else if (errorMsg.includes('导入会话不存在')) {
      importError.value = '导入会话已过期，请重新开始导入流程';
    } else {
      importError.value = errorMsg || '导入失败，请重试';
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
      } else {
        // 如果变为新的ID，仅重置加载标记和数据
        hasLoadedReview.value = false;
        reviewData.value = {};
        importError.value = '';
        importSuccess.value = false;
      }
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
</style>
