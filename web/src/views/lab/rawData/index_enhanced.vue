<template>
  <div class="raw-data-container">
    <!-- 页面标题 -->
    <div class="page-header">
      <a-page-header
        title="原始数据管理"
        sub-title="管理和导入实验室原始检测数据">
        <template #extra>
          <a-space>
            <a-button @click="handleRefresh">
              <template #icon><ReloadOutlined /></template>
              刷新
            </a-button>
            <a-button type="primary" @click="handleNewImport">
              <template #icon><UploadOutlined /></template>
              新建导入
            </a-button>
          </a-space>
        </template>
      </a-page-header>
    </div>

    <!-- 统计卡片 -->
    <div class="stats-section">
      <a-row :gutter="16">
        <a-col :span="6">
          <a-statistic-card
            title="总数据量"
            :value="stats.totalDataCount"
            :loading="loading"
            suffix="条">
            <template #prefix><DatabaseOutlined /></template>
          </a-statistic-card>
        </a-col>
        <a-col :span="6">
          <a-statistic-card
            title="本月导入"
            :value="stats.monthlyImportCount"
            :loading="loading"
            suffix="次">
            <template #prefix><ImportOutlined /></template>
          </a-statistic-card>
        </a-col>
        <a-col :span="6">
          <a-statistic-card
            title="成功导入"
            :value="stats.successImportCount"
            :loading="loading"
            suffix="次"
            :value-style="{ color: '#3f8600' }">
            <template #prefix><CheckCircleOutlined /></template>
          </a-statistic-card>
        </a-col>
        <a-col :span="6">
          <a-statistic-card
            title="失败导入"
            :value="stats.failedImportCount"
            :loading="loading"
            suffix="次"
            :value-style="{ color: '#cf1322' }">
            <template #prefix><CloseCircleOutlined /></template>
          </a-statistic-card>
        </a-col>
      </a-row>
    </div>

    <!-- 主内容区域 -->
    <div class="main-content">
      <a-tabs v-model:activeKey="activeTab" type="card" @change="handleTabChange">
        <!-- 数据管理页签 -->
        <a-tab-pane key="data" tab="数据管理">
          <!-- 筛选条件 -->
          <div class="filter-section">
            <a-card size="small">
              <a-form layout="inline">
                <a-form-item label="生产日期">
                  <a-range-picker
                    v-model:value="filterDateRange"
                    format="YYYY-MM-DD"
                    placeholder="选择日期范围"
                    @change="handleDataFilterChange" />
                </a-form-item>

                <a-form-item label="炉号">
                  <a-input-search
                    v-model:value="filterFurnaceNo"
                    placeholder="输入炉号"
                    style="width: 200px"
                    @search="handleDataFilterChange"
                    allow-clear />
                </a-form-item>

                <a-form-item label="产品规格">
                  <a-select
                    v-model:value="filterProductSpec"
                    placeholder="选择产品规格"
                    style="width: 200px"
                    allow-clear
                    @change="handleDataFilterChange">
                    <a-select-option value="">全部</a-select-option>
                    <a-select-option
                      v-for="spec in productSpecOptions"
                      :key="spec.id"
                      :value="spec.id">
                      {{ spec.name }}
                    </a-select-option>
                  </a-select>
                </a-form-item>

                <a-form-item>
                  <a-space>
                    <a-button @click="handleResetDataFilter">重置</a-button>
                    <a-button type="primary" @click="handleDataFilterChange">查询</a-button>
                  </a-space>
                </a-form-item>
              </a-form>
            </a-card>
          </div>

          <!-- 数据表格 -->
          <div class="data-table">
            <a-card>
              <template #extra>
                <a-space>
                  <a-button @click="handleExportData">
                    <template #icon><DownloadOutlined /></template>
                    导出数据
                  </a-button>
                  <a-button @click="handleBatchDeleteData" :disabled="!selectedDataRowKeys.length" danger>
                    <template #icon><DeleteOutlined /></template>
                    批量删除
                  </a-button>
                </a-space>
              </template>

              <!-- 自定义排序控制 -->
              <div class="table-toolbar">
                <CustomSortControl
                  v-model="sortRules"
                  @change="handleSortChange" />
              </div>

              <BasicTable
                @register="registerTable"
                :loading="loading"
                :row-selection="dataRowSelection">
                <template #bodyCell="{ column, record }">
                  <template v-if="column.key === 'productSpecName'">
                    <a-button type="link" @click="handleViewProductSpec(record)" v-if="record.productSpecId">
                      {{ record.productSpecName || '-' }}
                    </a-button>
                    <span v-else>-</span>
                  </template>
                  <template v-else-if="column.key === 'detectionColumns'">
                    <a-tooltip v-if="record.detectionColumns">
                      <template #title>
                        检测列号：{{ record.detectionColumns }}
                      </template>
                      <a-tag color="blue">{{ record.detectionColumns.split(',').length }}列</a-tag>
                    </a-tooltip>
                    <span v-else>-</span>
                  </template>
                  <template v-else-if="column.key?.startsWith('detection')">
                    <span class="detection-value">{{ record[column.key] ?? '-' }}</span>
                  </template>
                  <template v-else-if="column.key === 'creatorTime'">
                    {{ record[column.key] ? formatToDateTime(record[column.key]) : '-' }}
                  </template>
                </template>
              </BasicTable>
            </a-card>
          </div>
        </a-tab-pane>

        <!-- 导入历史页签 -->
        <a-tab-pane key="history" tab="导入历史">
          <ImportHistory @new-import="handleNewImport" />
        </a-tab-pane>

        <!-- 导入向导页签 -->
        <a-tab-pane key="import" tab="导入向导">
          <div class="import-wizard-content">
            <!-- 导入方式选择 -->
            <div class="import-method-selector">
              <a-alert
                message="选择导入方式"
                description="请选择适合您的导入方式"
                type="info"
                show-icon
                style="margin-bottom: 24px" />

              <a-row :gutter="[16, 16]">
                <a-col :xs="24" :sm="12" :lg="8">
                  <a-card
                    :hoverable="true"
                    class="import-method-card"
                    @click="handleQuickImport"
                    :body-style="{ padding: '24px' }">
                    <template #title>
                      <div class="method-title">
                        <RocketOutlined style="color: #1890ff; margin-right: 8px" />
                        快速导入
                      </div>
                    </template>
                    <div class="method-content">
                      <div class="method-description">
                        一键上传并导入Excel文件，适合熟悉数据格式的用户
                      </div>
                      <div class="method-features">
                        <div class="feature-item">
                          <CheckCircleOutlined style="color: #52c41a; margin-right: 4px" />
                          自动解析炉号和检测数据
                        </div>
                        <div class="feature-item">
                          <CheckCircleOutlined style="color: #52c41a; margin-right: 4px" />
                          增量导入，自动跳过已导入行
                        </div>
                        <div class="feature-item">
                          <CheckCircleOutlined style="color: #52c41a; margin-right: 4px" />
                          自动生成错误报告
                        </div>
                      </div>
                    </div>
                  </a-card>
                </a-col>

                <a-col :xs="24" :sm="12" :lg="8">
                  <a-card
                    :hoverable="true"
                    class="import-method-card"
                    @click="handleStepImport"
                    :body-style="{ padding: '24px' }">
                    <template #title>
                      <div class="method-title">
                        <SolutionOutlined style="color: #722ed1; margin-right: 8px" />
                        分步导入向导
                      </div>
                    </template>
                    <div class="method-content">
                      <div class="method-description">
                        4步向导式导入，每步都可查看和修改数据，适合复杂数据
                      </div>
                      <div class="method-features">
                        <div class="feature-item">
                          <CheckCircleOutlined style="color: #52c41a; margin-right: 4px" />
                          分步预览和修改数据
                        </div>
                        <div class="feature-item">
                          <CheckCircleOutlined style="color: #52c41a; margin-right: 4px" />
                          手动匹配产品规格和特性
                        </div>
                        <div class="feature-item">
                          <CheckCircleOutlined style="color: #52c41a; margin-right: 4px" />
                          支持导入会话恢复
                        </div>
                      </div>
                    </div>
                  </a-card>
                </a-col>

                <a-col :xs="24" :sm="12" :lg="8">
                  <a-card
                    :hoverable="true"
                    class="import-method-card"
                    @click="handleImportHistory"
                    :body-style="{ padding: '24px' }">
                    <template #title>
                      <div class="method-title">
                        <HistoryOutlined style="color: #13c2c2; margin-right: 8px" />
                        导入历史
                      </div>
                    </template>
                    <div class="method-content">
                      <div class="method-description">
                        查看历史导入记录，支持继续未完成的导入任务
                      </div>
                      <div class="method-features">
                        <div class="feature-item">
                          <CheckCircleOutlined style="color: #52c41a; margin-right: 4px" />
                          查看所有导入记录
                        </div>
                        <div class="feature-item">
                          <CheckCircleOutlined style="color: #52c41a; margin-right: 4px" />
                          继续未完成的导入
                        </div>
                        <div class="feature-item">
                          <CheckCircleOutlined style="color: #52c41a; margin-right: 4px" />
                          下载错误报告
                        </div>
                      </div>
                    </div>
                  </a-card>
                </a-col>
              </a-row>
            </div>

            <!-- 最近导入记录 -->
            <div class="recent-imports">
              <a-card title="最近导入记录" size="small">
                <template #extra>
                  <a-button type="link" @click="activeTab = 'history'">查看全部</a-button>
                </template>
                <BasicTable
                  @register="registerRecentTable"
                  :pagination="false"
                  size="middle" />
              </a-card>
            </div>
          </div>
        </a-tab-pane>
      </a-tabs>
    </div>

    <!-- 模态框 -->
    <ProductSpecModal @register="registerProductSpecModal" />
    <StepImportWizard
      @register="registerStepImportModal"
      @success="handleStepImportSuccess"
      @cancel="handleStepImportCancel" />

    <!-- 加载状态 -->
    <div v-if="loading" class="loading-overlay">
      <a-spin size="large" tip="正在加载数据..." />
    </div>
  </div>
