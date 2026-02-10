<template>
  <div class="step3-container">
    <!-- 步骤说明 -->
    <a-alert message="第四步：匹配外观特性" description="系统已根据炉号中的特性汉字自动匹配外观特性（支持1:n关系）。请检查并修正匹配结果。" type="info" show-icon
      style="margin-bottom: 20px" />

    <!-- 统计信息 -->
    <div class="statistics-section">
      <a-row :gutter="16">
        <a-col :span="5">
          <a-statistic title="总数据行数" :value="totalRows" />
        </a-col>
        <a-col :span="5">
          <a-statistic title="需要匹配" :value="needMatchCount" :value-style="{ color: '#1890ff' }" />
        </a-col>
        <a-col :span="5">
          <a-statistic title="已匹配" :value="matchedCount" :value-style="{ color: '#52c41a' }" />
        </a-col>
        <a-col :span="5">
          <a-statistic title="未匹配" :value="unmatchedCount" :value-style="{ color: '#ff4d4f' }" />
        </a-col>
        <a-col :span="4">
          <a-statistic title="匹配率" :value="matchRate" suffix="%" />
        </a-col>
      </a-row>
    </div>

    <!-- 筛选和操作 -->
    <div class="filter-section">
      <a-space>
        <a-button @click="handleRefresh" :loading="loading">
          <ReloadOutlined /> 刷新匹配结果
        </a-button>
        <a-button @click="handleBatchMatch" :disabled="!hasUnmatchedData">
          <ThunderboltOutlined /> 批量匹配
        </a-button>
        <a-button @click="handleOpenCorrection">
          人工匹配修正
        </a-button>
        <a-select v-model:value="filterStatus" style="width: 140px" placeholder="筛选状态" allow-clear
          @change="handleFilterChange">
          <a-select-option value="">全部</a-select-option>
          <a-select-option value="matched">已匹配</a-select-option>
          <a-select-option value="unmatched">未匹配</a-select-option>
          <a-select-option value="no_feature">无需匹配</a-select-option>
        </a-select>
        <a-input-search v-model:value="searchText" placeholder="搜索炉号或特性汉字" style="width: 250px" @search="handleSearch"
          allow-clear />
      </a-space>
    </div>

    <!-- 数据表格 -->
    <div class="table-section">
      <a-table :columns="columns" :data-source="displayData" :pagination="paginationConfig"
        :row-selection="rowSelection" :loading="loading" row-key="id" size="middle" :scroll="{ x: 1200 }"
        @change="handleTableChange">
        <template #bodyCell="{ column, record }">
          <template v-if="column.dataIndex === 'rowIndex'">
            <div class="row-index-cell">
              <!-- 置信度 < 90% 时在行号旁显示警告图标 -->
            </div>
          </template>

          <template v-if="column.dataIndex === 'furnaceNo'">
            <div class="furnace-no-cell">
              <div>{{ record.furnaceNo }}</div>
              <div v-if="record.featureSuffix" class="feature-tag">
                <a-tag size="small" color="orange">{{ record.featureSuffix }}</a-tag>
                <!-- 快捷添加到忽略词典 -->
                <a-tooltip title="添加到忽略词典">
                  <a-button type="link" size="small" class="ignore-btn" @click.stop="handleAddToIgnore(record)">
                    <StopOutlined />
                  </a-button>
                </a-tooltip>
              </div>
            </div>
          </template>

          <template v-else-if="column.dataIndex === 'matchedFeatures'">
            <div class="matched-features-cell">
              <div v-if="record.appearanceFeatureIds?.length" class="feature-list">
                <a-space wrap size="small">
                  <a-tag v-for="feature in getMatchedFeatureLabels(record).slice(0, 3)" :key="feature.id" color="blue">
                    {{ feature.label }}
                  </a-tag>
                  <a-tag v-if="getMatchedFeatureLabels(record).length > 3" color="default">
                    +{{ getMatchedFeatureLabels(record).length - 3 }}
                  </a-tag>
                </a-space>
              </div>
              <div v-else-if="record.featureSuffix" class="unmatched-features">
                <a-tag color="error">未匹配</a-tag>
              </div>
              <div v-else class="no-feature">
                <a-tag color="default">无需匹配</a-tag>
              </div>
            </div>
          </template>



          <template v-else-if="column.dataIndex === 'actions'">
            <a-space v-if="record.featureSuffix">
              <a-button type="primary" size="small" @click.stop="handleEditFeatures(record)">
                <EditOutlined /> 编辑
              </a-button>
              <a-button type="default" danger size="small" @click.stop="handleAddToIgnore(record)">
                <StopOutlined /> 忽略
              </a-button>
            </a-space>
            <span v-else class="no-action">-</span>
          </template>
        </template>
      </a-table>
    </div>

    <!-- 特性选择弹窗 -->
    <FeatureSelectDialog ref="featureSelectDialogRef" :keywords="currentKeywords" @confirm="handleFeatureConfirm"
      @cancel="handleFeatureCancel" />

    <CorrectionDialog ref="correctionDialogRef" />

    <!-- 添加到忽略词典确认弹窗 -->
    <!-- 添加到忽略词典确认弹窗 -->
    <!-- 添加到忽略词典确认弹窗 -->
    <a-modal :visible="ignoreModalVisible" title="添加到忽略词典" :confirm-loading="ignoreModalLoading" :width="416"
      @ok="handleIgnoreConfirm" @cancel="handleIgnoreCancel">
      <div style="display: flex; align-items: flex-start; padding: 12px 0 0 0;">
        <ExclamationCircleOutlined
          style="color: #faad14; font-size: 22px; flex-shrink: 0; margin-right: 12px; margin-top: 2px;" />
        <div style="flex: 1;">
          <h5 style="margin: 0 0 8px; font-weight: 500; font-size: 16px; line-height: 1.4; color: rgba(0, 0, 0, 0.88);">
            确定要将 "{{ ignoreModalSuffix }}" 添加到忽略词典吗？
          </h5>
          <div style="margin: 0; color: rgba(0, 0, 0, 0.45); font-size: 14px; line-height: 1.5;">
            添加后，系统将在当前和后续导入中自动忽略此后缀，不再将其视为外观特性进行匹配。此操作不可直接撤销。
          </div>
        </div>
      </div>
    </a-modal>

    <!-- 加载状态 -->
    <div v-if="loading" class="loading-overlay">
      <a-spin tip="正在加载数据..." />
    </div>
  </div>
