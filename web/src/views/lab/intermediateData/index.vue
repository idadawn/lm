<template>
  <div class="page-content-wrapper">
    <div class="page-content-wrapper-center">
      <div class="page-content-wrapper-content">
        <!-- 自定义排序控制 -->
        <div class="table-toolbar">
          <CustomSortControl v-model="sortRules" @change="handleSortChange" />
          <div class="color-fill-control">
            <span class="color-label">填充颜色:</span>
            <div class="color-palette">
              <div v-for="color in standardColors" :key="color" class="color-option" :style="{ backgroundColor: color }"
                :class="{ active: selectedColor === color }" @click="selectColor(color)" :title="color"></div>
            </div>
            <a-radio-group v-model:value="fillMode" size="small" button-style="solid">
              <a-radio-button value="cell">单元格</a-radio-button>
              <a-radio-button value="row">整行</a-radio-button>
              <a-radio-button value="column">整列</a-radio-button>
            </a-radio-group>
            <a-button size="small" @click="clearSelectedColor" :type="isClearMode ? 'primary' : 'default'"
              :danger="isClearMode">
              {{ isClearMode ? '清除模式' : '清除' }}
            </a-button>
            <a-button size="small" type="primary" @click="saveColorsBatch" :loading="savingColors">保存配置</a-button>
          </div>
        </div>

        <!-- 主表格 -->
        <div class="table-container">
          <BasicTable @register="registerTable">

            <template #tableTitle>
              <div class="spec-tabs-wrap">
                <a-tabs v-model:activeKey="selectedProductSpecId" type="card" size="small" class="spec-tabs">
                  <a-tab-pane v-for="spec in productSpecOptions" :key="spec.id" :tab="spec.name" />
                </a-tabs>
              </div>
              <!-- 数据统计 -->
              <div class="data-statistics">
                <span v-if="currentPagination.total > 0" class="data-count">
                  共 {{ currentPagination.total }} 条，
                  第 {{ currentPagination.current }} / {{ Math.ceil(currentPagination.total / currentPagination.pageSize)
                  }} 页
                </span>
              </div>
              <a-space>
                <a-upload :before-upload="handleQuickImport" :show-upload-list="false" accept=".xlsx,.xls">
                  <a-button>
                    <UploadOutlined /> 磁性数据导入
                  </a-button>
                </a-upload>
                <a-button type="primary" :loading="batchCalculating" @click="handleBatchRecalculate">
                  <ReloadOutlined v-if="!batchCalculating" /> 批量计算
                </a-button>
                <a-button type="primary" color="success" :loading="batchJudging" @click="handleBatchJudge">
                  <CheckCircleOutlined v-if="!batchJudging" /> 批量判定
                </a-button>
                <a-dropdown>
                  <a-button :loading="exporting">
                    <DownloadOutlined v-if="!exporting" /> 导出Excel
                  </a-button>
                  <template #overlay>
                    <a-menu @click="handleQuickExport">
                      <a-menu-item key="thisMonth">导出本月</a-menu-item>
                      <a-menu-item key="thisYear">导出本年</a-menu-item>
                      <a-menu-divider />
                      <a-menu-item key="custom">自定义导出...</a-menu-item>
                    </a-menu>
                  </template>
                </a-dropdown>
              </a-space>
            </template>
            <template #bodyCell="{ column, record, text }">

              <!-- 贴标列 -->
              <LabelingCell v-if="column.key === 'labeling'" :record="record" :text="text"
                :cell-class="getCellClass(record.id, column.key)" @click="handleCellColor(record.id, column.key)" />

              <!-- 日期列 (dateMonth - 可编辑) -->
              <template v-else-if="column.key === 'dateMonth'">
                <div :class="['cell-content', getCellClass(record.id, column.key)]"
                  @click="handleCellColor(record.id, column.key)">
                  <a-input v-if="editingCell?.id === record.id && editingCell?.field === 'dateMonth'"
                    v-model:value="editingValue" size="small" style="width: 100px" @blur="handleCellBlur"
                    @press-enter="handleCellSave" />
                  <span v-else @dblclick="handleCellEdit(record, 'dateMonth', record.dateMonth)" class="editable-cell">
                    {{ record.dateMonth || record.prodDateStr || '-' }}
                  </span>
                </div>
              </template>

              <!-- 日期字符串列（检测日期、生产日期等） -->
              <DateCell v-else-if="column.key === 'detectionDateStr' || column.key === 'prodDateStr'" :record="record"
                :text="text" :cell-class="getCellClass(record.id, column.key)"
                @click="handleCellColor(record.id, column.key)" />

              <!-- 性能数据列 -->
              <PerfCell v-else-if="column.key?.startsWith('perf')" :record="record" :column="column"
                :cell-class="getCellClass(record.id, column.key)" :has-permission="hasBtnP(PERM_MAGNETIC)"
                :get-field-precision="getFieldPrecision" @click="handleCellColor(record.id, column.key)"
                @save="val => handlePerfSave(record, column.key, val)" />

              <!-- 外观特性列 -->
              <FeatureCell v-else-if="column.key === 'appearanceFeatureList'"
                :record="record" :column="column" :cell-class="getCellClass(record.id, column.key)"
                :has-permission="hasBtnP(PERM_APPEARANCE)" :get-matched-feature-labels="getMatchedFeatureLabels"
                @click="handleCellColor(record.id, column.key)" @dblclick="handleOpenFeatureDialog(record)" />

              <!-- 可编辑的测量值（中Si、中B、花纹、断头数、单卷重量等） -->
              <EditableMeasurementCell v-else-if="isEditableMeasurement(column.key)" :record="record" :column="column"
                :cell-class="getCellClass(record.id, column.key)" :has-permission="hasBtnP(PERM_APPEARANCE)"
                @click="handleCellColor(record.id, column.key)"
                @save="val => handleMeasurementSave(record, column.key, val)" />

              <template v-else-if="column.key === 'calcStatus'">
                <div class="cell-content">
                  <a-tooltip v-if="isCalcFailed(record)" :title="record.calcErrorMessage">
                    <a-tag :color="getCalcStatusInfo(record.calcStatus).color" class="cursor-pointer"
                      @click.stop="handleRecalculate(record)">
                      <template #icon>
                        <ReloadOutlined :spin="record.recalculating" />
                      </template>
                      {{ record.recalculating ? '计算中' : getCalcStatusInfo(record.calcStatus).text }}
                    </a-tag>
                  </a-tooltip>
                  <a-tag v-else :color="getCalcStatusInfo(record.calcStatus).color" class="cursor-pointer"
                    @click.stop="handleRecalculate(record)">
                    <template #icon>
                      <ReloadOutlined :spin="record.recalculating" />
                    </template>
                    {{ record.recalculating ? '计算中' : getCalcStatusInfo(record.calcStatus).text }}
                  </a-tag>
                </div>
              </template>

              <!-- 判定状态 -->
              <template v-else-if="column.key === 'judgeStatus'">
                <div class="cell-content flex items-center justify-center">
                  <a-tag :color="isNeedJudge(record) ? 'orange' : 'success'"
                    :class="isNeedJudge(record) ? 'cursor-pointer' : ''"
                    :title="isNeedJudge(record) ? '贴标为空，需执行判定' : '贴标有值，无需判定'"
                    @click.stop="isNeedJudge(record) && handleRejudge(record)">
                    <template #icon>
                      <ReloadOutlined :spin="record.rejudging" v-if="record.rejudging || isNeedJudge(record)" />
                      <CheckCircleOutlined v-else />
                    </template>
                    {{ record.rejudging ? '判定中' : (isNeedJudge(record) ? '需要判定' : '无需判定') }}
                  </a-tag>
                </div>
              </template>

              <!-- 计算日志 -->
              <template v-else-if="column.key === 'calcLog'">
                <div class="cell-content cell-content--no-pointer" @click.stop>
                  <a-button type="link" size="small" @click.stop="openCalcLogDrawer(record)">日志</a-button>
                </div>
              </template>

              <!-- 数值列 - 负数红色显示 -->
              <NumericCell v-else-if="isNumericColumn(column.key)" :record="record" :column="column" :text="text"
                :cell-class="getCellClass(record.id, column.key)" :format-numeric-value="formatNumericValue"
                @click="handleCellColor(record.id, column.key)" />

              <!-- 动态检测列 -->
              <NumericCell v-else-if="column.key?.startsWith('detection')" :record="record" :column="column"
                :text="text" :cell-class="getCellClass(record.id, column.key)"
                :format-numeric-value="formatNumericValue" @click="handleCellColor(record.id, column.key)" />

              <!-- 动态带厚列 -->
              <NumericCell v-else-if="column.key?.startsWith('thickness') && column.key !== 'thicknessRange'"
                :record="record" :column="column" :text="text" :cell-class="getCellClass(record.id, column.key)"
                :format-numeric-value="formatNumericValue" @click="handleCellColor(record.id, column.key)" />

              <!-- 其他列 -->
              <DefaultCellWithNumeric v-else :record="record" :column="column"
                :cell-class="getCellClass(record.id, column.key)" :is-numeric-string="isNumericString"
                :format-value="formatValue" @click="handleCellColor(record.id, column.key)" />
            </template>
          </BasicTable>
        </div>

      </div>
      <MagneticDataImportQuickModal @register="registerQuickModal" @reload="handleImportSuccess" />
      <FeatureSelectDialog ref="featureSelectDialogRef" @confirm="handleFeatureSelectConfirm" />

      <!-- 导出日期选择模态框 -->
      <a-modal v-model:visible="exportModalVisible" title="导出中间数据" @ok="handleExport" :confirm-loading="exporting"
        ok-text="导出" cancel-text="取消" width="560px" :body-style="{ padding: '16px 24px' }">
        <a-form layout="vertical" :style="{ marginBottom: 0 }">
          <a-form-item label="快捷选择" style="margin-bottom: 16px">
            <div class="export-date-shortcuts">
              <a-tag v-for="item in exportDateShortcuts" :key="item.key"
                :color="selectedExportShortcut === item.key ? item.activeColor : 'default'"
                :class="['export-shortcut-tag', { 'export-shortcut-tag--active': selectedExportShortcut === item.key }]"
                @click="setExportRange(item.key)">
                {{ item.label }}
              </a-tag>
            </div>
          </a-form-item>

          <a-form-item label="生产日期范围" required style="margin-bottom: 12px">
            <JnpfDateRange v-model:value="exportDateRange" :placeholder="['开始日期', '结束日期']" style="width: 100%"
              format="YYYY-MM-DD" @change="handleExportDateChange" />
          </a-form-item>

          <a-form-item v-if="exportDateRange && exportDateRange.length === 2" style="margin-bottom: 0">
            <a-alert type="info"
              :message="`将导出 ${formatExportDate(exportDateRange[0])} 至 ${formatExportDate(exportDateRange[1])} 的数据`"
              show-icon />
          </a-form-item>
        </a-form>
      </a-modal>

      <a-drawer v-model:visible="calcLogDrawerVisible" title="计算日志" width="720">
        <a-table :columns="calcLogColumns" :data-source="calcLogData" :pagination="calcLogPagination"
          :loading="calcLogLoading" row-key="id" size="small" @change="handleCalcLogTableChange" />
      </a-drawer>
    </div>
  </div>
