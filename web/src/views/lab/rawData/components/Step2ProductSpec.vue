<template>
  <div class="step2-container">
    <!-- 步骤说明 -->
    <a-alert
      message="第三步：自动匹配产品规格"
      description="系统已根据检测数据自动匹配产品规格，请检查并修正匹配结果。未匹配的数据需要手动选择产品规格。本步骤只显示有效数据。"
      type="info"
      show-icon
      style="margin-bottom: 20px" />

    <!-- 统计信息 -->
    <div class="statistics-section">
      <a-row :gutter="16">
        <a-col :span="6">
          <a-statistic title="总数据行数" :value="totalRows" />
        </a-col>
        <a-col :span="6">
          <a-statistic title="已匹配" :value="matchedCount" :value-style="{ color: '#52c41a' }" />
        </a-col>
        <a-col :span="6">
          <a-statistic title="未匹配" :value="unmatchedCount" :value-style="{ color: '#ff4d4f' }" />
        </a-col>
        <a-col :span="6">
          <a-statistic title="匹配率" :value="matchRate" suffix="%" />
        </a-col>
      </a-row>
    </div>

    <!-- 数据正在处理中提示 -->
    <div v-if="isProcessing && data.length === 0" class="processing-alert" style="margin-bottom: 20px">
      <a-alert
        message="数据正在处理中"
        description="系统正在解析和处理上传的数据，请稍候...（自动重试中）"
        type="info"
        show-icon
      />
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
        <a-select
          v-model:value="filterStatus"
          style="width: 120px"
          placeholder="筛选状态"
          allow-clear
          @change="handleFilterChange">
          <a-select-option value="">全部</a-select-option>
          <a-select-option value="matched">已匹配</a-select-option>
          <a-select-option value="unmatched">未匹配</a-select-option>
        </a-select>
        <a-input-search
          v-model:value="searchText"
          placeholder="搜索炉号或产品规格"
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
        :scroll="{ x: 1000 }">
        <template #bodyCell="{ column, record }">
          <template v-if="column.dataIndex === 'prodDate'">
            {{ formatDate(record.prodDate) }}
          </template>
          <template v-else-if="column.dataIndex === 'furnaceNo'">
            <div class="furnace-no-cell">
              <div>{{ record.furnaceNo }}</div>
              <div v-if="record.featureSuffix" class="feature-tag">
                <a-tag size="small">{{ record.featureSuffix }}</a-tag>
              </div>
            </div>
          </template>

          <template v-else-if="column.dataIndex === 'productSpec'">
            <div class="product-spec-cell">
              <div v-if="record.productSpecId" class="matched-spec">
                <div class="spec-name">{{ record.productSpecName }}</div>
                <div class="spec-code">{{ record.productSpecCode }}</div>
              </div>
              <div v-else class="unmatched-spec">
                <a-tag color="error">未匹配</a-tag>
              </div>
            </div>
          </template>

          <template v-else-if="column.dataIndex === 'detectionColumns'">
            <div class="detection-info">
              <a-tag v-if="record.detectionColumns" color="blue">
                {{ record.detectionColumns }}列
              </a-tag>
              <span v-else>-</span>
            </div>
          </template>

          <template v-else-if="column.dataIndex === 'breakCount'">
            <span>{{ record.breakCount ?? '-' }}</span>
          </template>

          <template v-else-if="column.dataIndex === 'singleCoilWeight'">
            <span>{{ record.singleCoilWeight ?? '-' }}</span>
          </template>
        </template>
      </a-table>
    </div>

    <!-- 产品规格选择弹窗 -->
    <a-modal
      v-model:open="specModalVisible"
      title="选择产品规格"
      :width="800"
      :mask-closable="false"
      @ok="handleSpecModalOk"
      @cancel="handleSpecModalCancel">
      <div class="spec-selection-content">
        <!-- 搜索和筛选 -->
        <div class="spec-filter">
          <a-space>
            <a-input-search
              v-model:value="specSearchText"
              placeholder="搜索产品规格名称或编码"
              style="width: 250px"
              @search="handleSpecSearch"
              allow-clear />
            <a-select
              v-model:value="specFilterEnabled"
              style="width: 120px"
              placeholder="状态"
              allow-clear
              @change="handleSpecFilterChange">
              <a-select-option :value="true">启用</a-select-option>
              <a-select-option :value="false">禁用</a-select-option>
            </a-select>
          </a-space>
        </div>

        <!-- 规格列表 -->
        <a-table
          :columns="specColumns"
          :data-source="specList"
          :pagination="specPagination"
          :loading="specLoading"
          :row-selection="specRowSelection"
          row-key="id"
          size="small"
          @change="handleSpecTableChange">
          <template #bodyCell="{ column, record }">
            <template v-if="column.dataIndex === 'enabled'">
              <a-tag :color="record.enabled ? 'green' : 'red'">
                {{ record.enabled ? '启用' : '禁用' }}
              </a-tag>
            </template>

            <template v-else-if="column.dataIndex === 'detectionColumns'">
              <a-tag color="blue">{{ record.detectionColumns }}列</a-tag>
            </template>
          </template>
        </a-table>
      </div>
    </a-modal>

    <!-- 批量匹配弹窗 -->
    <a-modal
      v-model:open="batchMatchModalVisible"
      title="批量匹配产品规格"
      :width="600"
      @ok="handleBatchMatchOk"
      @cancel="handleBatchMatchCancel">
      <p>将为选中的 {{ selectedRowKeys.length }} 行数据批量设置产品规格：</p>
      <a-form :model="batchMatchForm" layout="vertical">
        <a-form-item
          label="产品规格"
          name="productSpecId"
          :rules="[{ required: true, message: '请选择产品规格' }]">
          <a-select
            v-model:value="batchMatchForm.productSpecId"
            placeholder="请选择产品规格"
            show-search
            :filter-option="filterProductSpecOption">
              <a-select-option
                v-for="spec in specList"
                :key="spec.id"
                :value="spec.id"
                :disabled="!spec.enabled">
                {{ spec.name }} ({{ spec.code }})
              </a-select-option>
          </a-select>
        </a-form-item>
      </a-form>
    </a-modal>

    <!-- 加载状态 -->
    <div v-if="loading" class="loading-overlay">
      <a-spin tip="正在加载数据..." />
    </div>
  </div>
