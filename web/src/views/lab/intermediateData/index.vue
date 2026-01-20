<template>
  <div class="page-content-wrapper">
    <div class="page-content-wrapper-center">
      <div class="page-content-wrapper-content">
        <!-- 自定义排序控制 -->
        <div class="table-toolbar">
          <CustomSortControl v-model="sortRules" @change="handleSortChange" />
        </div>

        <!-- 主表格 -->
        <div class="table-container">
          <BasicTable @register="registerTable">
            <template #bodyCell="{ column, record, text }">
              <!-- 贴标列 - 特殊样式处理 -->
              <template v-if="column.key === 'labeling'">
                <div :class="['status-cell', (record.labeling || text) === '性能不合' ? 'bg-red' : '']">
                  <EditableCell :record="record" :field="column.key" :value="record.labeling || text"
                    @save="val => handleCellSave2(record, column.key, val)" />
                </div>
              </template>

              <!-- 日期列 -->
              <template v-else-if="column.key === 'dateMonth'">
                <a-input v-if="editingCell?.id === record.id && editingCell?.field === 'dateMonth'"
                  v-model:value="editingValue" size="small" style="width: 100px" @blur="handleCellBlur"
                  @press-enter="handleCellSave" />
                <span v-else @dblclick="handleCellEdit(record, 'dateMonth', record.dateMonth)" class="editable-cell">
                  {{ record.dateMonth || record.prodDateStr || '-' }}
                </span>
              </template>

              <!-- 日期字符串列（检测日期、生产日期等） -->
              <template v-else-if="column.key === 'detectionDateStr' || column.key === 'prodDateStr'">
                <span>{{ text || '-' }}</span>
              </template>

              <!-- 性能数据列 -->
              <template v-else-if="column.key?.startsWith('perf')">
                <EditableCell :record="record" :field="column.key" :value="record[column.key]" type="number"
                  @save="val => handlePerfSave(record, column.key, val)" />
              </template>

              <!-- 外观特性列 -->
              <template v-else-if="isAppearanceColumn(column.key)">
                <EditableCell :record="record" :field="column.key" :value="record[column.key]"
                  :type="getAppearanceFieldType(column.key)"
                  @save="val => handleAppearanceSave(record, column.key, val)" />
              </template>

              <!-- 数值列 - 负数红色显示 -->
              <template v-else-if="isNumericColumn(column.key)">
                <span :class="{ 'text-danger': isNegative(text) }">{{ formatNumericValue(text) }}</span>
              </template>

              <!-- 动态检测列 -->
              <template v-else-if="column.key?.startsWith('detection')">
                <span>{{ formatNumericValue(text) }}</span>
              </template>

              <!-- 动态带厚列 -->
              <template v-else-if="column.key?.startsWith('thickness') && column.key !== 'thicknessRange'">
                <span :class="{ 'text-danger': isNegative(text) }">{{ formatNumericValue(text) }}</span>
              </template>

              <!-- 其他列 -->
              <template v-else>
                <NumericTableCell v-if="isNumericString(record[column.key])" :value="record[column.key]" />
                <span v-else>{{ formatValue(record[column.key]) }}</span>
              </template>
            </template>
          </BasicTable>
        </div>

      </div>
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
import { getAllAppearanceFeatureCategories, type AppearanceFeatureCategoryInfo } from '/@/api/lab/appearanceCategory';
import EditableCell from './components/EditableCell.vue';
import NumericTableCell from './components/NumericTableCell.vue';
import CustomSortControl from '../rawData/components/CustomSortControl.vue';

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
  afterFetch: data => {
    // 数据映射：将后端返回的 Detection1, Thickness1 等字段映射为小写
    if (Array.isArray(data)) {
      return data.map(item => {
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

onMounted(() => {
  loadProductSpecOptions();
  loadAppearanceCategories();
});
</script>

<style scoped>
.table-toolbar {
  margin-bottom: 16px;
}

.editable-cell {
  cursor: pointer;
  padding: 4px 8px;
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
