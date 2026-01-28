<template>
  <div class="p-4 h-full flex flex-col gap-4">
    <!-- 头部 -->
    <div class="bg-white p-4 rounded-lg shadow-sm flex justify-between items-center">
      <div>
        <h1 class="text-lg font-bold flex items-center gap-2">
          <span class="text-blue-600">
            <svg xmlns="http://www.w3.org/2000/svg" width="24" height="24" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">
              <rect x="3" y="3" width="18" height="18" rx="2" ry="2"></rect>
              <line x1="9" y1="3" x2="9" y2="21"></line>
              <line x1="3" y1="9" x2="21" y2="9"></line>
            </svg>
          </span>
          产品规格定义
        </h1>
        <p class="text-gray-500 text-sm mt-1">管理产品规格信息，包括扩展属性配置。</p>
      </div>
      <div class="flex gap-2">
        <!-- 搜索栏 -->
        <BasicForm @register="registerForm" class="search-form" />
        <a-button type="primary" :icon="h(PlusOutlined)" @click="addOrUpdateHandle()" class="bg-blue-600 border-blue-600 hover:bg-blue-500">
          {{ t('common.addText') }}
        </a-button>
      </div>
    </div>

    <!-- 卡片列表 -->
    <div class="flex-1 overflow-auto bg-gray-50 p-4 rounded-lg">
      <a-spin :spinning="loading">
        <div v-if="dataList.length === 0" class="flex flex-col items-center justify-center h-full text-gray-400">
          <svg xmlns="http://www.w3.org/2000/svg" width="64" height="64" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1" stroke-linecap="round" stroke-linejoin="round" class="mb-4">
            <rect x="3" y="3" width="18" height="18" rx="2" ry="2"></rect>
            <line x1="9" y1="3" x2="9" y2="21"></line>
            <line x1="3" y1="9" x2="21" y2="9"></line>
          </svg>
          <p class="text-lg mb-2">暂无产品规格数据</p>
          <p class="text-sm">点击上方"{{ t('common.addText') }}"按钮开始创建</p>
        </div>
        <draggable
          v-else
          v-model="dataList"
          item-key="id"
          handle=".drag-handle"
          :animation="200"
          ghost-class="ghost-card"
          chosen-class="chosen-card"
          @end="handleDragEnd"
          class="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-4">
          <template #item="{ element: record }">
            <div
              class="bg-white rounded-lg p-4 shadow-sm border border-gray-200 hover:border-blue-300 hover:shadow-sm transition-colors relative group cursor-move">
              <div class="flex items-start justify-between mb-3">
                <div class="flex items-center gap-3 flex-1">
                  <div class="drag-handle cursor-grab active:cursor-grabbing flex-shrink-0">
                    <svg xmlns="http://www.w3.org/2000/svg" width="20" height="20" viewBox="0 0 24 24" fill="none"
                      stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"
                      class="text-gray-400 hover:text-blue-600">
                      <circle cx="9" cy="12" r="1"></circle>
                      <circle cx="9" cy="5" r="1"></circle>
                      <circle cx="9" cy="19" r="1"></circle>
                      <circle cx="15" cy="12" r="1"></circle>
                      <circle cx="15" cy="5" r="1"></circle>
                      <circle cx="15" cy="19" r="1"></circle>
                    </svg>
                  </div>
                  <div class="w-12 h-12 rounded-lg bg-gradient-to-br from-blue-100 to-blue-200 flex items-center justify-center text-xl font-bold text-blue-700 border border-blue-200 shadow-sm">
                    {{ (record.code || '').charAt(0) || '?' }}
                  </div>
                  <div class="flex-1">
                    <div class="flex items-center gap-2">
                      <h3 class="font-bold text-lg text-gray-800">{{ record.code }}</h3>
                    </div>
                    <p class="text-sm text-gray-600 mt-1">{{ record.name || '未命名' }}</p>
                  </div>
                </div>
                <!-- 右上角：版本信息和操作按钮 -->
                <div class="flex flex-col items-end gap-1 relative" style="min-width: 60px;">
                  <!-- 版本信息 - 固定在右上角 -->
                  <div v-if="record._currentVersion" class="z-10">
                    <a-tag color="blue" size="small" style="margin: 0;">
                      v{{ record._currentVersion.version }}
                    </a-tag>
                  </div>
                  <!-- 操作按钮 - 在版本标签下方 -->
                  <div class="hidden group-hover:flex gap-1 flex-col" @mousedown.stop @click.stop>
                    <a-button type="link" size="small" @click.stop="addOrUpdateHandle(record.id)" @mousedown.stop style="padding: 0 4px;">编辑</a-button>
                    <a-button 
                      type="link" 
                      size="small" 
                      danger 
                      @click.stop="handleDeleteClick(record)" 
                      @mousedown.stop
                      style="padding: 0 4px;"
                    >删除</a-button>
                  </div>
                </div>
              </div>
              
              <!-- 详细信息 -->
              <div class="space-y-2 mt-3 pt-3 border-t border-gray-100">
                <div class="flex items-center justify-between text-sm">
                  <span class="text-gray-500">检测列：</span>
                  <span class="text-gray-800 font-medium">{{ record.detectionColumns || '-' }}</span>
                </div>
                <!-- 动态显示扩展属性 -->
                <template v-if="record.attributes && record.attributes.length > 0">
                  <div
                    v-for="attr in record.attributes.slice(0, 3)"
                    :key="attr.attributeKey"
                    class="flex items-center justify-between text-sm">
                    <span class="text-gray-500">{{ attr.attributeName || attr.attributeKey }}：</span>
                    <span class="text-gray-800 font-medium">
                      {{ attr.attributeValue || '-' }}
                      <span v-if="attr.unit" class="text-gray-500 ml-1">({{ attr.unit }})</span>
                    </span>
                  </div>
                  <div v-if="record.attributes.length > 3" class="text-xs text-gray-400">
                    还有 {{ record.attributes.length - 3 }} 个属性...
                  </div>
                </template>
                <div v-if="record.description" class="mt-2 pt-2 border-t border-gray-100">
                  <p class="text-xs text-gray-600 line-clamp-2">{{ record.description }}</p>
                </div>
              </div>
            </div>
          </template>
        </draggable>
      </a-spin>
      
      <!-- 分页 -->
      <div class="pagination-wrapper mt-4" v-if="dataList.length > 0">
        <a-pagination
          v-model:current="pagination.current"
          v-model:page-size="pagination.pageSize"
          :total="pagination.total"
          :show-size-changer="true"
          :show-total="(total) => `共 ${total} 条`"
          @change="handlePageChange"
          @showSizeChange="handlePageSizeChange" />
      </div>
    </div>
    <Form @register="registerFormModal" @reload="reload" />
  </div>
