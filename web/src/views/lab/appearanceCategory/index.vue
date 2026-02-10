<template>
  <div class="page-content-wrapper">
    <div class="page-content-wrapper-center">
      <div class="page-content-wrapper-content">
        <!-- 工具栏 -->
        <div class="toolbar">
          <div>
            <h1 class="text-lg font-bold">外观特性大类管理</h1>
            <p class="text-gray-500 text-sm mt-1">管理外观特性的大类分类，如：韧性、脆边、麻点等</p>
          </div>
          <div class="flex gap-2">
            <a-button type="primary" @click="handleCreate">
              <template #icon><PlusOutlined /></template>
              新增大类
            </a-button>
            <a-button @click="handleGoToFeatures">
              <template #icon><AppstoreOutlined /></template>
              管理特性
            </a-button>
          </div>
        </div>

        <!-- 搜索栏 -->
        <div class="search-bar">
          <a-input-search
            v-model:value="searchKeyword"
            placeholder="搜索大类名称"
            style="width: 300px"
            @search="handleSearch"
            @clear="handleSearch"
            allow-clear
          />
        </div>

        <!-- 表格 -->
        <BasicTable @register="registerTable">
          <template #bodyCell="{ column, record }">
            <template v-if="column.key === 'action'">
              <a-button type="link" size="small" @click="handleEdit(record)">编辑</a-button>
              <a-button type="link" size="small" danger @click="handleDelete(record)">删除</a-button>
            </template>
            <template v-else-if="column.key === 'featureCount'">
              <a-tag :color="record.featureCount > 0 ? 'blue' : 'default'">
                {{ record.featureCount || 0 }} 个特性
              </a-tag>
            </template>
          </template>
        </BasicTable>

        <!-- 编辑/新增弹窗 -->
        <CategoryModal @register="registerModal" @success="handleSuccess" />
      </div>
    </div>
  </div>
</template>

