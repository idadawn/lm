<template>
  <div class="reasoning-panel">
    <div class="panel-header">
      <span class="panel-title">
        <BulbOutlined class="panel-icon" />
        推理过程
      </span>
      <a-tag v-if="steps.length > 0" size="small" color="blue">{{ steps.length }} 步</a-tag>
    </div>

    <div class="steps-list" v-if="steps.length > 0">
      <div
        v-for="(step, idx) in steps"
        :key="step.id || idx"
        class="step-item"
        :class="[`status-${step.status || 'success'}`, { active: activeStepId === step.id }]"
        @click="$emit('selectStep', step)"
      >
        <div class="step-marker">
          <div class="step-dot" :class="step.status || 'success'">
            <span class="step-num">{{ idx + 1 }}</span>
          </div>
          <div v-if="idx < steps.length - 1" class="step-line" />
        </div>

        <div class="step-body">
          <div class="step-title-row">
            <span class="step-title">{{ step.title }}</span>
            <StepStatusBadge :status="step.status || 'success'" />
          </div>
          <div v-if="step.summary" class="step-summary">{{ step.summary }}</div>

          <!-- 条件详情 -->
          <div v-if="step.kind === 'condition'" class="step-condition">
            <div class="cond-row">
              <span class="cond-label">字段</span>
              <span class="cond-value">{{ step.field || '-' }}</span>
            </div>
            <div class="cond-row">
              <span class="cond-label">期望</span>
              <span class="cond-value">{{ step.expected || '-' }}</span>
            </div>
            <div class="cond-row">
              <span class="cond-label">实际</span>
              <span class="cond-value">{{ formatActual(step.actual) }}</span>
            </div>
            <div class="cond-row">
              <span class="cond-label">结果</span>
              <ConditionResult :satisfied="step.satisfied" />
            </div>
          </div>

          <!-- 本体引用 -->
          <div v-if="step.ontologyRefs && step.ontologyRefs.length > 0" class="step-refs">
            <span
              v-for="ref in step.ontologyRefs"
              :key="ref.id"
              class="ref-tag"
              @click.stop="$emit('highlightNode', ref.id)"
            >
              {{ ref.label }}
            </span>
          </div>
        </div>
      </div>
    </div>

    <a-empty v-else description="暂无推理步骤" class="empty-steps" />
  </div>
</template>

<script lang="ts" setup>
import { BulbOutlined } from '@ant-design/icons-vue';
import type { ReasoningStep } from '../types/ontology';
import StepStatusBadge from './StepStatusBadge.vue';
import ConditionResult from './ConditionResult.vue';

defineProps<{
  steps: ReasoningStep[];
  activeStepId?: string;
}>();

defineEmits<{
  (e: 'selectStep', step: ReasoningStep): void;
  (e: 'highlightNode', nodeId: string): void;
}>();

function formatActual(val: string | number | undefined): string {
  if (val === undefined || val === null) return '-';
  if (typeof val === 'number') return Number.isInteger(val) ? String(val) : val.toFixed(3);
  return String(val);
}
</script>

<style lang="less" scoped>
.reasoning-panel {
  width: 260px;
  background: #fff;
  border-right: 1px solid #F1F5F9;
  display: flex;
  flex-direction: column;
  overflow: hidden;
}

.panel-header {
  padding: 12px 16px;
  border-bottom: 1px solid #F1F5F9;
  display: flex;
  align-items: center;
  justify-content: space-between;
  flex-shrink: 0;
}

.panel-title {
  font-size: 14px;
  font-weight: 600;
  color: #1E293B;
  display: flex;
  align-items: center;
  gap: 6px;
}

.panel-icon {
  color: #F59E0B;
  font-size: 16px;
}

.steps-list {
  flex: 1;
  overflow-y: auto;
  padding: 12px 16px;
}

.step-item {
  display: flex;
  gap: 10px;
  position: relative;
  cursor: pointer;
  padding: 8px 0;
  border-radius: 6px;
  transition: background 0.15s;

  &:hover {
    background: #F8FAFC;
  }

  &.active {
    background: #EFF6FF;
  }
}

.step-marker {
  display: flex;
  flex-direction: column;
  align-items: center;
  flex-shrink: 0;
  width: 24px;
}

.step-dot {
  width: 24px;
  height: 24px;
  border-radius: 50%;
  display: flex;
  align-items: center;
  justify-content: center;
  font-size: 11px;
  font-weight: 600;
  color: #fff;

  &.success { background: #16A34A; }
  &.warning { background: #F59E0B; }
  &.failed { background: #DC2626; }
  &.running { background: #3B82F6; }
  &.pending { background: #94A3B8; }
}

.step-line {
  width: 2px;
  flex: 1;
  background: #E2E8F0;
  margin: 4px 0;
}

.step-body {
  flex: 1;
  min-width: 0;
}

.step-title-row {
  display: flex;
  align-items: center;
  justify-content: space-between;
  gap: 8px;
  margin-bottom: 2px;
}

.step-title {
  font-size: 13px;
  font-weight: 600;
  color: #1E293B;
}

.step-summary {
  font-size: 12px;
  color: #64748B;
  line-height: 1.5;
  margin-top: 2px;
}

.step-condition {
  margin-top: 8px;
  padding: 8px;
  background: #F8FAFC;
  border-radius: 6px;
  border: 1px solid #F1F5F9;
}

.cond-row {
  display: flex;
  justify-content: space-between;
  font-size: 11px;
  line-height: 1.8;

  .cond-label {
    color: #94A3B8;
  }
  .cond-value {
    color: #334155;
    font-weight: 500;
    max-width: 100px;
    overflow: hidden;
    text-overflow: ellipsis;
    white-space: nowrap;
    text-align: right;
  }
}

.step-refs {
  margin-top: 6px;
  display: flex;
  flex-wrap: wrap;
  gap: 4px;
}

.ref-tag {
  font-size: 11px;
  padding: 2px 6px;
  background: #EFF6FF;
  color: #2563EB;
  border-radius: 4px;
  cursor: pointer;
  transition: background 0.15s;

  &:hover {
    background: #DBEAFE;
  }
}

.empty-steps {
  margin-top: 40px;
}
</style>
