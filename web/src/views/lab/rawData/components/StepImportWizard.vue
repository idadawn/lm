<template>
  <BasicPopup
    v-bind="$attrs"
    @register="registerPopup"
    :title="getTitle"
    class="full-popup step-import-wizard-popup"
    :show-cancel-btn="false"
    :show-ok-btn="false">
    <template #title>
      <div class="ml-10px steps">
        <a-steps v-model:current="currentStep" type="navigation" size="small" :items="stepItems" />
      </div>
    </template>
    <template #insertToolbar>
      <a-space :size="10">
        <a-button @click="handlePrev" :disabled="currentStep <= 0">上一步</a-button>
        <a-button type="primary" @click="handleNext" :loading="loading" :disabled="isNextDisabled">
          {{ nextButtonText }}
        </a-button>
      </a-space>
    </template>

    <!-- 步骤内容 -->
    <div class="step-content h-full overflow-auto">
      <!-- 第一步：文件上传与数据解析 -->
      <Step1UploadAndParse
        v-if="currentStep === 0"
        ref="step1Ref"
        :import-session-id="importSessionId"
        @next="handleStep1Next"
        @cancel="handleCancel" />

      <!-- 第二步：产品规格识别 -->
      <Step2ProductSpec
        v-if="currentStep === 1"
        ref="step2Ref"
        :import-session-id="importSessionId"
        @prev="handlePrev"
        @next="handleStep2Next"
        @cancel="handleCancel" />

      <!-- 第三步：特性匹配 -->
      <Step3AppearanceFeature
        v-if="currentStep === 2"
        ref="step3Ref"
        :import-session-id="importSessionId"
        @prev="handlePrev"
        @next="handleStep3Next"
        @cancel="handleCancel" />

      <!-- 第四步：数据核对与完成 -->
      <Step4ReviewAndComplete
        v-if="currentStep === 3"
        ref="step4Ref"
        :import-session-id="importSessionId"
        @prev="handlePrev"
        @complete="handleComplete"
        @cancel="handleCancel" />
    </div>

    <!-- 加载状态 -->
    <div v-if="loading && currentStep !== 3" class="loading-overlay">
      <a-spin size="large" tip="正在处理..." />
    </div>
  </BasicPopup>
</template>

<script lang="ts" setup>
import { ref, computed } from 'vue';
import { BasicPopup, usePopupInner } from '/@/components/Popup';
import { useMessage } from '/@/hooks/web/useMessage';
import { useTabs } from '/@/hooks/web/useTabs';
import Step1UploadAndParse from './Step1UploadAndParse.vue';
import Step2ProductSpec from './Step2ProductSpec.vue';
import Step3AppearanceFeature from './Step3AppearanceFeature.vue';
import Step4ReviewAndComplete from './Step4ReviewAndComplete.vue';
import { createImportSession, updateImportSession, deleteImportSession } from '/@/api/lab/rawData';

const emit = defineEmits(['register', 'success', 'cancel']);

const [registerPopup, { closePopup }] = usePopupInner(init);
const { createMessage, createConfirm } = useMessage();
const { refreshPage } = useTabs();

// 步骤状态
const currentStep = ref(0);
const importSessionId = ref('');
const loading = ref(false);

// 步骤组件引用
const step1Ref = ref();
const step2Ref = ref();
const step3Ref = ref();
const step4Ref = ref();

const getTitle = computed(() => '分步导入向导');

// 步骤配置
const stepItems = computed(() => [
  {
    title: '文件上传与解析',
    // description: '上传文件并解析数据',
  },
  {
    title: '产品规格识别',
    // description: '匹配产品规格',
  },
  {
    title: '特性匹配',
    // description: '匹配外观特性',
  },
  {
    title: '数据核对与完成',
    // description: '核对数据并完成导入',
  },
]);

const nextButtonText = computed(() => {
  if (currentStep.value === stepItems.value.length - 1) {
    return '完成';
  }
  return '下一步';
});

const isNextDisabled = computed(() => {
  if (currentStep.value === 3) {
    // 第四步自动执行，完成后 step4Ref 会有状态，但这里主要依靠 step4Ref 内部逻辑
    // 如果 step4Ref 也是 loading，可以在这里控制
    return false;
  }
  return false;
});

// 初始化
async function init(data?: any) {
  // 重置所有状态
  resetState();

  // 如果有未完成的会话ID，则恢复会话
  if (data?.importSessionId) {
    importSessionId.value = data.importSessionId;
    // 根据会话的当前步骤跳转到对应步骤
    try {
      const session = await updateImportSession(importSessionId.value, { currentStep: 0 });
      currentStep.value = session.currentStep;
    } catch (error) {
      console.error('恢复会话失败:', error);
      // 恢复失败，创建新会话
      await createNewSession();
    }
  } else {
    // 创建新的导入会话
    await createNewSession();
  }
}

