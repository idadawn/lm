<template>
  <a-button v-bind="getBindValue" @click="onClick">
    <template #default="data">
      <slot v-bind="data || {}"></slot>
    </template>
  </a-button>
</template>

<script lang="ts">
  import { defineComponent } from 'vue';
  export default defineComponent({
    inheritAttrs: false,
  });
</script>
<script lang="ts" setup>
  import { computed, unref } from 'vue';
  import { omit } from 'lodash-es';
  import { useAttrs } from '/@/hooks/core/useAttrs';
  import { useMessage } from '/@/hooks/web/useMessage';
  import { useI18n } from '/@/hooks/web/useI18n';

  const props = defineProps({
    modelConfirm: { type: Object },
  });
  const attrs = useAttrs({ excludeDefaultKeys: false });
  const { createConfirm } = useMessage();
  const { t } = useI18n();
  const { modelConfirm } = props;
  function onClick() {
    createConfirm({
      iconType: modelConfirm?.iconType || 'warning',
      title: modelConfirm?.title || t('common.tipTitle'),
      content: modelConfirm?.content || t('common.delTip'),
      onOk: async () => {
        try {
          if (modelConfirm?.onOk) {
            await modelConfirm.onOk();
          }
        } catch (error) {
          console.error('确认操作失败:', error);
          // 确保即使出错也关闭弹窗
          throw error;
        } finally {
          // 确保无论如何都尝试清理弹窗
          cleanupModal();
        }
      },
      onCancel: () => {
        if (modelConfirm?.onCancel) {
          modelConfirm.onCancel();
        }
        // 确保取消时也清理弹窗
        cleanupModal();
      }
    });
  }

  // 清理可能残留的Modal
  function cleanupModal() {
    setTimeout(() => {
      // 查找并移除所有确认对话框的遮罩层和包装器
      const confirmModals = document.querySelectorAll('.ant-modal-confirm');
      confirmModals.forEach(modal => {
        const wrap = modal.closest('.ant-modal-wrap');
        const mask = modal.closest('.ant-modal-mask');
        if (wrap) {
          wrap.style.display = 'none';
          if (wrap.parentNode) {
            wrap.parentNode.removeChild(wrap);
          }
        }
      });

      // 清理所有modal相关的遮罩层
      const allModalMasks = document.querySelectorAll('.ant-modal-mask');
      allModalMasks.forEach(mask => {
        if (mask.parentNode) {
          mask.parentNode.removeChild(mask);
        }
      });

      // 清理body上的样式
      document.body.style.overflow = '';
      document.body.style.paddingRight = '';
    }, 0);
  }
  const getBindValue = computed(() => omit({ ...unref(attrs) }, ['enable', 'getPopupContainer', 'label', 'onClick', 'icon']));
</script>