<style lang="less">
  .toolbar-wrapper {
    background: @chart-editor-left-color;
    display: flex;
    flex-direction: row;
    justify-content: space-between;
    padding: 0 8px;
    height: 30px;
    color: @chart-font-color;
    font-size: 14px;
  }
</style>
<template>
  <div class="toolbar-wrapper">
    <div>{{ props.title }}</div>
  </div>
</template>
<script lang="ts" setup>
  const props = defineProps({
    title: {
      type: String as PropType<string>,
      default: '',
    },
  });
</script>
