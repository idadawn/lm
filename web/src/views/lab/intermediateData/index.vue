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
              <a-space>
                <a-upload :before-upload="handleQuickImport" :show-upload-list="false" accept=".xlsx,.xls">
                  <a-button>
                    <UploadOutlined /> 磁性数据导入
                  </a-button>
                </a-upload>
              </a-space>
            </template>
            <template #bodyCell="{ column, record, text }">

              <!-- 贴标列 - 特殊样式处理 -->
              <template v-if="column.key === 'labeling'">
                <div :class="['status-cell', (record.labeling || text) === '性能不合' ? 'bg-red' : '']"
                  :style="{ backgroundColor: getCellColor(record.id, column.key) }"
                  @click="handleCellColor(record.id, column.key)">
                  <EditableCell :record="record" :field="column.key" :value="record.labeling || text"
                    @save="val => handleCellSave2(record, column.key, val)" />
                </div>
              </template>

              <!-- 日期列 -->
              <template v-else-if="column.key === 'dateMonth'">
                <div class="cell-content" :style="{ backgroundColor: getCellColor(record.id, column.key) }"
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
              <template v-else-if="column.key === 'detectionDateStr' || column.key === 'prodDateStr'">
                <div class="cell-content" :style="{ backgroundColor: getCellColor(record.id, column.key) }"
                  @click="handleCellColor(record.id, column.key)">
                  <span>{{ text || '-' }}</span>
                </div>
              </template>

              <!-- 性能数据列 -->
              <template v-else-if="column.key?.startsWith('perf')">
                <div class="cell-content" :style="{ backgroundColor: getCellColor(record.id, column.key) }"
                  @click="handleCellColor(record.id, column.key)">
                  <EditableCell :record="record" :field="column.key" :value="record[column.key]" type="number"
                    @save="val => handlePerfSave(record, column.key, val)" />
                </div>
              </template>

              <!-- 外观特性列 -->
              <template v-else-if="isAppearanceColumn(column.key)">
                <div class="cell-content" :style="{ backgroundColor: getCellColor(record.id, column.key) }"
                  @click="handleCellColor(record.id, column.key)">
                  <EditableCell :record="record" :field="column.key" :value="record[column.key]"
                    :type="getAppearanceFieldType(column.key)"
                    @save="val => handleAppearanceSave(record, column.key, val)" />
                </div>
              </template>

              <!-- 数值列 - 负数红色显示 -->
              <template v-else-if="isNumericColumn(column.key)">
                <div class="cell-content" :style="{ backgroundColor: getCellColor(record.id, column.key) }"
                  @click="handleCellColor(record.id, column.key)">
                  <span :class="{ 'text-danger': isNegative(text) }">{{ formatNumericValue(text) }}</span>
                </div>
              </template>

              <!-- 动态检测列 -->
              <template v-else-if="column.key?.startsWith('detection')">
                <div class="cell-content" :style="{ backgroundColor: getCellColor(record.id, column.key) }"
                  @click="handleCellColor(record.id, column.key)">
                  <span>{{ formatNumericValue(text) }}</span>
                </div>
              </template>

              <!-- 动态带厚列 -->
              <template v-else-if="column.key?.startsWith('thickness') && column.key !== 'thicknessRange'">
                <div class="cell-content" :style="{ backgroundColor: getCellColor(record.id, column.key) }"
                  @click="handleCellColor(record.id, column.key)">
                  <span :class="{ 'text-danger': isNegative(text) }">{{ formatNumericValue(text) }}</span>
                </div>
              </template>

              <!-- 其他列 -->
              <template v-else>
                <div class="cell-content" :style="{ backgroundColor: getCellColor(record.id, column.key) }"
                  @click="handleCellColor(record.id, column.key)">
                  <NumericTableCell v-if="isNumericString(record[column.key])" :value="record[column.key]" />
                  <span v-else>{{ formatValue(record[column.key]) }}</span>
                </div>
              </template>
            </template>
          </BasicTable>
        </div>

      </div>
      <MagneticDataImportQuickModal @register="registerQuickModal" @reload="handleImportSuccess" />
    </div>
  </div>
</template>



