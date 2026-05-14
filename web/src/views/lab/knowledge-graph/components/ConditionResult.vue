<template>
  <span class="condition-result" :class="resultClass">
    <CheckCircleFilled v-if="satisfied === true" />
    <CloseCircleFilled v-else-if="satisfied === false" />
    <QuestionCircleFilled v-else />
    <span class="result-text">{{ resultText }}</span>
  </span>
</template>

<script lang="ts" setup>
import { computed } from 'vue';
import { CheckCircleFilled, CloseCircleFilled, QuestionCircleFilled } from '@ant-design/icons-vue';

const props = defineProps<{
  satisfied?: boolean | null;
}>();

const resultClass = computed(() => {
  if (props.satisfied === true) return 'result-pass';
  if (props.satisfied === false) return 'result-fail';
  return 'result-unknown';
});

const resultText = computed(() => {
  if (props.satisfied === true) return '满足';
  if (props.satisfied === false) return '不满足';
  return '未评估';
});
</script>

<style lang="less" scoped>
.condition-result {
  display: inline-flex;
  align-items: center;
  gap: 4px;
  font-size: 12px;
  font-weight: 500;

  &.result-pass {
    color: #16A34A;
  }
  &.result-fail {
    color: #DC2626;
  }
  &.result-unknown {
    color: #94A3B8;
  }
}

.result-text {
  font-size: 11px;
}
</style>