</template>



<script lang="ts" setup>
import { ref, computed, onMounted, onUnmounted, watch, nextTick, shallowRef } from 'vue';
import { BasicTable, useTable, BasicColumn } from '/@/components/Table';
import { useMessage } from '/@/hooks/web/useMessage';
import { usePermission } from '/@/hooks/web/usePermission';
import { dateUtil } from '/@/utils/dateUtil';
import type { Dayjs } from 'dayjs';
import {
  getIntermediateDataList,
  getProductSpecOptions,
  getIntermediateDataCalcLogs,
  updatePerformance,
  updateAppearance,
  updateBaseInfo,
  recalculateIntermediateData,
  judgeIntermediateData,
  exportIntermediateData,
} from '/@/api/lab/intermediateData';
import {
  saveIntermediateDataColors,
  getIntermediateDataColors,
} from '../../../api/lab/intermediateDataColor';
import type { CellColorInfo } from '/@/api/lab/model/intermediateDataColorModel';
import { getAllAppearanceFeatureCategories, type AppearanceFeatureCategoryInfo } from '/@/api/lab/appearanceCategory';
import { getAppearanceFeatureList } from '/@/api/lab/appearanceFeature';

// 单元格渲染器组件（性能优化）
import LabelingCell from './components/cells/LabelingCell.vue';
import PerfCell from './components/cells/PerfCell.vue';
import FeatureCell from './components/cells/FeatureCell.vue';
import DateCell from './components/cells/DateCell.vue';
import EditableMeasurementCell from './components/cells/EditableMeasurementCell.vue';
import NumericCell from './components/cells/NumericCell.vue';
import DefaultCellWithNumeric from './components/cells/DefaultCellWithNumeric.vue';
import CustomSortControl from '../rawData/components/CustomSortControl.vue';
import { useFormulaPrecision } from '/@/composables/useFormulaPrecision';
import { useColorStyles } from '/@/composables/useColorStyles';

import { useModal } from '/@/components/Modal';
import { UploadOutlined, ReloadOutlined, DownloadOutlined, CheckCircleOutlined } from '@ant-design/icons-vue';
import MagneticDataImportQuickModal from '../magneticData/MagneticDataImportQuickModal.vue';
import { createMagneticImportSession, uploadAndParseMagneticData } from '/@/api/lab/magneticData';
import type { IntermediateDataCalcLogItem } from '/@/api/lab/model/intermediateDataCalcLogModel';
import FeatureSelectDialog from '../appearance/components/FeatureSelectDialog.vue';



defineOptions({ name: 'IntermediateData' });

const { createMessage } = useMessage();
const { hasBtnP } = usePermission();

// 权限标识 (对应按钮编码)
const PERM_MAGNETIC = 'btn_edit_magnetic';
const PERM_APPEARANCE = 'btn_edit_appearance';
// const PERM_FILL_COLOR = 'btn_fill_color';
// const PERM_CALC = 'btn_calc';
// const PERM_JUDGE = 'btn_judge';


// 产品规格选项
const productSpecOptions = ref<any[]>([]);
const selectedProductSpecId = ref<string>('');

const calcLogDrawerVisible = ref<boolean>(false);
const calcLogLoading = ref<boolean>(false);
const calcLogData = ref<IntermediateDataCalcLogItem[]>([]);
const calcLogPagination = ref({
  current: 1,
  pageSize: 20,
  total: 0,
  showSizeChanger: true,
});
const calcLogTargetId = ref<string>('');

const featureSelectDialogRef = ref<any>(null);
const currentEditRecordId = ref<string>('');

// 特性大类列表
const appearanceCategories = ref<AppearanceFeatureCategoryInfo[]>([]);
// 所有外观特性列表
const allFeatures = ref<any[]>([]);

// 公式精度配置
const { getFieldPrecision } = useFormulaPrecision();

// 编辑状态
const editingCell = ref<{ id: string; field: string } | null>(null);
const editingValue = ref<any>(null);

// 颜色选择状态
const selectedColor = ref<string>('');
// 使用 shallowRef 减少深度响应式开销 - 对于大量颜色数据，不需要深层响应式
const coloredCells = shallowRef<Record<string, string>>({}); // 存储单元格颜色 { 'rowId::field': 'color' }
// const colorLoaded = ref<boolean>(false); // 颜色数据是否已加载

// 使用颜色样式管理器（使用CSS类替代内联样式，大幅提升性能）
const { getCellClass } = useColorStyles({ coloredCells });

const savingColors = ref<boolean>(false); // 是否正在保存颜色
const batchCalculating = ref<boolean>(false); // 是否正在批量计算
const batchJudging = ref<boolean>(false); // 是否正在批量判定
const judgeQueuePolling = ref<boolean>(false); // 是否正在轮询判定队列
const exporting = ref<boolean>(false); // 是否正在导出
const exportModalVisible = ref<boolean>(false); // 导出模态框是否可见
const exportDateRange = ref<number[]>([]); // 导出日期范围（时间戳数组）
const selectedExportShortcut = ref<string>(''); // 当前选中的导出快捷方式
const judgeStatusFilter = ref<number>(-1); // 判定状态筛选：-1全部 0需要判定 1无需判定
const lastFetchParams = ref<Record<string, any>>({}); // 当前查询条件(不含分页)
const isClearMode = ref<boolean>(false); // 是否处于清除颜色模式
const fillMode = ref<'cell' | 'row' | 'column'>('cell'); // 填充模式：单元格/整行/整列
let judgeQueuePollTimer: ReturnType<typeof setTimeout> | null = null;

// 导出日期快捷选项配置
const exportDateShortcuts = [
  { key: 'thisWeek', label: '本周', activeColor: 'blue' },
  { key: 'thisMonth', label: '本月', activeColor: 'blue' },
  { key: 'thisQuarter', label: '本季度', activeColor: 'blue' },
  { key: 'thisYear', label: '本年', activeColor: 'green' },
  { key: 'lastWeek', label: '上周', activeColor: 'orange' },
  { key: 'lastMonth', label: '上月', activeColor: 'orange' },
  { key: 'lastQuarter', label: '上季度', activeColor: 'orange' },
  { key: 'lastYear', label: '去年', activeColor: 'orange' },
];

// 存储表格数据用于行/列填充 - 使用 shallowRef 避免深度响应式
const tableData = shallowRef<any[]>([]);

// WPS 10个标准色
const standardColors = [
  '#C00000', // 深红
  '#FF0000', // 红色
  '#FFC000', // 橙色
  '#FFFF00', // 黄色
  '#92D050', // 浅绿
  '#00B050', // 绿色
  '#00B0F0', // 天蓝
  '#0070C0', // 蓝色
  '#002060', // 深蓝
  '#7030A0', // 紫色
];

// 排序配置（支持多字段排序）
const sortRules = ref([
  { field: 'prodDate', order: 'asc' as 'asc' | 'desc' },
  { field: 'furnaceBatchNo', order: 'asc' as 'asc' | 'desc' },
  { field: 'coilNo', order: 'asc' as 'asc' | 'desc' },
  { field: 'subcoilNo', order: 'asc' as 'asc' | 'desc' },
  { field: 'lineNo', order: 'asc' as 'asc' | 'desc' }
]);

