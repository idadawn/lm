<template>
  <div class="step1-container">
    <!-- 文件上传区域 -->
    <div class="upload-section">
      <a-alert
        message="第一步：文件上传"
        description="请上传Excel文件，系统将在下一步自动解析日期、炉号、宽度、带材重量及检测数据。支持动态检测列数，不再限制为22列。"
        type="info"
        show-icon
        style="margin-bottom: 24px" />


      <!-- 文件上传 -->
      <div class="file-upload-section">
        <h3 class="section-title">选择文件</h3>
        <div class="upload-area" :class="{ 'drag-over': isDragOver }"
             @drop="handleDrop"
             @dragover="handleDragOver"
             @dragleave="handleDragLeave">
          <a-upload
            v-model:file-list="fileList"
            :before-upload="beforeUpload"
            :show-upload-list="false"
            accept=".xlsx,.xls"
            :max-count="1"
            @change="handleFileChange">
            <div class="upload-trigger">
              <div v-if="!fileList.length" class="upload-placeholder">
                <InboxOutlined style="font-size: 36px; color: #1890ff; margin-bottom: 12px" />
                <div class="upload-text">
                  <div class="upload-title">点击或拖拽文件到此处上传</div>
                  <div class="upload-desc">支持 .xlsx 和 .xls 格式，文件大小不超过 50MB</div>
                </div>
              </div>
              <div v-else class="file-info">
                <FileExcelOutlined style="font-size: 32px; color: #52c41a; margin-bottom: 8px" />
                <div class="file-name">{{ fileList[0].name }}</div>
                <div class="file-size">{{ formatFileSize(fileList[0].size) }}</div>
              </div>
            </div>
          </a-upload>
        </div>
        <a-button v-if="fileList.length" type="link" danger @click="clearFile">
          <DeleteOutlined /> 清除文件
        </a-button>

        <!-- 模板验证错误信息 -->
        <div v-if="validationErrors.length > 0" class="validation-errors">
          <a-alert
            type="error"
            show-icon
            :message="`发现 ${validationErrors.length} 个问题，请检查后重新上传`"
            style="margin-bottom: 12px">
            <template #description>
              <div class="error-list">
                <div
                  v-for="(error, index) in validationErrors"
                  :key="index"
                  class="error-item">
                  <ExclamationCircleOutlined style="color: #ff4d4f; margin-right: 8px" />
                  <span>{{ error }}</span>
                </div>
              </div>
            </template>
          </a-alert>
        </div>
      </div>

    </div>

    <!-- 重复上传提示（无遮罩层） -->
    <div v-if="showDuplicateDialog" class="duplicate-dialog">
      <div class="duplicate-dialog-card">
        <div class="duplicate-dialog-title">文件已上传过</div>
        <div class="duplicate-dialog-message">{{ duplicateDialogMessage }}</div>
        <div class="duplicate-dialog-actions">
          <a-button @click="handleDuplicateCancel">取消导入</a-button>
          <a-button type="primary" :loading="uploading" @click="handleDuplicateConfirm">仍然上传</a-button>
        </div>
      </div>
    </div>

  </div>
</template>

<script lang="ts" setup>
import { ref, computed, watch } from 'vue';
import { message } from 'ant-design-vue';
import {
  DeleteOutlined,
  InboxOutlined,
  FileExcelOutlined,
  ExclamationCircleOutlined
} from '@ant-design/icons-vue';
import { createImportSession, deleteImportSession } from '/@/api/lab/rawData';
import { getSystemFields, parseExcelHeaders, getDefaultTemplate, validateExcelAgainstTemplate } from '/@/api/lab/excelTemplate';
import type { ExcelTemplateConfig } from '/@/api/lab/types/excelTemplate';

// Props
const props = defineProps<{
  importSessionId?: string;
}>();

const emit = defineEmits(['next', 'fileCleared', 'cancel']);

// 状态
const fileList = ref<any[]>([]);
const uploading = ref(false);
const isDragOver = ref(false);
const sessionId = ref<string>(props.importSessionId || '');
const fileBase64 = ref<string>('');
const fileName = ref<string>('');
const validationErrors = ref<string[]>([]);
const validating = ref(false);
const showDuplicateDialog = ref(false);
const duplicateDialogMessage = ref('');

