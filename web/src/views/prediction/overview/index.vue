<template>
  <div class="home-v">
    <!-- <GrowCard :loading="loading" class="enter-y" /> -->
    <SiteAnalysis class="!my-10px enter-y" :loading="loading" />
    <div class="md:flex enter-y">
      <VisitRadar class="md:w-1/3 w-full" :loading="loading" />
      <VisitGauge class="md:w-1/3 !md:mx-10px !md:my-0 !my-10px w-full" :loading="loading" />
      <SalesProductPie class="md:w-1/3 w-full" :loading="loading" />
    </div>
  </div>
</template>
<script lang="ts" setup>
  import { ref } from 'vue';
  // import GrowCard from '/@/views/basic/home/components/GrowCard.vue';
  import SiteAnalysis from './SiteAnalysis.vue';
  import VisitGauge from './VisitGauge.vue';
  import VisitRadar from './VisitRadar.vue';
  import SalesProductPie from './SalesProductPie.vue';

  const loading = ref(true);

  setTimeout(() => {
    loading.value = false;
  }, 500);
</script>
<style lang="less">
  .home-v {
    background: @app-main-background;
  }
</style>