</template>
<script lang="ts" setup>
  import { ref, reactive, h, onMounted, onUnmounted, nextTick } from 'vue';
  import { getProductSpecList, delProductSpec, updateProductSpec, getProductSpecVersionList } from '/@/api/lab/product';
  import { BasicForm, useForm } from '/@/components/Form';
  import { useMessage } from '/@/hooks/web/useMessage';
  import { useI18n } from '/@/hooks/web/useI18n';
  import { useModal } from '/@/components/Modal';
  import { PlusOutlined } from '@ant-design/icons-vue';
  import draggable from 'vuedraggable';
  import Form from './Form.vue';

  defineOptions({ name: 'ProductSpec' });

  const { createMessage, createConfirm } = useMessage();
  const { t } = useI18n();
  const [registerFormModal, { openModal: openFormModal }] = useModal();

  // 组件挂载状态
  const isMounted = ref(true);

  // 数据列表
  const dataList = ref<any[]>([]);
  const loading = ref(false);
  
  // 分页
  const pagination = reactive({
    current: 1,
    pageSize: 12,
    total: 0,
  });

  // 搜索表单
  const [registerForm, { validate, resetFields }] = useForm({
    schemas: [
      {
        field: 'keyword',
        label: '',
        component: 'Input',
        componentProps: {
          placeholder: t('common.enterKeyword'),
          allowClear: true,
          style: 'width: 200px',
        },
      },
    ],
    compact: true,
    showActionButtonGroup: false,
    autoSubmitOnEnter: true,
    submitFunc: handleSearch,
    resetFunc: handleReset,
  });

  // 全部数据
  const allDataList = ref<any[]>([]);

  // 版本信息缓存
  const versionInfoCache = ref<Map<string, any>>(new Map());

  // 加载数据
  async function loadData() {
    loading.value = true;
    try {
      const searchValues = await validate();
      const params = {
        ...searchValues,
      };
      
      const res = await getProductSpecList(params);
      if (res.data) {
        // 后端返回的是完整列表，不是分页结果
        let list = Array.isArray(res.data) ? res.data : (res.data.list || res.data.records || []);
        // 处理列表数据（不再硬编码解析特定属性）
        list = list.map((item, index) => {
          // 保留已有的版本信息
          const existingItem = allDataList.value.find(existing => existing.id === item.id);
          return {
            ...item,
            sortCode: item.sortCode ?? index + 1,
            // 保留版本信息，避免被清空
            _currentVersion: existingItem?._currentVersion || item._currentVersion,
          };
        });
        // 按排序码排序
        list.sort((a, b) => (a.sortCode || 0) - (b.sortCode || 0));
        allDataList.value = list;

        // 获取版本信息（异步加载，不会阻塞数据展示）
        loadVersionInfoForList(list);

        updatePagedData();
      }
    } catch (error) {
      console.error('加载数据失败', error);
    } finally {
      loading.value = false;
    }
  }

  // 更新分页数据（前端分页）
  function updatePagedData() {
    if (!isMounted.value) return;
    const start = (pagination.current - 1) * pagination.pageSize;
    const end = start + pagination.pageSize;
    dataList.value = allDataList.value.slice(start, end);
    pagination.total = allDataList.value.length;
  }

  // 搜索
  function handleSearch() {
    pagination.current = 1;
    loadData();
  }

  // 重置
  function handleReset() {
    pagination.current = 1;
    loadData();
  }

  // 分页变化
  function handlePageChange(page: number, pageSize: number) {
    pagination.current = page;
    pagination.pageSize = pageSize;
    updatePagedData();
  }

  // 每页条数变化
  function handlePageSizeChange(current: number, size: number) {
    pagination.current = 1;
    pagination.pageSize = size;
    updatePagedData();
  }

  // 刷新
  function reload() {
    // 清空版本缓存，强制重新加载
    versionInfoCache.value.clear();
    loadData();
  }

  // 加载版本信息
  async function loadVersionInfoForList(productSpecs: any[]) {
    // 只获取当前版本的版本信息
    const promises = productSpecs.map(async (spec) => {
      // 如果组件已卸载，停止执行
      if (!isMounted.value) return;
      
      // 如果缓存中有且数据中也有，直接使用缓存
      if (versionInfoCache.value.has(spec.id) && spec._currentVersion) {
        return;
      }
      
      try {
        const response = await getProductSpecVersionList(spec.id);
        
        // 再次检查组件是否仍然挂载
        if (!isMounted.value) return;
        
        // 确保返回的是数组格式（可能被包装在 data 字段中）
        const versions = Array.isArray(response) ? response : (response?.data || []);
        
        if (!Array.isArray(versions)) {
          console.warn(`产品规格 ${spec.id} 的版本信息格式不正确:`, response);
          return;
        }
        
        // 如果没有版本信息，跳过
        if (versions.length === 0) {
          return;
        }
        
        const currentVersion = versions.find(v => v.isCurrent === 1 || v.isCurrent === true) || versions[0];
        versionInfoCache.value.set(spec.id, currentVersion);
        
        // 更新全部数据列表
        const index = allDataList.value.findIndex(item => item.id === spec.id);
        if (index !== -1) {
          allDataList.value[index] = {
            ...allDataList.value[index],
            _currentVersion: currentVersion
          };
        }
        
        // 使用 nextTick 确保 DOM 更新在正确的时机进行
        await nextTick();
        if (isMounted.value) {
          updatePagedData();
        }
      } catch (error) {
        // 如果组件已卸载，不处理错误
        if (!isMounted.value) return;
        console.error(`获取产品规格 ${spec.id} 的版本信息失败`, error);
      }
    });

    // 并行执行，但不要等待全部完成
    await Promise.allSettled(promises);
  }

  // 编辑/新增
  function addOrUpdateHandle(id = '') {
    openFormModal(true, { id });
  }

  // 删除按钮点击处理（中间函数，用于调试和验证）
  function handleDeleteClick(record: any) {
    
    if (!record) {
      createMessage.error('删除失败：记录对象不存在');
      console.error('[删除] 记录对象为空');
      return;
    }

    const id = record.id || record.Id || record.ID || '';
    
    if (!id) {
      createMessage.error('删除失败：记录ID不存在');
      console.error('[删除] ID为空', { 
        record, 
        recordKeys: Object.keys(record),
        allDataList: allDataList.value 
      });
      return;
    }

    handleDelete(id);
  }

  // 删除
  function handleDelete(id: string) {
    if (!id) {
      createMessage.error('删除失败：记录ID不存在');
      console.error('删除失败：ID为空', { id, allDataList: allDataList.value });
      return;
    }

    // 先获取要删除的记录信息
    const record = allDataList.value.find(item => item.id === id);
    if (!record) {
      createMessage.error('删除失败：未找到要删除的记录');
      console.error('删除失败：未找到记录', { id, allDataList: allDataList.value });
      return;
    }

    const recordName = record?.name || record?.code || '该产品规格';

    createConfirm({
      iconType: 'warning',
      title: '确认删除',
      content: `确定要删除产品规格"${recordName}"吗？此操作不可恢复。`,
      onOk: async () => {
        try {
          const res = await delProductSpec(id);
          createMessage.success(res.msg || '删除成功');
          loadData();
        } catch (error: any) {
          const errorMsg = error?.response?.data?.msg || error?.message || '删除失败';
          createMessage.error(errorMsg);
          console.error('删除失败:', error);
        }
      },
      onCancel: () => {
        // 用户取消删除，确保没有残留的遮罩层
      }
    });
  }

  // 拖拽结束处理
  async function handleDragEnd() {
    try {
      const start = (pagination.current - 1) * pagination.pageSize;
      
      // 更新全部列表中的数据顺序
      dataList.value.forEach((item, index) => {
        const globalIndex = start + index;
        allDataList.value[globalIndex] = {
          ...item,
          sortCode: globalIndex + 1,
        };
      });
      
      // 批量更新当前页数据的排序码
      const updatePromises = dataList.value.map((item, index) => {
        const globalIndex = start + index;
        return updateProductSpec({
          id: item.id,
          code: item.code,
          name: item.name,
          detectionColumns: item.detectionColumns,
          description: item.description,
          sortCode: globalIndex + 1,
        });
      });
      
      await Promise.all(updatePromises);
      createMessage.success('排序已保存');
    } catch (error: any) {
      console.error('保存排序失败:', error);
      const errorMsg = error?.response?.data?.msg || error?.message || '保存排序失败，请稍后重试';
      createMessage.error(errorMsg);
      // 如果保存失败，重新加载数据恢复原顺序
      loadData();
    }
  }

  // 初始化
  onMounted(() => {
    isMounted.value = true;
    loadData();
  });

  // 组件卸载时清理
  onUnmounted(() => {
    isMounted.value = false;
  });
</script>

<style lang="less" scoped>
.search-form {
  :deep(.ant-form-item) {
    margin-bottom: 0;
  }
}

.ghost-card {
  opacity: 0.5;
  background: #f0f0f0;
}

.chosen-card {
  border-color: #1890ff;
  box-shadow: 0 4px 12px rgba(24, 144, 255, 0.2);
}

.pagination-wrapper {
  display: flex;
  justify-content: flex-end;
}
</style>
