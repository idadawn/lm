<template>
  <BasicPopup
    v-bind="$attrs"
    @register="registerPopup"
    title="磁性数据导入向导"
    showOkBtn
    @ok="handleSubmit"
    destroyOnClose
    class="full-popup magnetic-data-import-wizard"
    :continueLoading="activeStep < getStepList.length - 1">
    <!-- 步骤导航在标题栏 -->
    <template #title>
      <div class="steps-wrapper">
        <div class="steps">
          <a-steps v-model:current="activeStep" type="navigation" size="small" :key="key">
            <a-step v-for="item in getStepList" :key="item" :title="item" disabled />
          </a-steps>
        </div>
      </div>
    </template>

    <!-- 操作按钮在工具栏 -->
    <template #insertToolbar>
      <a-space :size="10">
        <a-button @click="handlePrev" :disabled="activeStep <= 0">{{ t('common.prev') }}</a-button>
        <a-button @click="handleNext" :disabled="activeStep >= getStepList.length - 1" :loading="nextLoading">
          {{ t('common.next') }}
        </a-button>
      </a-space>
    </template>

    <!-- 步骤内容 -->
    <div class="step-content">
      <!-- 第一步：文件上传与解析 -->
      <div v-show="activeStep === 0">
        <Step1UploadAndParse
          ref="step1Ref"
          :import-session-id="importSessionId"
          @next="handleStep1Next"
          @cancel="handleCancel"
        />
      </div>

      <!-- 第二步：数据核对与完成 -->
      <div v-show="activeStep === 1">
        <Step2ReviewAndComplete
          ref="step2Ref"
          :import-session-id="importSessionId"
          @complete="handleComplete"
          @prev="handlePrev"
          @cancel="handleCancel"
        />
      </div>
    </div>
  </BasicPopup>
</template>

<script lang="ts" setup>
  import { ref, computed, reactive, toRefs, watch } from 'vue';
  import { BasicPopup, usePopupInner } from '/@/components/Popup';
  import { useMessage } from '/@/hooks/web/useMessage';
  import { useI18n } from '/@/hooks/web/useI18n';

  // 导入步骤组件
  import Step1UploadAndParse from './components/Step1UploadAndParse.vue';
  import Step2ReviewAndComplete from './components/Step2ReviewAndComplete.vue';
  import { deleteMagneticImportSession } from '/@/api/lab/magneticData';

  interface State {
    activeStep: number;
    importSessionId: string;
    nextLoading: boolean;
    key: number;
  }


  const { t } = useI18n();
  const emit = defineEmits(['register', 'reload']);
  const [registerPopup, { closePopup, changeOkLoading }] = usePopupInner(init);
  const { createMessage } = useMessage();

  // 状态管理
  const state = reactive<State>({
    activeStep: 0,
    importSessionId: '',
    nextLoading: false,
    key: +new Date(),
  });


  const { activeStep, importSessionId, nextLoading, key } = toRefs(state);


  // 步骤组件引用
  const step1Ref = ref<InstanceType<typeof Step1UploadAndParse>>();
  const step2Ref = ref<InstanceType<typeof Step2ReviewAndComplete>>();

  // 步骤列表
  const stepList = ['文件上传与解析', '数据核对与完成'];
  const getStepList = computed(() => {
    return stepList;
  });

  // 监听步骤变化，更新 key 以刷新步骤组件
  watch(activeStep, () => {
    state.key = +new Date();
  });

  // 初始化
  function init(params?: any) {
    if (params && params.activeStep !== undefined) {
      state.activeStep = params.activeStep;
      state.importSessionId = params.importSessionId || '';
    } else {
      state.activeStep = 0;
      state.importSessionId = '';
    }

    state.nextLoading = false;
    state.key = +new Date();
  }

  // 步骤1完成（文件上传，文件数据已保存到后端）
  async function handleStep1Next(data: { sessionId: string }) {
    state.importSessionId = data.sessionId;
    state.activeStep = 1;
  }


  // 下一步
  async function handleNext() {
    try {
      state.nextLoading = true;

      switch (activeStep.value) {
        case 0:
          await step1Ref.value?.saveAndNext();
          break;
      }
    } catch (error) {
      console.error('步骤保存失败:', error);
      createMessage.error('步骤保存失败，请重试');
    } finally {
      state.nextLoading = false;
    }
  }

  // 上一步
  function handlePrev() {
    if (activeStep.value > 0) {
      state.activeStep--;
    }
  }

  // 提交（最后一步时调用）
  async function handleSubmit() {
    if (activeStep.value < getStepList.value.length - 1) {
      await handleNext();
      return;
    }

    // 最后一步，完成导入
    await handleComplete();
  }

  // 完成导入（导入已在进入最后一步时自动执行）
  async function handleComplete() {
    try {
      changeOkLoading(true);
      // 检查导入是否成功
      if (step2Ref.value?.canGoNext) {
        emit('reload');
        // 重置状态
        init();
        closePopup();
        createMessage.success('导入完成');
      } else {
        createMessage.warning('请等待导入完成');
      }
    } catch (error) {
      console.error('完成导入失败:', error);
      createMessage.error('完成导入失败，请重试');
    } finally {
      changeOkLoading(false);
    }
  }

  // 取消
  async function handleCancel() {
    try {
      if (importSessionId.value) {
        await deleteMagneticImportSession(importSessionId.value);
      }
      // 重置状态
      init();
      closePopup();
    } catch (error) {
      console.error('取消导入失败:', error);
      // 重置状态
      init();
      closePopup();
    }
  }