</template>

<script lang="ts" setup>
import { ref, computed, reactive, onMounted, nextTick, watch } from 'vue';
import { message } from 'ant-design-vue';
import {
  ReloadOutlined,
  ThunderboltOutlined
} from '@ant-design/icons-vue';
import { getProductSpecMatches, updateProductSpecMatches } from '/@/api/lab/rawData';
import { getProductSpecList } from '/@/api/lab/productSpec';
import type {
  RawDataRow,
  ProductSpec,
  Step2ProductSpecInput
} from '/@/api/lab/types/rawData';

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
const specList = ref<ProductSpec[]>([]);
const filterStatus = ref('');
const searchText = ref('');
const selectedRowKeys = ref<string[]>([]);

// 重试机制
const retryCount = ref(0);
const maxRetries = 5; // 最大重试次数
const retryDelay = 2000; // 重试延迟（毫秒）
const isProcessing = ref(false); // 数据是否正在处理中

// 弹窗状态
const specModalVisible = ref(false);
const specLoading = ref(false);
const specSearchText = ref('');
const specFilterEnabled = ref();
const currentRecord = ref<RawDataRow | null>(null);
const selectedSpecKeys = ref<string[]>([]);

// 批量匹配弹窗
const batchMatchModalVisible = ref(false);
const batchMatchForm = reactive({
  productSpecId: '',
});

// 分页配置
const paginationConfig = reactive({
  current: 1,
  pageSize: 20,
  total: 0,
  showSizeChanger: true,
  showQuickJumper: true,
  showTotal: (total: number) => `共 ${total} 条`,
});

