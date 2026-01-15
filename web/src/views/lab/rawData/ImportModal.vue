<template>
  <BasicModal
    v-bind="$attrs"
    @register="registerModal"
    title="原始数据导入"
    :width="800"
    :show-cancel-btn="false"
    :show-ok-btn="false"
    destroyOnClose>
    <div class="import-content">
      <a-alert
        message="导入说明"
        description="请上传Excel文件，系统将自动解析日期、炉号、宽度、带材重量及检测数据。炉号格式：[产线数字][班次汉字][8位日期]-[炉号]-[卷号]-[分卷号][可选特性汉字]，例如：1甲20251101-1-4-1脆"
        type="info"
        show-icon
        style="margin-bottom: 20px" />

      <!-- 导入策略选择 -->
      <div class="import-strategy" style="margin-bottom: 20px">
        <a-radio-group v-model:value="importStrategy" :disabled="uploading">
          <a-radio value="incremental">增量导入</a-radio>
          <a-radio value="full">全量导入</a-radio>
        </a-radio-group>
        <div class="strategy-description">
          <span v-if="importStrategy === 'incremental'">自动跳过已导入的行（基于文件名和行数）</span>
          <span v-else-if="importStrategy === 'full'">导入所有数据，不跳过任何行</span>
        </div>
      </div>

      <a-upload
        v-model:file-list="fileList"
        :before-upload="beforeUpload"
        accept=".xlsx,.xls"
        :max-count="1"
        @remove="handleRemove">
        <template #uploadButton>
          <a-button type="primary" :loading="uploading">
            <template #icon><UploadOutlined /></template>
            选择文件
          </a-button>
        </template>
      </a-upload>

      <!-- 数据预览 -->
      <div v-if="previewData" class="data-preview" style="margin-top: 20px">
        <a-divider>数据预览</a-divider>
        <div class="preview-info">
          <a-row :gutter="16">
            <a-col :span="8">
              <a-statistic title="总行数" :value="previewData.statistics.totalRows" />
            </a-col>
            <a-col :span="8">
              <a-statistic title="有效数据" :value="previewData.statistics.validDataRows" :value-style="{ color: '#3f8600' }" />
            </a-col>
            <a-col :span="8">
              <a-statistic title="无效数据" :value="previewData.statistics.invalidDataRows" :value-style="{ color: '#cf1322' }" />
            </a-col>
          </a-row>
        </div>

        <!-- 错误提示 -->
        <div v-if="previewData.errors?.length > 0" style="margin-top: 16px">
          <a-alert
            message="数据校验提示"
            :description="`发现 ${previewData.errors.length} 行数据存在问题，导入后可在错误报告中查看详情`"
            type="warning"
            show-icon />
        </div>
      </div>

      <!-- 导入结果 -->
      <div v-if="importResult" class="import-result" style="margin-top: 20px">
        <a-divider>导入结果</a-divider>
        <a-row :gutter="16">
          <a-col :span="8">
            <a-statistic title="成功数量" :value="importResult.successCount" :value-style="{ color: '#3f8600' }" />
          </a-col>
          <a-col :span="8">
            <a-statistic title="失败数量" :value="importResult.failCount" :value-style="{ color: '#cf1322' }" />
          </a-col>
          <a-col :span="8">
            <a-statistic title="有效数据" :value="importResult.validDataCount || 0" :value-style="{ color: '#1890ff' }" />
          </a-col>
        </a-row>

        <div v-if="importResult.failCount > 0" style="margin-top: 20px">
          <a-alert message="部分数据导入失败，请下载错误报告查看详情" type="warning" show-icon style="margin-bottom: 10px" />
          <a-button type="primary" @click="handleDownloadErrorReport" :loading="downloading">
            <template #icon><DownloadOutlined /></template>
            下载错误报告
          </a-button>
        </div>

        <div v-if="importResult.successCount > 0 && importResult.failCount === 0" style="margin-top: 20px">
          <a-result status="success" title="导入成功" sub-title="所有数据已成功导入" />
        </div>
      </div>
    </div>
    <template #insertFooter>
      <a-button @click="handleClose">{{ t('common.cancelText') }}</a-button>
      <a-button type="primary" @click="handleImport" :loading="uploading" :disabled="!fileList.length">
        开始导入
      </a-button>
    </template>
  </BasicModal>
