<template>
  <div :class="prefixCls">
    <div class="login-left-panel">
      <div class="login-text-container">
        <div class="text-title">精准数据，洞见未来</div>
        <div class="text-subtitle">下一代实验室智能分析平台</div>
        <div class="text-english">Precise Data, Insightful Future</div>
      </div>
    </div>
    <div class="login-right-panel">
      <div class="flex items-center absolute right-4 top-4">
        <AppDarkModeToggle class="enter-x mr-2" v-if="!sessionTimeout" />
      </div>
      <div :class="`${prefixCls}-form`" class="enter-x">
        <!-- <LoginFormTitle class="-enter-x xl:hidden" /> -->
        <LoginForm />
      </div>
      <div class="copyright">{{ getSysConfig.copyright }}</div>
    </div>
  </div>
</template>
<script lang="ts" setup>
  import { computed } from 'vue';
  import { AppDarkModeToggle } from '/@/components/Application';
  import LoginFormTitle from './LoginFormTitle.vue';
  import LoginForm from './LoginForm.vue';
  import { useDesign } from '/@/hooks/web/useDesign';
  import { useAppStore } from '/@/store/modules/app';

  defineProps({
    sessionTimeout: {
      type: Boolean,
    },
  });

  const { prefixCls } = useDesign('login-container');
  const appStore = useAppStore();

  const getSysConfig = computed(() => appStore.getSysConfigInfo);
</script>
<style lang="less">
  @import url('./index.less');
</style>