</script>

<style lang="less" scoped>
  :deep(.scrollbar) {
    padding: 0;
    height: 100%;
    .scrollbar__view {
      height: 100%;

      & > div {
        height: 100% !important;
      }
    }
  }

  .steps-wrapper {
    width: 100%;
    min-width: 600px;
    display: flex;
    align-items: center;
    padding: 0 16px;
    box-sizing: border-box;
    overflow-x: auto;
    overflow-y: hidden;
  }

  .steps {
    width: 100%;
    min-width: 600px;
    flex-shrink: 0;

    :deep(.ant-steps) {
      width: 100%;
      min-width: 600px;
    }

    :deep(.ant-steps-navigation) {
      display: flex;
      width: 100%;
      align-items: center;
      flex-wrap: nowrap;

      .ant-steps-item {
        flex: 0 0 auto !important;
        width: auto !important;
        min-width: fit-content;
        max-width: none;
        padding: 0 28px 0 12px;
        position: relative;

        .ant-steps-item-container {
          width: auto !important;
          min-width: fit-content;
          display: flex;
          flex-direction: row;
          align-items: center;
          justify-content: flex-start;
          padding: 4px 0;
          gap: 8px;
        }

        .ant-steps-item-content {
          width: auto !important;
          min-width: fit-content;
          max-width: none;
          display: flex;
          flex-direction: row;
          align-items: center;
          flex: 0 0 auto;
        }

        .ant-steps-item-title {
          width: auto !important;
          min-width: fit-content;
          max-width: none;
          text-align: left;
          padding: 0;
          white-space: nowrap;
          overflow: visible;
          margin: 0;
          font-size: 12px;
          line-height: 1.4;
        }
      }

      .ant-steps-item-tail {
        display: block !important;
        position: absolute;
        top: 50%;
        left: calc(100% - 28px);
        width: 24px;
        height: 1px;
        margin-top: -0.5px;
        padding: 0;
        background-color: #d9d9d9;
        z-index: 1;

        &::after {
          content: '';
          position: absolute;
          right: 0;
          top: 50%;
          width: 0;
          height: 0;
          border-top: 4px solid transparent;
          border-bottom: 4px solid transparent;
          border-left: 6px solid #d9d9d9;
          transform: translateY(-50%);
        }
      }

      .ant-steps-item:last-child .ant-steps-item-tail {
        display: none !important;
      }

      .ant-steps-item-icon {
        margin: 0;
        margin-right: 0;
        flex-shrink: 0;
        order: 1;
      }

      .ant-steps-item-content {
        order: 2;
        flex-shrink: 0;
      }
    }

    :deep(.ant-steps-small) {
      .ant-steps-item {
        padding: 0 20px 0 8px;

        .ant-steps-item-container {
          gap: 6px;
        }

        .ant-steps-item-title {
          font-size: 11px;
          line-height: 1.3;
          white-space: nowrap;
        }

        .ant-steps-item-tail {
          left: calc(100% - 20px);
          width: 20px;
        }
      }
    }
  }

  .step-content {
    min-height: 400px;
    height: 100%;
    overflow: auto;
  }
</style>