const calcStatusMap: Record<string, { text: string; color: string }> = {
  PENDING: { text: '未计算', color: 'default' },
  PROCESSING: { text: '计算中', color: 'processing' },
  SUCCESS: { text: '成功', color: 'success' },
  FAILED: { text: '失败', color: 'error' },
};

const calcStatusNumberMap: Record<number, string> = {
  0: 'PENDING',
  1: 'PROCESSING',
  2: 'SUCCESS',
  3: 'FAILED',
};

function normalizeCalcStatus(status: any) {
  if (status === null || status === undefined) {
    return 'PENDING';
  }
  if (typeof status === 'number') {
    return calcStatusNumberMap[status] || 'PENDING';
  }
  const key = String(status).toUpperCase();
  return calcStatusMap[key] ? key : 'PENDING';
}

function getCalcStatusInfo(status: any) {
  const key = normalizeCalcStatus(status);
  return calcStatusMap[key];
}


function isCalcFailed(record: any) {
  const key = normalizeCalcStatus(record?.calcStatus);
  return key === 'FAILED' && !!record?.calcErrorMessage;
}

function isNeedJudge(record: any) {
  const labelingValue = record?.labeling ?? record?.Labeling;
  if (labelingValue === null || labelingValue === undefined) {
    return true;
  }
  if (typeof labelingValue === 'string') {
    return labelingValue.trim().length === 0;
  }
  return false;
}

function formatDateTime(value: any) {
  if (!value) return '-';
  return dateUtil(value).format('YYYY-MM-DD HH:mm:ss');
}

const calcLogColumns = [
  { title: '时间', dataIndex: 'creatorTime', key: 'creatorTime', width: 160, customRender: ({ text }) => formatDateTime(text) },
  { title: '公式名称', dataIndex: 'formulaName', key: 'formulaName', width: 120, customRender: ({ record }) => record.formulaName || record.columnName },
  { title: '公式类型', dataIndex: 'formulaType', key: 'formulaType', width: 90 },
  { title: '错误类型', dataIndex: 'errorType', key: 'errorType', width: 110 },
  { title: '错误消息', dataIndex: 'errorMessage', key: 'errorMessage' },
  { title: '详情', dataIndex: 'errorDetail', key: 'errorDetail' },
];

// 加载特性列表
async function loadFeatures() {
  try {
    const featuresResponse = await getAppearanceFeatureList({ keyword: '' });
    allFeatures.value = featuresResponse.list || [];
  } catch (error) {
    console.error('加载特性数据失败', error);
  }
}

// function getFeatureName(featureId: string): string {
//   const feature = allFeatures.value.find(f => f.id === featureId);
//   return feature ? feature.name : featureId;
// }

function getMatchedFeatureLabels(record: any): Array<{ id: string; label: string }> {
  // 优先使用详细信息（如果后端返回了）
  if (record.appearanceFeatureDetails && record.appearanceFeatureDetails.length > 0) {
    return record.appearanceFeatureDetails.map((detail: any) => ({
      id: detail.featureId || detail.id,
      label: [detail.categoryName, detail.severityLevelName]
        .filter(Boolean)
        .join(' / '),
    }));
  }

  // 否则使用ID列表匹配本地加载的特性
  if (record.appearanceFeatureIds && record.appearanceFeatureIds.length > 0) {
    // 尝试解析JSON字符串
    let ids = record.appearanceFeatureIds;
    if (typeof ids === 'string') {
      try {
        ids = JSON.parse(ids);
      } catch (e) {
        ids = [];
      }
    }

    if (Array.isArray(ids)) {
      return ids.map((id: string) => {
        const feature = allFeatures.value.find(f => f.id === id);
        if (feature) {
          // 格式: 特性大类 / 特性等级
          const labelParts = [
            feature.category,     // 特性大类
            feature.severityLevel,// 特性等级
          ].filter(Boolean); // 过滤掉空值

          return {
            id,
            label: labelParts.length > 0 ? labelParts.join(' / ') : feature.name || id
          };
        }
        return {
          id,
          label: id,
        };
      });
    }
  }

  return [];
}


// 合并所有列（computed 自带缓存，依赖不变就不会重算）
const allColumns = computed(() => {
  const spec = productSpecOptions.value.find(item => item.id === selectedProductSpecId.value);
  const detectionColumns = spec?.detectionColumns || 15;

  const columns: BasicColumn[] = [
    // --- 基础信息区（固定左侧） ---
    { title: '检验日期', dataIndex: 'detectionDateStr', key: 'detectionDateStr', width: 80, fixed: 'left', align: 'center' },
    { title: '喷次', dataIndex: 'sprayNo', key: 'sprayNo', width: 120, fixed: 'left', align: 'center' },
    { title: '贴标', dataIndex: 'labeling', key: 'labeling', width: 100, fixed: 'left', align: 'center' },
    // --- 炉号与成分区（蓝色背景组） ---
    { title: '炉号', dataIndex: 'furnaceNoFormatted', key: 'furnaceNoFormatted', width: 140, fixed: 'left', align: 'center' },
    // --- 性能区 ---
    {
      title: '1.35T',
      key: 'perf_135t',
      align: 'center',
      children: [
        {
          title: 'Ss激磁功率 (VA/kg)',
          dataIndex: 'perfSsPower',
          key: 'perfSsPower',
          width: 90,
          align: 'right',
          edit: true,
          editComponent: 'InputNumber',
        }
      ],
    },
    {
      title: '50Hz',
      key: 'perf_50hz',
      align: 'center',
      children: [
        {
          title: 'Ps铁损 (W/kg)',
          dataIndex: 'perfPsLoss',
          key: 'perfPsLoss',
          width: 90,
          align: 'right',
          edit: true,
          editComponent: 'InputNumber',
        },
      ],
    },
    {
      title: 'Hc (A/m)',
      dataIndex: 'perfHc',
      key: 'perfHc',
      width: 80,
      align: 'right',
      edit: true,
      editComponent: 'InputNumber',
    },
    {
      title: '刻痕后性能',
      key: 'perf_after',
      align: 'center',
      children: [
        {
          title: 'Ss激磁功率 (VA/kg)',
          dataIndex: 'perfAfterSsPower',
          key: 'perfAfterSsPower',
          width: 90,
          align: 'right',
          edit: true,
          editComponent: 'InputNumber',
        },
        {
          title: 'Ps铁损 (W/kg)',
          dataIndex: 'perfAfterPsLoss',
          key: 'perfAfterPsLoss',
          width: 90,
          align: 'right',
          edit: true,
          editComponent: 'InputNumber',
        },
        {
          title: 'Hc (A/m)',
          dataIndex: 'perfAfterHc',
          key: 'perfAfterHc',
          width: 80,
          align: 'right',
          edit: true,
          editComponent: 'InputNumber',
        },
      ],
    },
    {
      title: '性能录入员',
      dataIndex: 'perfEditorName',
      key: 'perfEditorName',
      width: 80,
      align: 'center',
    },

    { title: '一米带重(g)', dataIndex: 'oneMeterWeight', key: 'oneMeterWeight', width: 80, align: 'right' },
    { title: '带宽 (mm)', dataIndex: 'width', key: 'width', width: 80, align: 'right' },
    // --- 动态列：带厚 ---
    {
      title: '带厚',
      key: 'thickness_group',
      align: 'center',
      children: [

        ...Array.from({ length: detectionColumns }, (_, i) => ({
          title: `${i + 1}`,
          dataIndex: `thickness${i + 1}`,
          key: `thickness${i + 1}`,
          width: 60,
          align: 'center' as const,
        })),
      ],
    },
    { title: '带厚范围', dataIndex: 'thicknessRange', key: 'thicknessRange', width: 90, align: 'center' },
    // --- 规格与物理特性 ---
    { title: '带厚极差', dataIndex: 'thicknessDiff', key: 'thicknessDiff', width: 70, align: 'right' },
    { title: '密度 (g/cm³)', dataIndex: 'density', key: 'density', width: 70, align: 'right' },
    { title: '叠片系数', dataIndex: 'laminationFactor', key: 'laminationFactor', width: 70, align: 'right' },

    // --- 外观缺陷区 ---
    { title: '外观特性', dataIndex: 'appearanceFeatureList', key: 'appearanceFeatureList', width: 220, align: 'left' },

    { title: '断头数(个)', dataIndex: 'breakCount', key: 'breakCount', width: 70, align: 'right' },
    { title: '单卷重量(kg)', dataIndex: 'singleCoilWeight', key: 'singleCoilWeight', width: 80, align: 'right' },
    { title: '外观检验员', dataIndex: 'appearEditorName', key: 'appearEditorName', width: 80, align: 'center' },
    // --- 汇总判定 ---
    { title: '平均厚度', dataIndex: 'avgThickness', key: 'avgThickness', width: 80, align: 'right' },
    { title: '磁性能判定', dataIndex: 'magneticResult', key: 'magneticResult', width: 70, align: 'center' },
    { title: '厚度判定', dataIndex: 'thicknessResult', key: 'thicknessResult', width: 70, align: 'center' },
    { title: '叠片系数判定', dataIndex: 'laminationResult', key: 'laminationResult', width: 90, align: 'center' },
    // --- 花纹 ---
    {
      title: '中Si',
      key: 'midSi',
      width: 70,
      align: 'center',
      children: [
        {
          title: '左',
          dataIndex: 'midSiLeft',
          key: 'midSiLeft',
          width: 50,
          align: 'right',
        },
        {
          title: '右',
          dataIndex: 'midSiRight',
          key: 'midSiRight',
          width: 50,
          align: 'right',
        },
      ],
    },
    {
      title: '中B',
      key: 'midB',
      width: 70,
      align: 'center',
      children: [
        {
          title: '左',
          dataIndex: 'midBLeft',
          key: 'midBLeft',
          width: 50,
          align: 'right',
        },
        {
          title: '右',
          dataIndex: 'midBRight',
          key: 'midBRight',
          width: 50,
          align: 'right',
        },
      ],
    },
    {
      title: '左花纹',
      key: 'left_pattern',
      align: 'center',
      children: [
        {
          title: '纹宽',
          dataIndex: 'leftPatternWidth',
          key: 'leftPatternWidth',
          width: 50,
          align: 'right',
        },
        {
          title: '纹距',
          dataIndex: 'leftPatternSpacing',
          key: 'leftPatternSpacing',
          width: 50,
          align: 'right',
        },
      ],
    },
    {
      title: '中花纹',
      key: 'mid_pattern',
      align: 'center',
      children: [
        {
          title: '纹宽',
          dataIndex: 'midPatternWidth',
          key: 'midPatternWidth',
          width: 50,
          align: 'right',
        },
        {
          title: '纹距',
          dataIndex: 'midPatternSpacing',
          key: 'midPatternSpacing',
          width: 50,
          align: 'right',
        },
      ],
    },
    {
      title: '右花纹',
      key: 'right_pattern',
      align: 'center',
      children: [
        {
          title: '纹宽',
          dataIndex: 'rightPatternWidth',
          key: 'rightPatternWidth',
          width: 50,
          align: 'right',
        },
        {
          title: '纹距',
          dataIndex: 'rightPatternSpacing',
          key: 'rightPatternSpacing',
          width: 50,
          align: 'right',
        },
      ],
    },

    { title: '四米带重(g)', dataIndex: 'coilWeight', key: 'coilWeight', width: 80, align: 'right' },

    // --- 动态列：叠片系数厚度分布 ---
    {
      title: '叠片系数厚度分布',
      key: 'lamination_dist_group',
      align: 'center',
      children: [
        ...Array.from({ length: detectionColumns }, (_, i) => ({
          title: `${i + 1}`,
          dataIndex: `detection${i + 1}`,
          key: `detection${i + 1}`,
          width: 60,
          align: 'center' as const,
        })),
      ],
    },

    { title: '最大厚度', dataIndex: 'maxThicknessRaw', key: 'maxThicknessRaw', width: 80, align: 'right' },
    { title: '最大平均厚度', dataIndex: 'maxAvgThickness', key: 'maxAvgThickness', width: 100, align: 'right' },
    // --- 录入信息 ---
    { title: '录入人', dataIndex: 'creatorUserName', key: 'creatorUserName', width: 80, align: 'center' },
    { title: '带型', dataIndex: 'stripType', key: 'stripType', width: 80, align: 'right' },
    { title: '一次交检', dataIndex: 'firstInspection', key: 'firstInspection', width: 80, align: 'center' },
    { title: '班次', dataIndex: 'shiftNo', key: 'shiftNo', width: 100, align: 'center' },

    // --- 计算状态与日志（固定右侧） ---
    // --- 计算状态与日志（固定右侧） ---
    { title: '计算状态', dataIndex: 'calcStatus', key: 'calcStatus', width: 100, fixed: 'right', align: 'center' },
    { title: '判定状态', dataIndex: 'judgeStatus', key: 'judgeStatus', width: 100, fixed: 'right', align: 'center' },
    { title: '日志', dataIndex: 'calcLog', key: 'calcLog', width: 70, fixed: 'right', align: 'center' },
  ];

  return columns;
});

