<template>
  <div class="p-4 h-full flex flex-col gap-4">
    <div class="bg-white p-4 rounded-lg shadow-sm flex justify-between items-center">
      <div>
        <h1 class="text-lg font-bold flex items-center gap-2">
          <span class="text-purple-600">
            <svg xmlns="http://www.w3.org/2000/svg" width="24" height="24" viewBox="0 0 24 24" fill="none"
              stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">
              <path d="M21 16V8a2 2 0 0 0-1-1.73l-7-4a2 2 0 0 0-2 0l-7 4A2 2 0 0 0 3 8v8a2 2 0 0 0 1 1.73l7 4a2 2 0 0 0 2 0l7-4A2 2 0 0 0 21 16z"></path>
              <polyline points="3.27 6.96 12 12.01 20.73 6.96"></polyline>
              <line x1="12" y1="22.08" x2="12" y2="12"></line>
            </svg>
          </span>
          公共维度管理
        </h1>
        <p class="text-gray-500 text-sm mt-1">管理公共维度信息，用于计算使用。支持版本管理，确保计算精度。</p>
      </div>
      <a-button type="primary" @click="handleCreate" class="bg-purple-600 border-purple-600 hover:bg-purple-500">
        + 新增公共维度
      </a-button>
    </div>

    <div class="flex-1 overflow-auto bg-gray-50 p-4 rounded-lg">
      <a-spin :spinning="loading">
        <div v-if="dataList.length === 0" class="flex flex-col items-center justify-center h-full text-gray-400">
          <svg xmlns="http://www.w3.org/2000/svg" width="64" height="64" viewBox="0 0 24 24" fill="none"
            stroke="currentColor" stroke-width="1" stroke-linecap="round" stroke-linejoin="round" class="mb-4">
            <path d="M21 16V8a2 2 0 0 0-1-1.73l-7-4a2 2 0 0 0-2 0l-7 4A2 2 0 0 0 3 8v8a2 2 0 0 0 1 1.73l7 4a2 2 0 0 0 2 0l7-4A2 2 0 0 0 21 16z"></path>
          </svg>
          <p class="text-lg mb-2">暂无公共维度数据</p>
          <a-button type="primary" @click="handleCreate">创建第一个公共维度</a-button>
        </div>
        <div v-else class="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-4">
          <div v-for="item in dataList" :key="item.id"
            class="bg-white rounded-lg p-4 shadow-sm border border-gray-200 hover:border-purple-400 hover:shadow-md transition-all relative group">
            <div class="flex items-start justify-between mb-3">
              <div class="flex items-center gap-3 flex-1">
                <div
                  class="w-12 h-12 rounded-lg bg-gradient-to-br from-purple-100 to-purple-200 flex items-center justify-center text-xl font-bold text-purple-700 border border-purple-200 shadow-sm">
                  {{ (item.dimensionName || '').charAt(0) || '?' }}
                </div>
                <div class="flex-1">
                  <div class="flex items-center gap-2">
                    <h3 class="font-bold text-lg text-gray-800">{{ item.dimensionName }}</h3>
                    <a-tag v-if="item._currentVersion" color="purple" size="small">
                      v{{ item._currentVersion.version }}
                    </a-tag>
                  </div>
                  <p class="text-sm text-gray-500 mt-1">键名: {{ item.dimensionKey }}</p>
                  <div class="flex items-center gap-4 mt-2 text-xs text-gray-600">
                    <a-tag :color="getValueTypeColor(item.valueType)" size="small">
                      {{ getValueTypeLabel(item.valueType) }}
                    </a-tag>
                    <span v-if="item.unit">单位: {{ item.unit }}</span>
                    <span v-if="item.precision !== null && item.precision !== undefined">精度: {{ item.precision }}位</span>
                  </div>
                </div>
              </div>
              <div class="hidden group-hover:flex gap-1 flex-col" @mousedown.stop @click.stop>
                <a-button type="link" size="small" @click.stop="handleEdit(item)">编辑</a-button>
                <a-button type="link" size="small" @click.stop="handleViewVersions(item)">版本</a-button>
                <a-button type="link" size="small" danger @click.stop="handleDelete(item)">删除</a-button>
              </div>
            </div>
          </div>
        </div>
      </a-spin>
    </div>

    <!-- 表单模态框 -->
    <PublicDimensionForm @register="registerFormModal" @reload="loadData" />

    <!-- 版本管理模态框 -->
    <VersionManageModal @register="registerVersionModal" />
  </div>