// 创建新会话
async function createNewSession() {
  try {
    const sessionId = await createImportSession({
      fileName: '',
      importStrategy: 'incremental',
      currentStep: 0,
      status: 'pending',
    });
    importSessionId.value = sessionId;
  } catch (error) {
    createMessage.error('创建导入会话失败');
    closePopup();
  }
}

async function handlePrev() {
  if (currentStep.value > 0) {
    currentStep.value--;
    // 更新会话状态（可选，视后端逻辑而定，这里主要是前端路由）
    try {
        await updateImportSession(importSessionId.value, {
            currentStep: currentStep.value,
        });
    } catch (error) {
        // quiet fail
    }
  }
}

async function handleNext() {
  // 调用当前步骤组件的 saveAndNext 方法
  // 如果是最后一步，saveAndNext 会触发 complete
  // 如果是中间步骤，saveAndNext 会触发 next 事件，进而调用 handleStepXNext

  try {
    if (currentStep.value === 0) {
      if (step1Ref.value?.saveAndNext) {
        await step1Ref.value.saveAndNext();
      }
    } else if (currentStep.value === 1) {
      if (step2Ref.value?.saveAndNext) {
        await step2Ref.value.saveAndNext();
      }
    } else if (currentStep.value === 2) {
      if (step3Ref.value?.saveAndNext) {
        await step3Ref.value.saveAndNext();
      }
    } else if (currentStep.value === 3) {
      // 这一步通常是自动的，但点击完成按钮时也可以触发确认
      if (step4Ref.value?.saveAndNext) {
        await step4Ref.value.saveAndNext();
      }
    }
  } catch (error) {
    // 错误处理通常在子组件中完成
    console.error('Next step error:', error);
  }
}

// 步骤1完成，由子组件emit触发
async function handleStep1Next(data: any) {
  try {
    loading.value = true;
    // 更新会话状态
    await updateImportSession(importSessionId.value, {
      currentStep: 1,
      fileName: data.fileName,
      totalRows: data.totalRows,
      validDataRows: data.validDataRows,
      status: 'in_progress',
    });
    currentStep.value = 1;
  } catch (error) {
    createMessage.error('保存第一步数据失败');
  } finally {
    loading.value = false;
  }
}

// 步骤2完成
async function handleStep2Next(_data: any) {
  try {
    loading.value = true;
    // 更新会话状态
    await updateImportSession(importSessionId.value, {
      currentStep: 2,
      status: 'in_progress',
    });
    currentStep.value = 2;
  } catch (error) {
    createMessage.error('保存第二步数据失败');
  } finally {
    loading.value = false;
  }
}

// 步骤3完成
async function handleStep3Next(_data: any) {
  try {
    loading.value = true;
    // 更新会话状态
    await updateImportSession(importSessionId.value, {
      currentStep: 3,
      status: 'in_progress',
    });
    currentStep.value = 3;
  } catch (error) {
    createMessage.error('保存第三步数据失败');
  } finally {
    loading.value = false;
  }
}

// 完成导入
async function handleComplete(_data: any) {
  try {
    // Step4 内部已经调用了 completeImport，这里主要是 UI 收尾
    createMessage.success('导入完成');

    // 重置状态
    resetState();

    emit('success');
    closePopup();

    // 刷新当前标签页
    await refreshPage();
  } catch (error: any) {
    createMessage.error(error?.message || '完成导入失败');
    // 即使出错也要重置状态
    resetState();
  }
}

// 取消导入
async function handleCancel() {
   createConfirm({
      iconType: 'warning',
      title: '提示',
      content: '确定要取消导入吗？已上传的数据可能会丢失。',
      onOk: async () => {
        try {
            // 如果有 sessionId，调用后端取消接口清理资源
            if (importSessionId.value) {
            try {
                await deleteImportSession(importSessionId.value);
            } catch (error) {
                // 如果取消失败，不影响关闭弹窗
                console.warn('取消导入会话失败:', error);
            }
            }

            // 重置状态
            resetState();

            emit('cancel');
            closePopup();

            // 刷新当前标签页
            await refreshPage();
        } catch (error) {
            console.error('取消导入时出错:', error);
            resetState();
            closePopup();
        }
      }
    });
}



// 重置所有状态
function resetState() {
  currentStep.value = 0;
  importSessionId.value = '';
  loading.value = false;

  // 重置步骤组件引用（如果组件支持重置方法）
  step1Ref.value?.reset?.();
  step2Ref.value?.reset?.();
  step3Ref.value?.reset?.();
  step4Ref.value?.reset?.();
}
</script>

<style lang="less" scoped>
.step-content {
  padding: 20px;
}

.loading-overlay {
  position: absolute;
  top: 0;
  left: 0;
  right: 0;
  bottom: 0;
  background: rgba(255, 255, 255, 0.8);
  display: flex;
  align-items: center;
  justify-content: center;
  z-index: 1000;
}

:deep(.step-import-wizard-popup) {
    .jnpf-basic-popup-content {
        padding: 0;
    }
}
</style>