</template>

<script lang="ts" setup>
import { ref, computed, reactive, onMounted, watch } from 'vue';
import { message } from 'ant-design-vue';
import {
  ReloadOutlined,
  ThunderboltOutlined,
  EditOutlined,
  ExclamationCircleOutlined,
  StopOutlined
} from '@ant-design/icons-vue';
import { getAppearanceFeatureMatches, updateAppearanceFeatureMatches, addToIgnoreDictionary } from '/@/api/lab/rawData';
import { getAppearanceFeatureList } from '/@/api/lab/appearanceFeature';
import type {
  RawDataRow,
  AppearanceFeature,
  Step3AppearanceFeatureInput
} from '/@/api/lab/types/rawData';
import FeatureSelectDialog from './FeatureSelectDialog.vue';
import type { AppearanceFeatureInfo } from '/@/api/lab/appearance';
import CorrectionDialog from '/@/views/lab/appearance/correction.vue';



const props = defineProps({
  importSessionId: {
    type: String,
    required: true,
  },
  active: {
    type: Boolean,
    default: false,
  },
  skipAutoLoad: {
    type: Boolean,
    default: false,
  },
});

const emit = defineEmits(['prev', 'next', 'cancel']);

// 状态
const loading = ref(false);
const data = ref<RawDataRow[]>([]);
const allFeatures = ref<AppearanceFeature[]>([]);
const filterStatus = ref('unmatched');
const searchText = ref('');
const hasLoaded = ref(false); // 标记是否已经加载过数据（避免重复加载）
const selectedRowKeys = ref<string[]>([]);

// 弹窗状态
const featureSelectDialogRef = ref<InstanceType<typeof FeatureSelectDialog> | null>(null);
const correctionDialogRef = ref<InstanceType<typeof CorrectionDialog> | null>(null);
const currentRecord = ref<RawDataRow | null>(null);

// 分页配置
const paginationConfig = reactive({
  current: 1,
  pageSize: 20,
  total: 0,
  showSizeChanger: true,
  showQuickJumper: true,
  showTotal: (total: number) => `共 ${total} 条`,
});