</template>

<script lang="ts" setup>
import { ref, onMounted } from 'vue';
import { useModal } from '/@/components/Modal';
import { useMessage } from '/@/hooks/web/useMessage';
import { getPublicDimensions, deletePublicDimension, getVersionList } from '/@/api/lab/publicDimension';
import PublicDimensionForm from './components/PublicDimensionForm.vue';
import VersionManageModal from './components/VersionManageModal.vue';

const [registerFormModal, { openModal: openFormModal }] = useModal();
const [registerVersionModal, { openModal: openVersionModal }] = useModal();
const { createMessage, createConfirm } = useMessage();

const dataList = ref<any[]>([]);
const loading = ref(false);

// 版本信息缓存
const versionInfoCache = ref<Map<string, any>>(new Map());

const loadData = async () => {
  loading.value = true;
  try {
    const res: any = await getPublicDimensions();
    let list = res.data || res || [];
    
    // 保留已有的版本信息
    list = list.map((item: any) => {
      const existingItem = dataList.value.find(existing => existing.id === item.id);
      return {
        ...item,
        _currentVersion: existingItem?._currentVersion || item._currentVersion,
      };
    });
    
    dataList.value = list;
    
    // 异步加载版本信息
    loadVersionInfoForList(list);
  } catch (error) {
    console.error('加载公共维度列表失败:', error);
    createMessage.error('加载公共维度列表失败');
    dataList.value = [];
  } finally {
    loading.value = false;
  }
};

// 加载版本信息
async function loadVersionInfoForList(dimensions: any[]) {
  const promises = dimensions.map(async (dimension) => {
    if (versionInfoCache.value.has(dimension.id) && dimension._currentVersion) {
      return;
    }
    
    try {
      const response = await getVersionList(dimension.id);
      const versions = Array.isArray(response) ? response : (response?.data || []);
      
      if (!Array.isArray(versions) || versions.length === 0) {
        return;
      }
      
      const currentVersion = versions.find(v => v.isCurrent === 1 || v.isCurrent === true) || versions[0];
      versionInfoCache.value.set(dimension.id, currentVersion);
      
      const index = dataList.value.findIndex(item => item.id === dimension.id);
      if (index !== -1) {
        dataList.value[index] = {
          ...dataList.value[index],
          _currentVersion: currentVersion
        };
      }
    } catch (error) {
      console.error(`获取公共维度 ${dimension.id} 的版本信息失败`, error);
    }
  });

  await Promise.allSettled(promises);
}

const handleCreate = () => {
  openFormModal(true, { id: '' });
};

const handleEdit = (record: any) => {
  openFormModal(true, { id: record.id });
};

const handleViewVersions = (record: any) => {
  openVersionModal(true, { dimensionId: record.id, dimensionName: record.dimensionName });
};

const handleDelete = (record: any) => {
  createConfirm({
    iconType: 'warning',
    title: '确认删除',
    content: `确定要删除公共维度"${record.dimensionName}"吗？此操作不可恢复。`,
    onOk: async () => {
      try {
        await deletePublicDimension(record.id);
        createMessage.success('删除成功');
        loadData();
      } catch (error: any) {
        const errorMsg = error?.response?.data?.msg || error?.message || '删除失败';
        createMessage.error(errorMsg);
      }
    },
  });
};

// 获取属性类型标签
function getValueTypeLabel(valueType: string): string {
  const map: Record<string, string> = {
    decimal: '小数',
    int: '整数',
    text: '文本',
  };
  return map[valueType] || valueType;
}

// 获取属性类型颜色
function getValueTypeColor(valueType: string): string {
  const map: Record<string, string> = {
    decimal: 'blue',
    int: 'green',
    text: 'orange',
  };
  return map[valueType] || 'default';
}

onMounted(() => {
  loadData();
});
</script>

<style scoped>
.ghost-card {
  opacity: 0.5;
  background: #f0f0f0;
}

.chosen-card {
  border-color: #722ed1;
  box-shadow: 0 4px 12px rgba(114, 46, 209, 0.2);
}
</style>
