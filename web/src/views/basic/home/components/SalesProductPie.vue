<template>
  <Card title="探伤合格率" :loading="loading">
    <div ref="chartRef" :style="{ width, height }">
      <Space wrap :size="state.sizeBigData">
        <div class="progressBox">
          <p>大棒</p>
          <Progress
            type="dashboard"
            strokeLinecap="butt"
            size="small"
            :percent="state.msg"
            :gapDegree="state.degreeData" />
        </div>
        <div>
          <p>中棒</p>
          <Progress type="circle" size="small" :percent="state.msg2" />
        </div>
        <div>
          <p>小棒</p>
          <Progress type="circle" :size="state.sizeData" :percent="state.msg3" />
        </div>
      </Space>
    </div>
  </Card>
</template>
<script lang="ts" setup>
  import { Ref, ref, watch, reactive } from 'vue';
  import { Card, Progress, Space } from 'ant-design-vue';
  //定义
  const state = reactive({
    msg: 80,
    msg2: 70,
    msg3: 72,
    degreeData: 140,
    sizeData: 10,
    sizeBigData: 10,
  });

  const props = defineProps({
    loading: Boolean,
    width: {
      type: String as PropType<string>,
      default: '100%',
    },
    height: {
      type: String as PropType<string>,
      default: '300px',
    },
  });
</script>
<style lang="" scoped></style>