// 计算属性
const totalRows = computed(() => Array.isArray(data.value) ? data.value.length : 0);
// 需要匹配的行数（只有有特性汉字的才需要匹配）
const needMatchCount = computed(() => Array.isArray(data.value) ? data.value.filter(row => row.featureSuffix).length : 0);
// 已匹配：有特性汉字且已匹配外观特性
const matchedCount = computed(() => Array.isArray(data.value) ? data.value.filter(row => row.featureSuffix && (row.appearanceFeatureIds?.length || 0) > 0).length : 0);
// 未匹配：有特性汉字但未匹配外观特性
const unmatchedCount = computed(() => Array.isArray(data.value) ? data.value.filter(row => row.featureSuffix && (row.appearanceFeatureIds?.length || 0) === 0).length : 0);
// 匹配率基于需要匹配的行数
const matchRate = computed(() => needMatchCount.value > 0 ? Math.round((matchedCount.value / needMatchCount.value) * 100) : 100);
const hasUnmatchedData = computed(() => unmatchedCount.value > 0);

// 批量选择的行（用于批量匹配）
const batchSelectedRows = ref<RawDataRow[]>([]);

// 已编辑过的行ID集合（用于保持在未匹配列表中）
const editedRowIds = ref<Set<string>>(new Set());

// 当前要添加的关键词列表（支持单行和批量）
const currentKeywords = computed(() => {
  // 如果有批量选择的行，返回所有选中行的featureSuffix
  if (batchSelectedRows.value.length > 0) {
    return batchSelectedRows.value
      .map(row => row.featureSuffix)
      .filter((k): k is string => !!k);
  }
  // 否则返回当前单行记录的featureSuffix
  if (currentRecord.value?.featureSuffix) {
    return [currentRecord.value.featureSuffix];
  }
  return [];
});

const displayData = computed(() => {
  // 确保 data.value 是数组
  if (!Array.isArray(data.value)) {
    paginationConfig.total = 0;
    return [];
  }

  let filtered = data.value;

  // 状态筛选
  if (filterStatus.value === 'matched') {
    // 已匹配：有特性汉字且已匹配外观特性
    filtered = filtered.filter(row => row.featureSuffix && (row.appearanceFeatureIds?.length || 0) > 0);
  } else if (filterStatus.value === 'unmatched') {
    // 未匹配：有特性汉字但未匹配外观特性，或已编辑过的行（保持在未匹配视图仅供检查）
    filtered = filtered.filter(row => {
      const isUnmatched = row.featureSuffix && (row.appearanceFeatureIds?.length || 0) === 0;
      const wasEdited = row.id && editedRowIds.value.has(row.id);
      return isUnmatched || (row.featureSuffix && wasEdited);
    });
  } else if (filterStatus.value === 'no_feature') {
    // 无需匹配：没有特性汉字
    filtered = filtered.filter(row => !row.featureSuffix);
  }

  // 搜索筛选
  if (searchText.value) {
    const search = searchText.value.toLowerCase();
    filtered = filtered.filter(row =>
      row.furnaceNo?.toLowerCase().includes(search) ||
      (row.featureSuffix && row.featureSuffix.toLowerCase().includes(search))
    );
  }

  // 分页
  paginationConfig.total = filtered.length;
  const start = (paginationConfig.current - 1) * paginationConfig.pageSize;
  const end = start + paginationConfig.pageSize;
  return filtered.slice(start, end);
});

// 表格列配置
const columns = [
  {
    title: '行号',
    dataIndex: 'rowIndex',
    width: 50,
    fixed: 'left',
    align: 'center',
  },
  {
    title: '炉号',
    dataIndex: 'furnaceNo',
    width: 200,
    align: 'left',
  },
  {
    title: '特性汉字',
    dataIndex: 'featureSuffix',
    width: 120,
    align: 'center',
  },
  {
    title: '匹配特性',
    dataIndex: 'matchedFeatures',
    width: 300,
    align: 'center',
  },

  {
    title: '操作',
    dataIndex: 'actions',
    width: 150,
    fixed: 'right',
    align: 'center',
  },
];

// 表格行选择配置
const rowSelection = computed(() => ({
  selectedRowKeys: selectedRowKeys.value,
  onChange: (keys: string[]) => {
    selectedRowKeys.value = keys;
  },
}));

