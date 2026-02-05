<template>
  <div :class="prefixCls">
    <div class="login-left-panel">
      <div class="login-logo-container">
        <img src="/resource/img/login_logo.png" alt="logo" />
      </div>
      <div class="login-text-container">
        <div class="text-title">让世界更节能 让生活更美好</div>
        <div class="text-english">Honoring energy. Elevating life.</div>
        <div class="text-subtitle">下一代实验室智能分析平台</div>
      </div>
    </div>
    <div class="login-right-panel">
      <div class="flex items-center absolute right-4 top-4">
        <AppDarkModeToggle class="enter-x mr-2" v-if="!sessionTimeout" />
      </div>
      <div :class="`${prefixCls}-form`" class="enter-x">
        <!-- <LoginFormTitle class="-enter-x xl:hidden" /> -->
        <div class="login-title">{{ title }}</div>
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
import { useGlobSetting } from '/@/hooks/setting';

defineProps({
  sessionTimeout: {
    type: Boolean,
  },
});

const { prefixCls } = useDesign('login-container');
const appStore = useAppStore();
const globSetting = useGlobSetting();
const title = computed(() => globSetting.title);

const getSysConfig = computed(() => appStore.getSysConfigInfo);
</script>
<style lang="less">
@import url('./index.less');
</style>
