<template>
  <a-modal v-model:visible="visible" title="选择特性" :width="1000" :confirm-loading="confirmLoading" @ok="handleConfirm"
    @cancel="handleCancel" :body-style="{ padding: '0' }" class="common-container-modal" destroy-on-close>
    <div class="page-content-wrapper" style="height: 100%; padding: 10px">
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
            <template #form-submitBefore>
              <a-button type="primary" @click="handleCreateFeature" style="margin-right: 10px">
                <template #icon>
                  <PlusOutlined />
                </template>
                新建特性
              </a-button>
            </template>
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

    <!-- 新建特性弹窗 -->
    <FeatureModal @register="registerFeatureModal" @success="handleFeatureCreated" />

    <template #footer>
      <div style="display: flex; justify-content: space-between; align-items: center;">
        <div style="text-align: left;">
          <a-button danger @click="handleClear" :disabled="selectedFeatures.length === 0">清空已选</a-button>
          <span v-if="selectedFeatures.length > 0" style="margin-left: 8px; color: #888">已选 {{ selectedFeatures.length
          }}
            项</span>
        </div>
        <div>
          <a-button @click="handleCancel">取消</a-button>
          <a-button type="primary" :loading="confirmLoading" @click="handleConfirm">确定</a-button>
        </div>
      </div>
    </template>
  </a-modal>
</template>

<script lang="ts" setup>
import { ref, reactive, unref, nextTick } from 'vue';
import { PlusOutlined } from '@ant-design/icons-vue';
import { BasicLeftTree, TreeItem, TreeActionType } from '/@/components/Tree';
import { BasicTable, useTable, BasicColumn } from '/@/components/Table';
import { useModal } from '/@/components/Modal';
import { useMessage } from '/@/hooks/web/useMessage';
import { Nullable } from '/@/utils/types';
import {
  getAppearanceFeatureList,
  getAppearanceFeatureInfo,
  AppearanceFeatureInfo,
  addKeywordToFeature,
} from '/@/api/lab/appearance';
import { getAllAppearanceFeatureCategories, AppearanceFeatureCategoryInfo } from '/@/api/lab/appearanceCategory';
import FeatureModal from '/@/views/lab/appearance/components/FeatureModal.vue';

const emit = defineEmits(['confirm', 'cancel']);

// Props
const props = defineProps<{
  keywords?: string[];  // 要添加到特性的关键词列表（支持多个）
}>();

const { createMessage } = useMessage();
const [registerFeatureModal, { openModal: openFeatureModal }] = useModal();

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
const [registerTable, { reload, setSelectedRowKeys, getForm }] = useTable({
  api: getAppearanceFeatureList,
  columns,
  immediate: false, // 不立即加载
  useSearchForm: true,
  tableSetting: { size: false, setting: false },
  isCanResizeParent: true,
  resizeHeightOffset: -74,
  rowSelection: {
    type: 'checkbox',
    onChange: (_selectedRowKeys: string[], selectedRows: AppearanceFeatureInfo[]) => {
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

// 新建特性
function handleCreateFeature() {
  openFeatureModal(true, { isUpdate: false });
}

// 特性创建成功后刷新列表
function handleFeatureCreated() {
  createMessage.success('特性创建成功');
  reload();
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

  // 参考 productModal.vue，在对话框打开后初始化数据
  initData();
}

// 关闭对话框
function close() {
  visible.value = false;
}

// 确认选择
async function handleConfirm() {
  confirmLoading.value = true;
  try {
    // 如果有关键词，则调用 AddKeyword API 将关键词添加到选中的每个特性（每个特性都会添加）
    const keywords = props.keywords?.filter(k => k) || [];
    const features = selectedFeatures.value;
    if (keywords.length > 0 && features.length > 0) {
      // 为每个（特性 × 关键词）组合发起请求，并行执行且不因单个失败而中断
      const tasks: Array<Promise<void>> = [];
      const taskMeta: Array<{ feature: AppearanceFeatureInfo; keyword: string }> = [];
      for (const feature of features) {
        for (const keyword of keywords) {
          taskMeta.push({ feature, keyword });
          tasks.push(
            addKeywordToFeature({
              FeatureId: feature.id,
              Keyword: keyword,
            }).then(() => {})
          );
        }
      }
      const results = await Promise.allSettled(tasks);
      const succeeded = results.filter(r => r.status === 'fulfilled').length;
      const failed = results.filter(r => r.status === 'rejected').length;
      const featuresSucceeded = new Set<string>();
      results.forEach((r, i) => {
        if (r.status === 'fulfilled') featuresSucceeded.add(taskMeta[i].feature.id);
      });
      if (succeeded > 0) {
        createMessage.success(
          `已将关键词添加到 ${featuresSucceeded.size} 个特性${failed > 0 ? `（${failed} 次失败）` : ''}`
        );
      }
      if (failed > 0) {
        results.forEach((r, i) => {
          if (r.status === 'rejected') {
            console.warn(
              `添加关键词 "${taskMeta[i].keyword}" 到特性 ${taskMeta[i].feature.name} 失败:`,
              (r as PromiseRejectedResult).reason
            );
          }
        });
      }
    }

    // 发送确认事件
    emit('confirm', selectedFeatures.value);
    close();
  } finally {
    confirmLoading.value = false;
  }
}

// 清空选择
function handleClear() {
  selectedFeatures.value = [];
  setSelectedRowKeys([]);
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