// 分页状态 - 需要在 useTable 之前定义
const currentPagination = ref({
  current: 1,
  pageSize: 50,
  total: 0,
});

const [registerTable, { reload, getDataSource }] = useTable({
  api: getIntermediateDataList,
  columns: allColumns,
  useSearchForm: true,
  immediate: false,
  bordered: true,
  size: 'small',
  // 传统分页配置
  pagination: {
    pageSize: 50,
    showSizeChanger: true,       // 显示每页条数选择器
    showQuickJumper: true,        // 显示快速跳转
    pageSizeOptions: ['20', '50', '100', '200'],  // 每页条数选项
    current: 1,                   // 当前页
    total: 0,                     // 总条数（动态设置）
    hideOnSinglePage: false,      // 即使只有一页也显示分页器
    onChange: (page: number, pageSize: number) => {
      currentPagination.value.current = page;
      currentPagination.value.pageSize = pageSize;
      reload({ page, pageSize });
    },
    onShowSizeChange: (_current: number, size: number) => {
      currentPagination.value.current = 1;
      currentPagination.value.pageSize = size;
      reload({ page: 1, pageSize: size });
    },
  },
  showIndexColumn: false,
  formConfig: {
    baseColProps: { span: 6 },
    labelWidth: 100,
    showAdvancedButton: false,
    schemas: [
      {
        field: 'keyword',
        label: '关键词',
        component: 'Input',
        colProps: { span: 4 },
        componentProps: {
          placeholder: '炉号、产线等',
          submitOnPressEnter: true,
        },
      },
      {
        field: 'detectionDateRange',
        label: '检测日期',
        component: 'DateRange',
        colProps: { span: 6 },
        componentProps: {
          placeholder: ['开始日期', '结束日期'],
          ranges: getDateRanges(),
        },
      },
      {
        field: 'prodDateRange',
        label: '生产日期',
        component: 'DateRange',
        colProps: { span: 6 },

        componentProps: {
          placeholder: ['开始日期', '结束日期'],
          ranges: getDateRanges(),
        },
      },
      {
        field: 'calcStatus',
        label: '计算状态',
        component: 'Select',
        colProps: { span: 4 },
        defaultValue: -1,
        componentProps: {
          options: [
            { label: '所有状态', value: -1 },
            { label: '未计算', value: 0 },
            { label: '计算中', value: 1 },
            { label: '成功', value: 2 },
            { label: '失败', value: 3 },
          ],
          fieldNames: { label: 'label', value: 'value' },
          placeholder: '所有状态',
          allowClear: false,
        },
      },
      {
        field: 'judgeStatus',
        label: '判定状态',
        component: 'Select',
        colProps: { span: 4 },
        defaultValue: -1,
        componentProps: {
          options: [
            { label: '全部状态', value: -1 },
            { label: '需要判定', value: 0 },
            { label: '无需判定', value: 1 },
          ],
          fieldNames: { label: 'label', value: 'value' },
          placeholder: '判定状态',
          allowClear: false,
        },
      },
    ],
    fieldMapToTime: [
      ['detectionDateRange', ['detectionStartDate', 'detectionEndDate'], 'YYYY-MM-DD'],
      ['prodDateRange', ['startDate', 'endDate'], 'YYYY-MM-DD'],
    ],
  },
  beforeFetch: params => {
    // 处理状态筛选: -1表示全部
    if (params.calcStatus === -1) {
      delete params.calcStatus;
    }
    judgeStatusFilter.value = Number(params.judgeStatus ?? -1);
    delete params.judgeStatus;

    // 使用表单中的产品规格ID
    if (params.productSpecId) {
      selectedProductSpecId.value = params.productSpecId;
    } else {
      params.productSpecId = selectedProductSpecId.value;
    }
    // 添加多字段排序规则
    if (sortRules.value.length > 0) {
      params.sortRules = JSON.stringify(sortRules.value);
    }
    const { page, currentPage, pageSize, ...queryParams } = params;
    lastFetchParams.value = { ...queryParams };
    return params;
  },
  afterFetch: async (data, res) => {
    // 更新分页信息 - 只更新本地状态，不调用 setPagination 避免循环
    if (res && res.data && res.data.pagination) {
      const pagination = res.data.pagination;
      currentPagination.value.total = pagination.total || 0;
      currentPagination.value.current = pagination.currentPage || 1;
      currentPagination.value.pageSize = pagination.pageSize || 50;
    }

    // 数据映射：将后端返回的 Detection1, Thickness1 等字段映射为小写
    if (Array.isArray(data)) {
      const spec = productSpecOptions.value.find(s => s.id === selectedProductSpecId.value);
      const detectionCount = spec?.detectionColumns || 15;

      // 数据映射：将后端返回的字段名统一转换为小写格式
      const mappedData = data.map(item => {
        const mapped: any = { ...item };

        // 映射检测列：Detection1 -> detection1 (支持大小写)
        for (let i = 1; i <= detectionCount; i++) {
          // 尝试大写和小写两种格式
          const detectionUpper = item['Detection' + i];
          const detectionLower = item['detection' + i];
          if (detectionUpper !== undefined) {
            mapped['detection' + i] = detectionUpper;
          } else if (detectionLower !== undefined) {
            mapped['detection' + i] = detectionLower;
          }

          const thicknessUpper = item['Thickness' + i];
          const thicknessLower = item['thickness' + i];
          if (thicknessUpper !== undefined) {
            mapped['thickness' + i] = thicknessUpper;
          } else if (thicknessLower !== undefined) {
            mapped['thickness' + i] = thicknessLower;
          }
        }

        return mapped;
      });

      const filteredData = judgeStatusFilter.value === -1
        ? mappedData
        : mappedData.filter(item => judgeStatusFilter.value === 0 ? isNeedJudge(item) : !isNeedJudge(item));

      if (judgeStatusFilter.value !== -1) {
        currentPagination.value.total = filteredData.length;
      }

      // 加载颜色数据（延迟执行避免阻塞渲染）
      if (filteredData.length > 0) {
        const dataIds = filteredData.map(item => item.id);

        // 使用 setTimeout 替代 nextTick，给浏览器更多时间完成初始渲染
        // 100ms 延迟确保数据已经渲染到 DOM，再加载颜色
        setTimeout(() => {
          loadColorsByIds(dataIds);
        }, 100);
      }

      // 保存表格数据用于行/列填充
      tableData.value = filteredData;

      return filteredData;
    }
    return data;
  },
});