// 监听 prop 变化，同步到内部状态
watch(
  () => props.importSessionId,
  (newId) => {
    if (newId) {
      sessionId.value = newId;
    }
  },
  { immediate: true }
);

// 计算是否可以进入下一步
const canGoNext = computed(() => {
  return fileList.value.length > 0 && !!sessionId.value && validationErrors.value.length === 0;
});

// 方法
function handleDragOver(e: DragEvent) {
  e.preventDefault();
  isDragOver.value = true;
}

function handleDragLeave() {
  isDragOver.value = false;
}

function handleDrop(e: DragEvent) {
  e.preventDefault();
  isDragOver.value = false;

  const files = e.dataTransfer?.files;
  if (files?.length) {
    const file = files[0];
    if (validateFile(file)) {
      handleFile(file);
    }
  }
}

function beforeUpload(file: File) {
  if (validateFile(file)) {
    handleFile(file);
  }
  return false; // 阻止自动上传
}

function validateFile(file: File): boolean {
  const isExcel = file.type === 'application/vnd.openxmlformats-officedocument.spreadsheetml.sheet' ||
                  file.type === 'application/vnd.ms-excel' ||
                  file.name.endsWith('.xlsx') ||
                  file.name.endsWith('.xls');

  if (!isExcel) {
    message.error('只能上传Excel文件！');
    return false;
  }

  const isLt50M = file.size / 1024 / 1024 < 50;

  if (!isLt50M) {
    message.error('文件大小不能超过50MB！');
    return false;
  }

  return true;
}

async function handleFile(file: File) {
  fileList.value = [{
    uid: Date.now(),
    name: file.name,
    size: file.size,
    originFileObj: file,
  }];

  uploading.value = true;
  validationErrors.value = [];

  try {
    // 转换为Base64
    const base64 = await fileToBase64(file);
    fileBase64.value = base64;
    fileName.value = file.name;

    // 验证Excel文件与模板
    await validateExcelTemplate(base64);

    // 如果有验证错误，不创建导入会话
    if (validationErrors.value.length > 0) {
      message.warning('文件上传完成，但发现模板验证问题，请查看下方错误提示');
      return;
    }

    // 如果已有 sessionId（从 prop 传入），直接使用；否则创建新的会话
    if (!sessionId.value) {
      await createSession(false);
    } else {
      // 如果已有 sessionId，更新会话的文件信息（如果需要）
      // 注意：这里假设文件已经在创建会话时保存，所以不需要再次上传
      // 如果需要更新文件，可以在这里调用更新接口
    }

    message.success('文件上传成功，下一步将进行数据解析');
  } catch (error: any) {
    const duplicateMessage = extractDuplicateMessage(error);
    if (duplicateMessage) {
      showDuplicateConfirm(duplicateMessage);
      return;
    }

    message.error(getErrorMessage(error) || '文件上传失败');
    fileList.value = [];
    // 只有在创建新会话失败时才清空 sessionId
    // 如果是从 prop 传入的，不清空
    if (!props.importSessionId) {
      sessionId.value = '';
    }
    fileBase64.value = '';
    fileName.value = '';
    validationErrors.value = [];
  } finally {
    uploading.value = false;
  }
}

// 验证Excel文件与模板
async function validateExcelTemplate(fileBase64: string) {
  validating.value = true;
  validationErrors.value = [];

  try {
    // 调用后端验证接口
    const validationRes: any = await validateExcelAgainstTemplate({
      templateCode: 'RawDataImport', // 使用枚举值
      fileName: fileName.value || 'uploaded_file.xlsx',
      fileData: fileBase64
    });

    // 处理后端返回的数据格式
    const validationResult = validationRes?.data || validationRes;

    if (!validationResult) {
      validationErrors.value.push('模板验证失败：未返回验证结果');
      return;
    }

    // 设置验证错误信息
    if (validationResult.errors && Array.isArray(validationResult.errors)) {
      validationErrors.value = validationResult.errors;
    } else {
      validationErrors.value = [];
    }

    // 如果验证失败，显示错误信息
    if (!validationResult.isValid && validationErrors.value.length === 0) {
      validationErrors.value.push('Excel文件与模板配置不匹配，请检查文件格式');
    }
  } catch (error: any) {
    console.error('模板验证失败:', error);
    validationErrors.value.push(`模板验证失败：${error.message || '未知错误'}`);
  } finally {
    validating.value = false;
  }
}