<script lang="ts" setup>
import { ref, computed, onMounted, watch, nextTick } from 'vue';
import { BasicTable, useTable, BasicColumn } from '/@/components/Table';
import { useMessage } from '/@/hooks/web/useMessage';
import { dateUtil } from '/@/utils/dateUtil';
import {
  getIntermediateDataList,
  getProductSpecOptions,
  updatePerformance,
  updateAppearance,
  updateBaseInfo,
} from '/@/api/lab/intermediateData';
import {
  saveIntermediateDataColors,
  getIntermediateDataColors,
  saveIntermediateDataCellColor,
} from '../../../api/lab/intermediateDataColor';
import type { CellColorInfo } from '/@/api/lab/model/intermediateDataColorModel';
import { getAllAppearanceFeatureCategories, type AppearanceFeatureCategoryInfo } from '/@/api/lab/appearanceCategory';

import EditableCell from './components/EditableCell.vue';
import NumericTableCell from './components/NumericTableCell.vue';
import CustomSortControl from '../rawData/components/CustomSortControl.vue';

import { useModal } from '/@/components/Modal';
import { UploadOutlined } from '@ant-design/icons-vue';
import MagneticDataImportQuickModal from '../magneticData/MagneticDataImportQuickModal.vue';
import { createMagneticImportSession, uploadAndParseMagneticData } from '/@/api/lab/magneticData';



defineOptions({ name: 'IntermediateData' });

const { createMessage } = useMessage();

// 产品规格选项
const productSpecOptions = ref<any[]>([]);
const selectedProductSpecId = ref<string>('');

// 特性大类列表
const appearanceCategories = ref<AppearanceFeatureCategoryInfo[]>([]);

// 编辑状态
const editingCell = ref<{ id: string; field: string } | null>(null);
const editingValue = ref<any>(null);

// 颜色选择状态
const selectedColor = ref<string>('');
const coloredCells = ref<Record<string, string>>({}); // 存储单元格颜色 { 'rowId::field': 'color' }
const colorLoaded = ref<boolean>(false); // 颜色数据是否已加载
const savingColors = ref<boolean>(false); // 是否正在保存颜色
const isClearMode = ref<boolean>(false); // 是否处于清除颜色模式
const fillMode = ref<'cell' | 'row' | 'column'>('cell'); // 填充模式：单元格/整行/整列

// 存储表格数据用于行/列填充
const tableData = ref<any[]>([]);

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

// 合并所有列
const allColumns = computed(() => {
  // 获取当前产品规格的检测列数量
  const spec = productSpecOptions.value.find(item => item.id === selectedProductSpecId.value);
  const detectionColumns = spec?.detectionColumns || 15; // 默认15列

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
        },
      ],
    },
    {
      title: 'Hc (A/m)',
      dataIndex: 'perfHc',
      key: 'perfHc',
      width: 80,
      align: 'right',
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
        },
        {
          title: 'Ps铁损 (W/kg)',
          dataIndex: 'perfAfterPsLoss',
          key: 'perfAfterPsLoss',
          width: 90,
          align: 'right',
        },
        {
          title: 'Hc (A/m)',
          dataIndex: 'perfAfterHc',
          key: 'perfAfterHc',
          width: 80,
          align: 'right',
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
    { title: '带厚范围', dataIndex: 'thicknessMin', key: 'thicknessMin', width: 90, align: 'center' as const },
    { title: '~', dataIndex: 'thicknessRangeSep', key: 'thicknessRangeSep', width: 40, align: 'center' as const },
    { title: '带厚范围', dataIndex: 'thicknessMax', key: 'thicknessMax', width: 90, align: 'center' as const },
    // --- 规格与物理特性 ---
    { title: '带厚极差', dataIndex: 'thicknessDiff', key: 'thicknessDiff', width: 70, align: 'right' },
    { title: '密度 (g/cm³)', dataIndex: 'density', key: 'density', width: 70, align: 'right' },
    { title: '叠片系数', dataIndex: 'laminationFactor', key: 'laminationFactor', width: 70, align: 'right' },

    // --- 外观缺陷区（动态从特性大类获取） ---
    ...appearanceCategories.value
      .filter(category => !category.parentId || category.parentId === '-1') // 只显示顶级分类（大类）
      .sort((a, b) => (a.sortCode || 0) - (b.sortCode || 0)) // 按排序码排序
      .map(category => ({
        title: category.name,
        dataIndex: category.id,
        key: category.id,
        width: 50,
        align: 'center' as const,
      })),

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

  ];

  return columns;
});

