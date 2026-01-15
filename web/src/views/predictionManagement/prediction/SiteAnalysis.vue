<template>
  <Card :tab-list="tabListTitle" v-bind="$attrs" :active-tab-key="activeKey" @tab-change="onTabChange">
    <p v-if="activeKey === 'tab1'">
      <VisitAxis />
    </p>
    <p v-if="activeKey === 'tab2'">
      <VisitAnalysisBar />
    </p>
    <p v-if="activeKey === 'tab3'">
      <VisitAnalysis />
    </p>
  </Card>
</template>
<script lang="ts" setup>
  import { ref } from 'vue';
  import { Card } from 'ant-design-vue';
  import VisitAnalysis from './VisitAnalysis.vue';
  import VisitAnalysisBar from './VisitAnalysisBar.vue';
  import VisitAxis from './VisitAxis.vue';
  import VisitRadar from './VisitRadar.vue';
  import VisitSource from './VisitSource.vue';

  const activeKey = ref('tab1');

  const tabListTitle = [
    {
      key: 'tab1',
      tab: '碳双命中率预测',
    },
    {
      key: 'tab2',
      tab: '温度达标率预测',
    },
    {
      key: 'tab3',
      tab: '含碳量达标率预测',
    },
  ];

  function onTabChange(key) {
    activeKey.value = key;
  }
</script>