// 外观特性字段（动态字段 + 固定字段）
// const appearanceFields = computed(() => {
//   const dynamicFields = appearanceCategories.value
//     .filter(category => !category.parentId || category.parentId === '-1') // 只包含顶级分类（大类）
//     .map(category => category.id);
// 
//   return [
//     ...dynamicFields,
//     'fishScale',
//     'breakCount',
//     'singleCoilWeight',
//     'appearEditorName',
//   ];
// });

// function isAppearanceColumn(key: string) {
//   return appearanceFields.value.includes(key);
// }

function isEditableMeasurement(key: string) {
  const editableKeys = [
    'midSiLeft', 'midSiRight',
    'midBLeft', 'midBRight',
    'leftPatternWidth', 'leftPatternSpacing',
    'midPatternWidth', 'midPatternSpacing',
    'rightPatternWidth', 'rightPatternSpacing',
    'breakCount', 'singleCoilWeight'
  ];
  return editableKeys.includes(key);
}

// 获取日期快捷方式
function getDateRanges() {
  const now = dateUtil();
  const startOfWeek = now.startOf('week');
  const endOfWeek = now.endOf('week');
  const startOfMonth = now.startOf('month');
  const endOfMonth = now.endOf('month');

  // 计算季度：根据月份计算季度开始和结束
  const month = now.month(); // 0-11
  const quarter = Math.floor(month / 3); // 0-3
  const startOfQuarter = now.month(quarter * 3).startOf('month');
  const endOfQuarter = now.month(quarter * 3 + 2).endOf('month');

  const startOfYear = now.startOf('year');
  const endOfYear = now.endOf('year');

  return {
    本周: [startOfWeek, endOfWeek],
    本月: [startOfMonth, endOfMonth],
    本季度: [startOfQuarter, endOfQuarter],
    本年: [startOfYear, endOfYear],
  };
}

// 加载产品规格选项
async function loadProductSpecOptions() {
  try {
    const result = await getProductSpecOptions();
    productSpecOptions.value = result.data || result || [];
    if (productSpecOptions.value.length > 0 && !selectedProductSpecId.value) {
      selectedProductSpecId.value = productSpecOptions.value[0].id;
      // 使用 nextTick 确保表格已经注册
      await nextTick();
      reload({ page: 1 });
    }
  } catch (error) {
  }
}

// 监听产品规格变化
watch(selectedProductSpecId, async (newVal, oldVal) => {
  if (newVal && newVal !== oldVal) {
    // 清空现有颜色数据
    coloredCells.value = {};
    // 重置到第一页
    currentPagination.value.current = 1;
    // 重新加载表格数据（会触发afterFetch重新加载颜色）
    await nextTick();
    reload({ page: 1 });
  }
});

// 处理排序变化（新的多字段排序）
function handleSortChange(newSortRules: any[]) {
  sortRules.value = newSortRules;
  // 重置到第一页
  currentPagination.value.current = 1;
  // 重新加载表格数据
  reload({ page: 1 });
}

function handleCalcLogTableChange(pagination: any) {
  loadCalcLogs(pagination?.current || 1, pagination?.pageSize || 20);
}

async function openCalcLogDrawer(record: any) {
  if (!record?.id) {
    return;
  }
  calcLogTargetId.value = record.id;
  calcLogDrawerVisible.value = true;
  await loadCalcLogs(1, calcLogPagination.value.pageSize);
}

async function loadCalcLogs(page = 1, pageSize = 20) {
  if (!calcLogTargetId.value) {
    return;
  }
  calcLogLoading.value = true;
  try {
    const res = await getIntermediateDataCalcLogs({
      intermediateDataId: calcLogTargetId.value,
      currentPage: page,
      pageSize,
    });
    const data = (res as any)?.data ?? res;
    const pageInfo = data?.pagination;
    calcLogData.value = data?.list || [];
    calcLogPagination.value = {
      ...calcLogPagination.value,
      current: pageInfo?.currentPage ?? page,
      pageSize: pageInfo?.pageSize ?? pageSize,
      total: pageInfo?.total ?? calcLogData.value.length,
    };
  } finally {
    calcLogLoading.value = false;
  }
}

// 单元格编辑
function handleCellEdit(record: any, field: string, value: any) {
  editingCell.value = { id: record.id, field };
  editingValue.value = value;
}

function handleCellBlur() {
  handleCellSave();
}

async function handleCellSave() {
  if (!editingCell.value) return;

  try {
    await updateBaseInfo({
      id: editingCell.value.id,
      [editingCell.value.field]: editingValue.value,
    });
    createMessage.success('保存成功');
    reload();
  } catch (error) {
    createMessage.error('保存失败');
  } finally {
    editingCell.value = null;
    editingValue.value = null;
  }
}

// 通用单元格保存
// async function handleCellSave2(record: any, field: string, value: any) {
//   try {
//     await updateBaseInfo({
//       id: record.id,
//       [field]: value,
//     });
//     createMessage.success('保存成功');
//     reload();
//   } catch (error) {
//     createMessage.error('保存失败');
//   }
// }

// 可编辑测量值保存（根据字段类型选择API，部分字段触发判定）
async function handleMeasurementSave(record: any, field: string, value: any) {
  // 需要触发判定的字段（只有断头数和单卷重量需要触发判定）
  const judgeRequiredFields = ['breakCount', 'singleCoilWeight'];
  // 所有外观相关字段使用 updateAppearance API
  const appearanceFields = [
    'breakCount', 'singleCoilWeight',
    'midSiLeft', 'midSiRight',
    'midBLeft', 'midBRight',
    'leftPatternWidth', 'leftPatternSpacing',
    'midPatternWidth', 'midPatternSpacing',
    'rightPatternWidth', 'rightPatternSpacing'
  ];

  try {
    if (appearanceFields.includes(field)) {
      await updateAppearance({
        id: record.id,
        [field]: value,
      });
    } else {
      await updateBaseInfo({
        id: record.id,
        [field]: value,
      });
    }

    // 断头数、单卷重量修改后触发判定；其他字段只保存不判定
    if (judgeRequiredFields.includes(field)) {
      createMessage.success('保存成功，正在判定...');
      try {
        await judgeIntermediateData([record.id]);
        setTimeout(() => {
          reload();
        }, 500);
      } catch (judgeError) {
        console.error('判定失败:', judgeError);
        reload();
      }
    } else {
      createMessage.success('保存成功');
      reload();
    }
  } catch (error) {
    createMessage.error('保存失败');
  }
}

// 性能数据保存
async function handlePerfSave(record: any, field: string, value: any) {
  try {
    // 构造完整的性能数据 payload
    // 注意：必须发送所有字段，否则后端修改为直接赋值逻辑后，未发送的字段会被清空
    const perfPayload = {
      id: record.id,
      perfSsPower: record.perfSsPower,
      perfPsLoss: record.perfPsLoss,
      perfHc: record.perfHc,
      perfAfterSsPower: record.perfAfterSsPower,
      perfAfterPsLoss: record.perfAfterPsLoss,
      perfAfterHc: record.perfAfterHc,
      [field]: value === '' ? null : value // 覆盖当前修改的字段，空字符串转null
    };

    // 确保其他字段的空字符串也转为null
    for (const key in perfPayload) {
      if (perfPayload[key] === '') {
        perfPayload[key] = null;
      }
    }

    // 更新性能数据
    await updatePerformance(perfPayload);

    // 设置处理中状态
    record.pendingPerfCalc = true;
    createMessage.success('保存成功，正在计算和判定...', 1.5);

    try {
      // 第一步：自动触发重新计算
      await recalculateIntermediateData([record.id]);

      // 等待计算完成后触发判定
      setTimeout(async () => {
        try {
          // 第二步：自动触发判定
          await judgeIntermediateData([record.id]);

          // 判定完成后刷新数据
          setTimeout(() => {
            reload();
            record.pendingPerfCalc = false;
          }, 500);
        } catch (judgeError) {
          console.error('判定失败:', judgeError);
          record.pendingPerfCalc = false;
          reload();
        }
      }, 800); // 等待计算完成
    } catch (calcError) {
      console.error('计算失败:', calcError);
      record.pendingPerfCalc = false;
      // 计算失败时不显示错误，避免干扰用户操作，刷新即可显示失败状态
      reload();
    }
  } catch (error) {
    createMessage.error('保存失败');
  }
}

