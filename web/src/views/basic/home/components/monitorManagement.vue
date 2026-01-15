<template>
  <div class="md:flex enter-y middleBox" @click="jumpSecond">
    <VisitAxisMonitor />
  </div>
  <div class="md:flex enter-y middleBox" @click="jumpSecond">
    <visitPieMonitor class="md:w-1/2 w-full" :loading="loading" />
    <visitThreePieMonitor class="md:w-1/2 w-full" :loading="loading" />
  </div>
</template>
<script lang="ts" setup>
  import { reactive, ref, toRefs, watch, computed, nextTick } from 'vue';
  import { useRouter } from 'vue-router';

  import VisitAxisMonitor from './VisitAxisMonitor.vue';
  import visitPieMonitor from './VisitPieMonitor.vue';
  import visitThreePieMonitor from './visitThreePieMonitor.vue';

  const router = useRouter();

  //跳到二级分析页面
  function jumpSecond() {
    // router.push('/predictionManagement/prediction')
  }
</script>
<style>
  .middleBox {
    margin-top: 40px;

    /* border: 1px solid red; */
  }

  .searchBox {
    display: flex;
    align-items: center;
  }
</style>
