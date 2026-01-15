<template>
  <div class="page-content-wrapper" v-loading="loading">
    <template v-if="!ajaxing">
      <template v-if="portalId">
        <PortalLayout :layout="layout" :enabledLock="enabledLock" v-if="type === 0" @layoutUpdatedEvent="layoutUpdatedEvent" />
        <div class="custom-page" v-if="type === 1">
          <component :is="currentView" v-if="linkType === 0" />
          <embed :src="url" width="100%" height="100%" type="text/html" v-if="linkType === 1" />
        </div>
      </template>
      <div class="portal-layout-nodata" v-else>
        <img src="../../../assets/images/dashboard-nodata.png" alt="" class="layout-nodata-img" />
        <p class="layout-nodata-txt">暂无数据</p>
      </div>
    </template>
  </div>
</template>
<script lang="ts" setup>
  import { onMounted, toRefs, onUnmounted } from 'vue';
  import { useRoute } from 'vue-router';
  import PortalLayout from '/@/components/VisualPortal/Portal/Layout/index.vue';
  import { usePortal } from '/@/views/basic/home/hooks/usePortal';

  defineOptions({ name: 'dynamicPortal' });
  defineEmits(['register']);

  const { state, initData, clearAutoRefresh, layoutUpdatedEvent } = usePortal();
  const { loading, layout, type, linkType, currentView, url, ajaxing, portalId, enabledLock } = toRefs(state);

  function init() {
    const route = useRoute();
    state.portalId = (route.meta.relationId as string) || '';
    if (!state.portalId) return;
    initData();
  }

  onMounted(() => init());
  onUnmounted(() => clearAutoRefresh());
</script>
<style lang="less">
  @import url('/@/components/VisualPortal/style/index.less');
</style>