// 外观特性保存
// async function handleAppearanceSave(record: any, field: string, value: any) {
//   try {
//     await updateAppearance({
//       id: record.id,
//       [field]: value,
//     });
//     createMessage.success('保存成功');
//     reload();
//   } catch (error) {
//     createMessage.error('保存失败');
//   }
// }

// 打开特性选择对话框
function handleOpenFeatureDialog(record: any) {
  if (!hasBtnP(PERM_APPEARANCE)) return;

  currentEditRecordId.value = record.id;

  // 获取当前已选的特性ID列表
  const matched = getMatchedFeatureLabels(record);
  const ids = matched.map(m => m.id);

  featureSelectDialogRef.value.open(ids);
}

// 确认选择特性
async function handleFeatureSelectConfirm(features: any[]) {
  if (!currentEditRecordId.value) return;

  try {
    const ids = features.map(f => f.id);
    const recordId = currentEditRecordId.value;

    // 保存到后端
    await updateAppearance({
      id: recordId,
      appearanceFeatureIds: JSON.stringify(ids),
    });

    createMessage.success('保存成功，正在判定...');

    // 外观特性修改后触发判定（不需要重新计算）
    try {
      await judgeIntermediateData([recordId]);
      setTimeout(() => {
        reload();
      }, 500);
    } catch (judgeError) {
      console.error('判定失败:', judgeError);
      reload();
    }
  } catch (error) {
    console.error('保存特性失败:', error);
    createMessage.error('保存失败');
  } finally {
    currentEditRecordId.value = '';
  }
}


// 重新计算
async function handleRecalculate(record: any) {
  if (record.recalculating) return;

  try {
    record.recalculating = true;
    await recalculateIntermediateData([record.id]);
    createMessage.success('已触发重新计算');
    // 稍后刷新
    setTimeout(() => {
      reload();
    }, 1000);
  } catch (error) {
    createMessage.error('触发计算失败');
    record.recalculating = false;
  }
}

// 重新判定
async function handleRejudge(record: any) {
  if (record.rejudging) return;

  try {
    record.rejudging = true;
    await judgeIntermediateData([record.id]);
    createMessage.success('已触发重新判定');
    // 稍后刷新
    setTimeout(() => {
      reload();
    }, 1000);
  } catch (error) {
    createMessage.error('触发判定失败');
    record.rejudging = false;
  }
}

// 批量重新计算（针对计算失败的数据）
async function handleBatchRecalculate() {
  if (batchCalculating.value) return;

  // 获取当前表格数据
  const data = getDataSource();
  if (!data || data.length === 0) {
    createMessage.warning('当前没有数据');
    return;
  }

  try {
    batchCalculating.value = true;
    const ids = data.map(record => record.id);
    await recalculateIntermediateData(ids);
    createMessage.success(`已触发 ${ids.length} 条数据的计算`);
    // 稍后刷新
    setTimeout(() => {
      reload();
      batchCalculating.value = false;
    }, 1000);
  } catch (error) {
    createMessage.error('批量触发计算失败');
    batchCalculating.value = false;
  }
}

// 批量判定（仅对“需要判定”的数据执行判定）
async function handleBatchJudge() {
  if (batchJudging.value) return;

  const baseParams = {
    ...lastFetchParams.value,
    productSpecId: selectedProductSpecId.value || lastFetchParams.value?.productSpecId,
  };

  if (!baseParams.productSpecId) {
    createMessage.warning('当前没有数据');
    return;
  }

  const pageSize = 500;
  const allRecords: any[] = [];
  const idSet = new Set<string>();
  let page = 1;
  let totalPages = 1;

  try {
    batchJudging.value = true;

    while (page <= totalPages) {
      const response = await getIntermediateDataList({
        ...baseParams,
        page,
        currentPage: page,
        pageSize,
      });
      const result = (response as any)?.data || response;
      const list: any[] = result?.list || [];
      const pagination = result?.pagination || {};
      const total = Number(pagination.total ?? list.length);
      const current = Number(pagination.currentPage ?? page);
      const size = Number(pagination.pageSize ?? pageSize);
      totalPages = Math.max(1, Math.ceil(total / Math.max(size, 1)));

      list.forEach(item => {
        const id = item?.id;
        if (!id || idSet.has(id)) return;
        idSet.add(id);
        allRecords.push(item);
      });

      if (list.length === 0 || current >= totalPages) {
        break;
      }
      page = current + 1;
    }
  } catch (error) {
    createMessage.error('加载全量数据失败');
    batchJudging.value = false;
    return;
  }

  // 筛选出需要判定的数据（贴标为空）
  const needJudgeRecords = allRecords.filter(record => isNeedJudge(record));

  if (needJudgeRecords.length === 0) {
    createMessage.info('没有需要判定的数据');
    batchJudging.value = false;
    return;
  }

  try {
    const ids = needJudgeRecords.map(record => record.id);
    const response = await judgeIntermediateData(ids);
    const result = (response as any)?.data || response || {};
    createMessage.success(result?.message || `已提交 ${ids.length} 条判定任务到队列`);
    await reload();
    startJudgeQueuePolling();
  } catch (error) {
    createMessage.error('批量判定失败');
  } finally {
    batchJudging.value = false;
  }
}
function handleQuickExport({ key }: { key: string }) {
  if (key === 'custom') {
    openExportModal();
    return;
  }

  // 设置日期范围并直接导出
  setExportRange(key);
  // 延迟一下确保日期范围已设置
  nextTick(() => {
    handleExport();
  });
}

// 打开导出模态框
function openExportModal() {
  exportModalVisible.value = true;
  // 默认选中本月
  setExportRange('thisMonth');
}

// 手动修改日期时清除快捷选择状态
function handleExportDateChange() {
  selectedExportShortcut.value = '';
}

// 格式化导出日期显示
function formatExportDate(timestamp: number): string {
  return dateUtil(timestamp).format('YYYY-MM-DD');
}

// 设置导出日期范围快捷选项
function setExportRange(type: string) {
  const now = dateUtil();
  selectedExportShortcut.value = type; // 设置选中状态

  let startDate: Dayjs;
  let endDate: Dayjs;

  switch (type) {
    case 'thisWeek':
      startDate = now.startOf('week');
      endDate = now.endOf('week');
      break;
    case 'thisMonth':
      startDate = now.startOf('month');
      endDate = now.endOf('month');
      break;
    case 'thisQuarter':
      const month = now.month();
      const quarter = Math.floor(month / 3);
      startDate = now.month(quarter * 3).startOf('month');
      endDate = now.month(quarter * 3 + 2).endOf('month');
      break;
    case 'thisYear':
      startDate = now.startOf('year');
      endDate = now.endOf('year');
      break;
    case 'lastWeek':
      startDate = now.subtract(1, 'week').startOf('week');
      endDate = now.subtract(1, 'week').endOf('week');
      break;
    case 'lastMonth':
      const lastMonth = now.subtract(1, 'month');
      startDate = lastMonth.startOf('month');
      endDate = lastMonth.endOf('month');
      break;
    case 'lastQuarter':
      const currentMonth = now.month();
      const currentQuarter = Math.floor(currentMonth / 3);
      const lastQuarterNum = currentQuarter === 0 ? 3 : currentQuarter - 1;
      const lastQuarterYear = currentQuarter === 0 ? now.subtract(1, 'year') : now;
      startDate = lastQuarterYear.month(lastQuarterNum * 3).startOf('month');
      endDate = lastQuarterYear.month(lastQuarterNum * 3 + 2).endOf('month');
      break;
    case 'lastYear':
      const lastYear = now.subtract(1, 'year');
      startDate = lastYear.startOf('year');
      endDate = lastYear.endOf('year');
      break;
    default:
      startDate = now.startOf('month');
      endDate = now.endOf('month');
  }

  // JnpfDateRange 使用时间戳数组
  exportDateRange.value = [startDate.valueOf(), endDate.valueOf()];
}

