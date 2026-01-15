<template>
  <div class="editor-tree-wrapper">
    <a-tabs v-model:activeKey="state.activeKey">
      <a-tab-pane :key="0" tab="指标"> </a-tab-pane>
      <a-tab-pane :key="1" tab="维度" force-render></a-tab-pane>
    </a-tabs>
    <div class="flex-1 overflow-auto">
      <div
        class="drag-item hover:bg-[#87c9ff] hover:text-white"
        v-for="(item, index) in items"
        :key="index"
        draggable="true"
        @dragstart="dragstart(item, $event)">
        {{ item.name }}
      </div>
    </div>
  </div>
</template>
<script lang="ts" setup>
  import { computed, reactive } from 'vue';
  import { props as _props } from './props';

  defineOptions({
    name: 'ZEditorTree',
  });

  const emit = defineEmits(['dragendNode']);
  const props = defineProps(_props);

  const state = reactive({
    activeKey: 0,
  });

  const items = computed(() => {
    return props.source[state.activeKey]?.children;
  });

  const dragstart = (item, e) => {
    e.dataTransfer.setData('chartData', JSON.stringify(item));
  };
</script>
<style lang="less" scoped>
  .editor-tree-wrapper {
    width: 100%;
    padding: 8px;
    display: flex;
    flex-direction: column;
    position: absolute;
    left: 0;
    bottom: 0;
    top: 0;

    .drag-item {
      cursor: move;
      line-height: 30px;
      margin: 4px 0;
      padding: 0 4px;
    }
  }
</style>
