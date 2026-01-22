<template>
  <a-modal v-model:visible="visible" title="选择特性" :width="1000" :confirm-loading="confirmLoading" @ok="handleConfirm"
    @cancel="handleCancel" :body-style="{ padding: '0' }" class="common-container-modal" destroy-on-close>
    <div class="page-content-wrapper" style="height: 600px; padding: 10px">
      <!-- 左侧：特性大类树 -->
      <div class="page-content-wrapper-left">
        <BasicLeftTree title="特性大类" ref="leftTreeRef" :treeData="treeData" :loading="treeLoading" @reload="reloadTree"
          @select="handleTreeSelect" :fieldNames="{ key: 'id', title: 'title', children: 'children' }" />
      </div>

      <!-- 右侧：特征名称表格 -->
      <div class="page-content-wrapper-center">
        <div class="page-content-wrapper-content">
          <BasicTable @register="registerTable" :searchInfo="searchInfo" @row-click="handleRowClick"
            class="jnpf-sub-table">
            <template #bodyCell="{ column, record }">
              <template v-if="column.key === 'category'">
                <a-tag color="purple">{{ record.category || '-' }}</a-tag>
              </template>
              <template v-else-if="column.key === 'severityLevel'">
                <a-tag color="orange">{{ record.severityLevel || '默认' }}</a-tag>
              </template>
            </template>
          </BasicTable>
        </div>
      </div>
    </div>
  </a-modal>
</template>

<script lang="ts" setup>
import { ref, reactive, unref, nextTick, watch } from 'vue';
import { BasicLeftTree, TreeItem, TreeActionType } from '/@/components/Tree';
import { BasicTable, useTable, BasicColumn } from '/@/components/Table';
import { useMessage } from '/@/hooks/web/useMessage';
import { Nullable } from '/@/utils/types';
import {
  getAppearanceFeatureList,
  getAppearanceFeatureInfo,
  AppearanceFeatureInfo,
} from '/@/api/lab/appearance';
import { getAllAppearanceFeatureCategories, AppearanceFeatureCategoryInfo } from '/@/api/lab/appearanceCategory';

const emit = defineEmits(['confirm', 'cancel']);

const { createMessage } = useMessage();

// 对话框状态
const visible = ref(false);
const confirmLoading = ref(false);
const selectedFeatures = ref<AppearanceFeatureInfo[]>([]);
const preSelectedFeatureIds = ref<string[]>([]);

// 左侧树相关
const leftTreeRef = ref<Nullable<TreeActionType>>(null);
const treeLoading = ref(false);
const treeData = ref<TreeItem[]>([]);

// 搜索信息
const searchInfo = reactive({
  categoryId: '',
});

// 表格列定义
const columns: BasicColumn[] = [
  { title: '特性名称', dataIndex: 'name', key: 'name', width: 150 },
  { title: '特性大类', key: 'category', width: 150 },
  { title: '特性等级', key: 'severityLevel', width: 150 },
  { title: '排序码', dataIndex: 'sortCode', key: 'sortCode', width: 100, align: 'right' },
];

// 表格配置
const [registerTable, { reload, setSelectedRowKeys, getForm, getDataSource }] = useTable({
  api: getAppearanceFeatureList,
  columns,
  immediate: false, // 不立即加载
  useSearchForm: true,
  tableSetting: { size: false, setting: false },
  isCanResizeParent: true,
  resizeHeightOffset: -74,
  rowSelection: {
    type: 'checkbox',
    onChange: (selectedRowKeys: string[], selectedRows: AppearanceFeatureInfo[]) => {
      selectedFeatures.value = selectedRows;
    },
  },
  formConfig: {
    baseColProps: { span: 8 }, // 调整搜索框布局
    schemas: [
      {
        field: 'keyword',
        label: '关键词',
        component: 'Input',
        componentProps: {
          placeholder: '请输入关键词',
          submitOnPressEnter: true,
        },
      },
    ],
  },
  beforeFetch: params => {
    // 如果选择了大类，添加筛选条件
    if (searchInfo.categoryId) {
      params.categoryId = searchInfo.categoryId;
    }
    return params;
  },
  afterFetch: (dataList) => {
    if (preSelectedFeatureIds.value.length > 0 && dataList && dataList.length > 0) {
      const preSelectedFeatures = dataList.filter((item: AppearanceFeatureInfo) =>
        preSelectedFeatureIds.value.includes(item.id)
      );
      if (preSelectedFeatures.length > 0) {
        nextTick(() => {
          selectedFeatures.value = preSelectedFeatures;
          setSelectedRowKeys(preSelectedFeatures.map(item => item.id));
        });
      }
    }
    return dataList;
  },
});