// 处理导出
async function handleExport() {
  if (exporting.value) return;

  if (!exportDateRange.value || exportDateRange.value.length !== 2) {
    createMessage.warning('请选择生产日期范围');
    return;
  }

  try {
    exporting.value = true;
    createMessage.loading({ content: '正在导出数据...', key: 'export', duration: 0 });

    const startDate = dateUtil(exportDateRange.value[0]).format('YYYY-MM-DD');
    const endDate = dateUtil(exportDateRange.value[1]).format('YYYY-MM-DD');

    const response = await exportIntermediateData(startDate, endDate);

    // 获取文件名
    const contentDisposition = response.headers?.get('content-disposition');
    let fileName = `中间数据导出_${new Date().toISOString().slice(0, 10)}.xlsx`;
    if (contentDisposition) {
      const match = contentDisposition.match(/filename[^;=\n]*=((['"]).*?\2|[^;\n]*)/);
      if (match) {
        fileName = decodeURIComponent(match[1].replace(/['"]/g, ''));
      }
    }

    // 下载文件
    const blob = new Blob([response.data], { type: 'application/vnd.openxmlformats-officedocument.spreadsheetml.sheet' });
    const link = document.createElement('a');
    link.href = URL.createObjectURL(blob);
    link.download = fileName;
    document.body.appendChild(link);
    link.click();
    document.body.removeChild(link);
    URL.revokeObjectURL(link.href);

    createMessage.success({ content: '导出成功', key: 'export' });
    exportModalVisible.value = false;
  } catch (error: any) {
    console.error('导出失败:', error);
    createMessage.error({ content: error.message || '导出失败', key: 'export' });
  } finally {
    exporting.value = false;
  }
}



// 格式化值
// 格式化值
function formatValue(value: any, _fieldName?: string) {
  if (value === null || value === undefined) return '-';
  // 直接返回，后端已处理精度
  return value;
}

// 格式化数值
function formatNumericValue(value: any, _fieldName?: string) {
  if (value === null || value === undefined) return '-';
  // 直接返回，后端已处理精度
  return value;
}



// 判断是否为数值列
function isNumericColumn(key: string): boolean {
  const numericKeys = [
    'oneMeterWeight',
    'width',
    'thicknessDiff',
    'density',
    'laminationFactor',
    'coilWeight',
    'avgThickness',
    'breakCount',
    'singleCoilWeight',
    'thicknessMax',
    'thicknessMin',
    'maxThicknessRaw',
    'maxAvgThickness',
    'stripType',
    'perfSsPower',
    'perfPsLoss',
    'perfHc',
    'perfAfterSsPower',
    'perfAfterPsLoss',
    'perfAfterHc',
    'leftPatternWidth',
    'leftPatternSpacing',
    'midPatternWidth',
    'midPatternSpacing',
    'rightPatternWidth',
    'rightPatternSpacing',
    'midSiLeft',
    'midSiRight',
    'midBLeft',
    'midBRight',
  ];
  return (
    numericKeys.includes(key) ||
    key?.startsWith('detection') ||
    (key?.startsWith('thickness') && key !== 'thicknessRange')
  );
}

// 判断是否为空格分隔的数字字符串
function isNumericString(value: any): boolean {
  if (!value) return false;
  if (typeof value === 'number') return false;
  const str = String(value).trim();
  if (!str) return false;

  // 排除日期格式（包含 / 或 - 分隔符的字符串）
  if (str.includes('/') || str.includes('-')) {
    // 检查是否是日期格式（如 2026/01/10 或 2026-01-10）
    const datePattern = /^\d{4}[\/\-]\d{1,2}[\/\-]\d{1,2}/;
    if (datePattern.test(str)) return false;
  }

  // 检查是否包含空格，且分割后的部分都是数字
  const parts = str.split(/\s+/).filter(p => p);
  if (parts.length < 2) return false; // 至少要有2个数字
  // 检查是否所有部分都是数字
  return parts.every(part => !isNaN(parseFloat(part)) && isFinite(parseFloat(part)));
}


// 加载特性大类列表
async function loadAppearanceCategories() {
  try {
    const res: any = await getAllAppearanceFeatureCategories();
    // 处理多种可能的响应格式
    let data: AppearanceFeatureCategoryInfo[] = [];
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
      }
    }
    appearanceCategories.value = data || [];
  } catch (error) {
    appearanceCategories.value = [];
  }
}

// getCellColor 函数已被 useColorStyles 中的 getCellClass 替代
// 使用CSS类替代内联样式，大幅提升性能（160,000+次响应式查找 -> ~100条CSS规则）

// 处理单元格颜色选择
async function handleCellColor(rowId: string, field: string) {

  // 根据填充模式获取需要处理的单元格
  let cellsToProcess: { rowId: string; field: string }[] = [];

  if (fillMode.value === 'cell') {
    // 单元格模式
    cellsToProcess = [{ rowId, field }];
  } else if (fillMode.value === 'row') {
    // 整行模式：获取所有列的字段名
    const columns = allColumns.value.flatMap(col => {
      if (col.children) {
        return col.children.map(child => child.key || child.dataIndex);
      }
      return [col.key || col.dataIndex];
    }).filter(Boolean);
    cellsToProcess = columns.map(f => ({ rowId, field: f as string }));
  } else if (fillMode.value === 'column') {
    // 整列模式：获取所有行的ID
    cellsToProcess = tableData.value.map(row => ({ rowId: row.id, field }));
  }


  // 清除模式
  if (isClearMode.value) {
    const newColorMap = { ...coloredCells.value };
    cellsToProcess.forEach(cell => {
      const key = `${cell.rowId}::${cell.field}`;
      delete newColorMap[key];
    });
    coloredCells.value = newColorMap;

    // 批量保存到后端（使用空颜色值表示删除）
    try {
      const colorsToSave = cellsToProcess.map(cell => ({
        intermediateDataId: cell.rowId,
        fieldName: cell.field,
        colorValue: '', // 空颜色值表示删除
      }));
      await saveIntermediateDataColors({
        colors: colorsToSave,
        productSpecId: selectedProductSpecId.value,
      });
      createMessage.success(`已清除 ${cellsToProcess.length} 个单元格的颜色`);
    } catch (error) {
      console.error('清除颜色失败:', error);
      createMessage.error('清除颜色失败');
    }
    return;
  }

  // 填充模式
  if (selectedColor.value) {
    const newColorMap = { ...coloredCells.value };
    cellsToProcess.forEach(cell => {
      const key = `${cell.rowId}::${cell.field}`;
      newColorMap[key] = selectedColor.value;
    });
    coloredCells.value = newColorMap;

    // 批量保存到后端
    try {
      const colorsToSave = cellsToProcess.map(cell => ({
        intermediateDataId: cell.rowId,
        fieldName: cell.field,
        colorValue: selectedColor.value,
      }));
      await saveIntermediateDataColors({
        colors: colorsToSave,
        productSpecId: selectedProductSpecId.value,
      });
      createMessage.success(`已填充 ${cellsToProcess.length} 个单元格`);
    } catch (error) {
      console.error('保存颜色失败:', error);
      createMessage.error('颜色保存失败');
    }
  }
}

// 选择颜色
function selectColor(color: string) {
  selectedColor.value = color;
  isClearMode.value = false; // 选择颜色时退出清除模式
}

// 清除选中的颜色（进入清除模式）
function clearSelectedColor() {
  selectedColor.value = '';
  isClearMode.value = true; // 进入清除模式
  createMessage.info('清除模式：点击单元格可清除颜色');
}

const isLoadingColors = ref(false);

// 根据ID列表加载颜色数据
async function loadColorsByIds(dataIds: string[]) {
  console.log('loadColorsByIds called', dataIds.length);
  if (!selectedProductSpecId.value || !dataIds || dataIds.length === 0) {
    return;
  }

  if (isLoadingColors.value) {
    console.warn('loadColorsByIds ignored (already loading)');
    return;
  }

  try {
    isLoadingColors.value = true;
    const response = await getIntermediateDataColors({
      productSpecId: selectedProductSpecId.value,
      intermediateDataIds: dataIds,
    });


    // 处理嵌套的响应结构 - colors 可能在 response.colors 或 response.data.colors
    const result = (response as any)?.data || response;
    const colors = result?.colors;

    if (colors && colors.length > 0) {
      // 创建新的对象以确保响应式更新
      const newColorMap: Record<string, string> = { ...coloredCells.value };
      colors.forEach((color: CellColorInfo) => {
        if (color.intermediateDataId && color.fieldName) {
          const key = `${color.intermediateDataId}::${color.fieldName}`;
          newColorMap[key] = color.colorValue;
        }
      });
      // 使用新对象替换，确保触发响应式更新
      try {
        console.log('Updating coloredCells.value');
        coloredCells.value = newColorMap;
      } catch (e) {
        console.error('更新颜色映射失败:', e);
      }
    } else {
      // 如果没有颜色数据，清空颜色映射
      coloredCells.value = {};
    }
  } catch (error) {
    console.error('加载颜色数据失败:', error);
  } finally {
    isLoadingColors.value = false;
  }
}

// 加载颜色数据
// async function loadColors() {
//   if (!selectedProductSpecId.value) return;
// 
//   try {
//     const response = await getIntermediateDataColors({
//       productSpecId: selectedProductSpecId.value,
//     });
// 
//     // 处理嵌套的响应结构
//     const result = (response as any)?.data || response;
//     const colors = result?.colors;
// 
//     if (colors && colors.length > 0) {
//       // 将颜色数据转换为Map格式
//       const colorMap: Record<string, string> = {};
//       colors.forEach((color: CellColorInfo) => {
//         const key = `${color.intermediateDataId}::${color.fieldName}`;
//         colorMap[key] = color.colorValue;
//       });
//       coloredCells.value = colorMap;
//     }
//     colorLoaded.value = true;
//   } catch (error) {
//     console.error('加载颜色数据失败:', error);
//     colorLoaded.value = false;
//   }
// }

// 保存当前颜色配置（批量保存）
async function saveColorsBatch() {
  if (!selectedProductSpecId.value) return;

  savingColors.value = true;
  try {
    const colors: CellColorInfo[] = Object.entries(coloredCells.value).map(([key, color]) => {
      const [intermediateDataId, fieldName] = key.split('::');
      return {
        intermediateDataId,
        fieldName,
        colorValue: color,
      };
    });


    await saveIntermediateDataColors({
      colors,
      productSpecId: selectedProductSpecId.value,
    });

    createMessage.success('颜色配置保存成功');
  } catch (error) {
    console.error('批量保存颜色失败:', error);
    createMessage.error('颜色配置保存失败');
  } finally {
    savingColors.value = false;
  }
}

function clearJudgeQueuePolling() {
  if (judgeQueuePollTimer) {
    clearTimeout(judgeQueuePollTimer);
    judgeQueuePollTimer = null;
  }
  judgeQueuePolling.value = false;
}

function startJudgeQueuePolling() {
  if (judgeQueuePolling.value) {
    return;
  }

  const maxAttempts = 30;
  let attempts = 0;
  judgeQueuePolling.value = true;

  const tick = async () => {
    attempts += 1;
    try {
      const baseParams = {
        ...lastFetchParams.value,
        productSpecId: selectedProductSpecId.value || lastFetchParams.value?.productSpecId,
      };
      const response = await getIntermediateDataList({
        ...baseParams,
        pageSize: 1,
        currentPage: 1,
        page: 1,
        judgeStatus: 1,
      });

      const result = (response as any)?.data || response;
      const totalProcessing = Number(result?.pagination?.total ?? 0);
      if (totalProcessing <= 0) {
        clearJudgeQueuePolling();
        await reload();
        createMessage.success('判定队列处理完成');
        return;
      }
    } catch (error) {
      clearJudgeQueuePolling();
      return;
    }

    if (attempts >= maxAttempts) {
      clearJudgeQueuePolling();
      return;
    }

    judgeQueuePollTimer = setTimeout(tick, 2000);
  };

  judgeQueuePollTimer = setTimeout(tick, 2000);
}

onUnmounted(() => {
  clearJudgeQueuePolling();
});

onMounted(() => {
  loadProductSpecOptions();
  loadAppearanceCategories();
  loadFeatures();
});

// --- 磁性数据导入相关逻辑 ---
const [registerQuickModal, { openModal: openQuickModal }] = useModal();

function handleImportSuccess() {
  createMessage.success('导入完成，相关数据已更新');
  reload({ page: 1 }); // 刷新中间数据表
}


async function handleQuickImport(file: File) {
  const isExcel = file.type === 'application/vnd.openxmlformats-officedocument.spreadsheetml.sheet' ||
    file.type === 'application/vnd.ms-excel' ||
    file.name.endsWith('.xlsx') ||
    file.name.endsWith('.xls');

  if (!isExcel) {
    createMessage.error('只能上传Excel文件！');
    return false;
  }

  try {
    createMessage.loading({ content: '正在快速解析文件...', key: 'quickImport', duration: 0 });

    // 1. 读取文件并转换为 Base64
    const reader = new FileReader();
    const fileData = await new Promise<string>((resolve, reject) => {
      reader.onload = () => {
        const res = reader.result as string;
        resolve(res.split(',')[1]);
      };
      reader.onerror = reject;
      reader.readAsDataURL(file);
    });

    // 2. 创建会话并保存文件
    const sessionId = await createMagneticImportSession({
      fileName: file.name,
      fileData: fileData,
    });

    if (!sessionId) throw new Error('创建会话失败');

    // 3. 执行解析
    await uploadAndParseMagneticData(sessionId, {
      fileName: file.name,
      fileData: fileData,
    });

    createMessage.success({ content: '解析成功，请最后核对数据', key: 'quickImport' });

    // 4. 打开模态窗并直接显现第二步
    openQuickModal(true, {
      importSessionId: sessionId,
    });
  } catch (error: any) {
    console.error('快捷导入失败:', error);
    createMessage.error({ content: error.message || '快捷导入失败', key: 'quickImport' });
  }

  return false; // 阻止自动上传
}

</script>

<style scoped>
/* 数据统计样式 */
.data-statistics {
  display: inline-flex;
  align-items: center;
  margin-right: 16px;
}

.data-count {
  font-size: 12px;
  color: #666;
  padding: 2px 8px;
  background: #f5f5f5;
  border-radius: 4px;
}

.table-toolbar {
  margin-bottom: 1px;
  display: flex;
  align-items: center;
  gap: 16px;
}

.spec-tabs {
  margin-bottom: 8px;
}

/* 颜色填充控制 */
.color-fill-control {
  display: flex;
  align-items: center;
  gap: 8px;
  padding: 4px 8px;
  background: #f5f5f5;
  border-radius: 4px;
}

.color-label {
  font-size: 12px;
  color: #666;
}

.color-palette {
  display: flex;
  gap: 4px;
}

.color-option {
  width: 20px;
  height: 20px;
  border: 1px solid #d9d9d9;
  border-radius: 2px;
  cursor: pointer;
  transition: all 0.3s;
}

.color-option:hover {
  transform: scale(1.1);
  border-color: #1890ff;
  box-shadow: 0 2px 4px rgba(0, 0, 0, 0.2);
}

.color-option.active {
  border-color: #1890ff;
  border-width: 2px;
}

/* 单元格内容 */
.cell-content {
  width: 100%;
  height: 100%;
  padding: 4px;
  cursor: pointer;
  transition: background-color 0.3s;
}

.cell-content:hover {
  opacity: 0.8;
}

.cell-content--no-pointer {
  cursor: default;
}

.cell-content--no-pointer:hover {
  opacity: 1;
}

.editable-cell {
  cursor: pointer;
  padding: 4px px;
  border-radius: 4px;
}

.editable-cell:hover {
  background: #f0f0f0;
}

/* 产品规格 Tab 放在查询下方，紧凑布局 */
.spec-tabs-wrap {
  display: inline-block;
  margin-right: 16px;
  vertical-align: middle;
}

.spec-tabs-wrap :deep(.ant-tabs) {
  margin-bottom: 0;
}

.spec-tabs-wrap :deep(.ant-tabs-card .ant-tabs-tab) {
  padding: 6px 16px;
  font-size: 14px;
  font-weight: bold;
  border-radius: 4px 4px 0 0;
  border: 1px solid #d9d9d9;
  margin-right: 4px !important;
  background-color: #f5f5f5;
  transition: all 0.3s;
}

.spec-tabs-wrap :deep(.ant-tabs-card .ant-tabs-tab-active) {
  background-color: #1890ff !important;
  color: white !important;
  border-color: #1890ff !important;
}

.spec-tabs-wrap :deep(.ant-tabs-card .ant-tabs-tab-active .ant-tabs-tab-btn) {
  color: white !important;
}

.spec-tabs-wrap :deep(.ant-tabs-nav) {
  margin-bottom: 0;
}

/* 查询表单与 Tab 之间减小间距 */
.page-content-wrapper-content :deep(.search-form) {
  margin-bottom: 0;
}

.page-content-wrapper-content :deep(.search-form .ant-form-item) {
  margin-bottom: 8px;
}

/* 表格容器样式 */
.table-container {
  width: 100%;
}

/* 针对工业大表格的样式微调 */
.table-container :deep(.ant-table-thead > tr > th) {
  padding: 4px 8px;
  font-size: 12px;
  background-color: #fafafa;
  text-align: center !important;
}

/* 蓝色表头样式 */
.table-container :deep(.header-blue),
.table-container :deep(.header-blue > .ant-table-thead > tr > th) {
  background-color: #00b0f0 !important;
  color: white !important;
}

/* 确保蓝色表头下的所有子表头也是蓝色 */
.table-container :deep(.header-blue th) {
  background-color: #00b0f0 !important;
  color: white !important;
}

/* 贴标单元格特殊处理 */
.status-cell {
  width: 100%;
  height: 100%;
  padding: 4px;
  display: flex;
  align-items: center;
  justify-content: center;
}

.status-cell.bg-red {
  background-color: #ff0000 !important;
  color: white;
  font-weight: bold;
}

/* 负数红色显示 */
.text-danger {
  color: #f5222d;
}

/* 表格单元格紧凑布局 */
.table-container :deep(.ant-table-tbody > tr > td) {
  padding: 4px 8px;
  font-size: 12px;
}

/* 导出日期快捷选择样式 */
.export-date-shortcuts {
  display: flex;
  flex-wrap: wrap;
  gap: 8px;
}

.export-shortcut-tag {
  cursor: pointer;
  padding: 4px 12px;
  font-size: 13px;
  border-radius: 4px;
  transition: all 0.3s;
  user-select: none;
}

.export-shortcut-tag:hover {
  transform: translateY(-2px);
  box-shadow: 0 2px 8px rgba(0, 0, 0, 0.15);
}

.export-shortcut-tag--active {
  font-weight: 600;
  border-width: 2px;
  box-shadow: 0 2px 8px rgba(24, 144, 255, 0.3);
}

/* 外观特性列表编辑样式 */
.feature-list-cell {
  position: relative;
  display: flex;
  align-items: center;
  gap: 4px;
}

.feature-list-cell.editable {
  cursor: pointer;
}

.feature-list-cell.editable:hover {
  background-color: #f0f5ff !important;
}

.feature-edit-icon {
  font-size: 12px;
  color: #1890ff;
  opacity: 0;
  transition: opacity 0.2s;
  flex-shrink: 0;
}

.feature-list-cell.editable:hover .feature-edit-icon {
  opacity: 1;
}
</style>
