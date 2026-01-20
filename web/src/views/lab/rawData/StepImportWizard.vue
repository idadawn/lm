<template>
  <BasicPopup
    v-bind="$attrs"
    @register="registerPopup"
    title="原始数据导入向导"
    showOkBtn
    @ok="handleSubmit"
    destroyOnClose
    class="full-popup raw-data-import-wizard"
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
      <!-- 第一步：文件上传 -->
      <div v-show="activeStep === 0">
        <Step1UploadAndParse
          ref="step1Ref"
          :import-session-id="importSessionId"
          @next="handleStep1Next"
          @file-cleared="handleFileCleared"
        />
      </div>

      <!-- 第二步：数据解析与预览 -->
      <div v-show="activeStep === 1">
        <Step2DataPreview
          ref="step2PreviewRef"
          :import-session-id="importSessionId"
          :file-name="fileName"
          :import-strategy="importStrategy"
          @next="handleStep2PreviewNext"
          @prev="handlePrev"
          @cancel="handleCancel"
        />
      </div>

      <!-- 第三步：产品规格识别 -->
      <div v-show="activeStep === 2">
        <Step2ProductSpec
          ref="step2Ref"
          :import-session-id="importSessionId"
          @next="handleStep2Next"
          @prev="handlePrev"
          @cancel="handleCancel"
        />
      </div>

      <!-- 第四步：特性匹配 -->
      <div v-show="activeStep === 3">
        <Step3AppearanceFeature
          ref="step3Ref"
          :import-session-id="importSessionId"
          @next="handleStep3Next"
          @prev="handlePrev"
          @cancel="handleCancel"
        />
      </div>

      <!-- 第五步：数据核对与完成 -->
      <div v-show="activeStep === 4">
        <Step4ReviewAndComplete
          ref="step4Ref"
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
  import { ref, computed, reactive, toRefs, watch, nextTick } from 'vue';
  import { BasicPopup, usePopupInner } from '/@/components/Popup';
  import { useMessage } from '/@/hooks/web/useMessage';
  import { useI18n } from '/@/hooks/web/useI18n';
  import { useTabs } from '/@/hooks/web/useTabs';

  // 导入步骤组件
  import Step1UploadAndParse from './components/Step1UploadAndParse.vue';
  import Step2DataPreview from './components/Step2DataPreview.vue';
  import Step2ProductSpec from './components/Step2ProductSpec.vue';
  import Step3AppearanceFeature from './components/Step3AppearanceFeature.vue';
  import Step4ReviewAndComplete from './components/Step4ReviewAndComplete.vue';
  import { deleteImportSession } from '/@/api/lab/rawData';
  import type { ImportStrategy } from '/@/api/lab/types/rawData';

  interface State {
    activeStep: number;
    importSessionId: string;
    fileName: string;
    importStrategy: ImportStrategy;
    nextLoading: boolean;
    key: number;
  }

  const { t } = useI18n();
  const emit = defineEmits(['register', 'reload']);
  const [registerPopup, { closePopup, changeOkLoading }] = usePopupInner(init);
  const { createMessage } = useMessage();
  const { refreshPage } = useTabs();

  // 状态管理
  const state = reactive<State>({
    activeStep: 0,
    importSessionId: '',
    fileName: '',
    importStrategy: 'incremental',
    nextLoading: false,
    key: +new Date(),
  });

  const { activeStep, importSessionId, fileName, importStrategy, nextLoading, key } = toRefs(state);

  // 步骤组件引用
  const step1Ref = ref<InstanceType<typeof Step1UploadAndParse>>();
  const step2PreviewRef = ref<InstanceType<typeof Step2DataPreview>>();
  const step2Ref = ref<InstanceType<typeof Step2ProductSpec>>();
  const step3Ref = ref<InstanceType<typeof Step3AppearanceFeature>>();
  const step4Ref = ref<InstanceType<typeof Step4ReviewAndComplete>>();

  // 步骤列表
  const stepList = ['文件上传', '数据解析与预览', '自动匹配产品规格', '匹配外观特性', '核对数据并完成导入'];
  const getStepList = computed(() => {
    return stepList;
  });

  // 监听步骤变化，更新 key 以刷新步骤组件
  watch(activeStep, () => {
    state.key = +new Date();
  });

  // 初始化
  function init() {
    state.activeStep = 0;
    state.importSessionId = '';
    state.fileName = '';
    state.importStrategy = 'incremental';
    state.nextLoading = false;
    state.key = +new Date();
  }

  // 步骤1完成（文件上传，文件数据已保存到后端）
  async function handleStep1Next(data: { sessionId: string; fileName: string; importStrategy: ImportStrategy }) {
    state.importSessionId = data.sessionId;
    state.fileName = data.fileName;
    state.importStrategy = data.importStrategy;
    state.activeStep = 1;
  }

  // 步骤2完成（数据解析与预览）
  async function handleStep2PreviewNext() {
    state.activeStep = 2;
    // 切换到第3步（产品规格识别）时，触发数据加载
    nextTick(() => {
      const step2Component = step2Ref.value as any;
      if (step2Component && typeof step2Component.triggerLoad === 'function') {
        console.log('StepImportWizard: 调用 step2Ref.triggerLoad');
        step2Component.triggerLoad();
      } else {
        console.warn('StepImportWizard: step2Ref.triggerLoad 不存在');
      }
    });
  }

  // 步骤3完成（产品规格识别）
  async function handleStep2Next() {
    state.activeStep = 3;
    // 切换到第4步（特性匹配）时，触发数据加载
    nextTick(() => {
      step3Ref.value?.triggerLoad?.();
    });
  }

  // 步骤4完成（特性匹配）
  async function handleStep3Next() {
    state.activeStep = 4;
    // 切换到第4步时，触发数据加载
    nextTick(() => {
      step4Ref.value?.triggerLoad?.();
    });
  }

  // 下一步
  async function handleNext() {
    try {
      state.nextLoading = true;

      switch (activeStep.value) {
        case 0:
          await step1Ref.value?.saveAndNext();
          break;
        case 1:
          await step2PreviewRef.value?.saveAndNext();
          break;
        case 2:
          await step2Ref.value?.saveAndNext();
          break;
        case 3:
          await step3Ref.value?.saveAndNext();
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
      if (step4Ref.value?.canGoNext) {
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

  // 处理文件清空事件（从第一步组件触发）
  function handleFileCleared() {
    // 当文件被清空时，跳转到第一步并重置状态
    state.activeStep = 0;
    state.importSessionId = '';
    state.fileName = '';
  }

  // 取消
  async function handleCancel() {
    try {
      // 先清空第一步的文件（会同时删除Session）
      if (step1Ref.value) {
        await step1Ref.value?.clearFile?.();
      } else if (importSessionId.value) {
        // 如果第一步组件不存在，直接删除Session
        await deleteImportSession(importSessionId.value);
      }
      // 重置状态
      init();
      closePopup();
      // 刷新当前页签
      await refreshPage();
    } catch (error) {
      console.error('取消导入失败:', error);
      // 重置状态
      init();
      closePopup();
      // 即使出错也尝试刷新页签
      try {
        await refreshPage();
      } catch (refreshError) {
        console.error('刷新页签失败:', refreshError);
      }
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
    min-width: 1000px; // 增加最小宽度，确保有足够空间显示所有步骤和箭头
    display: flex;
    align-items: center;
    padding: 0 16px;
    box-sizing: border-box;
    overflow-x: auto; // 如果内容超出，允许横向滚动
    overflow-y: hidden;
  }

  .steps {
    width: 100%; // 占满可用空间
    min-width: 1000px; // 与 wrapper 保持一致的最小宽度
    flex-shrink: 0; // 不允许收缩，确保有足够空间

    :deep(.ant-steps) {
      width: 100%; // 占满容器宽度
      min-width: 1000px; // 确保步骤组件有足够宽度
    }

    // 导航模式下，根据文字长度调整宽度
    :deep(.ant-steps-navigation) {
      display: flex;
      width: 100%;
      align-items: center;
      flex-wrap: nowrap; // 不换行

      .ant-steps-item {
        flex: 0 0 auto !important; // 强制根据内容自适应，不拉伸不收缩
        width: auto !important; // 强制宽度自适应
        min-width: fit-content; // 最小宽度适应内容
        max-width: none; // 不限制最大宽度
        padding: 0 28px 0 12px; // 增加左右边距，给箭头留出足够空间；左边距较小
        position: relative; // 确保 tail 定位正确
        
        .ant-steps-item-container {
          width: auto !important; // 容器宽度自适应内容
          min-width: fit-content; // 最小宽度适应内容
          display: flex;
          flex-direction: row; // 水平排列：数字在左，文字在右
          align-items: center; // 垂直居中对齐
          justify-content: flex-start;
          padding: 4px 0;
          gap: 8px; // 增加数字和文字之间的间距
        }
        
        .ant-steps-item-content {
          width: auto !important; // 内容宽度自适应
          min-width: fit-content; // 最小宽度适应内容
          max-width: none; // 不限制最大宽度
          display: flex;
          flex-direction: row;
          align-items: center;
          flex: 0 0 auto; // 不拉伸，根据内容自适应
        }
        
        .ant-steps-item-title {
          width: auto !important; // 标题宽度自适应
          min-width: fit-content; // 最小宽度适应内容
          max-width: none; // 不限制最大宽度
          text-align: left; // 左对齐
          padding: 0;
          white-space: nowrap; // 不换行
          overflow: visible; // 允许文字完整显示
          margin: 0;
          font-size: 12px; // 减小字体大小，确保长文本能完整显示
          line-height: 1.4;
        }
      }
      
      // 确保箭头（连接线）有足够空间显示，不被覆盖
      .ant-steps-item-tail {
        display: block !important; // 确保显示连接线
        position: absolute;
        top: 50%;
        left: calc(100% - 28px); // 从右边距开始，确保不被覆盖（与padding-right: 28px对应）
        width: 24px; // 增加箭头宽度，确保箭头完全可见
        height: 1px;
        margin-top: -0.5px;
        padding: 0;
        background-color: #d9d9d9;
        z-index: 1; // 确保在最上层
        
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
      
      // 最后一个步骤不需要箭头
      .ant-steps-item:last-child .ant-steps-item-tail {
        display: none !important;
      }
      
      // 确保图标（数字）和标题水平对齐，数字在左，文字在右
      .ant-steps-item-icon {
        margin: 0;
        margin-right: 0;
        flex-shrink: 0; // 图标不收缩
        order: 1; // 确保图标在文字前面（左侧）
      }
      
      .ant-steps-item-content {
        order: 2; // 确保内容在图标后面（右侧）
        flex-shrink: 0; // 内容不收缩
      }
    }
    
    // 小尺寸下的优化
    :deep(.ant-steps-small) {
      .ant-steps-item {
        padding: 0 20px 0 8px; // 小尺寸下增加左右边距，确保文字和箭头有足够空间

        .ant-steps-item-container {
          gap: 6px; // 小尺寸下减少间距
        }

        .ant-steps-item-title {
          font-size: 11px;
          line-height: 1.3;
          white-space: nowrap; // 不换行
        }

        .ant-steps-item-tail {
          left: calc(100% - 20px); // 小尺寸下调整箭头位置（与padding-right: 20px对应）
          width: 20px; // 小尺寸下保持足够箭头宽度
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