</template>

<script lang="ts" setup>
import { ref, reactive, onMounted } from 'vue';
import { message } from 'ant-design-vue';
import { useI18n } from '/@/hooks/web/useI18n';
import { useMessage } from '/@/hooks/web/useMessage';
import { useModal } from '/@/components/Modal';
import { BasicTable, useTable } from '/@/components/Table';
import { formatToDateTime } from '/@/utils/dateUtil';
import {
  getRawDataList,
  getImportHistoryList,
  getProductSpecList,
  deleteRawData
} from '/@/api/lab/rawData';
import ProductSpecModal from './components/ProductSpecModal.vue';
import StepImportWizard from './components/StepImportWizard.vue';
import CustomSortControl from './components/CustomSortControl.vue';
import ImportHistory from './ImportHistory.vue';
import type { ImportHistory as ImportHistoryType } from '/@/api/lab/types/rawData';

// 图标导入
import {
  ReloadOutlined,
  UploadOutlined,
  DatabaseOutlined,
  ImportOutlined,
  CheckCircleOutlined,
  CloseCircleOutlined,
  RocketOutlined,
  SolutionOutlined,
  HistoryOutlined,
  EyeOutlined,
  DownloadOutlined,
  DeleteOutlined
} from '@ant-design/icons-vue';

