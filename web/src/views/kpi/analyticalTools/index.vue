<template>
  <div class="page-content-wrapper">
    <div class="page-content-wrapper-center">
      <div class="p-4 page-content-wrapper-content custom-tabs">
        <a-tabs v-model:activeKey="activeKey">
          <a-tab-pane key="normalDistribution" tab="正态分布">
            <normal-distribution-chart />
          </a-tab-pane>
          <a-tab-pane key="spc" tab="Spc">
            <spc-chart />
          </a-tab-pane>
          <a-tab-pane key="kpi" tab="Kpi">
            <kpi-chart />
          </a-tab-pane>
        </a-tabs>
      </div>
    </div>
  </div>
</template>
<script setup lang="ts">
  import { ref } from 'vue';
  import NormalDistributionChart from './NormalDistributionChart.vue';
  import SpcChart from './SpcChart.vue';
  import KpiChart from './KpiChart.vue';

  defineOptions({ name: 'AnalyticalTools' });

  // 初始化 Tabs 默认选中
  const activeKey = ref('normalDistribution');
</script>
<style scoped lang="less">
  .page-content-wrapper .page-content-wrapper-center .page-content-wrapper-content {
    background: #fff;
  }

  .custom-tabs {
    .ant-tabs {
      height: 100%;
    }
    .ant-tabs-content-holder {
      height: 100%;
    }
    :deep(.ant-tabs-content) {
      height: 100%;
    }
  }
</style>
