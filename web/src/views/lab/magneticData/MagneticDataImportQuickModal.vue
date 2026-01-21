<template>
  <BasicModal
    v-bind="$attrs"
    @register="registerModal"
    title="快捷导入 - 最优数据核对"
    :width="1100"
    :minHeight="500"
    :footer="null"
    :destroyOnClose="true"
    class="magnetic-quick-import-modal"
  >
    <div class="modal-content-wrapper">
      <Step2ReviewAndComplete
        ref="step2Ref"
        :import-session-id="importSessionId"
        @complete="handleComplete"
        @cancel="handleCancel"
      />
    </div>
  </BasicModal>
</template>

<script lang="ts" setup>
  import { ref, nextTick } from 'vue';
  import { BasicModal, useModalInner } from '/@/components/Modal';
  import Step2ReviewAndComplete from './components/Step2ReviewAndComplete.vue';

  const emit = defineEmits(['register', 'reload']);
  const importSessionId = ref('');

  const [registerModal, { closeModal }] = useModalInner(async (data) => {
    importSessionId.value = data.importSessionId;
    await nextTick();
  });

  function handleComplete() {
    emit('reload');
    closeModal();
  }

  function handleCancel() {
    closeModal();
  }
</script>

<style lang="less" scoped>
  .magnetic-quick-import-modal {
    :deep(.ant-modal-body) {
      padding: 0;
    }
  }
  
  .modal-content-wrapper {
    height: 70vh;
    overflow: hidden;
    
    :deep(.step2-container) {
      padding: 16px;
      height: 100%;
      display: flex;
      flex-direction: column;
      
      .result-section, .review-section {
        height: 100%;
        display: flex;
        flex-direction: column;
      }
      
      .review-tabs {
        flex: 1;
        min-height: 0;
        
        .ant-tabs-content {
          height: 100%;
        }
      }
    }
  }
</style>
