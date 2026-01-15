<template>
  <BasicDrawer v-bind="$attrs" @register="registerDrawer" title="特性大类管理" width="800px" class="full-drawer" destroy-on-close>
    <BasicTable @register="registerTable">
      <template #tableTitle>
        <a-button type="primary" preIcon="icon-ym icon-ym-btn-add" @click="addOrUpdateHandle()">新增大类</a-button>
      </template>
      <template #bodyCell="{ column, record }">
        <template v-if="column.key === 'action'">
          <TableAction
            :actions="[
              {
                label: '编辑',
                onClick: addOrUpdateHandle.bind(null, record.id),
              },
              {
                label: '删除',
                color: 'error',
                modelConfirm: {
                  onOk: async () => {
                    await handleDelete(record.id);
                  },
                },
              },
            ]"
          />
        </template>
        <template v-else-if="column.key === 'featureCount'">
          <a-tag :color="record.featureCount > 0 ? 'blue' : 'default'">
            {{ record.featureCount || 0 }} 个特性
          </a-tag>
        </template>
      </template>
    </BasicTable>
  </BasicDrawer>
  <CategoryModal @register="registerModal" @success="handleSuccess" />
</template>

<script lang="ts" setup>
  import { ref } from 'vue';
  import { BasicDrawer, useDrawerInner } from '/@/components/Drawer';
  import { BasicTable, useTable, TableAction, BasicColumn } from '/@/components/Table';
  import { useModal } from '/@/components/Modal';
  import { useMessage } from '/@/hooks/web/useMessage';
  import {
    getAppearanceFeatureCategoryList,
    getAppearanceFeatureCategoryInfo,
    delAppearanceFeatureCategory,
    AppearanceFeatureCategoryInfo,
  } from '/@/api/lab/appearanceCategory';
  import CategoryModal from '../../appearanceCategory/components/CategoryModal.vue';

  const { createMessage } = useMessage();
  const [registerDrawer] = useDrawerInner();
  const [registerModal, { openModal: openFormModal }] = useModal();

  // 创建父级名称映射
  const parentNameMap = ref<Record<string, string>>({});

  const buildParentMap = (tree: AppearanceFeatureCategoryInfo[]) => {
    const map: Record<string, string> = {};
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

  const columns: BasicColumn[] = [
    { title: '大类名称', dataIndex: 'name', key: 'name', width: 200 },
    {
      title: '父级分类',
      key: 'parentName',
      width: 150,
      customRender: ({ record }: { record: AppearanceFeatureCategoryInfo }) => {
        if (!record.parentId) {
          return '顶级分类';
        }
        // 从树形数据中查找父级名称
        const findParentName = (nodes: AppearanceFeatureCategoryInfo[], targetId: string): string | null => {
          for (const node of nodes) {
            if (node.id === targetId) {
              return node.name;
            }
            if (node.children && node.children.length > 0) {
              const found = findParentName(node.children, targetId);
              if (found) return found;
            }
          }
          return null;
        };
        // 先从映射中查找，如果找不到则从当前表格数据中查找
        return parentNameMap.value[record.id] || '-';
      },
    },
    { title: '特性数量', key: 'featureCount', width: 120, align: 'center' },
    { title: '排序码', dataIndex: 'sortCode', key: 'sortCode', width: 100, align: 'right' },
  ];

  const [registerTable, { reload }] = useTable({
    api: getAppearanceFeatureCategoryList,
    columns,
    pagination: false,
    isTreeTable: true,
    resizeHeightOffset: -10,
    actionColumn: {
      width: 150,
      title: '操作',
      dataIndex: 'action',
    },
    afterFetch: data => {
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

      // 构建父级名称映射（用于显示父级名称）
      buildParentMap(treeData);

      // isTreeTable 可以直接使用树形数据，不需要展平
      return treeData;
    },
  });

  async function addOrUpdateHandle(id = '') {
    if (id) {
      // 编辑：调用API获取详情
      try {
        const record = await getAppearanceFeatureCategoryInfo(id);
        openFormModal(true, { isUpdate: true, record });
      } catch (error: any) {
        const errorMsg = error?.response?.data?.msg || error?.message || '获取详情失败';
        createMessage.error(errorMsg);
      }
    } else {
      // 新增
      openFormModal(true, { isUpdate: false });
    }
  }

  async function handleDelete(id: string) {
    try {
      await delAppearanceFeatureCategory(id);
      createMessage.success('删除成功');
      reload();
    } catch (error: any) {
      const errorMsg = error?.response?.data?.msg || error?.message || '删除失败';
      createMessage.error(errorMsg);
    }
  }

  function handleSuccess() {
    reload();
  }
</script>
