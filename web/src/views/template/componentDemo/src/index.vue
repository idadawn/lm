<template>
  <div class="z-editor-form-wrapper">
    {{ state.name }}
  </div>
</template>
<script lang="ts" setup>
  import { reactive } from 'vue';
  import { props as _props } from './props';

  defineOptions({
    name: 'ZEditorForm',
  });

  const props = defineProps(_props);
  const state = reactive<any>({
    name: 'test',
    source: props.source,
  });
  
</script>
<style lang="less" scoped></style>
