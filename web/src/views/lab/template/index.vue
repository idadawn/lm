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
      <a-button type="primary" @click="handleCreate" class="bg-blue-600 border-blue-600 hover:bg-blue-500">
        + 新增导入模板
      </a-button>
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
          <a-button type="primary" @click="handleCreate">创建第一个导入模板</a-button>
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
                    <a-tag v-if="item.isDefault === 1" color="green" size="small">
                      默认
                    </a-tag>
                    <a-tag :color="getTemplateTypeColor(item.templateType)" size="small">
                      {{ getTemplateTypeLabel(item.templateType) }}
                    </a-tag>
                  </div>
                  <p class="text-sm text-gray-500 mt-1">编码: {{ item.templateCode }}</p>
                  <div class="flex items-center gap-4 mt-2 text-xs text-gray-600">
                    <span v-if="item.productSpecName">产品规格: {{ item.productSpecName }}</span>
                    <span>版本: v{{ item.version }}</span>
                  </div>
                  <p class="text-sm text-gray-600 mt-2 line-clamp-2">{{ item.description || '暂无描述' }}</p>
                </div>
              </div>
              <div class="hidden group-hover:flex gap-1 flex-col" @mousedown.stop @click.stop>
                <a-button type="link" size="small" @click.stop="handleEdit(item)">编辑</a-button>
                <a-button v-if="item.isDefault === 0" type="link" size="small" @click.stop="handleSetDefault(item)">
                  设默认
                </a-button>
                <a-button type="link" size="small" danger @click.stop="handleDelete(item)">删除</a-button>
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
import { getExcelTemplates, deleteExcelTemplate, setTemplateAsDefault } from '/@/api/lab/excelTemplate';
import TemplateForm from './TemplateForm.vue';

const [registerFormModal, { openModal: openFormModal }] = useModal();
const { createMessage, createConfirm } = useMessage();

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

const handleCreate = () => {
  openFormModal(true, { id: '' });
};

const handleEdit = (record: any) => {
  openFormModal(true, { id: record.id });
};

const handleSetDefault = (record: any) => {
  createConfirm({
    iconType: 'info',
    title: '设置默认模板',
    content: `确定要将"${record.templateName}"设置为默认导入模板吗？`,
    onOk: async () => {
      try {
        await setTemplateAsDefault(record.id);
        createMessage.success('设置默认模板成功');
        loadData();
      } catch (error: any) {
        const errorMsg = error?.response?.data?.msg || error?.message || '设置失败';
        createMessage.error(errorMsg);
      }
    },
  });
};

const handleDelete = (record: any) => {
  createConfirm({
    iconType: 'warning',
    title: '确认删除',
    content: `确定要删除导入模板"${record.templateName}"吗？此操作不可恢复。`,
    onOk: async () => {
      try {
        await deleteExcelTemplate(record.id);
        createMessage.success('删除成功');
        loadData();
      } catch (error: any) {
        const errorMsg = error?.response?.data?.msg || error?.message || '删除失败';
        createMessage.error(errorMsg);
      }
    },
  });
};

// 获取模板类型标签
function getTemplateTypeLabel(templateType: string): string {
  const map: Record<string, string> = {
    system: '系统模板',
    user: '个人模板',
  };
  return map[templateType] || templateType;
}

// 获取模板类型颜色
function getTemplateTypeColor(templateType: string): string {
  const map: Record<string, string> = {
    system: 'blue',
    user: 'green',
  };
  return map[templateType] || 'default';
}

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