<template>
  <view v-if="steps && steps.length > 0" class="kg-reasoning-chain">
    <view class="kg-reasoning-chain__header" @tap="toggle">
      <text class="kg-reasoning-chain__title">知识图谱推理过程（{{ steps.length }} 步）</text>
      <text class="kg-reasoning-chain__caret">{{ expanded ? '▲' : '▼' }}</text>
    </view>
    <view v-if="expanded" class="kg-reasoning-chain__list">
      <view
        v-for="(step, index) in steps"
        :key="index"
        class="kg-reasoning-chain__row"
      >
        <text class="kg-reasoning-chain__index">{{ index + 1 }}</text>
        <view class="kg-reasoning-chain__body">
          <view class="kg-reasoning-chain__tags">
            <text
              class="kg-reasoning-chain__tag"
              :class="`kg-reasoning-chain__tag--${step.kind}`"
            >{{ kindLabel(step.kind) }}</text>
            <text
              v-if="step.kind === 'condition' && step.satisfied !== undefined"
              class="kg-reasoning-chain__tag"
              :class="step.satisfied ? 'kg-reasoning-chain__tag--ok' : 'kg-reasoning-chain__tag--fail'"
            >{{ step.satisfied ? '满足' : '不满足' }}</text>
          </view>
          <text class="kg-reasoning-chain__label">{{ step.label }}</text>
          <view v-if="step.kind === 'condition'" class="kg-reasoning-chain__meta">
            <text v-if="step.expected !== undefined">期望：{{ step.expected }}</text>
            <text v-if="step.actual !== undefined">实际：{{ step.actual }}</text>
          </view>
          <text v-else-if="step.detail" class="kg-reasoning-chain__detail">{{ step.detail }}</text>
        </view>
      </view>
    </view>
  </view>
</template>

<script>
const KIND_LABEL = {
  record: '命中记录',
  spec: '产品规格',
  rule: '判定规则',
  condition: '条件评估',
  grade: '最终结论',
  fallback: '降级'
};

export default {
  name: 'KgReasoningChain',
  props: {
    steps: { type: Array, default: () => [] },
    defaultOpen: { type: Boolean, default: false }
  },
  data() {
    return { expanded: this.defaultOpen };
  },
  methods: {
    toggle() { this.expanded = !this.expanded; },
    kindLabel(kind) { return KIND_LABEL[kind] || kind; }
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
    color: #475569;

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
