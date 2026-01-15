<template>
  <BasicModal v-bind="$attrs" @register="registerModal" title="关于平台" :footer="null" :width="400" class="about-modal">
    <div class="about-modal-main">
      <img class="about-logo" :src="logoImg" />
      <div>
        <p class="title">
          {{ getSysConfig.sysName }}
        </p>
        <p>版本：{{ getSysConfig.sysVersion }}</p>
      </div>
    </div>
  </BasicModal>
</template>
<script lang="ts" setup>
  import { computed, ref } from 'vue';
  import { BasicModal, useModalInner } from '/@/components/Modal';
  import { useAppStore } from '/@/store/modules/app';
  import { useGlobSetting } from '/@/hooks/setting';
  import logoImg from '/@/assets/images/zhichang.png';

  const [registerModal] = useModalInner();
  const appStore = useAppStore();
  const globSetting = useGlobSetting();
  const apiUrl = ref(globSetting.apiUrl);

  const getSysConfig = computed(() => appStore.getSysConfigInfo);
</script>
<style lang="less">
  .about-modal {
    .scrollbar {
      padding: 0 0 40px !important;
    }

    .about-modal-main {
      display: flex;
      align-items: center;
      height: 150px;
      padding: 0 30px;
      line-height: 24px;

      .about-logo {
        display: inline-block;
        width: 95px;
        height: 95px;
        line-height: 95px;
        text-align: center;
        border-radius: 10px;
        color: #fff;
        margin-right: 30px;
      }
    }

    .about-modal-main-tip {
      font-size: 12px;
      padding-left: 30px;
      margin: 0;
    }

    .title {
      font-size: 16px;
      font-weight: 600;

      a {
        color: @text-color;
      }
    }
  }
</style>