const { t } = useI18n();
const { createMessage } = useMessage();

// 状态
const loading = ref(false);
const activeTab = ref('data');
const selectedDataRowKeys = ref<string[]>([]);

// 筛选条件
const filterDateRange = ref<any>(null);
const filterFurnaceNo = ref<string>('');
const filterProductSpec = ref<string>('');
const productSpecOptions = ref<any[]>([]);

// 排序规则
const sortRules = ref([
  { field: 'prodDate', order: 'desc' as 'asc' | 'desc' },
  { field: 'furnaceNo', order: 'asc' as 'asc' | 'desc' }
]);

// 统计信息
const stats = reactive({
  totalDataCount: 0,
  monthlyImportCount: 0,
  successImportCount: 0,
  failedImportCount: 0
});

// 表格列配置
const dataColumns = [
  { title: '检测日期', dataIndex: 'prodDate', width: 120, fixed: 'left' },
  { title: '炉号', dataIndex: 'furnaceNo', width: 150, fixed: 'left' },
  { title: '产线', dataIndex: 'lineNo', width: 80, align: 'center' },
  { title: '班次', dataIndex: 'shift', width: 80, align: 'center' },
  { title: '宽度', dataIndex: 'width', width: 100, align: 'right' },
  { title: '带材重量', dataIndex: 'coilWeight', width: 120, align: 'right' },
  { title: '检测列数', dataIndex: 'detectionColumns', width: 100, align: 'center' },
  { title: '产品规格', dataIndex: 'productSpecName', width: 150 },
  { title: '创建时间', dataIndex: 'createTime', width: 160 },
  { title: '操作', key: 'actions', width: 120, fixed: 'right' }
];

