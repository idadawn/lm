<template>
  <div class="step1-container">
    <!-- 文件上传区域 -->
    <div class="upload-section">
      <a-alert
        message="第一步：文件上传与解析"
        description="请上传Excel文件，系统将自动解析B列(原始炉号)、H列(Ps铁损)、I列(Ss激磁功率)、F列(Hc)、P列(检测时间)。"
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
      </div>

      <!-- 解析结果预览 -->
      <div v-if="parsedData.length > 0" class="parse-result-section">
        <h3 class="section-title">解析结果</h3>
        <a-alert
          :message="`共解析 ${totalRows} 行数据，其中有效数据 ${validDataRows} 行`"
          :type="validDataRows > 0 ? 'success' : 'warning'"
          show-icon
          style="margin-bottom: 16px" />

        <a-table
          :columns="columns"
          :data-source="parsedData"
          :pagination="{ pageSize: 10 }"
          size="small"
          :scroll="{ y: 300 }">
          <template #bodyCell="{ column, record }">
            <template v-if="column.key === 'isScratched'">
              <a-tag :color="record.isScratched ? 'orange' : 'blue'">
                {{ record.isScratched ? '是' : '否' }}
              </a-tag>
            </template>
            <template v-else-if="column.key === 'detectionTime'">
              {{ record.detectionTime ? formatToDateTime(record.detectionTime) : '-' }}
            </template>
            <template v-else-if="column.key === 'isValid'">
              <a-tag :color="record.isValid ? 'success' : 'error'">
                {{ record.isValid ? '有效' : '无效' }}
              </a-tag>
            </template>
          </template>
        </a-table>
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
  FileExcelOutlined
} from '@ant-design/icons-vue';
import { createMagneticImportSession, uploadAndParseMagneticData } from '/@/api/lab/magneticData';
import { formatToDateTime } from '/@/utils/dateUtil';

// Props
const props = defineProps<{
  importSessionId?: string;
}>();

const emit = defineEmits(['next', 'cancel']);

// 状态
const fileList = ref<any[]>([]);
const uploading = ref(false);
const isDragOver = ref(false);
const sessionId = ref<string>(props.importSessionId || '');
const fileBase64 = ref<string>('');
const fileName = ref<string>('');
const parsedData = ref<any[]>([]);
const totalRows = ref(0);
const validDataRows = ref(0);

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

// 表格列定义
const columns = [
  { title: '行号', dataIndex: 'rowIndex', key: 'rowIndex', width: 80 },
  { title: '原始炉号', dataIndex: 'originalFurnaceNo', key: 'originalFurnaceNo', width: 150 },
  { title: '炉号', dataIndex: 'furnaceNo', key: 'furnaceNo', width: 120 },
  { title: '是否刻痕', key: 'isScratched', width: 100 },
  { title: 'Ps铁损(H)', dataIndex: 'psLoss', key: 'psLoss', width: 100 },
  { title: 'Ss激磁功率(I)', dataIndex: 'ssPower', key: 'ssPower', width: 120 },
  { title: 'Hc(F)', dataIndex: 'hc', key: 'hc', width: 100 },
  { title: '检测时间(P)', dataIndex: 'detectionTime', key: 'detectionTime', width: 150 },
  { title: '状态', key: 'isValid', width: 80 },
];

// 计算是否可以进入下一步
const canGoNext = computed(() => {
  return fileList.value.length > 0 && !!sessionId.value && validDataRows.value > 0;
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

    // 如果已有 sessionId（从 prop 传入），直接使用；否则创建新的会话
    let currentSessionId = props.importSessionId || sessionId.value;
    
    if (!currentSessionId) {
      // 创建导入会话（上传文件并保存到后端）
      const result = await createMagneticImportSession({
        fileName: file.name,
        fileData: base64,
      });

      if (!result || (typeof result !== 'string' && !result)) {
        throw new Error('创建导入会话失败：未返回有效的会话ID');
      }
      currentSessionId = result;
      sessionId.value = currentSessionId;
      
      if (!sessionId.value) {
        throw new Error('创建导入会话失败：会话ID为空');
      }
    }

    // 上传并解析文件
    const parseResult = await uploadAndParseMagneticData(currentSessionId, {
      fileName: file.name,
      fileData: base64,
    });

    parsedData.value = parseResult.parsedData || [];
    totalRows.value = parseResult.totalRows || 0;
    validDataRows.value = parseResult.validDataRows || 0;

    if (validDataRows.value > 0) {
      message.success(`文件上传成功，共解析 ${totalRows.value} 行数据，其中有效数据 ${validDataRows.value} 行`);
    } else {
      message.warning(`文件上传成功，但未找到有效数据`);
    }
  } catch (error: any) {
    message.error(error.message || '文件上传失败');
    fileList.value = [];
    if (!props.importSessionId) {
      sessionId.value = '';
    }
    fileBase64.value = '';
    fileName.value = '';
    parsedData.value = [];
    totalRows.value = 0;
    validDataRows.value = 0;
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
  if (!props.importSessionId) {
    sessionId.value = '';
  }
  fileBase64.value = '';
  fileName.value = '';
  parsedData.value = [];
  totalRows.value = 0;
  validDataRows.value = 0;
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

  if (validDataRows.value === 0) {
    message.warning('没有有效数据，无法继续');
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

  // 传递 sessionId 和文件信息到下一步
  emit('next', {
    sessionId: currentSessionId,
    fileName: fileName.value,
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

.parse-result-section {
  margin-top: 24px;
  padding: 16px;
  background: #fafafa;
  border-radius: 8px;
  border: 1px solid #f0f0f0;
}
</style>
