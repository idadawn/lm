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

      <!-- 导入策略选择 -->
      <div class="strategy-section">
        <h3 class="section-title">选择导入策略</h3>
        <a-radio-group v-model:value="importStrategy" class="strategy-group">
          <a-radio value="incremental" class="strategy-item">
            <template #default>
              <div class="strategy-content">
                <div class="strategy-header"><FileAddOutlined /> 增量导入（推荐）</div>
                <div class="strategy-desc">自动跳过已导入的行，适合日常数据更新</div>
              </div>
            </template>
          </a-radio>
          <a-radio value="full" class="strategy-item">
            <template #default>
              <div class="strategy-content">
                <div class="strategy-header"><FileTextOutlined /> 全量导入</div>
                <div class="strategy-desc">导入所有数据，不跳过任何行</div>
              </div>
            </template>
          </a-radio>
        </a-radio-group>
      </div>

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
      </div>

    </div>

  </div>
</template>

<script lang="ts" setup>
import { ref, computed } from 'vue';
import { message } from 'ant-design-vue';
import {
  FileAddOutlined,
  FileTextOutlined,
  DeleteOutlined,
  InboxOutlined,
  FileExcelOutlined
} from '@ant-design/icons-vue';
import { createImportSession } from '/@/api/lab/rawData';
import type {
  ImportStrategy
} from '/@/api/lab/types/rawData';

const emit = defineEmits(['next', 'cancel']);

// 状态
const fileList = ref<any[]>([]);
const uploading = ref(false);
const isDragOver = ref(false);
const importStrategy = ref<ImportStrategy>('incremental');
const sessionId = ref<string>('');
const fileBase64 = ref<string>('');
const fileName = ref<string>('');

// 计算是否可以进入下一步
const canGoNext = computed(() => {
  return fileList.value.length > 0 && !!sessionId.value;
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

  try {
    // 转换为Base64
    const base64 = await fileToBase64(file);
    fileBase64.value = base64;
    fileName.value = file.name;

    // 创建导入会话（上传文件并保存到后端）
    const result = await createImportSession({
      fileName: file.name,
      importStrategy: importStrategy.value,
      fileData: base64,  // 将文件数据一起发送到后端保存
    });

    // 保存 sessionId（后端直接返回字符串 sessionId）
    if (!result || (typeof result !== 'string' && !result)) {
      throw new Error('创建导入会话失败：未返回有效的会话ID');
    }
    sessionId.value = result;
    
    if (!sessionId.value) {
      throw new Error('创建导入会话失败：会话ID为空');
    }

    message.success('文件上传成功，下一步将进行数据解析');
  } catch (error: any) {
    message.error(error.message || '文件上传失败');
    fileList.value = [];
    sessionId.value = '';
    fileBase64.value = '';
    fileName.value = '';
  } finally {
    uploading.value = false;
  }
}

function handleFileChange(info: any) {
  if (info.file.status === 'done' || info.file.originFileObj) {
    handleFile(info.file.originFileObj || info.file);
  }
}

function clearFile() {
  fileList.value = [];
  sessionId.value = '';
  fileBase64.value = '';
  fileName.value = '';
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

async function handleNext() {
  if (!fileList.value.length) {
    message.warning('请先选择文件');
    return;
  }

  if (!sessionId.value) {
    message.error('导入会话ID缺失，请重新上传文件');
    return;
  }

  // 传递 sessionId 和文件信息到下一步（文件数据已保存在后端，不再传递 fileData）
  emit('next', {
    sessionId: sessionId.value,
    fileName: fileName.value,
    importStrategy: importStrategy.value,
  });
}

// 保存并进入下一步（供父组件调用）
async function saveAndNext() {
  await handleNext();
}

function handleCancel() {
  emit('cancel');
}

// 暴露给父组件
defineExpose({
  canGoNext,
  saveAndNext,
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