// 模态框注册
const [registerProductSpecModal, { openModal: openProductSpecModal }] = useModal();
const [registerStepImportModal, { openModal: openStepImportModal }] = useModal();

// 表格注册
const [registerTable, { reload: reloadTable }] = useTable({
  title: '原始数据列表',
  api: getRawDataList,
  columns: dataColumns,
  useSearchForm: false,
  showTableSetting: true,
  showIndexColumn: true,
  rowKey: 'id',
  pagination: {
    pageSize: 20,
    showSizeChanger: true,
    showQuickJumper: true
  },
  beforeFetch: (params) => {
    // 应用筛选条件
    if (filterDateRange.value?.length === 2) {
      params.startDate = filterDateRange.value[0].format('YYYY-MM-DD');
      params.endDate = filterDateRange.value[1].format('YYYY-MM-DD');
    }
    if (filterFurnaceNo.value) {
      params.furnaceNo = filterFurnaceNo.value;
    }
    if (filterProductSpec.value) {
      params.productSpecId = filterProductSpec.value;
    }
    // 应用排序规则
    if (sortRules.value.length > 0) {
      params.sortRules = sortRules.value;
    }
    return params;
  },
  afterFetch: (data) => {
    stats.totalDataCount = data.total || 0;
    return data;
  }
});

const [registerRecentTable] = useTable({
  api: () => getImportHistoryList({ page: 1, pageSize: 10 }),
  columns: [
    { title: '文件名', dataIndex: 'fileName', width: 200 },
    { title: '导入策略', dataIndex: 'importStrategy', width: 100 },
    { title: '状态', dataIndex: 'status', width: 100 },
    { title: '数据量', dataIndex: 'totalRows', width: 100 },
    { title: '导入时间', dataIndex: 'createTime', width: 160 },
    { title: '操作人', dataIndex: 'creatorName', width: 120 }
  ],
  useSearchForm: false,
  showIndexColumn: false,
  pagination: false,
  rowKey: 'id'
});

// 行选择配置
const dataRowSelection = computed(() => ({
  selectedRowKeys: selectedDataRowKeys.value,
  onChange: (keys: string[]) => {
    selectedDataRowKeys.value = keys;
  }
}));

// 方法
async function loadData() {
  loading.value = true;
  try {
    // 加载产品规格选项
    const specResponse = await getProductSpecList({ enabled: true });
    productSpecOptions.value = specResponse.list || [];

    // 加载统计数据
    await loadStatistics();
  } catch (error) {
    createMessage.error('加载数据失败');
  } finally {
    loading.value = false;
  }
}

