<template>
  <BasicPopup v-bind="$attrs" @register="registerPopup" title="新建流程" class="full-popup" destroyOnClose>
    <FlowList @select="onSelect" :flowType="flowType" :isEntrust="isEntrust" />
  </BasicPopup>
</template>
<script lang="ts" setup>
  import { ref } from 'vue';
  import { BasicPopup, usePopupInner } from '/@/components/Popup';
  import FlowList from '../flowQuickLaunch/FlowList.vue';

  const emit = defineEmits(['register', 'select']);
  const [registerPopup, { closePopup }] = usePopupInner(init);

  const flowType = ref<number>(0);
  const isEntrust = ref<boolean>(false);

  function init(data) {
    flowType.value = data.flowType || 0;
    isEntrust.value = data.isEntrust || false;
  }

  function onSelect(data) {
    emit('select', data);
    closePopup();
  }
</script>
