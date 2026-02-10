<template>
  <div class="p-4 h-full flex flex-col gap-4">
    <div class="bg-white p-4 rounded-lg shadow-sm flex justify-between items-center">
      <div>
        <h1 class="text-lg font-bold flex items-center gap-2">
          <span class="text-green-600">
            <svg xmlns="http://www.w3.org/2000/svg" width="24" height="24" viewBox="0 0 24 24" fill="none"
              stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">
              <circle cx="12" cy="12" r="10"></circle>
              <path d="M12 6v6l4 2"></path>
            </svg>
          </span>
          单位管理
        </h1>
        <p class="text-gray-500 text-sm mt-1">管理物理单位维度及单位定义，支持单位换算和标准化。</p>
      </div>
      <a-space>
        <a-button type="primary" @click="handleCreateCategory" class="bg-green-600 border-green-600 hover:bg-green-500">
          + 新增单位维度
        </a-button>
        <a-button type="default" @click="handleCreateUnit">
          + 新增单位
        </a-button>
      </a-space>
    </div>

    <div class="flex-1 overflow-auto bg-gray-50 p-4 rounded-lg">
      <a-tabs v-model:activeKey="activeTab" type="card">
        <!-- 单位维度标签页 -->
        <a-tab-pane key="category" tab="单位维度">
          <div v-if="categoryList.length === 0" class="flex flex-col items-center justify-center h-64 text-gray-400">
            <svg xmlns="http://www.w3.org/2000/svg" width="64" height="64" viewBox="0 0 24 24" fill="none"
              stroke="currentColor" stroke-width="1" stroke-linecap="round" stroke-linejoin="round" class="mb-4">
              <circle cx="12" cy="12" r="10"></circle>
            </svg>
            <p class="text-lg mb-2">暂无单位维度数据</p>
            <a-button type="primary" @click="handleCreateCategory">创建第一个单位维度</a-button>
          </div>
          <div v-else class="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-4">
            <div v-for="item in categoryList" :key="item.id"
              class="bg-white rounded-lg p-4 shadow-sm border border-gray-200 hover:border-green-400 hover:shadow-md transition-all relative group">
              <div class="flex items-start justify-between mb-3">
                <div class="flex items-center gap-3 flex-1">
                  <div
                    class="w-12 h-12 rounded-lg bg-gradient-to-br from-green-100 to-green-200 flex items-center justify-center text-xl font-bold text-green-700 border border-green-200 shadow-sm">
                    {{ item.name.charAt(0) }}
                  </div>
                  <div class="flex-1">
                    <div class="flex items-center gap-2">
                      <h3 class="font-bold text-lg text-gray-800">{{ item.name }}</h3>
                    </div>
                    <p class="text-sm text-gray-500 mt-1">编码: {{ item.code }}</p>
                    <p v-if="item.description" class="text-sm text-gray-600 mt-2">{{ item.description }}</p>
                  </div>
                </div>
                <div class="hidden group-hover:flex gap-1 flex-col" @mousedown.stop @click.stop>
                  <a-button type="link" size="small" @click.stop="handleEditCategory(item)">编辑</a-button>
                  <a-button type="link" size="small" danger @click.stop="handleDeleteCategory(item)">删除</a-button>
                </div>
              </div>
            </div>
          </div>
        </a-tab-pane>

        <!-- 单位定义标签页 -->
        <a-tab-pane key="unit" tab="单位定义">
          <div class="mb-4">
            <a-select v-model:value="selectedCategoryId" placeholder="筛选维度" allowClear style="width: 200px"
              @change="loadUnitList">
              <a-select-option v-for="cat in categoryList" :key="cat.id" :value="cat.id">
                {{ cat.name }}
              </a-select-option>
            </a-select>
          </div>

          <div v-if="unitList.length === 0" class="flex flex-col items-center justify-center h-64 text-gray-400">
            <svg xmlns="http://www.w3.org/2000/svg" width="64" height="64" viewBox="0 0 24 24" fill="none"
              stroke="currentColor" stroke-width="1" stroke-linecap="round" stroke-linejoin="round" class="mb-4">
              <path d="M12 2v20M17 5H9.5a3.5 3.5 0 0 0 0 7h5a3.5 3.5 0 0 1 0 7H6" />
            </svg>
            <p class="text-lg mb-2">暂无单位定义数据</p>
            <a-button type="primary" @click="handleCreateUnit">创建第一个单位</a-button>
          </div>
          <div v-else class="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-4">
            <div v-for="item in unitList" :key="item.id" :class="[
              'bg-white rounded-lg p-4 shadow-sm border hover:shadow-md transition-all relative group',
              item.isBase ? 'border-blue-300 border-2' : 'border-gray-200'
            ]">
              <div class="flex items-start justify-between mb-3">
                <div class="flex items-center gap-3 flex-1">
                  <div :class="[
                    'w-12 h-12 rounded-lg flex items-center justify-center text-xl font-bold border shadow-sm',
                    item.isBase
                      ? 'bg-blue-50 text-blue-600 border-blue-200'
                      : 'bg-gray-50 text-gray-600 border-gray-200'
                  ]">
                    {{ item.symbol }}
                  </div>
                  <div class="flex-1">
                    <div class="flex items-center gap-2">
                      <h3 class="font-bold text-lg text-gray-800">{{ item.name }}</h3>
                      <a-tag v-if="item.isBase" color="blue" size="small">基准单位</a-tag>
                    </div>
                    <p class="text-sm text-gray-500 mt-1">符号: {{ item.symbol }}</p>
                    <div class="flex items-center gap-4 mt-2 text-xs text-gray-600">
                      <span>精度: {{ item.precision }}位</span>
                      <span>比例: {{ item.scaleToBase }}</span>
                      <span v-if="item.offset !== 0">偏移: {{ item.offset }}</span>
                    </div>
                  </div>
                </div>
                <div class="hidden group-hover:flex gap-1 flex-col" @mousedown.stop @click.stop>
                  <a-button type="link" size="small" @click.stop="handleEditUnit(item)">编辑</a-button>
                  <a-button type="link" size="small" danger @click.stop="handleDeleteUnit(item)"
                    :disabled="item.isBase === 1 || item.isBase === true">
                    删除
                  </a-button>
                </div>
              </div>
            </div>
          </div>
        </a-tab-pane>
      </a-tabs>
    </div>

    <!-- 单位维度Modal -->
    <UnitCategoryModal @register="registerCategoryModal" @success="handleCategorySuccess" />

    <!-- 单位定义Modal -->
    <UnitDefinitionModal @register="registerUnitModal" @success="handleUnitSuccess" />

  </div>