function handleFileChange(info: any) {
  if (info.file.status === 'done' || info.file.originFileObj) {
    handleFile(info.file.originFileObj || info.file);
  }
}

async function clearFile() {
  try {
    // 如果已经有sessionId，删除后端的session
    if (sessionId.value) {
      await deleteImportSession(sessionId.value);
    }
    // 如果是从 prop 传入的 sessionId，也尝试删除
    if (props.importSessionId && props.importSessionId !== sessionId.value) {
      await deleteImportSession(props.importSessionId);
    }
  } catch (error) {
    console.error('删除导入会话失败:', error);
    // 即使删除失败，也继续清理前端状态
  } finally {
    fileList.value = [];
    sessionId.value = '';
    fileBase64.value = '';
    fileName.value = '';
    validationErrors.value = [];
    // 通知父组件文件已清空，需要跳转到第一步
    emit('fileCleared');
  }
}

function formatFileSize(bytes: number): string {
  if (bytes === 0) return '0 B';
  const k = 1024;
  const sizes = ['B', 'KB', 'MB', 'GB'];
  const i = Math.floor(Math.log(bytes) / Math.log(k));
  return parseFloat((bytes / Math.pow(k, i)).toFixed(2)) + ' ' + sizes[i];
}

function fileToBase64(file: File): Promise<string> {
  return new Promise((resolve, reject) => {
    const reader = new FileReader();
    reader.onload = () => {
      const result = reader.result as string;
      const base64 = result.includes(',') ? result.split(',')[1] : result;
      resolve(base64);
    };
    reader.onerror = reject;
    reader.readAsDataURL(file);
  });
}

async function createSession(forceUpload: boolean) {
  const result = await createImportSession({
    fileName: fileName.value,
    fileData: fileBase64.value,
    forceUpload,
  });

  if (!result || (typeof result !== 'string' && !result)) {
    throw new Error('创建导入会话失败：未返回有效的会话ID');
  }
  sessionId.value = result;

  if (!sessionId.value) {
    throw new Error('创建导入会话失败：会话ID为空');
  }
}

function getErrorMessage(error: any): string {
  return (
    error?.message ||
    error?.response?.data?.message ||
    error?.response?.data?.Message ||
    error?.msg ||
    ''
  );
}

function extractDuplicateMessage(error: any): string | null {
  const messageText = getErrorMessage(error);
  const marker = '[DUPLICATE_UPLOAD]';
  const code = error?.code || error?.response?.data?.code;
  if (code === 'COM1026' || code === 1026) {
    return messageText?.replace(marker, '').trim() || '文件已上传过，是否继续上传？';
  }
  if (!messageText) return null;
  if (messageText.includes(marker)) {
    return messageText.replace(marker, '').trim();
  }
  if (messageText.includes('已上传过') || messageText.includes('已上传') || messageText.includes('已存在')) {
    return messageText.trim();
  }
  return null;
}

function showDuplicateConfirm(content: string) {
  duplicateDialogMessage.value = content || '文件已上传过，是否继续上传？';
  showDuplicateDialog.value = true;
}

async function handleDuplicateConfirm() {
  try {
    uploading.value = true;
    await createSession(true);
    message.success('已确认继续上传');
  } catch (error: any) {
    message.error(getErrorMessage(error) || '文件上传失败');
    fileList.value = [];
    if (!props.importSessionId) {
      sessionId.value = '';
    }
    fileBase64.value = '';
    fileName.value = '';
    validationErrors.value = [];
  } finally {
    uploading.value = false;
    showDuplicateDialog.value = false;
  }
}