// 方法
async function loadData() {
  loading.value = true;
  try {
    const response = await getAppearanceFeatureMatches(props.importSessionId);

    // 处理不同的响应格式
    let rawData: any[] = [];
    if (Array.isArray(response)) {
      rawData = response;
    } else if (response?.data && Array.isArray(response.data)) {
      rawData = response.data;
    } else if (response?.list && Array.isArray(response.list)) {
      rawData = response.list;
    } else if (response?.items && Array.isArray(response.items)) {
      rawData = response.items;
    }

    data.value = rawData;

    // 添加行号
    data.value.forEach((row, index) => {
      row.rowIndex = index + 1;
    });

    // 设置选中行
    selectedRowKeys.value = data.value
      .filter(row => !row.appearanceFeatureIds?.length)
      .map(row => row.id!);

    // 标记为已加载
    hasLoaded.value = true;
  } catch (error) {
    console.error('加载特性匹配数据失败:', error);
    message.error('加载数据失败');
    data.value = []; // 确保 data 始终是数组
    // 加载失败时不标记为已加载，允许重试
    hasLoaded.value = false;
  } finally {
    loading.value = false;
  }
}

async function loadFeatures() {
  try {
    const featuresResponse = await getAppearanceFeatureList({ keyword: '' });
    allFeatures.value = featuresResponse.list || [];
  } catch (error) {
    message.error('加载特性数据失败');
  }
}
function getFeatureName(featureId: string): string {
  const feature = allFeatures.value.find(f => f.id === featureId);
  return feature ? feature.name : featureId;
}

function getMatchedFeatureLabels(record: RawDataRow): Array<{ id: string; label: string }> {
  if (record.matchDetails && record.matchDetails.length > 0) {
    return record.matchDetails.map(detail => ({
      id: detail.featureId,
      label: [detail.categoryName, detail.severityLevelName, detail.featureName]
        .filter(Boolean)
        .join(' / '),
    }));
  }

  if (record.appearanceFeatureIds && record.appearanceFeatureIds.length > 0) {
    return record.appearanceFeatureIds.map(id => ({
      id,
      label: getFeatureName(id),
    }));
  }

  return [];
}



function handleRefresh() {
  hasLoaded.value = false; // 重置加载标记，允许重新加载
  loadData();
}

function handleBatchMatch() {
  if (selectedRowKeys.value.length === 0) {
    message.warning('请先选择要批量匹配的数据');
    return;
  }

  // 获取选中的行数据
  const selectedRows = data.value.filter(row =>
    row.id && selectedRowKeys.value.includes(row.id)
  );

  // 过滤出有特性汉字的行
  const rowsWithSuffix = selectedRows.filter(row => row.featureSuffix);
  if (rowsWithSuffix.length === 0) {
    message.warning('选中的数据中没有需要匹配的特性汉字');
    return;
  }

  // 设置批量选中的行
  batchSelectedRows.value = rowsWithSuffix;

  // 打开特性选择对话框
  featureSelectDialogRef.value?.open([]);
}

function handleOpenCorrection() {
  correctionDialogRef.value?.open({ autoOpen: false });
}

function handleFilterChange() {
  paginationConfig.current = 1;
}

function handleSearch() {
  paginationConfig.current = 1;
}

// 处理表格分页变化
function handleTableChange(pagination: any) {
  paginationConfig.current = pagination.current;
  paginationConfig.pageSize = pagination.pageSize;
}

async function handleEditFeatures(record: RawDataRow) {
  if (!record) {
    console.error('record is null or undefined');
    return;
  }

  currentRecord.value = record;
  const existingIds = record.appearanceFeatureIds || [];
  featureSelectDialogRef.value?.open(existingIds);
}

