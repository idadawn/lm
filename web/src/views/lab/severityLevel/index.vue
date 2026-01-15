<template>
  <div class="p-4 h-full flex flex-col gap-4">
    <div class="bg-white p-4 rounded-lg shadow-sm flex justify-between items-center">
      <div>
        <h1 class="text-lg font-bold flex items-center gap-2">
          <span class="text-orange-600">
            <svg xmlns="http://www.w3.org/2000/svg" width="24" height="24" viewBox="0 0 24 24" fill="none"
              stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">
              <path d="M12 2v20M17 5H9.5a3.5 3.5 0 0 0 0 7h5a3.5 3.5 0 0 1 0 7H6" />
            </svg>
          </span>
          特性等级管理
        </h1>
        <p class="text-gray-500 text-sm mt-1">维护特性等级定义。每个等级的名称必须唯一，用于AI匹配和特征分类。</p>
      </div>
      <a-button type="primary" @click="handleCreate" class="bg-orange-600 border-orange-600 hover:bg-orange-500">
        + 新增特性等级
      </a-button>
    </div>

    <div class="flex-1 overflow-auto bg-gray-50 p-4 rounded-lg">
      <div v-if="list.length === 0" class="flex flex-col items-center justify-center h-full text-gray-400">
        <svg xmlns="http://www.w3.org/2000/svg" width="64" height="64" viewBox="0 0 24 24" fill="none"
          stroke="currentColor" stroke-width="1" stroke-linecap="round" stroke-linejoin="round" class="mb-4">
          <path d="M12 2v20M17 5H9.5a3.5 3.5 0 0 0 0 7h5a3.5 3.5 0 0 1 0 7H6" />
        </svg>
        <p class="text-lg mb-2">暂无特性等级数据</p>
      </div>
      <div v-else>
        <draggable
          v-model="list"
          item-key="id"
          handle=".drag-handle"
          :animation="200"
          ghost-class="ghost-card"
          chosen-class="chosen-card"
          @end="handleDragEnd"
          class="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-4">
          <template #item="{ element: item }">
            <div
              :class="[
                'bg-white rounded-lg p-4 shadow-sm border hover:shadow-md transition-shadow relative group cursor-move',
                item.isDefault ? 'border-blue-300 border-2' : 'border-gray-100'
              ]">
              <div class="flex items-start justify-between mb-3">
                <div class="flex items-center gap-3 flex-1">
                  <div class="drag-handle cursor-grab active:cursor-grabbing flex-shrink-0">
                    <svg xmlns="http://www.w3.org/2000/svg" width="20" height="20" viewBox="0 0 24 24" fill="none"
                      stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"
                      class="text-gray-400 hover:text-gray-600">
                      <circle cx="9" cy="12" r="1"></circle>
                      <circle cx="9" cy="5" r="1"></circle>
                      <circle cx="9" cy="19" r="1"></circle>
                      <circle cx="15" cy="12" r="1"></circle>
                      <circle cx="15" cy="5" r="1"></circle>
                      <circle cx="15" cy="19" r="1"></circle>
                    </svg>
                  </div>
                  <div
                    :class="[
                      'w-12 h-12 rounded flex items-center justify-center text-xl font-bold border',
                      item.isDefault 
                        ? 'bg-blue-50 text-blue-600 border-blue-200' 
                        : 'bg-orange-50 text-orange-600 border-orange-100'
                    ]">
                    {{ item.name.charAt(0) }}
                  </div>
                  <div class="flex-1">
                    <div class="flex items-center gap-2">
                      <h3 class="font-bold text-lg text-gray-800">{{ item.name }}</h3>
                      <!-- 默认项标识 - 标题旁图标 -->
                      <span v-if="item.isDefault" class="text-blue-500" title="默认等级">
                        <svg xmlns="http://www.w3.org/2000/svg" width="18" height="18" viewBox="0 0 24 24" fill="currentColor">
                          <path
                            d="M12 2l3.09 6.26L22 9.27l-5 4.87 1.18 6.88L12 17.77l-6.18 3.25L7 14.14 2 9.27l6.91-1.01L12 2z" />
                        </svg>
                      </span>
                    </div>
                    <div class="flex items-center gap-2 mt-1 flex-wrap">
                      <span v-if="item.description && item.description.trim()"
                        class="px-2 py-0.5 bg-gray-100 text-gray-600 rounded text-xs border border-gray-200">
                        {{ item.description }}
                      </span>
                      <span v-if="!item.description || !item.description.trim()"
                        class="px-2 py-0.5 bg-gray-50 text-gray-400 rounded text-xs border border-gray-200">
                        无描述
                      </span>
                      <span v-if="item.isDefault"
                        class="px-2 py-0.5 bg-blue-100 text-blue-600 rounded text-xs font-semibold border border-blue-200">
                        默认等级
                      </span>
                      <span v-if="!item.enabled" class="px-2 py-0.5 bg-red-100 text-red-600 rounded text-xs">
                        已禁用
                      </span>
                    </div>
                  </div>
                </div>
                <div class="flex flex-col items-end gap-2">
                  <!-- 默认项标识 - 右上角徽章，放在按钮上方 -->
                  <div v-if="item.isDefault"
                    class="bg-gradient-to-r from-blue-500 to-blue-600 text-white px-2 py-1 rounded-full text-xs font-bold shadow-md flex items-center gap-1">
                    <svg xmlns="http://www.w3.org/2000/svg" width="12" height="12" viewBox="0 0 24 24" fill="currentColor">
                      <path
                        d="M12 2l3.09 6.26L22 9.27l-5 4.87 1.18 6.88L12 17.77l-6.18 3.25L7 14.14 2 9.27l6.91-1.01L12 2z" />
                    </svg>
                    默认
                  </div>
                  <div class="hidden group-hover:flex gap-2">
                    <a-button type="link" size="small" @click="handleEdit(item)">编辑</a-button>
                    <a-button type="link" size="small" danger @click="handleDelete(item)">删除</a-button>
                  </div>
                </div>
              </div>
            </div>
          </template>
        </draggable>
      </div>
    </div>

    <!-- Modal -->
    <SeverityLevelModal @register="registerModal" @success="handleSuccess" />

    <!-- Delete Confirm Modal -->
    <a-modal v-model:open="deleteModalVisible" title="删除确认" ok-text="确认删除" ok-type="danger" @ok="handleDeleteOk"
      :confirmLoading="deleteLoading">
      <div class="p-4">
        <div class="flex items-center gap-3 mb-4 text-red-500">
          <InfoCircleFilled class="text-2xl" />
          <span class="font-bold">确认删除</span>
        </div>
        <p>确定要删除此特性等级吗？</p>
        <p class="text-gray-500 mt-2 text-sm">此操作不可恢复。</p>
      </div>
    </a-modal>
  </div>