async function loadStatistics() {
  try {
    // 获取最近30天的导入统计
    const today = new Date();
    const thirtyDaysAgo = new Date(today.getTime() - 30 * 24 * 60 * 60 * 1000);

    const historyResponse = await getImportHistoryList({
      startTime: thirtyDaysAgo.toISOString(),
      endTime: today.toISOString()
    });

    const history = historyResponse.list || [];

    stats.monthlyImportCount = history.length;
    stats.successImportCount = history.filter(h => h.status === 'completed').length;
    stats.failedImportCount = history.filter(h => h.status === 'failed').length;
  } catch (error) {
    console.error('加载统计失败:', error);
  }
}

// 事件处理
function handleTabChange(key: string) {
  activeTab.value = key;
  if (key === 'data') {
    reloadTable();
  }
}

function handleRefresh() {
  loadData();
  if (activeTab.value === 'data') {
    reloadTable();
  }
}

function handleNewImport() {
  activeTab.value = 'import';
}

function handleDataFilterChange() {
  reloadTable();
}

function handleResetDataFilter() {
  filterDateRange.value = null;
  filterFurnaceNo.value = '';
  filterProductSpec.value = '';
  reloadTable();
}

function handleSortChange(newSortRules: any[]) {
  sortRules.value = newSortRules;
  reloadTable();
}

function handleViewProductSpec(record: any) {
  if (record.productSpecId) {
    openProductSpecModal(true, { productSpecId: record.productSpecId });
  }
}

function handleExportData() {
  // TODO: 实现数据导出功能
  createMessage.info('数据导出功能开发中...');
}

function handleBatchDeleteData() {
  if (selectedDataRowKeys.value.length === 0) {
    createMessage.warning('请先选择要删除的数据');
    return;
  }
  // TODO: 实现批量删除功能
  createMessage.info(`即将删除 ${selectedDataRowKeys.value.length} 条数据`);
}

function handleQuickImport() {
  // 打开快速导入模态框
  // TODO: 实现快速导入功能
  createMessage.info('快速导入功能开发中...');
}

function handleStepImport() {
  // 打开分步导入向导
  openStepImportModal(true);
}

function handleImportHistory() {
  activeTab.value = 'history';
}

function handleStepImportSuccess() {
  createMessage.success('导入成功');
  loadData();
  activeTab.value = 'history';
}

function handleStepImportCancel() {
  createMessage.info('导入已取消');
}

// 生命周期
onMounted(() => {
  loadData();
});
</script>

<style lang="less" scoped>
.raw-data-container {
  padding: 16px;
}

.page-header {
  margin-bottom: 16px;
}

.stats-section {
  margin-bottom: 24px;
}

.main-content {
  .filter-section {
    margin-bottom: 16px;
  }

  .data-table {
    .table-toolbar {
      margin-bottom: 16px;
    }
  }

  .import-wizard-content {
    .import-method-selector {
      margin-bottom: 32px;

      .import-method-card {
        cursor: pointer;
        transition: all 0.3s;

        &:hover {
          transform: translateY(-2px);
          box-shadow: 0 4px 12px rgba(0, 0, 0, 0.1);
        }

        .method-title {
          font-size: 16px;
          font-weight: 500;
          display: flex;
          align-items: center;
        }

        .method-content {
          .method-description {
            margin-bottom: 16px;
            color: #666;
            line-height: 1.5;
          }

          .method-features {
            .feature-item {
              margin-bottom: 8px;
              font-size: 14px;
              display: flex;
              align-items: center;
            }
          }
        }
      }
    }

    .recent-imports {
      margin-top: 32px;
    }
  }

  .detection-value {
    font-family: 'Courier New', monospace;
    font-size: 12px;
  }
}

.loading-overlay {
  position: fixed;
  top: 0;
  left: 0;
  right: 0;
  bottom: 0;
  background: rgba(255, 255, 255, 0.8);
  display: flex;
  align-items: center;
  justify-content: center;
  z-index: 1000;
}
</style>