async function handleFeatureConfirm(features: AppearanceFeatureInfo[]) {
  // 判断是批量模式还是单行模式
  const isBatchMode = batchSelectedRows.value.length > 0;

  // 2. Call API to save to backend immediately
  try {
    loading.value = true;

    if (isBatchMode) {
      // 批量模式：为所有选中的行设置相同的特性
      const matches = batchSelectedRows.value.map(row => ({
        rowId: row.id!,
        appearanceFeatureIds: features.map(f => f.id),
      }));

      const updateData: Step3AppearanceFeatureInput = {
        importSessionId: props.importSessionId,
        matches,
      };

      await updateAppearanceFeatureMatches(props.importSessionId, updateData);
      message.success(`已为 ${batchSelectedRows.value.length} 条数据保存特性`);
    } else if (currentRecord.value) {
      // 单行模式
      const recordInArray = data.value.find(r => r.id === currentRecord.value?.id);
      const target = recordInArray || currentRecord.value;

      // Update local state
      target.appearanceFeatureIds = features.map(f => f.id);
      target.matchDetails = features.map(f => ({
        featureId: f.id,
        featureName: f.name,
        categoryId: f.categoryId,
        categoryName: f.category,
        severityLevelId: f.severityLevelId,
        severityLevelName: f.severityLevel,
      }));
      target.matchConfidence = features.length > 0 ? 1.0 : undefined;

      const updateData: Step3AppearanceFeatureInput = {
        importSessionId: props.importSessionId,
        matches: [{
          rowId: target.id!,
          appearanceFeatureIds: target.appearanceFeatureIds || [],
        }],
      };

      await updateAppearanceFeatureMatches(props.importSessionId, updateData);
      message.success('特性已保存');
    }

    // 注意：关键词添加已由 FeatureSelectDialog 组件处理

    // 记录已编辑的行ID，使其保持在未匹配视图中
    // 注意：需要创建新的 Set 实例来触发 Vue 响应式更新
    const newEditedIds = new Set(editedRowIds.value);
    if (isBatchMode) {
      batchSelectedRows.value.forEach(row => {
        if (row.id) newEditedIds.add(row.id);
      });
    } else if (currentRecord.value?.id) {
      newEditedIds.add(currentRecord.value.id);
    }
    editedRowIds.value = newEditedIds;

    console.log('已编辑的行IDs:', Array.from(editedRowIds.value));

    // Reload data to ensure backend persistence and consistency
    await loadData();
  } catch (error) {
    console.error('保存特性失败:', error);
    message.error('保存失败，请重试');
    loading.value = false;
  } finally {
    // 清理状态
    currentRecord.value = null;
    batchSelectedRows.value = [];
    if (!loading.value) {
      // already false
    }
  }
}


function handleFeatureCancel() {
  currentRecord.value = null;
}

// 添加到忽略词典 - 模态窗状态
const ignoreModalVisible = ref(false);
const ignoreModalLoading = ref(false);
const ignoreModalSuffix = ref('');
const ignoreModalRecord = ref<RawDataRow | null>(null);

// 打开添加到忽略词典弹窗
function handleAddToIgnore(record: RawDataRow) {
  if (!record.featureSuffix) return;
  ignoreModalSuffix.value = record.featureSuffix;
  ignoreModalRecord.value = record;
  ignoreModalVisible.value = true;
}

// 确认添加到忽略词典
async function handleIgnoreConfirm() {
  if (!ignoreModalSuffix.value) return;

  ignoreModalLoading.value = true;
  try {
    await addToIgnoreDictionary(ignoreModalSuffix.value);
    message.success('添加成功');
    cleanupSuffix(ignoreModalSuffix.value);
    ignoreModalVisible.value = false;
  } catch (error: any) {
    console.error(error);
    const msg = error?.response?.data?.msg || error?.message || '操作失败';
    if (msg.includes('已在忽略词典中')) {
      message.warning('该词语已在忽略词典中');
      cleanupSuffix(ignoreModalSuffix.value);
      ignoreModalVisible.value = false;
    } else {
      message.error(msg);
    }
  } finally {
    ignoreModalLoading.value = false;
  }
}

// 取消添加到忽略词典
function handleIgnoreCancel() {
  ignoreModalVisible.value = false;
  ignoreModalSuffix.value = '';
  ignoreModalRecord.value = null;
}

function cleanupSuffix(suffix: string) {
  if (!suffix) return;
  let clearedCount = 0;
  data.value.forEach(row => {
    if (row.featureSuffix === suffix) {
      row.featureSuffix = undefined;
      clearedCount++;
    }
  });
  if (clearedCount > 0) {
    message.info(`已清除 ${clearedCount} 条数据的后缀显示。`);
  }
}



// 暴露给父组件的方法
const canGoNext = computed(() => true); // 始终可以点击下一步，由 saveAndNext 处理确认逻辑

async function saveAndNext() {
  if (unmatchedCount.value > 0) {
    const confirmed = await new Promise<boolean>((resolve) => {
      const userConfirmed = confirm(`还有 ${unmatchedCount.value} 行数据未匹配特性，确定要继续吗？`);
      resolve(userConfirmed);
    });

    if (!confirmed) {
      return;
    }
  }

  try {
    // 保存当前匹配结果
    const updateData: Step3AppearanceFeatureInput = {
      importSessionId: props.importSessionId,
      matches: data.value.map(row => ({
        rowId: row.id!,
        appearanceFeatureIds: row.appearanceFeatureIds || [],
      })),
    };

    await updateAppearanceFeatureMatches(props.importSessionId, updateData);
    emit('next');
  } catch (error) {
    message.error('保存失败，请重试');
  }
}

