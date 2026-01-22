<template>
  <div class="step3-container">
    <!-- 步骤说明 -->
    <a-alert
      message="第四步：匹配外观特性"
      description="系统已根据炉号中的特性汉字自动匹配外观特性（支持1:n关系）。请检查并修正匹配结果。"
      type="info"
      show-icon
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
        <a-select
          v-model:value="filterStatus"
          style="width: 140px"
          placeholder="筛选状态"
          allow-clear
          @change="handleFilterChange">
          <a-select-option value="">全部</a-select-option>
          <a-select-option value="matched">已匹配</a-select-option>
          <a-select-option value="unmatched">未匹配</a-select-option>
          <a-select-option value="no_feature">无需匹配</a-select-option>
        </a-select>
        <a-input-search
          v-model:value="searchText"
          placeholder="搜索炉号或特性汉字"
          style="width: 250px"
          @search="handleSearch"
          allow-clear />
      </a-space>
    </div>

    <!-- 数据表格 -->
    <div class="table-section">
      <a-table
        :columns="columns"
        :data-source="displayData"
        :pagination="paginationConfig"
        :row-selection="rowSelection"
        :loading="loading"
        row-key="id"
        size="middle"
        :scroll="{ x: 1200 }"
        @change="handleTableChange">
        <template #bodyCell="{ column, record }">
          <template v-if="column.dataIndex === 'rowIndex'">
            <div class="row-index-cell">
              <span>{{ record.rowIndex }}</span>
              <!-- 置信度 < 90% 时在行号旁显示警告图标 -->
              <ExclamationCircleOutlined
                v-if="record.matchConfidence && record.matchConfidence < 0.9"
                class="warning-icon"
                style="color: #faad14; margin-left: 4px; font-size: 14px" />
            </div>
          </template>

          <template v-if="column.dataIndex === 'furnaceNo'">
            <div class="furnace-no-cell">
              <div>{{ record.furnaceNo }}</div>
              <div v-if="record.featureSuffix" class="feature-tag">
                <a-tag size="small" color="orange">{{ record.featureSuffix }}</a-tag>
              </div>
            </div>
          </template>

          <template v-else-if="column.dataIndex === 'matchedFeatures'">
            <div class="matched-features-cell">
              <div v-if="record.appearanceFeatureIds?.length" class="feature-list">
                <a-space wrap size="small">
                  <a-tag
                    v-for="feature in getMatchedFeatureLabels(record).slice(0, 3)"
                    :key="feature.id"
                    color="blue">
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

          <template v-else-if="column.dataIndex === 'matchConfidence'">
            <div class="confidence-cell">
              <a-progress
                v-if="record.matchConfidence"
                :percent="Math.round(record.matchConfidence * 100)"
                :stroke-color="getConfidenceColor(record.matchConfidence)"
                size="small"
                :show-info="false" />
              <span v-if="record.matchConfidence" class="confidence-text">
                {{ Math.round(record.matchConfidence * 100) }}%
              </span>
              <span v-else>-</span>
              <!-- 置信度 < 90% 时显示警告标识 -->
              <a-tag
                v-if="record.matchConfidence && record.matchConfidence < 0.9"
                color="warning"
                size="small"
                style="margin-left: 8px">
                <ExclamationCircleOutlined /> 需人工确认
              </a-tag>
            </div>
          </template>

          <template v-else-if="column.dataIndex === 'actions'">
            <a-space v-if="record.featureSuffix">
              <a-button
                type="primary"
                size="small"
                @click.stop="handleEditFeatures(record)">
                <EditOutlined /> 编辑
              </a-button>
            </a-space>
            <span v-else class="no-action">-</span>
          </template>
        </template>
      </a-table>
    </div>

    <!-- 特性选择弹窗 -->
    <FeatureSelectDialog
      ref="featureSelectDialogRef"
      @confirm="handleFeatureConfirm"
      @cancel="handleFeatureCancel"
    />

    <CorrectionDialog ref="correctionDialogRef" />

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
  ExclamationCircleOutlined
} from '@ant-design/icons-vue';
import { getAppearanceFeatureMatches, updateAppearanceFeatureMatches } from '/@/api/lab/rawData';
import { getAppearanceFeatureList } from '/@/api/lab/appearanceFeature';
import type {
  RawDataRow,
  AppearanceFeature,
  Step3AppearanceFeatureInput
} from '/@/api/lab/types/rawData';
import FeatureSelectDialog from '/@/views/lab/appearance/components/FeatureSelectDialog.vue';
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
const filterStatus = ref('');
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
    // 未匹配：有特性汉字但未匹配外观特性
    filtered = filtered.filter(row => row.featureSuffix && (row.appearanceFeatureIds?.length || 0) === 0);
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
    width: 80,
    fixed: 'left',
  },
  {
    title: '炉号',
    dataIndex: 'furnaceNo',
    width: 180,
  },
  {
    title: '特性汉字',
    dataIndex: 'featureSuffix',
    width: 120,
  },
  {
    title: '匹配特性',
    dataIndex: 'matchedFeatures',
    width: 300,
  },
  {
    title: '匹配置信度',
    dataIndex: 'matchConfidence',
    width: 120,
    align: 'center',
  },
  {
    title: '操作',
    dataIndex: 'actions',
    width: 150,
    fixed: 'right',
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

function getConfidenceColor(confidence: number): string {
  if (confidence >= 0.9) return '#52c41a';
  if (confidence >= 0.7) return '#faad14';
  return '#ff4d4f';
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
  message.info('批量匹配功能开发中...');
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
  console.log('handleEditFeatures called', record);
  if (!record) {
    console.error('record is null or undefined');
    return;
  }
  
  currentRecord.value = record;
  const existingIds = record.appearanceFeatureIds || [];
  featureSelectDialogRef.value?.open(existingIds);
}

function handleFeatureConfirm(features: AppearanceFeatureInfo[]) {
  if (!currentRecord.value) return;
  currentRecord.value.appearanceFeatureIds = features.map(f => f.id);
  currentRecord.value.matchDetails = features.map(f => ({
    featureId: f.id,
    featureName: f.name,
    categoryId: f.categoryId,
    categoryName: f.category,
    severityLevelId: f.severityLevelId,
    severityLevelName: f.severityLevel,
  }));
  currentRecord.value.matchConfidence = features.length > 0 ? 1.0 : undefined;
  currentRecord.value = null;
}

function handleFeatureCancel() {
  currentRecord.value = null;
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
    0%, 100% {
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
