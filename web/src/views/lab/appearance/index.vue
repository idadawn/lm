<template>
  <div class="page-content-wrapper">
    <!-- 左侧：特性大类树 -->
    <div class="page-content-wrapper-left">
      <BasicLeftTree title="特性大类" ref="leftTreeRef" :treeData="treeData" :loading="treeLoading" @reload="reloadTree"
        @select="handleTreeSelect" :dropDownActions="leftDropDownActions"
        :fieldNames="{ key: 'id', title: 'title', children: 'children' }" />
    </div>

    <!-- 中间：外观特征定义表格 -->
    <div class="page-content-wrapper-center">
      <div class="page-content-wrapper-content">
        <BasicTable @register="registerTable" :searchInfo="searchInfo">
          <template #tableTitle>
            <div class="flex items-center justify-between w-full">
              <div class="flex items-center gap-2">
                <a-button type="primary" preIcon="icon-ym icon-ym-btn-add" @click="handleCreate">新增特性</a-button>
                <a-button type="default" @click="handleGoToSeverityLevels">管理特性等级</a-button>
                <a-button type="default" @click="handleManualMatch">人工修正列表</a-button>
              </div>
              <a-button type="primary" :class="testPanelCollapsed ? 'bg-purple-600' : ''"
                @click="testPanelCollapsed = !testPanelCollapsed" style="margin-left: 16px">
                <template #icon>
                  <MenuUnfoldOutlined v-if="testPanelCollapsed" />
                  <MenuFoldOutlined v-else />
                </template>
                语义匹配测试
              </a-button>
            </div>
          </template>
          <template #bodyCell="{ column, record }">
            <template v-if="column.key === 'action'">
              <TableAction :actions="getTableActions(record)" />
            </template>
            <template v-else-if="column.key === 'category'">
              <a-tag color="purple">{{ record.category || '-' }}</a-tag>
            </template>
            <template v-else-if="column.key === 'severityLevel'">
              <a-tag color="orange">{{ record.severityLevel || '默认' }}</a-tag>
            </template>
          </template>
        </BasicTable>
      </div>
    </div>

    <!-- 右侧：语义匹配测试面板 -->
    <div class="page-content-wrapper-right" :class="testPanelCollapsed ? 'collapsed' : ''">
      <div class="test-panel bg-purple-600 text-white p-6 rounded-lg shadow-lg h-full overflow-auto">
        <h2 class="text-lg font-bold mb-2 flex items-center gap-2">
          <svg xmlns="http://www.w3.org/2000/svg" width="20" height="20" viewBox="0 0 24 24" fill="none"
            stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">
            <circle cx="12" cy="12" r="10" />
            <line x1="12" y1="16" x2="12" y2="12" />
            <line x1="12" y1="8" x2="12.01" y2="8" />
          </svg>
          语义匹配测试 (规则 + AI)
        </h2>
        <p class="text-purple-100 text-sm mb-4">
          在此输入炉号后缀或备注信息，系统将使用规则引擎和AI模型进行智能匹配。
        </p>

        <div class="mb-2">输入测试文本</div>
        <a-input-search v-model:value="testText" placeholder="例如: 微脆微脆微划 或 1-1有些发脆..." enter-button="规则 + AI 匹配" size="large"
          @search="handleSemanticSearch" class="custom-search-input" />
        <div class="mt-1 text-xs text-purple-200">连续两位中文会自动分割后匹配（如：微脆、微划）</div>

        <div v-if="searchMethod" class="mt-3 text-xs text-purple-200">
          匹配方式: {{ searchMethod }}
          <a-tooltip title="匹配优先级：规则引擎 → AI分析 → 文本模糊匹配">
            <InfoCircleOutlined class="ml-1" />
          </a-tooltip>
        </div>

        <div v-if="matchResult.length > 0" class="mt-4 bg-white/10 p-3 rounded border border-white/20">
          <div class="text-sm font-bold mb-2">匹配结果:</div>
          <div v-for="res in matchResult" :key="res.id" class="mb-3 p-3 bg-white/10 rounded border border-white/20">
            <!-- 输入文字 -->
            <div class="text-xs mb-2">
              <span class="text-purple-300 font-medium">输入文字: </span>
              <span class="text-white font-semibold">{{ res.query || '-' }}</span>
            </div>
            <!-- 100%匹配结果 -->
            <template v-if="res.isPerfectMatch">
              <!-- 特征大类 -->
              <div class="text-xs mb-2">
                <span class="text-purple-300 font-medium">特征大类: </span>
                <span class="text-white font-semibold">{{ res.category || '-' }}</span>
              </div>
              <!-- 特性分类 -->
              <div v-if="res.categoryPath" class="text-xs mb-2">
                <span class="text-purple-300 font-medium">特性分类: </span>
                <span class="text-purple-100">{{ res.categoryPath }}</span>
              </div>
              <!-- 特征名称 -->
              <div class="text-xs mb-2">
                <span class="text-purple-300 font-medium">特征名称: </span>
                <span class="text-white font-semibold">{{ res.featureName || '-' }}</span>
              </div>
              <!-- 特性等级 -->
              <div class="text-xs mb-2">
                <span class="text-purple-300 font-medium">特性等级: </span>
                <span class="px-2 py-0.5 bg-orange-500 text-white rounded font-semibold">
                  {{ res.severityLevel || '默认' }}
                </span>
              </div>
              <!-- 匹配方式 -->
              <div class="text-xs mb-2">
                <span class="text-purple-300 font-medium">匹配方式: </span>
                <span class="text-purple-100">{{ getMatchMethodText(res.matchMethod) }}</span>
                <span class="ml-2 px-2 py-0.5 bg-green-500 text-white rounded text-xs">100%匹配</span>
              </div>
            </template>
            <!-- 需要人工确认的结果 -->
            <template v-else>
              <div class="text-xs mb-2">
                <span class="text-purple-300 font-medium">匹配方式: </span>
                <span class="text-purple-100">{{ getMatchMethodText(res.matchMethod) }}</span>
                <span class="ml-2 px-2 py-0.5 bg-yellow-500 text-white rounded text-xs">需要确认</span>
              </div>
              <!-- AI识别结果 -->
              <div v-if="res.manualCorrections && res.manualCorrections.length > 0" class="mt-3">
                <div class="text-xs font-bold mb-2 text-yellow-200">AI识别结果:</div>
                <div v-for="(option, idx) in res.manualCorrections" :key="idx"
                  class="mb-2 p-2 bg-yellow-500/20 border border-yellow-500/50 rounded">
                  <div class="text-xs mb-1">
                    <span class="text-white font-semibold">{{ option.featureName }}</span>
                    <span v-if="option.severityLevel" class="ml-2 px-2 py-0.5 bg-orange-500 text-white rounded">
                      {{ option.severityLevel }}
                    </span>
                  </div>
                  <div v-if="option.category" class="text-xs mb-1">
                    <span class="text-purple-300">大类: </span>
                    <span class="text-white">{{ option.category }}</span>
                    <span v-if="option.categoryPath" class="text-purple-300 ml-2">分类: </span>
                    <span v-if="option.categoryPath" class="text-white">{{ option.categoryPath }}</span>
                  </div>
                  <div class="text-xs mb-1 text-yellow-200">
                    {{ option.suggestion }}
                  </div>
                  <!-- 只有 add_keyword 类型才显示确认添加按钮 -->
                  <div v-if="option.actionType === 'add_keyword'" class="flex gap-2 mt-2">
                    <a-button type="primary" size="small" @click="handleAddKeyword(res.query, option)">
                      <template #icon>
                        <PlusOutlined />
                      </template>
                      确认添加到关键词
                    </a-button>
                  </div>
                  <!-- 其他类型显示跳转修正列表按钮 -->
                  <div v-else class="flex gap-2 mt-2">
                    <a-button type="default" size="small" @click="handleManualMatch">
                      <template #icon>
                        <EditOutlined />
                      </template>
                      前往人工修正列表查看
                    </a-button>
                  </div>
                </div>
              </div>
              <div v-else class="mt-2 text-xs text-yellow-200">
                未找到匹配选项
              </div>
              <!-- 人工修正入口 -->
              <div class="mt-3 pt-3 border-t border-yellow-500/30">
                <a-button type="default" size="small" @click="handleManualMatch" block>
                  <template #icon>
                    <EditOutlined />
                  </template>
                  人工修正匹配列表
                </a-button>
              </div>
            </template>
          </div>
        </div>
        <div v-else-if="searched" class="mt-4">
          <div class="text-sm text-purple-200 mb-2">未匹配到任何定义的特性.</div>
          <a-button type="primary" size="small" @click="handleManualMatch">
            <template #icon>
              <PlusOutlined />
            </template>
            人工匹配
          </a-button>
        </div>
      </div>
    </div>

    <!-- Modals -->
    <FeatureModal @register="registerModal" @success="handleSuccess" />

    <!-- 特性大类管理抽屉 -->
    <CategoryDrawer @register="registerDrawer" @visible-change="onCategoryDrawerVisibleChange" />

    <!-- 人工匹配修正列表弹窗 -->
    <CorrectionDialog ref="correctionDialogRef" />

    <!-- 人工匹配对话框 - 暂时保留用于兼容 -->
    <!-- <FeatureMatchDialog
      ref="featureMatchDialogRef"
      :input-text="testText"
      :auto-match-results="[]"
      @confirm="handleMatchConfirm"
      @cancel="handleMatchCancel"
    /> -->
  </div>
