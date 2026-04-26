<template>
  <a-collapse
    v-if="steps && steps.length > 0"
    class="kg-reasoning-chain"
    :default-active-key="defaultOpen ? ['chain'] : []"
    ghost
  >
    <a-collapse-panel key="chain" :header="`知识图谱推理过程（${steps.length} 步）`">
      <ol class="kg-reasoning-chain__list">
        <li
          v-for="(step, index) in steps"
          :key="index"
          class="kg-reasoning-chain__row"
        >
          <span class="kg-reasoning-chain__index">{{ index + 1 }}</span>
          <div class="kg-reasoning-chain__body">
            <div class="kg-reasoning-chain__tags">
              <a-tag :color="kindColor(step.kind)">{{ kindLabel(step.kind) }}</a-tag>
              <a-tag
                v-if="step.kind === 'condition' && step.satisfied !== undefined"
                :color="step.satisfied ? 'green' : 'red'"
              >{{ step.satisfied ? '满足' : '不满足' }}</a-tag>
            </div>
            <div class="kg-reasoning-chain__label">{{ step.label }}</div>
            <div
              v-if="step.kind === 'condition'"
              class="kg-reasoning-chain__condition-meta"
            >
              <span v-if="step.expected !== undefined">期望：<code>{{ step.expected }}</code></span>
              <span v-if="step.actual !== undefined">实际：<code>{{ step.actual }}</code></span>
            </div>
            <div
              v-else-if="step.detail"
              class="kg-reasoning-chain__detail"
            >{{ step.detail }}</div>
          </div>
        </li>
      </ol>
    </a-collapse-panel>
  </a-collapse>
</template>

<script lang="ts" setup>
import type { ReasoningStep, ReasoningStepKind } from '/@/types/reasoning-protocol';

interface Props {
  steps: ReasoningStep[];
  defaultOpen?: boolean;
}

withDefaults(defineProps<Props>(), { defaultOpen: false });

const KIND_LABEL_MAP: Record<ReasoningStepKind, string> = {
  record: '命中记录',
  spec: '产品规格',
  rule: '判定规则',
  condition: '条件评估',
  grade: '最终结论',
  fallback: '降级',
};

const KIND_COLOR_MAP: Record<ReasoningStepKind, string> = {
  record: 'default',
  spec: 'blue',
  rule: 'purple',
  condition: 'cyan',
  grade: 'gold',
  fallback: 'red',
};

function kindLabel(kind: ReasoningStepKind) {
  return KIND_LABEL_MAP[kind] ?? kind;
}
function kindColor(kind: ReasoningStepKind) {
  return KIND_COLOR_MAP[kind] ?? 'default';
}
</script>

<style lang="less" scoped>
.kg-reasoning-chain {
  margin-top: 12px;
  border-radius: 12px;
  background: #fafbfc;

  &__list {
    margin: 0;
    padding-left: 0;
    list-style: none;
  }

  &__row {
    display: flex;
    gap: 12px;
    padding: 8px 0 8px 12px;
    border-left: 2px solid #e2e8f0;
  }

  &__index {
    flex-shrink: 0;
    width: 22px;
    height: 22px;
    border-radius: 50%;
    background: #f1f5f9;
    color: #64748b;
    font-size: 12px;
    line-height: 22px;
    text-align: center;
    margin-top: 2px;
  }

  &__body {
    flex: 1;
    display: flex;
    flex-direction: column;
    gap: 4px;
  }

  &__tags {
    display: flex;
    flex-wrap: wrap;
    gap: 4px;
  }

  &__label {
    color: #334155;
    font-size: 13px;
  }

  &__condition-meta {
    display: flex;
    gap: 16px;
    font-size: 12px;
    color: #64748b;

    code {
      background: #f1f5f9;
      padding: 1px 6px;
      border-radius: 4px;
    }
  }

  &__detail {
    font-size: 12px;
    color: #94a3b8;
  }
}
</style>
