<template>
  <component :is="Comp" :class="prefixCls" v-bind="getBindValue" v-model:value="innerValue" @change="onChange">
    <template #[item]="data" v-for="item in Object.keys($slots)"><slot :name="item" v-bind="data || {}"></slot></template>
    <template #prefix v-if="prefixIcon">
      <i :class="prefixIcon"></i>
    </template>
    <template #suffix v-if="suffixIcon">
      <i :class="suffixIcon"></i>
    </template>
  </component>
</template>

<script lang="ts" setup>
  import { Input } from 'ant-design-vue';
  import { computed, ref, unref, watch } from 'vue';
  import { inputProps } from './props';
  import { useAttrs } from '/@/hooks/core/useAttrs';
  import { useDesign } from '/@/hooks/web/useDesign';

  const InputPassword = Input.Password;
  defineOptions({ name: 'JnpfInput', inheritAttrs: false });
  const props = defineProps(inputProps);
  const emit = defineEmits(['update:value', 'change']);
  const attrs = useAttrs({ excludeDefaultKeys: false });
  const innerValue = ref('');
  const Comp = props.showPassword ? InputPassword : Input;
  const { prefixCls } = useDesign('input');

  const getBindValue = computed(() => ({ ...unref(attrs) }));

  watch(
    () => props.value,
    val => {
      setValue(val);
    },
    { immediate: true },
  );

  function setValue(value) {
    innerValue.value = value;
  }
  function onChange(e) {
    emit('update:value', e.target.value);
    emit('change', e.target.value);
  }
</script>
<style lang="less" scoped>
  @prefix-cls: ~'@{namespace}-input';

  .@{prefix-cls} {
    :deep(.ant-input-prefix),
    :deep(.ant-input-suffix) {
      i {
        line-height: 20px;
        color: @text-color-help-dark;
      }
    }
  }
</style>