function handleDuplicateCancel() {
  showDuplicateDialog.value = false;
  emit('cancel');
  message.info('已取消导入');
  void clearFile();
}

async function handleNext() {
  if (!fileList.value.length) {
    message.warning('请先选择文件');
    return;
  }

  // 检查是否有验证错误
  if (validationErrors.value.length > 0) {
    message.error('请先解决模板验证问题，再进入下一步');
    return;
  }

  // 检查 sessionId：优先使用 prop，其次使用内部状态
  const currentSessionId = props.importSessionId || sessionId.value;

  if (!currentSessionId || currentSessionId.trim() === '') {
    message.error('导入会话ID缺失，请重新上传文件');
    return;
  }

  // 如果内部 sessionId 为空但 prop 有值，同步到内部状态
  if (!sessionId.value && props.importSessionId) {
    sessionId.value = props.importSessionId;
  }

  // 传递 sessionId 和文件信息到下一步（文件数据已保存在后端，不再传递 fileData）
  emit('next', {
    sessionId: currentSessionId,
    fileName: fileName.value,
    importStrategy: 'incremental', // 固定为 incremental（已废弃，但需要传递以保持兼容）
  });
}

// 保存并进入下一步（供父组件调用）
async function saveAndNext() {
  await handleNext();
}

// 暴露给父组件
defineExpose({
  canGoNext,
  saveAndNext,
  clearFile, // 暴露清空文件的方法
});
</script>

<style lang="less" scoped>
.step1-container {
  padding: 0;
  position: relative;
  background: #fff;
}

.upload-section {
  padding: 0;
}

.duplicate-dialog {
  position: fixed;
  top: 50%;
  left: 50%;
  transform: translate(-50%, -50%);
  z-index: 2000;
  pointer-events: none;
}

.duplicate-dialog-card {
  width: 360px;
  background: #fff;
  border: 1px solid #f0f0f0;
  border-radius: 8px;
  box-shadow: 0 8px 24px rgba(0, 0, 0, 0.16);
  padding: 16px;
  pointer-events: auto;
}

.duplicate-dialog-title {
  font-size: 14px;
  font-weight: 600;
  color: #262626;
  margin-bottom: 8px;
}

.duplicate-dialog-message {
  font-size: 13px;
  color: #595959;
  line-height: 1.5;
  margin-bottom: 12px;
}

.duplicate-dialog-actions {
  display: flex;
  justify-content: flex-end;
  gap: 8px;
}

.section-title {
  font-size: 15px;
  font-weight: 600;
  margin-bottom: 12px;
  color: #262626;
  line-height: 1.5;
}

.strategy-section {
  margin-bottom: 24px;
  padding: 16px;
  background: #fafafa;
  border-radius: 8px;
  border: 1px solid #f0f0f0;
}

.strategy-group {
  width: 100%;
  display: flex;
  gap: 12px;
}

.strategy-item {
  flex: 1;
  display: block;
  height: auto;
  padding: 16px;
  border: 1px solid #e8e8e8;
  border-radius: 8px;
  background: #fff;
  transition: all 0.3s;
  cursor: pointer;

  &:hover {
    border-color: #1890ff;
    box-shadow: 0 2px 8px rgba(24, 144, 255, 0.1);
    transform: translateY(-1px);
  }

  &.ant-radio-wrapper-checked {
    border-color: #1890ff;
    background: #f0f9ff;
    box-shadow: 0 2px 8px rgba(24, 144, 255, 0.15);
  }

  :deep(.ant-radio) {
    margin-top: 2px;
  }
}

.strategy-content {
  margin-left: 8px;
}

.strategy-header {
  font-weight: 500;
  font-size: 14px;
  margin-bottom: 6px;
  color: #262626;
  display: flex;
  align-items: center;

  .anticon {
    margin-right: 8px;
    font-size: 16px;
  }
}

.strategy-desc {
  font-size: 13px;
  color: #8c8c8c;
  line-height: 1.5;
}

.file-upload-section {
  margin-bottom: 24px;
  padding: 16px;
  background: #fafafa;
  border-radius: 8px;
  border: 1px solid #f0f0f0;
}

