<template>
  <BasicPopup v-bind="$attrs" @register="registerPopup" class="full-popup report-popup">
    <iframe :src="state.url" width="100%" height="100%" frameborder="0" />
  </BasicPopup>
</template>
<script lang="ts" setup>
  import { reactive, onMounted } from 'vue';
  import { BasicPopup, usePopupInner } from '/@/components/Popup';
  import { useGlobSetting } from '/@/hooks/setting';
  import { getToken } from '/@/utils/auth';

  interface State {
    url: string;
  }

  defineEmits(['register']);
  const { report } = useGlobSetting();
  const [registerPopup, { closePopup }] = usePopupInner(init);
  const state = reactive<State>({
    url: '',
  });

  function init(data) {
    state.url = `${report}/preview.html?id=${data.id}&token=${getToken()}&page=1`;
  }
  function handleMessage(e) {
    const data = e.data;
    if (data !== 'closeDialog') return;
    state.url = '';
    closePopup();
  }

  onMounted(() => {
    window.addEventListener('message', handleMessage);
  });
</script>
<style lang="less">
  .report-popup {
    .jnpf-basic-popup-header {
      display: none !important;
    }
  }
</style>
