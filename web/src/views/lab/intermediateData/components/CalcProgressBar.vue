<template>
  <div v-if="visible" class="calc-progress-bar">
    <div v-for="progress in activeList" :key="progress.batchId" class="progress-item">
      <div class="progress-info">
        <span class="progress-icon">
          <LoadingOutlined v-if="progress.status === 'PROCESSING'" spin />
          <CheckCircleOutlined v-else-if="progress.status === 'COMPLETED'" class="icon-success" />
          <CloseCircleOutlined v-else-if="progress.status === 'FAILED'" class="icon-error" />
        </span>
        <span class="progress-label">
          {{ progress.taskType === 'JUDGE' ? '批量判定' : progress.taskType === 'MAGNETIC_JUDGE' ? '磁性导入判定' : '后台计算' }}
        </span>
        <span class="progress-detail">
          {{ progress.completed }}/{{ progress.total }} 条
          <template v-if="progress.failedCount > 0">
            <span class="failed-count">({{ progress.failedCount }} 失败)</span>
          </template>
        </span>
      </div>
      <a-progress
        :percent="calcPercent(progress)"
        :status="progressStatus(progress)"
        :stroke-color="progressColor(progress)"
        size="small"
        class="progress-bar"
        :format="(p) => `${p}%`"
      />
      <span class="progress-message">{{ progress.message }}</span>
    </div>

    <!-- 完成/失败的提示 -->
    <div v-for="progress in completedList" :key="'done-' + progress.batchId" class="progress-item completed">
      <div class="progress-info">
        <span class="progress-icon">
          <CheckCircleOutlined v-if="progress.status === 'COMPLETED'" class="icon-success" />
          <CloseCircleOutlined v-else class="icon-error" />
        </span>
        <span class="progress-detail">
          {{ progress.status === 'COMPLETED' ? (progress.taskType === 'MAGNETIC_JUDGE' || progress.taskType === 'JUDGE' ? '判定完成' : '处理完成') : (progress.taskType === 'MAGNETIC_JUDGE' || progress.taskType === 'JUDGE' ? '判定失败' : '处理失败') }}:
          成功 {{ progress.successCount }} 条
          <template v-if="progress.failedCount > 0">
            , 失败 {{ progress.failedCount }} 条
          </template>
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
.calc-progress-bar {
  background: linear-gradient(135deg, #e6f7ff 0%, #f0f5ff 100%);
  border: 1px solid #91d5ff;
  border-radius: 6px;
  padding: 8px 16px;
  margin-bottom: 12px;
  transition: all 0.3s ease;
}

.progress-item {
  display: flex;
  align-items: center;
  gap: 12px;
  padding: 4px 0;

  &.completed {
    opacity: 0.8;
  }

  & + .progress-item {
    border-top: 1px dashed #d9d9d9;
    padding-top: 8px;
    margin-top: 4px;
  }
}

.progress-info {
  display: flex;
  align-items: center;
  gap: 6px;
  white-space: nowrap;
  min-width: 180px;
}

.progress-icon {
  font-size: 16px;
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
  font-size: 13px;
}

.progress-detail {
  font-size: 13px;
  color: #595959;
}

.failed-count {
  color: #ff4d4f;
  font-weight: 500;
}

.progress-bar {
  flex: 1;
  min-width: 120px;
  max-width: 300px;
}

.progress-message {
  font-size: 12px;
  color: #8c8c8c;
  white-space: nowrap;
  overflow: hidden;
  text-overflow: ellipsis;
  max-width: 200px;
}
</style>