const [registerTable, { reload, getForm }] = useTable({
  api: getIntermediateDataList,
  columns: allColumns,
  useSearchForm: true,
  immediate: false,
  scroll: { x: 4000 },
  bordered: true,
  size: 'small',
  pagination: false,
  showIndexColumn: false,
  formConfig: {
    baseColProps: { span: 6 },
    labelWidth: 100,
    showAdvancedButton: false,
    schemas: [
      {
        field: 'productSpecId',
        label: '产品规格',
        component: 'Select',
        colProps: { span: 4 },
        componentProps: {
          placeholder: '请选择产品规格',
          options: productSpecOptions,
          fieldNames: { label: 'name', value: 'id' },
          allowClear: false,
        },
      },
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
        defaultValue: (() => {
          const now = dateUtil();
          return [now.startOf('month'), now.endOf('month')];
        })(),
        componentProps: {
          placeholder: ['开始日期', '结束日期'],
          ranges: getDateRanges(),
        },
      },
    ],
    fieldMapToTime: [
      ['detectionDateRange', ['detectionStartDate', 'detectionEndDate'], 'YYYY-MM-DD'],
      ['prodDateRange', ['startDate', 'endDate'], 'YYYY-MM-DD'],
    ],
  },
  beforeFetch: params => {
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
    return params;
  },
  afterFetch: async data => {
    console.log('=== afterFetch called ===');
    console.log('Original data count:', Array.isArray(data) ? data.length : 0);
    console.log('selectedProductSpecId:', selectedProductSpecId.value);

    // 数据映射：将后端返回的 Detection1, Thickness1 等字段映射为小写
    if (Array.isArray(data)) {
      const mappedData = data.map(item => {
        const mapped: any = { ...item };

        // 映射检测列：Detection1 -> detection1 (支持大小写)
        const spec = productSpecOptions.value.find(s => s.id === selectedProductSpecId.value);
        const detectionCount = spec?.detectionColumns || 15;
        for (let i = 1; i <= detectionCount; i++) {
          // 尝试大写和小写两种格式
          const detectionUpper = item[`Detection${i}`];
          const detectionLower = item[`detection${i}`];
          if (detectionUpper !== undefined) {
            mapped[`detection${i}`] = detectionUpper;
          } else if (detectionLower !== undefined) {
            mapped[`detection${i}`] = detectionLower;
          }

          const thicknessUpper = item[`Thickness${i}`];
          const thicknessLower = item[`thickness${i}`];
          if (thicknessUpper !== undefined) {
            mapped[`thickness${i}`] = thicknessUpper;
          } else if (thicknessLower !== undefined) {
            mapped[`thickness${i}`] = thicknessLower;
          }
        }

        return mapped;
      });

      console.log('Mapped data count:', mappedData.length);

      // 加载颜色数据
      if (mappedData.length > 0) {
        const dataIds = mappedData.map(item => item.id);
        console.log('About to load colors for IDs:', dataIds.slice(0, 5), '...');
        console.log('Full ID list for debugging:', dataIds);

        // 检查ID格式 - 对比保存时使用的ID格式
        dataIds.forEach(id => {
          console.log('Data ID format check:', id, 'length:', id?.length, 'has dash:', id?.includes('-'));
          // 检查原始数据对象，看看是否有其他ID字段
          const originalItem = data.find(d => d.id === id);
          if (originalItem) {
            console.log('Original item keys:', Object.keys(originalItem));
            console.log('Original item ID:', originalItem.id);
            console.log('RawDataId field:', originalItem.rawDataId || originalItem.RawDataId);
          }
        });

        await loadColorsByIds(dataIds);

        // 强制触发视图更新
        await nextTick();
        // 创建一个延迟，确保颜色应用到DOM
        setTimeout(() => {
          console.log('Colors should be applied after delay');
        }, 100);
      } else {
        console.log('No data to load colors for');
      }

      // 保存表格数据用于行/列填充
      tableData.value = mappedData;
      return mappedData;
    }
    return data;
  },
});

