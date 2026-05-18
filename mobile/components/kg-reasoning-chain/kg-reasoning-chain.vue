<template>
  <view v-if="steps && steps.length > 0" class="kg-reasoning-chain">
    <view class="kg-reasoning-chain__header" @tap="toggle">
      <text class="kg-reasoning-chain__title">💡 推理过程（{{ steps.length }} 步）</text>
      <text class="kg-reasoning-chain__caret">{{ expanded ? '▲' : '▼' }}</text>
    </view>
    <view v-if="expanded" class="kg-reasoning-chain__list">
      <view
        v-for="(step, index) in steps"
        :key="step.id || index"
        class="kg-reasoning-chain__row"
        :class="{ 'kg-reasoning-chain__row--running': step.status === 'running' }"
      >
        <text class="kg-reasoning-chain__index">{{ index + 1 }}</text>
        <view class="kg-reasoning-chain__body">
          <view class="kg-reasoning-chain__tags">
            <text
              class="kg-reasoning-chain__tag"
              :class="`kg-reasoning-chain__tag--${step.kind}`"
              :style="{ background: kindColor(step.kind) }"
            >{{ kindLabel(step.kind) }}</text>
            <text
              v-if="step.kind === 'condition' && step.satisfied !== undefined"
              class="kg-reasoning-chain__tag"
              :class="step.satisfied ? 'kg-reasoning-chain__tag--ok' : 'kg-reasoning-chain__tag--fail'"
            >{{ step.satisfied ? '满足' : '不满足' }}</text>
          </view>
          <text class="kg-reasoning-chain__label">{{ stepLabel(step) }}</text>
          <view v-if="step.kind === 'condition'" class="kg-reasoning-chain__meta">
            <text v-if="step.expected !== undefined">期望：{{ step.expected }}</text>
            <text v-if="step.actual !== undefined">实际：{{ step.actual }}</text>
          </view>
          <text v-else-if="stepDetail(step)" class="kg-reasoning-chain__detail">{{ stepDetail(step) }}</text>
        </view>
      </view>
    </view>
  </view>
</template>

<script>
import { REASONING_STEP_PRESENTATION } from '@/utils/reasoning-step-presentation.js';

export default {
  name: 'KgReasoningChain',
  props: {
    steps: { type: Array, default: () => [] },
    defaultOpen: { type: Boolean, default: false }
  },
  data() {
    return {
      expanded: this.defaultOpen,
      userToggled: false  // 用户手动操作过后，不再被 defaultOpen 自动覆盖
    };
  },
  watch: {
    // 流式结束后父组件会把 defaultOpen 切回 false → 自动收起
    defaultOpen(open) {
      if (this.userToggled) return;
      this.expanded = open;
    }
  },
  methods: {
    toggle() {
      this.expanded = !this.expanded;
      this.userToggled = true;
    },
    kindLabel(kind) {
      return (REASONING_STEP_PRESENTATION[kind] || { label: '推理步骤' }).label;
    },
    kindColor(kind) {
      return (REASONING_STEP_PRESENTATION[kind] || { color: '#64748b' }).color;
    },
    // nlq-agent 推送 {title, summary}，旧 KG 推理用 {label, detail}——都支持
    stepLabel(step) {
      return step.label || step.title || '';
    },
    stepDetail(step) {
      return step.detail || step.summary || '';
    }
  }
};
</script>

<style lang="scss" scoped>
.kg-reasoning-chain {
  margin-top: 16rpx;
  padding: 16rpx 24rpx;
  background: #fafbfc;
  border: 1rpx solid #e2e8f0;
  border-radius: 16rpx;

  &__header {
    display: flex;
    justify-content: space-between;
    align-items: center;
  }

  &__title {
    font-size: 26rpx;
    color: #475569;
    font-weight: 500;
  }

  &__caret {
    font-size: 22rpx;
    color: #94a3b8;
  }

  &__list { margin-top: 16rpx; }

  &__row {
    display: flex;
    gap: 16rpx;
    padding: 12rpx 0 12rpx 16rpx;
    border-left: 4rpx solid #e2e8f0;
    /* 新增：每条新步骤入场时滑入；配合 chat.vue 的节流队列形成"一条条往上冒"效果 */
    animation: kgReasoningRowIn 0.32s cubic-bezier(0.2, 0.7, 0.3, 1) both;

    /* status=running 的占位行：左边框换成动态色 + 文字微脉冲，提示"进行中" */
    &--running {
      border-left-color: #6366f1;
      animation:
        kgReasoningRowIn 0.32s cubic-bezier(0.2, 0.7, 0.3, 1) both,
        kgReasoningRowPulse 1.4s ease-in-out 0.32s infinite;
    }
  }

  @keyframes kgReasoningRowIn {
    from {
      opacity: 0;
      transform: translateY(-4rpx);
    }
    to {
      opacity: 1;
      transform: translateY(0);
    }
  }

  @keyframes kgReasoningRowPulse {
    0%, 100% { background: transparent; }
    50% { background: rgba(99, 102, 241, 0.06); }
  }

  &__index {
    flex-shrink: 0;
    width: 36rpx;
    height: 36rpx;
    line-height: 36rpx;
    text-align: center;
    background: #f1f5f9;
    color: #64748b;
    font-size: 22rpx;
    border-radius: 50%;
  }

  &__body {
    flex: 1;
    display: flex;
    flex-direction: column;
    gap: 6rpx;
  }

  &__tags {
    display: flex;
    flex-wrap: wrap;
    gap: 8rpx;
  }

  &__tag {
    font-size: 20rpx;
    padding: 4rpx 12rpx;
    border-radius: 20rpx;
    background: #e2e8f0;
    color: #ffffff;  /* 白字配上 :style 注入的彩色背景；旧 kind 也兼容 */

    &--record { background: #f1f5f9; color: #475569; }
    &--spec { background: #dbeafe; color: #1e40af; }
    &--rule { background: #ede9fe; color: #6d28d9; }
    &--condition { background: #cffafe; color: #155e75; }
    &--grade { background: #fef3c7; color: #92400e; }
    &--fallback { background: #fee2e2; color: #b91c1c; }
    &--ok { background: #dcfce7; color: #15803d; }
    &--fail { background: #fee2e2; color: #b91c1c; }
  }

  &__label {
    font-size: 26rpx;
    color: #334155;
  }

  &__meta {
    display: flex;
    flex-direction: column;
    font-size: 22rpx;
    color: #64748b;
  }

  &__detail {
    font-size: 22rpx;
    color: #94a3b8;
  }
}
</style>
