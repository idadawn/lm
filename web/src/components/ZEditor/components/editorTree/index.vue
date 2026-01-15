<template>
  <div class="editor-tree-wrapper">
    <a-tree :show-line="true" v-model:expandedKeys="expandedKeys" :tree-data="nodeList" :field-names="fieldNames">
      <template #title="{ dataRef,name, level }" #scope>
        <template v-if="level == 2">
          <div class="drag-item" draggable="true" @dragend="dragend(dataRef, $event)">
            {{ name }}
          </div>
        </template>
        <template v-else>{{ name }}</template>
      </template>
    </a-tree>
  </div>
</template>
<script lang="ts" setup>
  import { ref, computed } from 'vue';
  import { props as _props } from './props';
  import type { TreeProps } from 'ant-design-vue';

  defineOptions({
    name: 'ZEditorTree',
  });

  const emit = defineEmits(['dragendNode']);
  const props = defineProps(_props);

  const expandedKeys = ref<string[]>(['0-0-1']);
  const fieldNames: TreeProps['fieldNames'] = {
    title: 'name',
  };
  const nodeList = computed(() => {
    return props.source;
  });
  const dragend = (item, e) => {
    emit('dragendNode', { item, e });
  };
</script>
<style lang="less" scoped>
  .editor-tree-wrapper {
    width: 100%;
    padding: 10px 8px;

    .drag-item {
      cursor: move;
    }
  }
</style>