</template>

<script lang="ts" setup>
defineOptions({ name: 'labUnit' });
import { ref, onMounted } from 'vue';
import { useModal } from '/@/components/Modal';
import { useMessage } from '/@/hooks/web/useMessage';
import UnitCategoryModal from './components/UnitCategoryModal.vue';
import UnitDefinitionModal from './components/UnitDefinitionModal.vue';
import {
  getUnitCategoryList,
  deleteUnitCategory,
  getUnitDefinitionList,
  deleteUnitDefinition,
} from '/@/api/lab/unit';

const [registerCategoryModal, { openModal: openCategoryModal }] = useModal();
const [registerUnitModal, { openModal: openUnitModal }] = useModal();
const { createMessage, createConfirm } = useMessage();

const activeTab = ref('category');
const categoryList = ref<any[]>([]);
const unitList = ref<any[]>([]);
const selectedCategoryId = ref<string | undefined>(undefined);

const loadCategoryList = async () => {
  try {
    const res: any = await getUnitCategoryList();
    categoryList.value = res.data || res || [];
  } catch (error) {
    console.error('加载单位维度列表失败:', error);
    createMessage.error('加载单位维度列表失败');
    categoryList.value = [];
  }
};

const loadUnitList = async () => {
  try {
    const res: any = await getUnitDefinitionList(selectedCategoryId.value);
    unitList.value = res.data || res || [];
  } catch (error) {
    console.error('加载单位定义列表失败:', error);
    createMessage.error('加载单位定义列表失败');
    unitList.value = [];
  }
};

const handleCreateCategory = () => {
  openCategoryModal(true, { isUpdate: false });
};

const handleEditCategory = (record: any) => {
  openCategoryModal(true, { isUpdate: true, record });
};


const handleDeleteCategory = async (record: any) => {
  if (!record || !record.id) {
    createMessage.error('删除失败：记录ID不存在');
    return;
  }

  const recordName = record?.name || record?.code || '该单位维度';

  createConfirm({
    iconType: 'warning',
    title: '确认删除',
    content: `确定要删除单位维度"${recordName}"吗？此操作不可恢复。`,
    okText: '确认删除',
    cancelText: '取消',
    onOk: async () => {
      try {
        await deleteUnitCategory(record.id);
        createMessage.success('删除成功');
        await loadCategoryList();
      } catch (error: any) {
        const errorMsg = error?.response?.data?.msg || error?.message || '删除失败，请稍后重试';
        createMessage.error(errorMsg);
        throw error; // 重新抛出错误，阻止 Modal 自动关闭
      }
    },
  });
};

const handleCreateUnit = async () => {
  // 确保单位维度列表已加载
  if (categoryList.value.length === 0) {
    await loadCategoryList();
  }
  openUnitModal(true, { isUpdate: false, categoryList: categoryList.value });
};

const handleEditUnit = async (record: any) => {
  // 确保单位维度列表已加载
  if (categoryList.value.length === 0) {
    await loadCategoryList();
  }
  openUnitModal(true, { isUpdate: true, record, categoryList: categoryList.value });
};

const handleDeleteUnit = (record: any) => {
  if (!record || !record.id) {
    createMessage.error('删除失败：记录ID不存在');
    console.error('[删除] 记录或ID为空', { record });
    return;
  }

  if (record.isBase === 1 || record.isBase === true) {
    createMessage.warning('基准单位不能删除');
    return;
  }

  const recordName = record?.name || record?.symbol || '该单位定义';
  
  // 使用 createConfirm，确保弹窗能正确关闭
  createConfirm({
    iconType: 'warning',
    title: '确认删除',
    content: `确定要删除单位定义"${recordName}"吗？此操作不可恢复。`,
    okText: '确认删除',
    cancelText: '取消',
    onOk: async () => {
      try {
        await deleteUnitDefinition(record.id);
        createMessage.success('删除成功');
        await loadUnitList();
      } catch (error: any) {
        console.error('删除失败:', error);
        const errorMsg = error?.response?.data?.msg || error?.message || '删除失败，请稍后重试';
        createMessage.error(errorMsg);
        throw error; // 重新抛出错误，阻止 Modal 自动关闭
      }
    },
  });
};


const handleCategorySuccess = () => {
  loadCategoryList();
};

const handleUnitSuccess = () => {
  loadUnitList();
};

onMounted(() => {
  loadCategoryList();
  loadUnitList();
});
</script>

<style scoped>
.ghost-card {
  opacity: 0.5;
  background: #f0f0f0;
}

.chosen-card {
  border-color: #52c41a;
  box-shadow: 0 4px 12px rgba(82, 196, 26, 0.2);
}
</style>