// 规格列表分页
const specPagination = reactive({
  current: 1,
  pageSize: 10,
  total: 0,
  showSizeChanger: true,
  showQuickJumper: true,
});

// 计算属性
const totalRows = computed(() => data.value.length);
const matchedCount = computed(() => data.value.filter(row => row.productSpecId).length);
const unmatchedCount = computed(() => data.value.filter(row => !row.productSpecId).length);
const matchRate = computed(() => totalRows.value > 0 ? Math.round((matchedCount.value / totalRows.value) * 100) : 0);
const hasUnmatchedData = computed(() => unmatchedCount.value > 0);

const displayData = computed(() => {
  // 确保 data.value 是数组
  if (!Array.isArray(data.value)) {
    return [];
  }

  let filtered = data.value;

  // 状态筛选
  if (filterStatus.value === 'matched') {
    filtered = filtered.filter(row => row.productSpecId);
  } else if (filterStatus.value === 'unmatched') {
    filtered = filtered.filter(row => !row.productSpecId);
  }

  // 搜索筛选
  if (searchText.value) {
    const search = searchText.value.toLowerCase();
    filtered = filtered.filter(row =>
      row.furnaceNo?.toLowerCase().includes(search) ||
      (row.productSpecName && row.productSpecName.toLowerCase().includes(search)) ||
      (row.productSpecCode && row.productSpecCode.toLowerCase().includes(search))
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
    title: '生产日期',
    dataIndex: 'prodDate',
    width: 120,
  },
  {
    title: '炉号',
    dataIndex: 'furnaceNo',
    width: 180,
  },
  {
    title: '宽度',
    dataIndex: 'width',
    width: 100,
    align: 'center',
  },
  {
    title: '带材重量',
    dataIndex: 'coilWeight',
    width: 120,
    align: 'center',
  },
  {
    title: '断头数',
    dataIndex: 'breakCount',
    width: 100,
    align: 'center',
  },
  {
    title: '单卷重量',
    dataIndex: 'singleCoilWeight',
    width: 120,
    align: 'center',
  },
  {
    title: '检测列数',
    dataIndex: 'detectionColumns',
    width: 100,
    align: 'center',
  },
  {
    title: '产品规格',
    dataIndex: 'productSpec',
    width: 200,
  },
];

// 规格表格列配置
const specColumns = [
  {
    title: '规格编码',
    dataIndex: 'code',
    width: 120,
  },
  {
    title: '规格名称',
    dataIndex: 'name',
    width: 200,
  },
  {
    title: '检测列数',
    dataIndex: 'detectionColumns',
    width: 100,
    align: 'center',
  },
  {
    title: '状态',
    dataIndex: 'enabled',
    width: 80,
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

// 规格表格行选择
const specRowSelection = computed(() => ({
  type: 'radio' as const,
  selectedRowKeys: selectedSpecKeys.value,
  onChange: (keys: string[]) => {
    selectedSpecKeys.value = keys;
    if (keys.length > 0) {
      const spec = specList.value.find(s => s.id === keys[0]);
      if (spec && currentRecord.value) {
        currentRecord.value.productSpecId = spec.id;
        currentRecord.value.productSpecCode = spec.code;
        currentRecord.value.productSpecName = spec.name;
        currentRecord.value.detectionColumns = spec.detectionColumns;
      }
    }
  },
}));

// 方法
async function loadData(isRetry = false) {
  console.log('Step2ProductSpec: loadData 被调用', { isRetry, importSessionId: props.importSessionId });
  
  // 如果是重试，更新重试计数
  if (isRetry) {
    retryCount.value++;
  } else {
    // 新加载时重置计数
    retryCount.value = 0;
    isProcessing.value = true;
  }

  loading.value = true;
  try {
    console.log('Step2ProductSpec: 调用 getProductSpecMatches API');
    const response = await getProductSpecMatches(props.importSessionId);
    console.log('Step2ProductSpec: getProductSpecMatches 返回:', response);

    // 确保返回的是数组
    let result = response;
    if (response && typeof response === 'object' && !Array.isArray(response)) {
      // 如果返回的是对象，尝试获取 data 或 list 属性
      result = response.data || response.list || response.items || [];
    }

    // 确保是数组
    if (!Array.isArray(result)) {
      result = [];
    }

    // 检查是否为空数组（可能表示数据还在处理中）
    if (result.length === 0 && retryCount.value < maxRetries) {
      // 数据可能还在处理中，延迟重试
      console.log(`数据可能还在处理中，第${retryCount.value + 1}次重试，${retryDelay}ms后重试...`);

      // 显示处理中的提示
      isProcessing.value = true;

      // 延迟重试
      setTimeout(() => {
        loadData(true);
      }, retryDelay);

      // 不清空现有数据，保持loading状态
      return;
    }

    // 如果达到最大重试次数或已有数据，更新UI
    isProcessing.value = false;

    // 转换数据格式：后端返回的是 RawDataProductSpecMatchOutput，需要转换为 RawDataRow
    data.value = result.map((item: any) => ({
      id: item.rawDataId || item.id || item.RawDataId,
      furnaceNo: item.furnaceNo || item.FurnaceNo,
      prodDate: item.prodDate || item.ProdDate,
      width: item.width || item.Width,
      coilWeight: item.coilWeight || item.CoilWeight,
      breakCount: item.breakCount || item.BreakCount,
      singleCoilWeight: item.singleCoilWeight || item.SingleCoilWeight,
      detectionColumns: item.detectionColumns || item.DetectionColumns,
      productSpecId: item.productSpecId || item.ProductSpecId,
      productSpecName: item.productSpecName || item.ProductSpecName,
      productSpecCode: item.productSpecCode || item.ProductSpecCode,
      productSpecMatchStatus: item.matchStatus || item.MatchStatus,
      rowIndex: 0, // 将在下面设置
    }));

    // 添加行号
    data.value.forEach((row, index) => {
      row.rowIndex = index + 1;
    });

    // 设置选中行
    selectedRowKeys.value = data.value
      .filter(row => !row.productSpecId)
      .map(row => row.id!)
      .filter(id => id); // 过滤掉 undefined/null

    // 标记为已加载
    hasLoaded.value = true;
  } catch (error) {
    console.error('加载数据失败:', error);

    // 检查是否是解析文件不存在的错误（可能数据还在处理中）
    const errorMsg = error?.message || '';
    if (errorMsg.includes('解析数据文件不存在') && retryCount.value < maxRetries) {
      // 数据可能还在处理中，延迟重试
      console.log(`数据可能还在处理中（服务端错误），第${retryCount.value + 1}次重试，${retryDelay}ms后重试...`);

      // 显示处理中的提示
      isProcessing.value = true;

      // 延迟重试
      setTimeout(() => {
        loadData(true);
      }, retryDelay);

      // 不清空现有数据，保持loading状态
      return;
    }

    // 其他错误或达到最大重试次数
    message.error('加载数据失败');
    data.value = []; // 确保失败时也是空数组
    // 加载失败时不标记为已加载，允许重试
    hasLoaded.value = false;
    isProcessing.value = false;
  } finally {
    // 只在不是重试或重试完成时才停止loading
    if (!isRetry || retryCount.value >= maxRetries || data.value.length > 0) {
      loading.value = false;
    }
  }
}

async function loadProductSpecs() {
  // 如果正在加载，避免重复请求
  if (specLoading.value) {
    return;
  }

  specLoading.value = true;
  try {
    const response = await getProductSpecList({
      page: specPagination.current,
      pageSize: specPagination.pageSize,
      keyword: specSearchText.value,
      enabled: specFilterEnabled.value,
    });
    specList.value = response.list || [];
    specPagination.total = response.total || 0;
  } catch (error) {
    console.error('加载产品规格失败:', error);
    message.error('加载产品规格失败');
    specList.value = [];
    specPagination.total = 0;
  } finally {
    specLoading.value = false;
  }
}

function handleRefresh() {
  hasLoaded.value = false; // 重置加载标记，允许重新加载
  retryCount.value = 0; // 重置重试计数器
  isProcessing.value = false; // 重置处理状态
  loadData();
}

function handleBatchMatch() {
  if (selectedRowKeys.value.length === 0) {
    message.warning('请先选择要批量匹配的数据');
    return;
  }
  batchMatchModalVisible.value = true;
}

function handleFilterChange() {
  paginationConfig.current = 1;
}

function handleSearch() {
  paginationConfig.current = 1;
}

async function handleSelectSpec(record: RawDataRow) {
  if (!record) {
    console.error('record is null or undefined');
    return;
  }

  try {
    // 先设置当前记录
    currentRecord.value = record;

    // 如果记录已有产品规格，预选中该规格
    if (record.productSpecId) {
      selectedSpecKeys.value = [record.productSpecId];
    } else {
      selectedSpecKeys.value = [];
    }

    // 先打开弹窗，让用户立即看到反馈
    specModalVisible.value = true;

    // 等待弹窗渲染完成后再加载数据，避免阻塞UI
    await nextTick();

    // 如果规格列表为空，才加载数据（避免重复加载）
    if (specList.value.length === 0) {
      // 异步加载产品规格列表，不阻塞弹窗显示
      loadProductSpecs().catch(error => {
        console.error('加载产品规格失败:', error);
      });
    } else {
      // 如果已有数据，确保选中状态正确
      await nextTick();
    }
  } catch (error) {
    console.error('Error in handleSelectSpec:', error);
    message.error('打开规格选择弹窗失败');
    specModalVisible.value = false;
  }
}

function handleClearSpec(record: RawDataRow) {
  record.productSpecId = '';
  record.productSpecCode = '';
  record.productSpecName = '';
  record.detectionColumns = '';
}

function handleSpecSearch() {
  specPagination.current = 1;
  loadProductSpecs();
}

function handleSpecFilterChange() {
  specPagination.current = 1;
  loadProductSpecs();
}

function handleSpecTableChange(pagination: any) {
  specPagination.current = pagination.current;
  specPagination.pageSize = pagination.pageSize;
  loadProductSpecs();
}

async function handleSpecModalOk() {
  if (!currentRecord.value?.productSpecId) {
    message.warning('请选择产品规格');
    return;
  }
  specModalVisible.value = false;
  currentRecord.value = null;
  selectedSpecKeys.value = [];
}

function handleSpecModalCancel() {
  specModalVisible.value = false;
  currentRecord.value = null;
  selectedSpecKeys.value = [];
}

async function handleBatchMatchOk() {
  if (!batchMatchForm.productSpecId) {
    message.warning('请选择产品规格');
    return;
  }

  const spec = specList.value.find(s => s.id === batchMatchForm.productSpecId);
  if (!spec) return;

  const updateData: Step2ProductSpecInput = {
    importSessionId: props.importSessionId,
    matches: selectedRowKeys.value.map(id => ({
      rowId: id,
      productSpecId: spec.id,
    })),
  };

  await updateProductSpecMatches(props.importSessionId, updateData);

  // 更新本地数据
  data.value.forEach(row => {
    if (row.id && selectedRowKeys.value.includes(row.id)) {
      row.productSpecId = spec.id;
      row.productSpecCode = spec.code;
      row.productSpecName = spec.name;
      row.detectionColumns = spec.detectionColumns;
    }
  });

  message.success('批量匹配成功');
  batchMatchModalVisible.value = false;
}

function handleBatchMatchCancel() {
  batchMatchModalVisible.value = false;
}

function filterProductSpecOption(input: string, option: any) {
  const text = option.children?.[0]?.text || '';
  return text.toLowerCase().includes(input.toLowerCase());
}

// 格式化日期
function formatDate(date: string | number | Date | undefined) {
  if (!date) return '-';
  try {
    let d: Date;
    // 处理时间戳（可能是字符串或数字）
    if (typeof date === 'number' || (typeof date === 'string' && /^\d+$/.test(date))) {
      const timestamp = typeof date === 'string' ? parseInt(date, 10) : date;
      // 判断是秒级还是毫秒级时间戳
      d = new Date(timestamp > 1000000000000 ? timestamp : timestamp * 1000);
    } else if (date instanceof Date) {
      d = date;
    } else {
      d = new Date(date);
    }

    if (isNaN(d.getTime())) return String(date);

    const year = d.getFullYear();
    const month = d.getMonth() + 1;
    const day = d.getDate();
    // 格式: YYYY/M/D
    return `${year}/${month}/${day}`;
  } catch {
    return String(date);
  }
}

async function handlePrev() {
  emit('prev');
}

async function handleNext() {
  if (unmatchedCount.value > 0) {
    message.warning(`还有 ${unmatchedCount.value} 行数据未匹配产品规格，请先处理`);
    return;
  }

  try {
    // 保存当前匹配结果
    const updateData: Step2ProductSpecInput = {
      importSessionId: props.importSessionId,
      matches: data.value.map(row => ({
        rowId: row.id,
        productSpecId: row.productSpecId || '',
      })),
    };

    await updateProductSpecMatches(props.importSessionId, updateData);
    emit('next');
  } catch (error) {
    message.error('保存失败，请重试');
  }
}

function handleCancel() {
  emit('cancel');
}

// 计算是否可以进入下一步
const canGoNext = computed(() => {
  // 所有数据都已匹配产品规格
  return data.value.length > 0 && unmatchedCount.value === 0;
});

// 保存并进入下一步（供父组件调用）
async function saveAndNext() {
  await handleNext();
}

// 标记是否已经加载过数据（避免重复加载）
const hasLoaded = ref(false);

// 生命周期
onMounted(() => {
  // 只在激活步骤且允许自动加载时加载
  if (props.active && !props.skipAutoLoad && props.importSessionId && !hasLoaded.value) {
    loadData();
  }
});

// 监听 importSessionId 变化，当从空变为有值时自动加载数据
watch(
  () => [props.importSessionId, props.active, props.skipAutoLoad],
  ([newId, isActive, skip], [oldId]) => {
    if (skip) {
      return;
    }
    // 当 importSessionId 从空变为有值，且处于激活步骤时，触发加载
    if (isActive && newId && !oldId && !hasLoaded.value) {
      loadData();
    }
  },
  { immediate: false }
);

// 添加一个方法来手动触发加载（供父组件调用）
function triggerLoad() {
  console.log('Step2ProductSpec: triggerLoad 被调用');
  if (props.skipAutoLoad) {
    return;
  }
  if (props.active && props.importSessionId && props.importSessionId.trim() !== '') {
    hasLoaded.value = false; // 重置加载标记，允许重新加载
    retryCount.value = 0; // 重置重试计数器
    isProcessing.value = false; // 重置处理状态
    loadData();
  } else {
    console.warn('Step2ProductSpec: triggerLoad 被调用，但 importSessionId 为空');
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
.step2-container {
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

  .product-spec-cell {
    .matched-spec {
      .spec-name {
        font-weight: 500;
        margin-bottom: 4px;
      }
      .spec-code {
        font-size: 12px;
        color: #8c8c8c;
      }
    }
  }

  .detection-info {
    text-align: center;
  }

  // 确保操作按钮可以正常点击
  :deep(.ant-table-cell-fix-right) {
    position: relative;
    z-index: 10;

    .ant-space {
      pointer-events: auto;
      position: relative;
      z-index: 11;

      .ant-btn {
        pointer-events: auto;
        cursor: pointer;
        position: relative;
        z-index: 12;
        user-select: none;

        &:hover {
          z-index: 13;
        }
      }
    }
  }

  // 确保固定列不会遮挡按钮
  :deep(.ant-table-cell-fix-right-first) {
    z-index: 10;
  }
}

.spec-selection-content {
  .spec-filter {
    margin-bottom: 16px;
  }
}

:deep(.ant-table-row:hover) {
  .product-spec-cell {
    .unmatched-spec {
      color: #1890ff;
    }
  }
}
</style>
