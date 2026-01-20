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
        <a-alert
          message="第二步：数据核对与完成导入"
          description="请核对以下数据统计信息，确认无误后点击完成导入。系统将根据炉号更新中间数据表的磁性性能数据。"
          type="info"
          show-icon
          style="margin-bottom: 24px" />
        
        <!-- 统计数据 -->
        <div class="review-stats">
          <a-row :gutter="24">
            <a-col :span="8">
              <a-statistic title="总数据行" :value="stats.totalRows" />
            </a-col>
            <a-col :span="8">
              <a-statistic title="有效数据" :value="stats.validDataRows" :value-style="{ color: '#52c41a' }" />
            </a-col>
            <a-col :span="8">
              <a-statistic title="预计更新" :value="stats.validDataRows" :value-style="{ color: '#1890ff' }" />
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
              <ul style="margin: 0; padding-left: 20px; max-height: 200px; overflow-y: auto">
                <li v-for="(error, index) in reviewData.errors.slice(0, 10)" :key="index">{{ error }}</li>
                <li v-if="reviewData.errors.length > 10">... 还有 {{ reviewData.errors.length - 10 }} 条错误</li>
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
import { getMagneticImportReview, completeMagneticImport } from '/@/api/lab/magneticData';

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
  updatedRows: reviewData.value?.updatedRows || 0,
  skippedRows: reviewData.value?.skippedRows || 0,
}));

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
  min-height: 400px;
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
  .review-stats {
    padding: 24px;
    background: #fafafa;
    border-radius: 8px;
    margin-bottom: 24px;
  }

  .error-alert {
    margin-top: 24px;
  }

  .review-actions {
    margin-top: 24px;
    text-align: center;
  }
}
</style>