</template>

<script lang="ts" setup>
import { ref, reactive, unref, onMounted, nextTick } from 'vue';
import { useRouter } from 'vue-router';
import {
  EditOutlined,
  PlusOutlined,
  InfoCircleOutlined,
  MenuFoldOutlined,
  MenuUnfoldOutlined,
} from '@ant-design/icons-vue';
import { BasicLeftTree, TreeItem, TreeActionType } from '/@/components/Tree';
import { BasicTable, useTable, TableAction, BasicColumn, ActionItem } from '/@/components/Table';
import { useModal } from '/@/components/Modal';
import { useDrawer } from '/@/components/Drawer';
import { useMessage } from '/@/hooks/web/useMessage';
import { Nullable } from '/@/utils/types';
import FeatureModal from './components/FeatureModal.vue';
import CorrectionDialog from './correction.vue';
// import FeatureMatchDialog from './components/FeatureMatchDialog.vue'; // 暂时注释，新的批量匹配已包含人工修正选项
import CategoryDrawer from './components/CategoryDrawer.vue';
import {
  getAppearanceFeatureList,
  delAppearanceFeature,
  batchMatchAppearanceFeature,
  type MatchItemOutput,
  type ManualCorrectionOption,
  AppearanceFeatureInfo,
} from '/@/api/lab/appearance';
import { confirmCorrection } from '/@/api/lab/featureLearning';
import { getAllAppearanceFeatureCategories, AppearanceFeatureCategoryInfo } from '/@/api/lab/appearanceCategory';