// 外观特性字段（动态字段 + 固定字段）
const appearanceFields = computed(() => {
  const dynamicFields = appearanceCategories.value
    .filter(category => !category.parentId || category.parentId === '-1') // 只包含顶级分类（大类）
    .map(category => category.id);

  return [
    ...dynamicFields,
    'fishScale',
    'midSi',
    'midB',
    'leftPattern',
    'midPattern',
    'rightPattern',
    'breakCount',
    'singleCoilWeight',
    'appearEditorName',
  ];
});

function isAppearanceColumn(key: string) {
  return appearanceFields.value.includes(key);
}

function getAppearanceFieldType(key: string) {
  if (key === 'breakCount') return 'number';
  if (key === 'singleCoilWeight') return 'number';
  return 'text';
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
      // 设置表单默认值
      const form = getForm();
      if (form) {
        form.setFieldsValue({ productSpecId: selectedProductSpecId.value });
      }
      // 使用 nextTick 确保表格已经注册
      await nextTick();
      reload();
    }
  } catch (error) {
  }
}

// 监听产品规格变化
watch(selectedProductSpecId, async (newVal, oldVal) => {
  console.log('Product spec changed from', oldVal, 'to', newVal);
  if (newVal && newVal !== oldVal) {
    // 清空现有颜色数据
    coloredCells.value = {};
    console.log('Cleared color data due to spec change');

    // 重新加载表格数据（会触发afterFetch重新加载颜色）
    await nextTick();
    reload();
  }
});