.upload-area {
  border: 2px dashed #d9d9d9;
  border-radius: 8px;
  padding: 16px;
  text-align: center;
  transition: all 0.3s;

  &.drag-over {
    border-color: #1890ff;
    background-color: #f0f9ff;
  }
}

.upload-trigger {
  cursor: pointer;
}

.upload-placeholder {
  padding: 20px 0;
}

.upload-title {
  font-size: 14px;
  margin-bottom: 6px;
  color: #262626;
}

.upload-desc {
  font-size: 12px;
  color: #8c8c8c;
}

.file-info {
  padding: 20px 0;
}

.file-name {
  font-weight: 500;
  margin-bottom: 4px;
}

.file-size {
  font-size: 12px;
  color: #8c8c8c;
}

.preview-section {
  margin-bottom: 24px;
  padding: 16px;
  background: #fafafa;
  border-radius: 8px;
  border: 1px solid #f0f0f0;
}

.preview-statistics {
  padding: 20px;
  background: #fff;
  border-radius: 8px;
  margin-bottom: 16px;
  border: 1px solid #f0f0f0;
}

.error-alert {
  margin-bottom: 16px;
  margin-top: 16px;
}

.error-summary {
  max-height: 150px;
  overflow-y: auto;
}

.error-item {
  padding: 4px 0;
  font-size: 12px;
  color: #595959;
  border-bottom: 1px solid #f0f0f0;

  &:last-child {
    border-bottom: none;
  }
}

.validation-errors {
  margin-top: 16px;
}

.error-list {
  max-height: 300px;
  overflow-y: auto;
  padding: 8px 0;
}

.error-list .error-item {
  display: flex;
  align-items: flex-start;
  padding: 8px 0;
  font-size: 13px;
  color: #ff4d4f;
  line-height: 1.6;
  border-bottom: 1px solid #ffeaea;

  &:last-child {
    border-bottom: none;
  }

  .anticon {
    margin-top: 2px;
    flex-shrink: 0;
  }
}

.more-errors {
  margin-top: 8px;
  font-size: 12px;
  color: #8c8c8c;
  font-style: italic;
}

.preview-table-container {
  background: #fff;
  border: 1px solid #f0f0f0;
  border-radius: 8px;
  overflow: hidden;
  box-shadow: 0 1px 2px rgba(0, 0, 0, 0.03);
}

.preview-more {
  padding: 12px;
  text-align: center;
  color: #8c8c8c;
  font-size: 14px;
  background: #fafafa;
  border-top: 1px solid #f0f0f0;
}

.furnace-no-cell {
  .parsed-info {
    margin-top: 4px;
  }
}

.detection-value {
  font-family: 'Courier New', monospace;
  font-size: 12px;
}

.step-actions {
  display: flex;
  justify-content: flex-end;
  align-items: center;
  gap: 12px;
  padding: 16px 24px;
  margin-top: 24px;
  border-top: 1px solid #f0f0f0;
  background: #fafafa;

  :deep(.ant-btn) {
    min-width: 100px;
    height: 36px;
    font-size: 14px;
    font-weight: 500;
    border-radius: 4px;
    transition: all 0.3s cubic-bezier(0.4, 0, 0.2, 1);

    &.ant-btn-primary {
      box-shadow: 0 2px 4px rgba(24, 144, 255, 0.2);

      &:hover:not(:disabled) {
        box-shadow: 0 4px 8px rgba(24, 144, 255, 0.3);
        transform: translateY(-1px);
      }

      &:active:not(:disabled) {
        transform: translateY(0);
        box-shadow: 0 2px 4px rgba(24, 144, 255, 0.2);
      }

      &:disabled {
        opacity: 0.6;
        cursor: not-allowed;
        box-shadow: none;
      }
    }

    &:not(.ant-btn-primary) {
      &:hover:not(:disabled) {
        border-color: #40a9ff;
        color: #40a9ff;
        background: #f0f9ff;
      }

      &:disabled {
        opacity: 0.6;
        cursor: not-allowed;
      }
    }
  }
}
</style>
