<template>
  <a-form-item label="默认值">
    <a-input v-model:value="activeData.__config__.defaultValue" placeholder="请输入默认值" />
  </a-form-item>
  <a-form-item label="最小行数">
    <a-input-number v-model:value="activeData.autoSize.minRows" placeholder="最小行数" :min="1" />
  </a-form-item>
  <a-form-item label="最大行数">
    <a-input-number v-model:value="activeData.autoSize.maxRows" placeholder="最大行数" :min="1" />
  </a-form-item>
  <a-form-item label="最多输入">
    <a-input-number v-model:value="activeData.maxlength" placeholder="请输入字符长度" :min="0" addonAfter="个字符" />
  </a-form-item>
  <a-form-item label="能否清空">
    <a-switch v-model:checked="activeData.clearable" />
  </a-form-item>
  <a-form-item label="是否只读" v-show="showType === 'pc'">
    <a-switch v-model:checked="activeData.readonly" />
  </a-form-item>
</template>
<script lang="ts" setup>
  import { inject, computed } from 'vue';

  defineOptions({ inheritAttrs: false });
  defineProps(['activeData']);

  const getShowType: (() => string | undefined) | undefined = inject('getShowType');
  const showType = computed(() => (getShowType as () => string | undefined)());
</script>
