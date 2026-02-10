<template>
  <RouterView>
    <template #default="{ Component, route }">
      <keep-alive v-if="openCache" :include="getCaches">
        <component :is="Component || ExceptionComponent" :key="route.name" />
      </keep-alive>
      <component v-else :is="Component || ExceptionComponent" :key="route.name" />
    </template>
  </RouterView>
  <FrameLayout v-if="getCanEmbedIFramePage" />
</template>

<script lang="ts">
import { computed, defineComponent, unref, watch } from 'vue';

import FrameLayout from '/@/layouts/iframe/index.vue';
import { EXCEPTION_COMPONENT } from '/@/router/constant';

import { useRootSetting } from '/@/hooks/setting/useRootSetting';

import { useTransitionSetting } from '/@/hooks/setting/useTransitionSetting';
import { useMultipleTabSetting } from '/@/hooks/setting/useMultipleTabSetting';
import { getTransitionName } from './transition';

import { useMultipleTabStore } from '/@/store/modules/multipleTab';

export default defineComponent({
  name: 'PageLayout',
  components: { FrameLayout },
  setup() {
    const { getShowMultipleTab } = useMultipleTabSetting();
    const tabStore = useMultipleTabStore();

    const { getOpenKeepAlive, getCanEmbedIFramePage } = useRootSetting();

    const { getBasicTransition, getEnableTransition } = useTransitionSetting();

    const openCache = computed(() => unref(getOpenKeepAlive) && unref(getShowMultipleTab));

    const getCaches = computed((): string[] => {
      if (!unref(getOpenKeepAlive)) {
        return [];
      }
      return tabStore.getCachedTabList;
    });

    // DEBUG: 调试 keep-alive 缓存
    watch([openCache, getCaches], ([cache, caches]) => {
      console.log('[PageLayout] openCache:', cache, '| getCaches:', caches);
    }, { immediate: true });

    return {
      getTransitionName,
      openCache,
      getEnableTransition,
      getBasicTransition,
      getCaches,
      getCanEmbedIFramePage,
      ExceptionComponent: EXCEPTION_COMPONENT,
    };
  },
});
</script>