<script lang="ts" setup>
  import { ref, computed } from 'vue';
  import { useRouter } from 'vue-router';
  import { PlusOutlined, AppstoreOutlined } from '@ant-design/icons-vue';
  import { BasicTable, useTable } from '/@/components/Table';
  import { useModal } from '/@/components/Modal';
  import { useMessage } from '/@/hooks/web/useMessage';
  import {
    getAppearanceFeatureCategoryList,
    delAppearanceFeatureCategory,
    AppearanceFeatureCategoryInfo,
  } from '/@/api/lab/appearanceCategory';
  import CategoryModal from './components/CategoryModal.vue';

  defineOptions({ name: 'labAppearanceCategory' });

  const router = useRouter();
  const { createMessage, createConfirm } = useMessage();
  const [registerModal, { openModal }] = useModal();
  const searchKeyword = ref('');

  // 将树形数据展平为列表
  const flattenTree = (tree: AppearanceFeatureCategoryInfo[]): AppearanceFeatureCategoryInfo[] => {
    const result: AppearanceFeatureCategoryInfo[] = [];
    const traverse = (nodes: AppearanceFeatureCategoryInfo[]) => {
      nodes.forEach(node => {
        // 确保 id 字段存在（处理可能的字段名差异）
        const nodeId = node.id || (node as any).Id || (node as any).ID || (node as any).F_Id || (node as any).f_Id;
        if (!nodeId) {
          console.warn('[展平树] 节点缺少ID字段:', node);
          console.warn('[展平树] 节点的所有键:', Object.keys(node));
        }
        
        // 创建一个新对象，保留需要的字段
        const flatNode: AppearanceFeatureCategoryInfo = {
          id: nodeId || node.id || '', // 确保使用正确的 ID，如果都没有则使用空字符串
          name: node.name || '',
          sortCode: node.sortCode,
          featureCount: node.featureCount || 0,
          parentId: node.parentId,
          description: node.description,
          rootId: node.rootId,
          path: node.path,
          // 不包含 children，因为这是展平后的列表
        };
        
        // 如果 ID 为空，记录警告但继续处理
        if (!flatNode.id) {
          console.error('[展平树] 警告：展平后的节点ID仍为空:', flatNode);
        }
        
        result.push(flatNode);
        if (node.children && node.children.length > 0) {
          traverse(node.children);
        }
      });
    };
    traverse(tree);
    return result;
  };

  // 创建父级名称映射（通过ID查找父级名称）
  const parentNameMap = ref<Record<string, string>>({});
  
  const buildParentMap = (tree: AppearanceFeatureCategoryInfo[]) => {
    const map: Record<string, string> = {};
    // 先构建所有节点的映射（id -> name）
    const idToNameMap: Record<string, string> = {};
    const traverse = (nodes: AppearanceFeatureCategoryInfo[]) => {
      nodes.forEach(node => {
        idToNameMap[node.id] = node.name;
        if (node.children && node.children.length > 0) {
          traverse(node.children);
        }
      });
    };
    traverse(tree);
    
    // 然后为每个有父级的节点设置父级名称
    const setParentNames = (nodes: AppearanceFeatureCategoryInfo[]) => {
      nodes.forEach(node => {
        if (node.parentId && idToNameMap[node.parentId]) {
          map[node.id] = idToNameMap[node.parentId];
        }
        if (node.children && node.children.length > 0) {
          setParentNames(node.children);
        }
      });
    };
    setParentNames(tree);
    parentNameMap.value = map;
  };

  const [registerTable, { reload }] = useTable({
    api: getAppearanceFeatureCategoryList,
    columns: [
      { title: '大类名称', dataIndex: 'name', key: 'name', width: 150 },
      { 
        title: '父级分类', 
        key: 'parentName', 
        width: 150,
        customRender: ({ record }: { record: AppearanceFeatureCategoryInfo }) => {
          return record.parentId ? (parentNameMap.value[record.id] || '-') : '顶级分类';
        }
      },
      { title: '特性数量', key: 'featureCount', width: 120, align: 'center' },
      { title: '排序码', dataIndex: 'sortCode', key: 'sortCode', width: 100, align: 'right' },
    ],
    useSearchForm: false,
    immediate: true,
    actionColumn: {
      width: 150,
      title: '操作',
      dataIndex: 'action',
      fixed: 'right',
    },
    beforeFetch: (params) => {
      if (searchKeyword.value) {
        params.keyword = searchKeyword.value;
      }
      return params;
    },
    afterFetch: (data) => {
      // 处理不同的响应格式
      let treeData: AppearanceFeatureCategoryInfo[] = [];
      if (Array.isArray(data)) {
        treeData = data;
      } else if (data?.data && Array.isArray(data.data)) {
        treeData = data.data;
      } else if (data?.list && Array.isArray(data.list)) {
        treeData = data.list;
      } else {
        console.warn('未知的响应格式:', data);
        treeData = [];
      }
      
      // 后端返回的是树形结构，需要展平
      const flatData = flattenTree(treeData);
      // 构建父级名称映射
      buildParentMap(treeData);
      
      return flatData;
    },
  });

  function handleCreate() {
    openModal(true, { isUpdate: false });
  }

  function handleEdit(record: AppearanceFeatureCategoryInfo) {
    openModal(true, { isUpdate: true, record });
  }

  async function handleDelete(record: AppearanceFeatureCategoryInfo) {
    if (!record || !record.id) {
      createMessage.error('删除失败：记录ID不存在');
      console.error('删除失败：ID为空', { record });
      return;
    }

    const recordName = record.name || '该特性大类';

    createConfirm({
      iconType: 'warning',
      title: '确认删除',
      content: `确定要删除特性大类"${recordName}"吗？此操作不可恢复。`,
      onOk: async () => {
        try {
          await delAppearanceFeatureCategory(record.id);
          createMessage.success('删除成功');
          reload();
        } catch (error: any) {
          const errorMsg = error?.response?.data?.msg || error?.message || '删除失败';
          createMessage.error(errorMsg);
          console.error('删除失败:', error);
        }
      },
      onCancel: () => {
      }
    });
  }

  function handleSearch() {
    reload();
  }

  function handleSuccess() {
    reload();
  }

  function handleGoToFeatures() {
    router.push('/lab/appearance');
  }
</script>

<style scoped>
  .toolbar {
    display: flex;
    justify-content: space-between;
    align-items: center;
    margin-bottom: 16px;
    padding: 16px;
    background: white;
    border-radius: 4px;
  }

  .search-bar {
    margin-bottom: 16px;
    padding: 12px;
    background: white;
    border-radius: 4px;
  }
</style>
