<template>
  <div v-if="steps && steps.length > 0" class="reasoning-chain" data-testid="reasoning-chain">
    <button
      type="button"
      class="reasoning-chain__header"
      :aria-expanded="expanded"
      aria-controls="reasoning-chain-list"
      @click="toggle"
    >
      <span class="reasoning-chain__header-left">
        <span class="reasoning-chain__icon">
          <BulbOutlined />
        </span>
        <span class="reasoning-chain__title">推理过程（{{ steps.length }} 步）</span>
      </span>
      <span class="reasoning-chain__caret" :class="{ 'reasoning-chain__caret--open': expanded }">
        <DownOutlined />
      </span>
    </button>

    <transition name="reasoning-chain-expand">
      <div
        v-if="expanded"
        id="reasoning-chain-list"
        class="reasoning-chain__list"
      >
        <div
          v-for="(step, index) in steps"
          :key="index"
          class="reasoning-chain__row"
          :data-testid="`reasoning-step`"
        >
          <span class="reasoning-chain__index">{{ index + 1 }}</span>
          <div class="reasoning-chain__body">
            <div class="reasoning-chain__tags">
              <a-tag
                :color="getPresentation(step.kind).color"
                class="reasoning-chain__kind-tag"
              >{{ getPresentation(step.kind).label }}</a-tag>
              <a-tag
                v-if="step.kind === 'condition' && step.satisfied !== undefined"
                :color="step.satisfied ? '#15803d' : '#b91c1c'"
                class="reasoning-chain__kind-tag"
              >{{ step.satisfied ? '满足' : '不满足' }}</a-tag>
            </div>
            <div class="reasoning-chain__label">{{ step.label }}</div>
            <div v-if="step.kind === 'condition'" class="reasoning-chain__meta">
              <span v-if="step.expected !== undefined" class="reasoning-chain__meta-item">
                期望：{{ step.expected }}
              </span>
              <span v-if="step.actual !== undefined" class="reasoning-chain__meta-item">
                实际：{{ step.actual }}
              </span>
            </div>
            <div v-else-if="step.detail" class="reasoning-chain__detail">{{ step.detail }}</div>
          </div>
        </div>
      </div>
    </transition>
  </div>
</template>

<script setup lang="ts">
import { ref } from 'vue';
import { BulbOutlined, DownOutlined } from '@ant-design/icons-vue';
import type { ReasoningStep } from '/@/types/reasoning-protocol';
import { REASONING_STEP_PRESENTATION } from '/@/types/reasoning-step-presentation';

interface Props {
  steps: ReasoningStep[];
  defaultOpen?: boolean;
}

const props = withDefaults(defineProps<Props>(), {
  defaultOpen: false,
});

const expanded = ref(props.defaultOpen);

function toggle() {
  expanded.value = !expanded.value;
}

function getPresentation(kind: string) {
  return REASONING_STEP_PRESENTATION[kind] ?? { label: kind, color: '#475569', icon: 'question-circle' };
}
</script>

<style lang="less" scoped>
.reasoning-chain {
  margin-bottom: 10px;
  background: #fafbfc;
  border: 1px solid #e2e8f0;
  border-radius: 8px;
  overflow: hidden;

  &__header {
    display: flex;
    align-items: center;
    justify-content: space-between;
    width: 100%;
    min-height: 44px;
    padding: 10px 14px;
    background: transparent;
    border: none;
    cursor: pointer;
    text-align: left;
    transition: background 0.15s ease;
    gap: 8px;

    &:hover {
      background: #f1f5f9;
    }

    &:focus-visible {
      outline: 2px solid #1890ff;
      outline-offset: -2px;
    }
  }

  &__header-left {
    display: flex;
    align-items: center;
    gap: 6px;
  }

  &__icon {
    color: #64748b;
    font-size: 13px;
    line-height: 1;
  }

  &__title {
    font-size: 12px;
    font-weight: 500;
    color: #475569;
    white-space: nowrap;
  }

  &__caret {
    color: #94a3b8;
    font-size: 11px;
    transition: transform 0.2s ease;
    flex-shrink: 0;

    &--open {
      transform: rotate(180deg);
    }
  }

  &__list {
    border-top: 1px solid #e2e8f0;
    padding: 8px 0;
  }

  &__row {
    display: flex;
    gap: 10px;
    padding: 8px 14px 8px 18px;
    border-left: 3px solid #e2e8f0;
    margin: 0 0 0 0;
    transition: border-color 0.15s;

    &:not(:last-child) {
      border-bottom: 1px solid #f1f5f9;
    }
  }

  &__index {
    flex-shrink: 0;
    width: 20px;
    height: 20px;
    line-height: 20px;
    text-align: center;
    background: #f1f5f9;
    color: #64748b;
    font-size: 11px;
    border-radius: 50%;
    font-weight: 600;
    margin-top: 2px;
  }

  &__body {
    flex: 1;
    display: flex;
    flex-direction: column;
    gap: 4px;
    min-width: 0;
  }

  &__tags {
    display: flex;
    flex-wrap: wrap;
    gap: 4px;
  }

  &__kind-tag {
    font-size: 11px !important;
    line-height: 18px !important;
    padding: 0 7px !important;
    margin-right: 0 !important;
    border-radius: 10px !important;
    border: none !important;
  }

  &__label {
    font-size: 13px;
    color: #334155;
    line-height: 1.5;
    word-break: break-word;
  }

  &__meta {
    display: flex;
    flex-wrap: wrap;
    gap: 8px;
  }

  &__meta-item {
    font-size: 11px;
    color: #64748b;
  }

  &__detail {
    font-size: 11px;
    color: #94a3b8;
    line-height: 1.4;
    word-break: break-word;
  }
}

// Expand/collapse transition
.reasoning-chain-expand-enter-active,
.reasoning-chain-expand-leave-active {
  transition: opacity 0.2s ease, transform 0.2s ease;
  transform-origin: top;
}

.reasoning-chain-expand-enter-from,
.reasoning-chain-expand-leave-to {
  opacity: 0;
  transform: scaleY(0.95);
}

.reasoning-chain-expand-enter-to,
.reasoning-chain-expand-leave-from {
  opacity: 1;
  transform: scaleY(1);
}
</style>