// 处理排序变化（新的多字段排序）
function handleSortChange(newSortRules: any[]) {
  sortRules.value = newSortRules;
  // 重新加载表格数据
  reload();
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
async function handleCellSave2(record: any, field: string, value: any) {
  try {
    await updateBaseInfo({
      id: record.id,
      [field]: value,
    });
    createMessage.success('保存成功');
    reload();
  } catch (error) {
    createMessage.error('保存失败');
  }
}

// 性能数据保存
async function handlePerfSave(record: any, field: string, value: any) {
  try {
    await updatePerformance({
      id: record.id,
      [field]: value,
    });
    createMessage.success('保存成功');
    reload();
  } catch (error) {
    createMessage.error('保存失败');
  }
}

// 外观特性保存
async function handleAppearanceSave(record: any, field: string, value: any) {
  try {
    await updateAppearance({
      id: record.id,
      [field]: value,
    });
    createMessage.success('保存成功');
    reload();
  } catch (error) {
    createMessage.error('保存失败');
  }
}



// 格式化值
function formatValue(value: any) {
  if (value === null || value === undefined) return '-';
  if (typeof value === 'number') {
    return value.toFixed(2);
  }
  return value;
}

// 格式化数值
function formatNumericValue(value: any) {
  if (value === null || value === undefined) return '-';
  if (typeof value === 'number') {
    return value.toFixed(2);
  }
  const num = parseFloat(value);
  if (!isNaN(num)) {
    return num.toFixed(2);
  }
  return value || '-';
}

// 判断是否为负数
function isNegative(value: any): boolean {
  if (value === null || value === undefined) return false;
  const num = typeof value === 'number' ? value : parseFloat(value);
  return !isNaN(num) && num < 0;
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

// 监听产品规格变化
watch(
  selectedProductSpecId,
  async (newValue) => {
    if (newValue) {
      // 清除之前的颜色数据
      coloredCells.value = {};
      colorLoaded.value = false;

      const form = getForm();
      if (form) {
        form.setFieldsValue({ productSpecId: newValue });
        // 使用 nextTick 确保表格已经注册
        await nextTick();
        reload();
      }
    }
  }
);

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

// 获取单元格颜色
function getCellColor(rowId: string, field: string): string {
  const key = `${rowId}::${field}`;
  const color = coloredCells.value[key] || '';
  // 调试日志 - 仅在有颜色数据时打印
  if (Object.keys(coloredCells.value).length > 0 && !color) {
    // 只打印前几次查询，避免日志过多
    const existingKeys = Object.keys(coloredCells.value).slice(0, 3);
    console.log('getCellColor - key:', key, 'found:', !!color, 'sample keys:', existingKeys);
  }
  return color;
}

// 处理单元格颜色选择
async function handleCellColor(rowId: string, field: string) {
  console.log('=== handleCellColor called ===');
  console.log('rowId:', rowId, 'field:', field, 'selectedColor:', selectedColor.value, 'isClearMode:', isClearMode.value, 'fillMode:', fillMode.value);

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

  console.log('Cells to process:', cellsToProcess.length);

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

// 根据ID列表加载颜色数据
async function loadColorsByIds(dataIds: string[]) {
  console.log('=== loadColorsByIds called ===');
  console.log('selectedProductSpecId:', selectedProductSpecId.value);
  console.log('dataIds:', dataIds);

  if (!selectedProductSpecId.value || !dataIds || dataIds.length === 0) {
    console.log('Skip loading colors due to missing params');
    return;
  }

  try {
    console.log('Fetching colors for spec:', selectedProductSpecId.value, 'dataIds count:', dataIds.length);
    const response = await getIntermediateDataColors({
      productSpecId: selectedProductSpecId.value,
      intermediateDataIds: dataIds,
    });

    console.log('Color API response:', response);

    // 处理嵌套的响应结构 - colors 可能在 response.colors 或 response.data.colors
    const result = (response as any)?.data || response;
    const colors = result?.colors;

    if (colors && colors.length > 0) {
      console.log('Loading', colors.length, 'colors');
      // 创建新的对象以确保响应式更新
      const newColorMap: Record<string, string> = { ...coloredCells.value };
      colors.forEach((color: CellColorInfo) => {
        const key = `${color.intermediateDataId}::${color.fieldName}`;
        newColorMap[key] = color.colorValue;
        console.log('Set color for key:', key, 'value:', color.colorValue);
      });
      // 使用新对象替换，确保触发响应式更新
      coloredCells.value = newColorMap;
      console.log('coloredCells updated, keys:', Object.keys(coloredCells.value));
    } else {
      console.log('No colors returned from API');
      // 如果没有颜色数据，清空颜色映射
      coloredCells.value = {};
    }
  } catch (error) {
    console.error('加载颜色数据失败:', error);
  }
}

// 加载颜色数据
async function loadColors() {
  if (!selectedProductSpecId.value) return;

  try {
    const response = await getIntermediateDataColors({
      productSpecId: selectedProductSpecId.value,
    });

    // 处理嵌套的响应结构
    const result = (response as any)?.data || response;
    const colors = result?.colors;

    if (colors && colors.length > 0) {
      // 将颜色数据转换为Map格式
      const colorMap: Record<string, string> = {};
      colors.forEach((color: CellColorInfo) => {
        const key = `${color.intermediateDataId}::${color.fieldName}`;
        colorMap[key] = color.colorValue;
      });
      coloredCells.value = colorMap;
    }
    colorLoaded.value = true;
  } catch (error) {
    console.error('加载颜色数据失败:', error);
    colorLoaded.value = false;
  }
}

// 保存当前颜色配置（批量保存）
async function saveColorsBatch() {
  if (!selectedProductSpecId.value) return;

  savingColors.value = true;
  try {
    const colors: CellColorInfo[] = Object.entries(coloredCells.value).map(([key, color]) => {
      const [intermediateDataId, fieldName] = key.split('::');
      console.log('Batch save - ID from key:', intermediateDataId, 'length:', intermediateDataId?.length);
      return {
        intermediateDataId,
        fieldName,
        colorValue: color,
      };
    });

    console.log('Batch saving colors count:', colors.length);
    console.log('First few colors:', colors.slice(0, 3));

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

onMounted(() => {
  loadProductSpecOptions();
  loadAppearanceCategories();
});

// --- 磁性数据导入相关逻辑 ---
const [registerQuickModal, { openModal: openQuickModal }] = useModal();

function handleImportSuccess() {
  createMessage.success('导入完成，相关数据已更新');
  reload(); // 刷新中间数据表
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
.table-toolbar {
  margin-bottom: 1px;
  display: flex;
  align-items: center;
  gap: 16px;
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

.editable-cell {
  cursor: pointer;
  padding: 4px px;
  border-radius: 4px;
}

.editable-cell:hover {
  background: #f0f0f0;
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
</style>
