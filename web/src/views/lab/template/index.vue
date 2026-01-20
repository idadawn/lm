<template>
  <div class="p-4 h-full flex flex-col gap-4">
    <div class="bg-white p-4 rounded-lg shadow-sm flex justify-between items-center">
      <div>
        <h1 class="text-lg font-bold flex items-center gap-2">
          <span class="text-blue-600">
            <svg xmlns="http://www.w3.org/2000/svg" width="24" height="24" viewBox="0 0 24 24" fill="none"
              stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">
              <path d="M14 2H6a2 2 0 0 0-2 2v16a2 2 0 0 0 2 2h12a2 2 0 0 0 2-2V8z"></path>
              <polyline points="14 2 14 8 20 8"></polyline>
              <line x1="16" y1="13" x2="8" y2="13"></line>
              <line x1="16" y1="17" x2="8" y2="17"></line>
              <polyline points="10 9 9 9 8 9"></polyline>
            </svg>
          </span>
          Excel导入模板管理
        </h1>
        <p class="text-gray-500 text-sm mt-1">管理Excel导入模板配置，用于快速读取和验证导入数据。模板可关联产品规格。</p>
      </div>
    </div>

    <div class="flex-1 overflow-auto bg-gray-50 p-4 rounded-lg">
      <a-spin :spinning="loading">
        <div v-if="dataList.length === 0" class="flex flex-col items-center justify-center h-full text-gray-400">
          <svg xmlns="http://www.w3.org/2000/svg" width="64" height="64" viewBox="0 0 24 24" fill="none"
            stroke="currentColor" stroke-width="1" stroke-linecap="round" stroke-linejoin="round" class="mb-4">
            <path d="M14 2H6a2 2 0 0 0-2 2v16a2 2 0 0 0 2 2h12a2 2 0 0 0 2-2V8z"></path>
            <polyline points="14 2 14 8 20 8"></polyline>
            <line x1="16" y1="13" x2="8" y2="13"></line>
            <line x1="16" y1="17" x2="8" y2="17"></line>
            <polyline points="10 9 9 9 8 9"></polyline>
          </svg>
          <p class="text-lg mb-2">暂无导入模板</p>
        </div>
        <div v-else class="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-4">
          <div v-for="item in dataList" :key="item.id"
            class="bg-white rounded-lg p-4 shadow-sm border border-gray-200 hover:border-blue-400 hover:shadow-md transition-all relative group">
            <div class="flex items-start justify-between mb-3">
              <div class="flex items-center gap-3 flex-1">
                <div
                  class="w-12 h-12 rounded-lg bg-gradient-to-br from-blue-100 to-blue-200 flex items-center justify-center text-xl font-bold text-blue-700 border border-blue-200 shadow-sm">
                  {{ (item.templateName || '').charAt(0) || 'T' }}
                </div>
                <div class="flex-1">
                  <div class="flex items-center gap-2">
                    <h3 class="font-bold text-lg text-gray-800">{{ item.templateName }}</h3>
                  </div>
                  <p class="text-sm text-gray-500 mt-1">编码: {{ item.templateCode }}</p>
                  <div class="flex items-center gap-4 mt-2 text-xs text-gray-600">
                    <!-- Version removed -->
                  </div>
                  <p class="text-sm text-gray-600 mt-2 line-clamp-2">{{ item.description || '暂无描述' }}</p>
                </div>
              </div>
              <div class="hidden group-hover:flex gap-1 flex-col" @mousedown.stop @click.stop>
                <a-button type="link" size="small" @click.stop="handleEdit(item)">编辑</a-button>

              </div>
            </div>
            <div class="text-xs text-gray-500 flex justify-between">
              <span>创建: {{ formatTime(item.creatorTime) }}</span>
              <span v-if="item.lastModifyTime">更新: {{ formatTime(item.lastModifyTime) }}</span>
            </div>
          </div>
        </div>
      </a-spin>
    </div>

    <!-- 表单模态框 -->
    <TemplateForm @register="registerFormModal" @reload="loadData" />
  </div>
</template>

<script lang="ts" setup>
import { ref, onMounted } from 'vue';
import { useModal } from '/@/components/Modal';
import { useMessage } from '/@/hooks/web/useMessage';
import { getExcelTemplates } from '/@/api/lab/excelTemplate';
import TemplateForm from './TemplateForm.vue';

const [registerFormModal, { openModal: openFormModal }] = useModal();
const { createMessage } = useMessage();

const dataList = ref<any[]>([]);
const loading = ref(false);

const loadData = async () => {
  loading.value = true;
  try {
    const res: any = await getExcelTemplates();
    let list = res.data || res || [];
    dataList.value = list;
  } catch (error) {
    console.error('加载模板列表失败:', error);
    createMessage.error('加载模板列表失败');
    dataList.value = [];
  } finally {
    loading.value = false;
  }
};



const handleEdit = (record: any) => {
  console.log('handleEdit clicked', record);
  openFormModal(true, { id: record.id });
};





// 格式化时间
function formatTime(timeStr: string): string {
  if (!timeStr) return '';
  const date = new Date(timeStr);
  return date.toLocaleDateString() + ' ' + date.toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' });
}

onMounted(() => {
  loadData();
});
</script>

<style scoped>
.line-clamp-2 {
  display: -webkit-box;
  -webkit-line-clamp: 2;
  -webkit-box-orient: vertical;
  overflow: hidden;
}
</style>