// 生命周期
onMounted(() => {
  // 仅在激活步骤且允许自动加载时加载数据
  if (props.active && !props.skipAutoLoad && props.importSessionId && !hasLoaded.value) {
    loadData();
  }
  loadFeatures(); // 特性列表可以提前加载
});

// 监听 importSessionId 变化，当从空变为有值时自动加载数据
watch(
  () => [props.importSessionId, props.active, props.skipAutoLoad],
  ([newId, isActive, skip], [oldId]) => {
    if (skip) {
      return;
    }
    // 当 importSessionId 从空变为有值，且处于激活步骤时触发加载
    if (isActive && newId && !oldId && !hasLoaded.value) {
      loadData();
    }
  },
  { immediate: false }
);

// 添加一个方法来手动触发加载（供父组件调用）
function triggerLoad() {
  if (props.skipAutoLoad) {
    return;
  }
  if (props.active && props.importSessionId && props.importSessionId.trim() !== '') {
    // 重置加载标记，允许重新加载（即使之前已加载过）
    hasLoaded.value = false;
    loadData();
  }
}

// 暴露给父组件
defineExpose({
  canGoNext,
  saveAndNext,
  triggerLoad, // 暴露触发加载的方法
});
</script>

<style lang="less" scoped>
.step3-container {
  padding: 0;
}

.statistics-section {
  padding: 20px;
  background: #fafafa;
  border-radius: 8px;
  margin-bottom: 20px;
}

.filter-section {
  margin-bottom: 16px;
  display: flex;
  justify-content: space-between;
  align-items: center;
}

.table-section {
  .furnace-no-cell {
    .feature-tag {
      margin-top: 4px;
      display: flex;
      align-items: center;
      gap: 4px;

      .ignore-btn {
        padding: 0;
        height: 20px;
        line-height: 20px;
        font-size: 12px;
        color: #ff4d4f;
        opacity: 0;
        transition: opacity 0.3s;
      }
    }

    &:hover .ignore-btn {
      opacity: 1;
    }
  }

  .matched-features-cell {
    .feature-list {
      .ant-tag {
        margin-bottom: 4px;
      }
    }
  }

  .confidence-cell {
    display: flex;
    align-items: center;
    gap: 8px;

    .confidence-text {
      font-size: 12px;
      color: #666;
    }
  }

  .row-index-cell {
    display: flex;
    align-items: center;

    .warning-icon {
      color: #faad14;
      margin-left: 4px;
      font-size: 14px;
      animation: pulse 2s infinite;
    }
  }

  @keyframes pulse {

    0%,
    100% {
      opacity: 1;
    }

    50% {
      opacity: 0.5;
    }
  }

  // 确保操作按钮可以正常点击
  :deep(.ant-table-cell-fix-right) {
    .ant-space {
      pointer-events: auto;

      .ant-btn {
        pointer-events: auto;
        cursor: pointer;
        position: relative;
        z-index: 1;
      }
    }
  }
}

.feature-selection-content {
  .current-feature {
    margin-bottom: 16px;
  }

  .feature-categories {
    margin-bottom: 16px;

    .category-features {
      padding: 16px 0;
    }

    .feature-item {
      padding: 12px;
      border: 1px solid #d9d9d9;
      border-radius: 6px;
      cursor: pointer;
      transition: all 0.3s;
      height: 100%;

      &:hover {
        border-color: #1890ff;
        background-color: #f0f9ff;
      }

      &.selected {
        border-color: #1890ff;
        background-color: #e6f7ff;
      }

      .feature-header {
        display: flex;
        justify-content: space-between;
        align-items: center;
        margin-bottom: 8px;

        .feature-name {
          font-weight: 500;
        }
      }

      .feature-desc {
        font-size: 12px;
        color: #666;
        margin-bottom: 4px;
      }

      .feature-confidence {
        font-size: 12px;
        color: #999;
      }
    }
  }

  .selected-features {
    padding: 16px;
    background: #fafafa;
    border-radius: 6px;

    h4 {
      margin-bottom: 12px;
      font-size: 14px;
    }
  }
}
</style>
