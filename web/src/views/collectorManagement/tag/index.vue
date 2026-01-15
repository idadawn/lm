<template>
  <a-tabs v-model:activeKey="activeKey" style="background: #fff; margin-bottom: 0px">
    <a-tab-pane key="1" tab="普通标签">
      <tag></tag>
    </a-tab-pane>
    <a-tab-pane key="2" tab="逻辑标签" force-render>
      <tagLogic></tagLogic>
    </a-tab-pane>
  </a-tabs>
</template>

<script lang="ts" setup>
  import { ref } from 'vue';
  import tag from './tag.vue';
  import tagLogic from './tagLogic.vue';

  defineOptions({
    name: 'tagIndex',
  });

  const activeKey = ref('1');
</script>
<style scoped>
  :deep(.ant-tabs-nav) {
    padding-left: 10px;
    margin-bottom: 0px;
  }
</style>
