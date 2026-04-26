<template>
  <view class="kg-demo">
    <view class="kg-demo__header">
      <text class="kg-demo__title">KG 推理链 Demo</text>
      <text class="kg-demo__subtitle">使用 fixtures JSON 离线渲染</text>
    </view>
    <kg-reasoning-chain :steps="steps" :default-open="true" />
    <button class="kg-demo__btn" @tap="connectLive">连接真实后端</button>
  </view>
</template>

<script>
import KgReasoningChain from '@/components/kg-reasoning-chain/kg-reasoning-chain.vue';
import { streamNlqChat } from '@/utils/sse-client.js';

// fixtures JSON cloned from
// nlq-agent/packages/shared-types/fixtures/reasoning-steps.fixture.json
const FIXTURE_STEPS = [
  { kind: 'record', label: '命中检测记录：炉号 1丙20260110-1，批次 BATCH-001' },
  { kind: 'spec', label: '产品规格 120' },
  { kind: 'rule', label: '判定规则：C级（优先级 1）' },
  {
    kind: 'condition',
    label: '带宽 >= 119.5',
    field: 'F_WIDTH',
    expected: '>= 119.5',
    actual: 119.8,
    satisfied: true
  },
  {
    kind: 'condition',
    label: 'Ps铁损 <= 1.30',
    field: 'F_PERF_PS_LOSS',
    expected: '<= 1.30',
    actual: 1.46,
    satisfied: false
  },
  { kind: 'grade', label: '1/2 条满足，主要差距在 Ps铁损上限；按规则归入 C 级' }
];

export default {
  components: { KgReasoningChain },
  data() {
    return {
      steps: FIXTURE_STEPS
    };
  },
  methods: {
    connectLive() {
      this.steps = [];
      streamNlqChat(
        {
          messages: [
            { role: 'user', content: '为什么炉号 1丙20260110-1 是 C 级？' }
          ],
          session_id: `mobile-demo-${Date.now()}`
        },
        {
          onReasoningStep: (step) => {
            this.steps = this.steps.concat([step]);
          },
          onResponseMetadata: (payload) => {
            if (Array.isArray(payload.reasoning_steps) && payload.reasoning_steps.length > 0) {
              this.steps = payload.reasoning_steps.slice();
            }
          },
          onError: (err) => {
            uni.showToast({ title: err.message, icon: 'none' });
          }
        }
      );
    }
  }
};
</script>

<style lang="scss" scoped>
.kg-demo {
  padding: 32rpx;

  &__header { margin-bottom: 24rpx; }
  &__title { font-size: 36rpx; font-weight: 600; color: #1e293b; }
  &__subtitle { display: block; margin-top: 8rpx; font-size: 24rpx; color: #64748b; }
  &__btn {
    margin-top: 24rpx;
    background: #1890ff;
    color: #fff;
    font-size: 28rpx;
    border-radius: 12rpx;
  }
}
</style>
