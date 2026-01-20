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


      <a-upload
        v-model:file-list="fileList"
        :before-upload="beforeUpload"
        accept=".xlsx,.xls"
        :max-count="1"
        @remove="handleRemove">
        <a-button type="primary" :loading="uploading">
          <template #icon><UploadOutlined /></template>
          选择文件
        </a-button>
      </a-upload>

      <!-- 文件选择状态提示 -->
      <div v-if="fileList.length > 0" style="margin-top: 8px; color: #52c41a; font-size: 14px;">
        ✓ 已选择文件：{{ fileList[0].name }} ({{ formatFileSize(fileList[0].size) }})
      </div>
      <div v-else style="margin-top: 8px; color: #999; font-size: 14px;">
        请选择Excel文件（支持 .xlsx, .xls 格式，最大10MB）
      </div>

      <!-- 导入选项 -->
      <div v-if="fileList.length > 0" class="import-options" style="margin-top: 16px">
        <a-checkbox v-model:checked="skipExistingFurnaceNo">
          跳过已存在的炉号数据
        </a-checkbox>
        <div class="option-description" style="font-size: 12px; color: #666; margin-top: 4px">
          勾选后，如果炉号已存在，系统会自动跳过该行数据，不会重复导入
        </div>
      </div>


      <!-- 导入结果 -->
      <div v-if="importResult" class="import-result" style="margin-top: 20px">
        <a-divider>导入结果</a-divider>
        <a-row :gutter="16">
          <a-col :span="6">
            <a-statistic title="成功数量" :value="importResult.successRows" :value-style="{ color: '#3f8600' }" />
          </a-col>
          <a-col :span="6">
            <a-statistic title="失败数量" :value="importResult.failRows" :value-style="{ color: '#cf1322' }" />
          </a-col>
          <a-col :span="6">
            <a-statistic title="跳过数量" :value="importResult.skippedRows" :value-style="{ color: '#faad14' }" />
          </a-col>
          <a-col :span="6">
            <a-statistic title="总行数" :value="importResult.totalRows" :value-style="{ color: '#1890ff' }" />
          </a-col>
        </a-row>

        <!-- 显示重复炉号信息 -->
        <div v-if="importResult.duplicateFurnaceNos && importResult.duplicateFurnaceNos.length > 0" style="margin-top: 20px">
          <a-alert
            message="重复炉号提示"
            :description="`发现 ${importResult.duplicateFurnaceNos.length} 个重复炉号，已根据设置处理`"
            type="warning"
            show-icon
            style="margin-bottom: 10px" />
          <div style="font-size: 12px; color: #666; margin-top: 8px">
            重复炉号：{{ importResult.duplicateFurnaceNos.slice(0, 5).join('、') }}{{ importResult.duplicateFurnaceNos.length > 5 ? '...' : '' }}
          </div>
        </div>

        <!-- 显示错误信息 -->
        <div v-if="importResult.errors && importResult.errors.length > 0" style="margin-top: 20px">
          <a-alert message="导入过程中出现错误" type="error" show-icon style="margin-bottom: 10px" />
          <div style="max-height: 200px; overflow-y: auto; font-size: 12px; color: #666; padding: 8px; background: #fff2f0; border-radius: 4px;">
            <div v-for="(error, index) in importResult.errors.slice(0, 10)" :key="index" style="margin-bottom: 4px;">
              {{ error }}
            </div>
            <div v-if="importResult.errors.length > 10" style="color: #999; font-style: italic;">
              还有 {{ importResult.errors.length - 10 }} 条错误信息未显示...
            </div>
          </div>
        </div>

        <div v-if="importResult.successRows > 0 && importResult.failRows === 0" style="margin-top: 20px">
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
  import { simpleImportRawData } from '/@/api/lab/rawData';
  import { UploadOutlined } from '@ant-design/icons-vue';
  import type { UploadFile } from 'ant-design-vue';

  const emit = defineEmits(['register', 'reload']);
  const [registerModal, { closeModal }] = useModalInner(init);
  const { createMessage } = useMessage();
  const { t } = useI18n();

  const fileList = ref<UploadFile[]>([]);
  const uploading = ref(false);
  const importResult = ref<any>(null);
  const skipExistingFurnaceNo = ref(true); // 默认跳过已存在的炉号

  function init() {
    fileList.value = [];
    uploading.value = false;
    importResult.value = null;
    skipExistingFurnaceNo.value = true;
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
    // 返回 false 会阻止自动上传，但文件会显示在 fileList 中
    return false;
  }

  function handleRemove() {
    fileList.value = [];
    importResult.value = null;
  }

  async function handleImport() {
    if (!fileList.value.length) {
      createMessage.warning('请先选择文件');
      return;
    }

    const fileItem = fileList.value[0];
    const file = fileItem.originFileObj as File || (fileItem as any);
    if (!file) {
      createMessage.warning('文件不存在');
      return;
    }

    uploading.value = true;
    try {
      // 读取文件并转换为Base64
      const base64 = await fileToBase64(file);
      const fileName = file.name;

      // 调用简化导入接口（直接上传、解析、保存）
      const result = await simpleImportRawData({
        fileData: base64,
        fileName: fileName,
        skipExistingFurnaceNo: skipExistingFurnaceNo.value,
      });

      importResult.value = result;
      createMessage.success(`导入完成：成功 ${result.successRows} 条，失败 ${result.failRows} 条，跳过 ${result.skippedRows} 条`);

      // 显示重复炉号提示
      if (result.duplicateFurnaceNos && result.duplicateFurnaceNos.length > 0) {
        createMessage.warning(`发现 ${result.duplicateFurnaceNos.length} 个重复炉号，已根据设置处理`);
      }

      if (result.successRows > 0) {
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


  function handleClose() {
    closeModal();
    if (importResult.value?.successRows > 0) {
      emit('reload');
    }
  }

  // 格式化文件大小
  function formatFileSize(bytes: number): string {
    if (bytes === 0) return '0 B';
    const k = 1024;
    const sizes = ['B', 'KB', 'MB', 'GB'];
    const i = Math.floor(Math.log(bytes) / Math.log(k));
    return parseFloat((bytes / Math.pow(k, i)).toFixed(2)) + ' ' + sizes[i];
  }
</script>
<style scoped>
  .import-content {
    padding: 10px 0;
    max-height: 70vh; /* 防止内容过多超出屏幕高度 */
    overflow-y: auto; /* 添加垂直滚动 */
  }
  .import-result {
    padding: 10px;
    background: #f5f5f5;
    border-radius: 4px;
  }
  .import-options {
    padding: 12px;
    background: #fafafa;
    border-radius: 4px;
    border: 1px solid #e8e8e8;
  }
  .option-description {
    margin-top: 8px;
    font-size: 12px;
    color: #666;
  }
</style>