</template>
<script lang="ts" setup>
  import { ref } from 'vue';
  import { BasicModal, useModalInner } from '/@/components/Modal';
  import { useMessage } from '/@/hooks/web/useMessage';
  import { useI18n } from '/@/hooks/web/useI18n';
  import { importRawData, previewRawData } from '/@/api/lab/rawData';
  import { UploadOutlined, DownloadOutlined } from '@ant-design/icons-vue';
  import type { UploadFile } from 'ant-design-vue';
  import { downloadByData } from '/@/utils/file/download';
  import type { ImportStrategy, DataPreviewResult } from '/@/api/lab/types/rawData';

  const emit = defineEmits(['register', 'reload']);
  const [registerModal, { closeModal }] = useModalInner(init);
  const { createMessage } = useMessage();
  const { t } = useI18n();

  const fileList = ref<UploadFile[]>([]);
  const uploading = ref(false);
  const downloading = ref(false);
  const importResult = ref<any>(null);
  const importStrategy = ref<ImportStrategy>('incremental');
  const previewData = ref<DataPreviewResult | null>(null);

  function init() {
    fileList.value = [];
    uploading.value = false;
    downloading.value = false;
    importResult.value = null;
    importStrategy.value = 'incremental';
    previewData.value = null;
  }

  function beforeUpload(file: File) {
    const isExcel = file.type === 'application/vnd.openxmlformats-officedocument.spreadsheetml.sheet' ||
                    file.type === 'application/vnd.ms-excel' ||
                    file.name.endsWith('.xlsx') ||
                    file.name.endsWith('.xls');
    if (!isExcel) {
      createMessage.error('只能上传Excel文件！');
      return false;
    }
    const isLt10M = file.size / 1024 / 1024 < 10;
    if (!isLt10M) {
      createMessage.error('文件大小不能超过10MB！');
      return false;
    }
    return false; // 阻止自动上传
  }

  function handleRemove() {
    fileList.value = [];
    importResult.value = null;
    previewData.value = null;
  }

  async function handleImport() {
    if (!fileList.value.length) {
      createMessage.warning('请先选择文件');
      return;
    }

    const file = fileList.value[0].originFileObj || fileList.value[0];
    if (!file) {
      createMessage.warning('文件不存在');
      return;
    }

    uploading.value = true;
    try {
      // 读取文件并转换为Base64
      const base64 = await fileToBase64(file);
      const fileName = file.name;

      // 首先预览数据
      try {
        const previewResult = await previewRawData({
          fileData: base64,
          fileName: fileName,
          importStrategy: importStrategy.value,
        });
        previewData.value = previewResult;
      } catch (previewError) {
        console.warn('预览失败，继续导入:', previewError);
      }

      // 调用导入接口
      const result = await importRawData({
        fileData: base64,
        fileName: fileName,
        importStrategy: importStrategy.value,
      });

      importResult.value = result.data;
      createMessage.success(`导入完成：成功 ${result.data.successCount} 条，失败 ${result.data.failCount} 条`);

      if (result.data.successCount > 0) {
        emit('reload');
      }
    } catch (error: any) {
      createMessage.error(error.message || '导入失败');
    } finally {
      uploading.value = false;
    }
  }

  function fileToBase64(file: File): Promise<string> {
    return new Promise((resolve, reject) => {
      const reader = new FileReader();
      reader.onload = () => {
        const result = reader.result as string;
        // 移除 data:application/vnd.openxmlformats-officedocument.spreadsheetml.sheet;base64, 前缀
        const base64 = result.includes(',') ? result.split(',')[1] : result;
        resolve(base64);
      };
      reader.onerror = reject;
      reader.readAsDataURL(file);
    });
  }

  function handleDownloadErrorReport() {
    if (!importResult.value?.errorReport) {
      createMessage.warning('错误报告不存在');
      return;
    }

    downloading.value = true;
    try {
      // 将Base64转换为Blob
      const byteCharacters = atob(importResult.value.errorReport);
      const byteNumbers = new Array(byteCharacters.length);
      for (let i = 0; i < byteCharacters.length; i++) {
        byteNumbers[i] = byteCharacters.charCodeAt(i);
      }
      const byteArray = new Uint8Array(byteNumbers);
      const blob = new Blob([byteArray], {
        type: 'application/vnd.openxmlformats-officedocument.spreadsheetml.sheet',
      });

      // 下载文件
      downloadByData(blob, importResult.value.errorReportFileName || '错误报告.xlsx');
    } catch (error: any) {
      createMessage.error('下载失败：' + error.message);
    } finally {
      downloading.value = false;
    }
  }

  function handleClose() {
    closeModal();
    if (importResult.value?.successCount > 0) {
      emit('reload');
    }
  }
</script>
<style scoped>
  .import-content {
    padding: 10px 0;
  }
  .import-result {
    padding: 10px;
    background: #f5f5f5;
    border-radius: 4px;
  }
  .import-strategy {
    padding: 12px;
    background: #fafafa;
    border-radius: 4px;
    border: 1px solid #e8e8e8;
  }
  .strategy-description {
    margin-top: 8px;
    font-size: 12px;
    color: #666;
  }
  .data-preview {
    padding: 16px;
    background: #fafafa;
    border-radius: 4px;
    border: 1px solid #e8e8e8;
  }
  .preview-info {
    margin-bottom: 16px;
  }
</style>