// 初始化数据
async function initData() {
  treeLoading.value = true;

  try {
    const res: any = await getAllAppearanceFeatureCategories();

    let data: AppearanceFeatureCategoryInfo[] = [];

    // 处理多种可能的响应格式
    if (Array.isArray(res)) {
      data = res;
    } else if (res && typeof res === 'object') {
      if (Array.isArray(res.data)) {
        data = res.data;
      } else if (Array.isArray(res.list)) {
        data = res.list;
      } else if (Array.isArray(res.result)) {
        data = res.result;
      } else if (Array.isArray(res.items)) {
        data = res.items;
      } else if (res.data?.list && Array.isArray(res.data.list)) {
        data = res.data.list;
      }
    }

    // 转换为树形数据格式，并补充“全部”
    treeData.value = [
      { id: '__all__', title: '全部', key: '__all__', children: [] },
      ...convertToTreeData(data),
    ];
    treeLoading.value = false;

    if (treeData.value.length > 0) {
      searchInfo.categoryId = '';
      nextTick(() => {
        const leftTree = unref(leftTreeRef);
        if (leftTree) {
          leftTree.setSelectedKeys(['__all__']);
        }
        try {
          getForm().resetFields();
        } catch (e) {
          setTimeout(() => {
            try {
              getForm().resetFields();
            } catch (err) {
              console.debug('[特性选择对话框] 表格尚未准备好，将在用户交互时自动加载');
            }
          }, 200);
        }
      });
    }
  } catch (error: any) {
    console.error('[特性选择对话框] 加载特性大类失败:', error);
    const errorMsg = error?.response?.data?.msg || error?.message || '加载特性大类失败';
    createMessage.error(errorMsg);
    treeLoading.value = false;
  }
}

// 将特性大类数据转换为树形数据格式
function convertToTreeData(categories: AppearanceFeatureCategoryInfo[]): TreeItem[] {
  if (!categories || categories.length === 0) {
    return [];
  }

  const convertNode = (node: AppearanceFeatureCategoryInfo): TreeItem => {
    const treeItem: TreeItem = {
      id: node.id,
      title: node.name || '',
      key: node.id,
      children: undefined,
    };

    // 处理子节点
    if (node.children && Array.isArray(node.children) && node.children.length > 0) {
      treeItem.children = node.children.map(convertNode);
    }

    return treeItem;
  };

  return categories.map(convertNode);
}

// 重新加载树
function reloadTree() {
  treeData.value = [];
  initData();
}

// 处理树节点选择
function handleTreeSelect(id: string) {
  if (!id) return;
  if (id === '__all__') {
    searchInfo.categoryId = '';
  } else {
    if (searchInfo.categoryId === id) return;
    searchInfo.categoryId = id;
  }
  // 参考 productModal.vue，使用 resetFields 触发表格刷新
  getForm().resetFields();
}

// 处理表格行点击
function handleRowClick(record: AppearanceFeatureInfo) {
  const currentSelected = selectedFeatures.value || [];
  const index = currentSelected.findIndex(item => item.id === record.id);

  let newSelected: AppearanceFeatureInfo[] = [];
  if (index === -1) {
    newSelected = [...currentSelected, record];
  } else {
    newSelected = currentSelected.filter(item => item.id !== record.id);
  }

  selectedFeatures.value = newSelected;
  setSelectedRowKeys(newSelected.map(item => item.id));
}

// 打开对话框
async function open(existingFeatureIds?: string | string[]) {
  console.log('[FeatureSelectDialog] open 方法被调用, existingFeatureIds:', existingFeatureIds);
  visible.value = true;
  selectedFeatures.value = [];
  preSelectedFeatureIds.value = Array.isArray(existingFeatureIds)
    ? existingFeatureIds.filter(Boolean)
    : existingFeatureIds
      ? [existingFeatureIds]
      : [];

  if (preSelectedFeatureIds.value.length > 0) {
    try {
      const promises = preSelectedFeatureIds.value.map(id => getAppearanceFeatureInfo(id));
      const features = await Promise.all(promises);
      const validFeatures = features.filter(f => f && f.id) as AppearanceFeatureInfo[];

      if (validFeatures.length > 0) {
        selectedFeatures.value = validFeatures;
        setSelectedRowKeys(validFeatures.map(f => f.id));
      }
    } catch (error) {
      console.error('[FeatureSelectDialog] 获取预选特性详情失败:', error);
      setSelectedRowKeys(preSelectedFeatureIds.value);
    }
  }

  console.log('[FeatureSelectDialog] visible 设置为:', visible.value);
  // 参考 productModal.vue，在对话框打开后初始化数据
  initData();
}

// 关闭对话框
function close() {
  visible.value = false;
}

// 确认选择
function handleConfirm() {
  if (selectedFeatures.value.length === 0) {
    createMessage.warning('请选择至少一个特性');
    return;
  }

  emit('confirm', selectedFeatures.value);
  close();
}

// 取消
function handleCancel() {
  emit('cancel');
  close();
}


// 暴露方法
defineExpose({
  open,
  close,
});
</script>

<style lang="less" scoped>
/* 只需要少量的调整，大部分样式由 global classes 处理 */
</style>