defineOptions({ name: 'AppearanceFeature' });

const router = useRouter();
const { createMessage } = useMessage();
const [registerModal, { openModal }] = useModal();
const [registerDrawer, { openDrawer }] = useDrawer();

// 左侧树相关
const leftTreeRef = ref<Nullable<TreeActionType>>(null);
const treeLoading = ref(false);
const treeData = ref<TreeItem[]>([]);
const correctionDialogRef = ref<InstanceType<typeof CorrectionDialog> | null>(null);

// 打开特性大类管理抽屉
function handleGoToCategories() {
  openDrawer(true);
}

const leftDropDownActions = [
  {
    label: '管理特性大类',
    onClick: handleGoToCategories,
  },
];

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
const [registerTable, { reload, setLoading, getForm }] = useTable({
  api: getAppearanceFeatureList,
  columns,
  immediate: false,
  useSearchForm: true,
  formConfig: {
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
  actionColumn: {
    width: 150,
    title: '操作',
    dataIndex: 'action',
  },
  beforeFetch: params => {
    // 如果选择了大类，添加筛选条件
    if (searchInfo.categoryId && searchInfo.categoryId !== 'all') {
      params.categoryId = searchInfo.categoryId;
    }
    // 如果参数中有 categoryId 且为 'all'，则移除该参数
    if (params.categoryId === 'all') {
      delete params.categoryId;
    }
    return params;
  },
});

// 语义匹配测试相关
const testText = ref('');
const testPanelCollapsed = ref(true); // 默认收起
const matchResult = ref<MatchItemOutput[]>([]);
const searched = ref(false);
const searchMethod = ref('');
// const featureMatchDialogRef = ref<any>(null); // 暂时注释

// 获取表格操作按钮
function getTableActions(record: AppearanceFeatureInfo): ActionItem[] {
  return [
    {
      label: '编辑',
      onClick: () => handleEdit(record),
    },
    {
      label: '删除',
      color: 'error',
      modelConfirm: {
        title: '确认删除',
        content: `确定要删除特性"${record.name || record.id}"吗？此操作不可恢复。`,
        onOk: async () => {
          await handleDelete(record);
        },
      },
    },
  ];
}

// 初始化数据
async function initData(isInit = false) {
  treeLoading.value = true;
  if (isInit) setLoading(true);

  try {
    const res: any = await getAllAppearanceFeatureCategories();

    let data: AppearanceFeatureCategoryInfo[] = [];

    // 处理多种可能的响应格式
    if (Array.isArray(res)) {
      // 直接返回数组
      data = res;
    } else if (res && typeof res === 'object') {
      // 包装在对象中
      if (Array.isArray(res.data)) {
        data = res.data;
      } else if (Array.isArray(res.list)) {
        data = res.list;
      } else if (Array.isArray(res.result)) {
        data = res.result;
      } else if (Array.isArray(res.items)) {
        data = res.items;
      }
    }


    // 如果数据为空，给出提示
    if (!data || data.length === 0) {
      console.warn('[外观特性管理] 特性大类数据为空，请检查数据库是否有数据');
      treeData.value = [];
      treeLoading.value = false;
      if (isInit) setLoading(false);
      return;
    }

    // 转换为树形数据格式
    const categoryNodes = convertToTreeData(data);

    // 添加"所有大类"节点
    treeData.value = [
      { id: 'all', title: '所有大类', key: 'all' },
      ...categoryNodes
    ];

    nextTick(() => {
      if (isInit && treeData.value.length > 0) {
        // 默认选择第一个节点 (所有大类)
        searchInfo.categoryId = 'all';
      }
      const leftTree = unref(leftTreeRef);
      if (searchInfo.categoryId) {
        leftTree?.setSelectedKeys([searchInfo.categoryId]);
      }
      if (isInit && searchInfo.categoryId) {
        reload();
      }
      treeLoading.value = false;
      if (isInit) setLoading(false);
    });
  } catch (error: any) {
    console.error('[外观特性管理] 加载特性大类失败:', error);
    console.error('[外观特性管理] 错误详情:', {
      message: error?.message,
      response: error?.response,
      data: error?.response?.data,
    });
    const errorMsg = error?.response?.data?.msg || error?.message || '加载特性大类失败';
    createMessage.error(errorMsg);
    treeLoading.value = false;
    if (isInit) setLoading(false);
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
    };

    // 处理子节点
    if (node.children && Array.isArray(node.children) && node.children.length > 0) {
      treeItem.children = node.children.map(convertNode);
    } else {
      // 如果没有子节点，不设置 children 字段（或设置为 undefined）
      treeItem.children = undefined;
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
  if (!id || searchInfo.categoryId === id) return;
  searchInfo.categoryId = id;
  getForm().resetFields();
  reload();
}

// 新增特性
function handleCreate() {
  openModal(true, {
    isUpdate: false,
    categoryId: (searchInfo.categoryId && searchInfo.categoryId !== 'all') ? searchInfo.categoryId : undefined,
  });
}

// 编辑特性
function handleEdit(record: AppearanceFeatureInfo) {
  openModal(true, {
    isUpdate: true,
    record,
  });
}

// 删除特性
async function handleDelete(record: AppearanceFeatureInfo) {
  if (!record || !record.id) {
    createMessage.error('删除失败：记录ID不存在');
    return;
  }
  try {
    await delAppearanceFeature(record.id);
    createMessage.success('删除成功');
    reload();
  } catch (error: any) {
    const errorMsg = error?.response?.data?.msg || error?.message || '删除失败';
    createMessage.error(errorMsg);
  }
}

// 保存成功回调
function handleSuccess() {
  reload();
}

// 跳转到特性等级管理
function handleGoToSeverityLevels() {
  router.push('/lab/severity-level');
}

/** 将输入按两位中文特性名分割为多个 query（如 "微脆微脆微划" → ["微脆","微脆","微划"]），去重后返回 */
function splitTwoCharSegments(text: string): string[] {
  if (!text || !text.trim()) return [];
  const trimmed = text.trim();
  // 提取连续中文字符段（CJK 统一汉字），再按每 2 字切分
  const cjkRegex = /[\u4e00-\u9fff]+/g;
  const segments: string[] = [];
  let m: RegExpExecArray | null;
  while ((m = cjkRegex.exec(trimmed)) !== null) {
    const segment = m[0];
    for (let i = 0; i < segment.length; i += 2) {
      const two = segment.slice(i, i + 2);
      if (two.length === 2) segments.push(two);
      // 若最后剩 1 字也加入，便于匹配单字特性名
      else if (two.length === 1) segments.push(two);
    }
  }
  // 若没有中文段，整句作为一条（兼容 "1-1有些发脆" 等）
  if (segments.length === 0) return [trimmed];
  // 去重并保持顺序，减少重复匹配
  const seen = new Set<string>();
  const unique: string[] = [];
  for (const s of segments) {
    if (!seen.has(s)) {
      seen.add(s);
      unique.push(s);
    }
  }
  return unique;
}

// 语义匹配搜索（使用新的批量匹配API）
const handleSemanticSearch = async () => {
  if (!testText.value) return;
  searched.value = true;
  searchMethod.value = '匹配中...';
  try {
    // 先按两位中文分割，再批量匹配（用户常输入如 "微脆微脆微划"）
    const queries = splitTwoCharSegments(testText.value);
    const items = queries.map((query, i) => ({ id: String(i + 1), query }));
    const res: any = await batchMatchAppearanceFeature({ items });

    let resultArray: MatchItemOutput[] = [];
    if (Array.isArray(res)) {
      resultArray = res;
    } else if (res?.data && Array.isArray(res.data)) {
      resultArray = res.data;
    } else if (res?.list && Array.isArray(res.list)) {
      resultArray = res.list;
    }

    matchResult.value = resultArray;

    if (resultArray.length > 0) {
      const perfectCount = resultArray.filter((r) => r.isPerfectMatch).length;
      const total = resultArray.length;
      if (perfectCount === total) {
        searchMethod.value = total === 1
          ? `匹配成功 (${getMatchMethodText(resultArray[0].matchMethod)})`
          : `匹配成功 共 ${total} 条 (已按两位中文分割)`;
      } else if (perfectCount > 0) {
        searchMethod.value = `部分需确认 共 ${total} 条`;
      } else {
        searchMethod.value = total === 1
          ? `需要人工确认 (${getMatchMethodText(resultArray[0].matchMethod)})`
          : `需确认 共 ${total} 条 (已按两位中文分割)`;
      }
    } else {
      searchMethod.value = '无匹配结果';
    }
  } catch (e) {
    console.error(e);
    searchMethod.value = '搜索失败';
    matchResult.value = [];
  }
};

// 获取匹配方式显示文本
const getMatchMethodText = (method: string | undefined): string => {
  const methodMap: Record<string, string> = {
    name: '特性名称精确匹配',
    keyword: '关键词匹配',
    ai: 'AI分析',
    none: '无匹配',
  };
  return methodMap[method || ''] || method || '未知';
};

// 人工匹配 (查看修正列表)
const handleManualMatch = () => {
  if (correctionDialogRef.value) {
    correctionDialogRef.value.open({ autoOpen: true });
  } else {
    console.error('correctionDialogRef is null');
  }
};

// 人工匹配确认
const handleMatchConfirm = async (data: any) => {
  matchResult.value = [data.feature];
  searchMethod.value = `人工匹配 (${data.matchMode})`;
  createMessage.success(`已匹配到: ${data.feature.category} - ${data.feature.name}`);

  try {
    const { saveAppearanceFeatureCorrection } = await import('/@/api/lab/appearance');
    await saveAppearanceFeatureCorrection({
      inputText: data.inputText,
      autoMatchedFeatureId: matchResult.value.length > 0 ? matchResult.value[0].id : undefined,
      correctedFeatureId: data.feature.id,
      matchMode: data.matchMode,
      scenario: 'test',
      remark: `测试页面人工匹配`,
    });
  } catch (error) {
    console.error('保存人工修正记录失败:', error);
  }
};

// 人工匹配取消
const handleMatchCancel = () => {
};

// 创建特性等级
const handleCreateSeverityLevel = async (result: AppearanceFeatureInfo) => {
  if (!result.suggestedSeverity) {
    createMessage.warning('缺少必要信息');
    return;
  }

  try {
    const { createSeverityLevel } = await import('/@/api/lab/severityLevel');

    // 创建特性等级
    await createSeverityLevel({
      name: result.suggestedSeverity,
      description: `特性等级：${result.suggestedSeverity}`,
      enabled: true,
      isDefault: false,
    });

    createMessage.success(`已创建特性等级 "${result.suggestedSeverity}"`);

    // 重新搜索以获取最新结果（现在等级已存在，可以继续匹配）
    await handleSemanticSearch();
  } catch (error: any) {
    const errorMsg = error?.response?.data?.msg || error?.message || '创建失败';
    createMessage.error(errorMsg);
  }
};

// 添加关键词（从人工修正选项）
const handleAddKeyword = async (query: string, option: ManualCorrectionOption) => {
  try {
    if (option.correctionId) {
      // 使用新接口：确认修正记录（这会自动添加关键词并更新状态）
      await confirmCorrection(option.correctionId);
    } else {
      // 兼容旧逻辑（如果没有 correctionId，例如非AI生成的选项）
      const { addKeywordToFeature } = await import('/@/api/lab/appearance');
      await addKeywordToFeature({
        FeatureId: option.featureId,
        Keyword: query,
      });
    }

    createMessage.success(`已将"${query}"添加到特性"${option.featureName}"的关键词列表`);

    // 重新搜索以获取最新结果
    await handleSemanticSearch();
  } catch (error: any) {
    const errorMsg = error?.response?.data?.msg || error?.message || '添加关键词失败';
    createMessage.error(errorMsg);
  }
};

// 选择特性（从人工修正选项）
const handleSelectFeature = async (option: ManualCorrectionOption) => {
  createMessage.info(`已选择特性: ${option.featureName}`);
  // 这里可以根据需要添加更多逻辑，比如保存选择记录等
};

// 创建或添加关键词（统一处理）- 保留用于兼容旧代码
const handleCreateOrAddKeyword = async (result: AppearanceFeatureInfo) => {
  if (!result.suggestedSeverity || !result.categoryId || !result.name) {
    createMessage.warning('缺少必要信息');
    return;
  }

  try {
    const { createOrAddKeyword } = await import('/@/api/lab/appearance');

    // 特性名称应该是"严重脆"（程度词+特性名称），而不是只使用"脆"
    const featureName = result.suggestedFeatureName || `${result.suggestedSeverity}${result.name}`;

    // 调用新接口
    const response: any = await createOrAddKeyword({
      inputText: testText.value,
      autoMatchedFeatureId: result.id,
      categoryId: result.categoryId,
      featureName: featureName,
      severityLevelName: result.suggestedSeverity,
      description: result.description,
      scenario: 'test',
    });

    // 显示成功消息
    const message = response?.data?.message || response?.message || '操作成功';
    createMessage.success(message);

    // 更新匹配结果状态
    result.requiresSeverityConfirmation = false;

    // 重新搜索以获取最新结果
    await handleSemanticSearch();
  } catch (error: any) {
    const errorMsg = error?.response?.data?.msg || error?.message || '操作失败';
    createMessage.error(errorMsg);
  }
};

// 特性大类管理抽屉关闭时重新加载树
function onCategoryDrawerVisibleChange(visible: boolean) {
  if (!visible) {
    reloadTree();
  }
}

onMounted(() => {
  initData(true);
});
</script>

<style lang="less" scoped>
:deep(.custom-search-input) {
  .ant-input {
    background: rgba(255, 255, 255, 0.2);
    border: 1px solid rgba(255, 255, 255, 0.3);
    color: white;

    &::placeholder {
      color: rgba(255, 255, 255, 0.6);
    }
  }

  .ant-input-search-button {
    background: white !important;
    color: #7e22ce !important;
    border: none !important;
    font-weight: bold;
  }
}

.page-content-wrapper-right {
  width: 400px;
  margin-left: 5px;
  transition: width 0.3s ease-in-out;
  overflow: hidden;

  &.collapsed {
    width: 0;
  }
}

.test-panel {
  min-height: 100%;
}
</style>
