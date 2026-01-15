<template>
  <Tooltip :title="getTitle" placement="bottom" :mouseEnterDelay="0.5">
    <span @click="toggle">
      <FullscreenOutlined v-if="!isFullscreen" />
      <FullscreenExitOutlined v-else />
    </span>
  </Tooltip>
</template>
<script lang="ts">
  import { defineComponent, computed, ref, onMounted, onUnmounted, unref } from 'vue';
  import { Tooltip } from 'ant-design-vue';
  import { useI18n } from '/@/hooks/web/useI18n';

  import { FullscreenExitOutlined, FullscreenOutlined } from '@ant-design/icons-vue';

  export default defineComponent({
    name: 'FullScreen',
    components: { FullscreenExitOutlined, FullscreenOutlined, Tooltip },

    setup() {
      const { t } = useI18n();
      const isFullscreen = ref(false);

      const toggle = () => {
        const doc: any = document;
        const main: any = document.documentElement;
        if (
          !doc.fullscreenElement &&
          !doc.mozFullScreenElement &&
          !doc.webkitFullscreenElement &&
          !doc.msFullscreenElement
        ) {
          if (main.requestFullscreen) {
            main.requestFullscreen();
          } else if (main.msRequestFullscreen) {
            main.msRequestFullscreen();
          } else if (main.mozRequestFullScreen) {
            main.mozRequestFullScreen();
          } else if (main.webkitRequestFullscreen) {
            main.webkitRequestFullscreen();
          }
        } else {
          if (doc.exitFullscreen) {
            doc.exitFullscreen();
          } else if (doc.msExitFullscreen) {
            doc.msExitFullscreen();
          } else if (doc.mozCancelFullScreen) {
            doc.mozCancelFullScreen();
          } else if (doc.webkitExitFullscreen) {
            doc.webkitExitFullscreen();
          }
        }
      };

      const getTitle = computed(() => {
        return unref(isFullscreen) ? t('layout.header.tooltipExitFull') : t('layout.header.tooltipEntryFull');
      });

      const updateFullscreenState = () => {
        const doc: any = document;
        isFullscreen.value = !!(
          doc.fullscreenElement ||
          doc.mozFullScreenElement ||
          doc.webkitFullscreenElement ||
          doc.msFullscreenElement
        );
      };

      onMounted(() => {
        updateFullscreenState();
        window.addEventListener('fullscreenchange', updateFullscreenState);
        window.addEventListener('mozfullscreenchange', updateFullscreenState);
        window.addEventListener('webkitfullscreenchange', updateFullscreenState);
        window.addEventListener('msfullscreenchange', updateFullscreenState);
      });

      onUnmounted(() => {
        window.removeEventListener('fullscreenchange', updateFullscreenState);
        window.removeEventListener('mozfullscreenchange', updateFullscreenState);
        window.removeEventListener('webkitfullscreenchange', updateFullscreenState);
        window.removeEventListener('msfullscreenchange', updateFullscreenState);
      });

      return {
        getTitle,
        isFullscreen,
        toggle,
      };
    },
  });
</script>
