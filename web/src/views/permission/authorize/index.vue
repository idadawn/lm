<template>
  <div class="page-content-wrapper">
    <AuthorizePopup @register="registerAuthorize" />
  </div>
</template>

<script lang="ts" setup>
  import { onMounted } from 'vue';
  import { usePopup } from '/@/components/Popup';
  import AuthorizePopup from './AuthorizePopup.vue';

  const [registerAuthorize, { openPopup: openAuthorizePopup }] = usePopup();

  onMounted(() => {
    openAuthorizePopup(true, { id: '0', fullName: '权限批量设置', type: 'Batch' });
  });
</script>