</template>

<style scoped>
.ghost-card {
  opacity: 0.5;
  background: #f0f0f0;
}

.chosen-card {
  border-color: #ff9500;
  box-shadow: 0 4px 12px rgba(255, 149, 0, 0.2);
}
</style>

<script lang="ts" setup>
import { ref, onMounted } from 'vue';
import { InfoCircleFilled } from '@ant-design/icons-vue';
import { useModal } from '/@/components/Modal';
import { useMessage } from '/@/hooks/web/useMessage';
import draggable from 'vuedraggable';
import SeverityLevelModal from './components/SeverityLevelModal.vue';
import { getSeverityLevelList, delSeverityLevel, updateSeverityLevel, SeverityLevelInfo } from '/@/api/lab/severityLevel';

const [registerModal, { openModal }] = useModal();
const { createMessage } = useMessage();
const list = ref<SeverityLevelInfo[]>([]);
const deleteModalVisible = ref(false);
const deleteLoading = ref(false);
const currentDeleteRecord = ref<SeverityLevelInfo | null>(null);

const loadData = async () => {
  try {
    const res: any = await getSeverityLevelList({});
    let data: SeverityLevelInfo[] = [];
    if (Array.isArray(res)) {
      data = res;
    } else if (res?.data && Array.isArray(res.data)) {
      data = res.data;
    } else if (res?.list && Array.isArray(res.list)) {
      data = res.list;
    }
    
    // 确保所有记录都有排序码，如果没有则按当前索引设置
    data = data.map((item, index) => ({
      ...item,
      sortCode: item.sortCode ?? index + 1,
    }));
    
    list.value = data;
  } catch (error) {
    console.error('加载特性等级列表失败:', error);
    createMessage.error('加载特性等级列表失败');
    list.value = [];
  }
};

const handleCreate = () => {
  openModal(true, { isUpdate: false });
};

const handleEdit = (record: SeverityLevelInfo) => {
  openModal(true, { isUpdate: true, record });
};

const handleDelete = (record: SeverityLevelInfo) => {
  currentDeleteRecord.value = record;
  deleteModalVisible.value = true;
};

const handleDeleteOk = async () => {
  if (!currentDeleteRecord.value) return;
  try {
    deleteLoading.value = true;
    await delSeverityLevel(currentDeleteRecord.value.id);
    createMessage.success('删除成功');
    deleteModalVisible.value = false;
    loadData();
  } catch (error: any) {
    console.error('删除失败:', error);
    const errorMsg = error?.response?.data?.msg || error?.message || '删除失败，请稍后重试';
    createMessage.error(errorMsg);
  } finally {
    deleteLoading.value = false;
  }
};

const handleSuccess = () => {
  loadData();
};

// 拖拽结束处理
const handleDragEnd = async () => {
  try {
    // 批量更新排序码
    const updatePromises = list.value.map((item, index) => {
      return updateSeverityLevel({
        id: item.id,
        name: item.name,
        description: item.description,
        enabled: item.enabled,
        isDefault: item.isDefault,
        sortCode: index + 1, // 从1开始排序
      });
    });
    
    await Promise.all(updatePromises);
    createMessage.success('排序已保存');
    // 更新本地数据的排序码，避免重新加载
    list.value = list.value.map((item, index) => ({
      ...item,
      sortCode: index + 1,
    }));
  } catch (error: any) {
    console.error('保存排序失败:', error);
    const errorMsg = error?.response?.data?.msg || error?.message || '保存排序失败，请稍后重试';
    createMessage.error(errorMsg);
    // 如果保存失败，重新加载数据恢复原顺序
    loadData();
  }
};

onMounted(() => {
  loadData();
});
</script>
