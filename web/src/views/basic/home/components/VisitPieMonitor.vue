<template>
  <Card title="炼钢成分合格率" :loading="loading">
    <div ref="chartRef" :style="{ width, height }">
      <Space wrap :size="state.sizeBigData">
        <div class="progressBox">
          <Progress
            type="dashboard"
            strokeLinecap="butt"
            size="small"
            :percent="state.msg"
            :gapDegree="state.degreeData" />
          <Progress
            type="dashboard"
            strokeLinecap="butt"
            size="small"
            :percent="state.msg"
            :status="state.exceptionFlag"
            :gapDegree="state.degreeData" />
          <!-- <Progress type="circle" :percent="state.msg2" status="exception" />
          <Progress type="circle" :percent="state.msg3" /> -->
        </div>
        <div class="boxmsg">
          <p>不合格炉数</p>
          <p>90</p>
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
    msg3: 100,
    exceptionFlag: 'exception',
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
