<template>
  <div v-if="visible" class="calc-progress-wrapper">
    <div class="calc-progress-bar">
      <!-- 活跃任务 -->
      <div v-for="progress in activeList" :key="progress.batchId" class="progress-item">
        <span class="progress-icon">
          <LoadingOutlined v-if="progress.status === 'PROCESSING'" spin />
          <CheckCircleOutlined v-else-if="progress.status === 'COMPLETED'" class="icon-success" />
          <CloseCircleOutlined v-else-if="progress.status === 'FAILED'" class="icon-error" />
        </span>
        <span class="progress-label">
          {{ progress.taskType === 'JUDGE' ? '判定' : progress.taskType === 'MAGNETIC_JUDGE' ? '磁性判定' : '计算' }}
        </span>
        <div class="progress-bar-row">
          <a-progress
            :percent="calcPercent(progress)"
            :status="progressStatus(progress)"
            :stroke-color="progressColor(progress)"
            size="small"
            class="progress-bar"
            :format="(p) => `${p}%`"
          />
          <span class="progress-detail">
            {{ progress.completed }}/{{ progress.total }}
            <span v-if="progress.failedCount > 0" class="failed-count">({{ progress.failedCount }}失败)</span>
          </span>
        </div>
      </div>

      <!-- 完成/失败的提示 -->
      <div v-for="progress in completedList" :key="'done-' + progress.batchId" class="progress-item completed">
        <span class="progress-icon">
          <CheckCircleOutlined v-if="progress.status === 'COMPLETED'" class="icon-success" />
          <CloseCircleOutlined v-else class="icon-error" />
        </span>
        <span class="progress-detail">
          {{ progress.status === 'COMPLETED' ? (progress.taskType === 'MAGNETIC_JUDGE' || progress.taskType === 'JUDGE' ? '判定完成' : '处理完成') : (progress.taskType === 'MAGNETIC_JUDGE' || progress.taskType === 'JUDGE' ? '判定失败' : '处理失败') }}
          {{ progress.successCount }}条
          <span v-if="progress.failedCount > 0" class="failed-count">/ {{ progress.failedCount }}失败</span>
        </span>
      </div>
    </div>
  </div>
</template>

<script lang="ts" setup>
import { computed } from 'vue';
import { LoadingOutlined, CheckCircleOutlined, CloseCircleOutlined } from '@ant-design/icons-vue';
import type { CalcProgressData } from '/@/hooks/web/useCalcProgress';

const props = defineProps<{
  activeList: CalcProgressData[];
  completedList: CalcProgressData[];
}>();

const visible = computed(() => {
  return props.activeList.length > 0 || props.completedList.length > 0;
});

function calcPercent(progress: CalcProgressData): number {
  if (!progress.total || progress.total === 0) return 0;
  return Math.round((progress.completed / progress.total) * 100);
}

function progressStatus(progress: CalcProgressData): 'active' | 'success' | 'exception' {
  if (progress.status === 'COMPLETED') return 'success';
  if (progress.status === 'FAILED') return 'exception';
  return 'active';
}

function progressColor(progress: CalcProgressData): string {
  if (progress.status === 'FAILED') return '#ff4d4f';
  if (progress.status === 'COMPLETED') return '#52c41a';
  return '#1890ff';
}
</script>

<style lang="less" scoped>
.calc-progress-wrapper {
  display: flex;
  justify-content: center;
  padding: 4px 0;
}

.calc-progress-bar {
  display: flex;
  flex-wrap: wrap;
  justify-content: center;
  gap: 12px;
  background: linear-gradient(135deg, #e6f7ff 0%, #f0f5ff 100%);
  border: 1px solid #91d5ff;
  border-radius: 16px;
  padding: 4px 16px;
  max-width: 80%;
}

.progress-item {
  display: flex;
  align-items: center;
  gap: 6px;
  white-space: nowrap;
  font-size: 12px;

  &.completed {
    opacity: 0.8;
  }
}

.progress-icon {
  font-size: 14px;
  display: flex;
  align-items: center;
}

.icon-success {
  color: #52c41a;
}

.icon-error {
  color: #ff4d4f;
}

.progress-label {
  font-weight: 600;
  color: #1890ff;
  font-size: 12px;
}

/* 进度条与右侧数量/失败信息同一行，并与百分比垂直对齐 */
.progress-bar-row {
  display: flex;
  align-items: center;
  gap: 8px;
  min-height: 20px;
}

.progress-bar-row :deep(.ant-progress) {
  display: flex;
  align-items: center;
  margin-bottom: 0;
}

.progress-bar-row :deep(.ant-progress-outer),
.progress-bar-row :deep(.ant-progress-inner) {
  border-radius: 4px;
}

.progress-bar-row :deep(.ant-progress-text) {
  min-width: 32px;
  font-size: 12px;
  line-height: 20px;
  margin-left: 8px;
}

.progress-detail {
  font-size: 12px;
  line-height: 20px;
  color: #595959;
  flex-shrink: 0;
}

.failed-count {
  color: #ff4d4f;
  font-weight: 500;
}

.progress-bar {
  width: 100px;
}
</style>
