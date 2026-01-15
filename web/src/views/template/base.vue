<template>
  <div class="page-content-wrapper">
    <div class="page-content-wrapper-left">left</div>
    <div class="page-content-wrapper-center bg-white">
      <div class="page-content-wrapper-search-box"> search box </div>
      <div class="page-content-wrapper-content">
        content
      </div>
    </div>
  </div>
</template>
<script lang="ts" setup>
  import { reactive } from 'vue';
  defineOptions({
    name: 'base',
  });
  const state = reactive<any>({

  });
</script>
<style lang="less" scoped>

</style>
