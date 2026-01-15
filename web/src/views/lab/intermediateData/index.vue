<template>
  <div class="page-content-wrapper">
    <div class="page-content-wrapper-center">
      <div class="page-content-wrapper-content">
        <!-- 产品规格选择 -->
        <div class="product-spec-selector">
          <span class="label">产品规格：</span>
          <a-select
            v-model:value="selectedProductSpecId"
            placeholder="请选择产品规格"
            style="width: 200px"
            @change="handleProductSpecChange">
            <a-select-option v-for="spec in productSpecOptions" :key="spec.id" :value="spec.id">
              {{ spec.name }}
            </a-select-option>
          </a-select>
          <a-button type="primary" style="margin-left: 16px" :loading="generating" @click="handleGenerateData">
            <template #icon><SyncOutlined /></template>
            生成中间数据
          </a-button>
        </div>

        <!-- 主表格 -->
        <BasicTable @register="registerTable">
          <template #headerCell="{ column }">
            <!-- 只处理叶子列（没有children的列），分组表头使用默认显示 -->
            <template v-if="!column.children">
              <template v-if="column.key === 'perfSsPower'">
                <div style="line-height: 1.2">
                  <div>Ss激磁功率</div>
                  <div style="font-size: 11px; color: #999">(VA/kg)</div>
                </div>
              </template>
              <template v-else-if="column.key === 'perfPsLoss'">
                <div style="line-height: 1.2">
                  <div>Ps铁损</div>
                  <div style="font-size: 11px; color: #999">(W/kg)</div>
                </div>
              </template>
              <template v-else-if="column.key === 'perfHc'">
                <div style="line-height: 1.2">
                  <div>Hc</div>
                  <div style="font-size: 11px; color: #999">(A/m)</div>
                </div>
              </template>
              <template v-else-if="column.key === 'perfAfterSsPower'">
                <div style="line-height: 1.2">
                  <div>Ss激磁功率</div>
                  <div style="font-size: 11px; color: #999">(VA/kg)</div>
                </div>
              </template>
              <template v-else-if="column.key === 'perfAfterPsLoss'">
                <div style="line-height: 1.2">
                  <div>Ps铁损</div>
                  <div style="font-size: 11px; color: #999">(W/kg)</div>
                </div>
              </template>
              <template v-else-if="column.key === 'perfAfterHc'">
                <div style="line-height: 1.2">
                  <div>Hc</div>
                  <div style="font-size: 11px; color: #999">(A/m)</div>
                </div>
              </template>
              <template v-else>
                {{ column.title }}
              </template>
            </template>
          </template>
          <template #bodyCell="{ column, record }">
            <!-- 日期列 -->
            <template v-if="column.key === 'dateMonth'">
              <a-input
                v-if="editingCell?.id === record.id && editingCell?.field === 'dateMonth'"
                v-model:value="editingValue"
                size="small"
                style="width: 100px"
                @blur="handleCellBlur"
                @press-enter="handleCellSave" />
              <span v-else @dblclick="handleCellEdit(record, 'dateMonth', record.dateMonth)" class="editable-cell">
                {{ record.dateMonth || '-' }}
              </span>
            </template>

            <!-- 喷次列 -->
            <template v-else-if="column.key === 'sprayNo'">
              {{ record.sprayNo || '-' }}
            </template>

            <!-- 炉号列 -->
            <template v-else-if="column.key === 'furnaceNo'">
              {{ record.furnaceNo || '-' }}
            </template>

            <!-- 带厚分布子表 -->
            <template v-else-if="column.key === 'thicknessDistList'">
              <ThicknessSubTable :data="record.thicknessDistList || []" />
            </template>

            <!-- 叠片系数分布子表 -->
            <template v-else-if="column.key === 'laminationDistList'">
              <LaminationSubTable :data="record.laminationDistList || []" />
            </template>

            <!-- 性能数据列 -->
            <template v-else-if="column.key?.startsWith('perf')">
              <EditableCell
                :record="record"
                :field="column.key"
                :value="record[column.key]"
                type="number"
                @save="val => handlePerfSave(record, column.key, val)" />
            </template>

            <!-- 外观特性列 -->
            <template v-else-if="isAppearanceColumn(column.key)">
              <EditableCell
                :record="record"
                :field="column.key"
                :value="record[column.key]"
                :type="getAppearanceFieldType(column.key)"
                @save="val => handleAppearanceSave(record, column.key, val)" />
            </template>

            <!-- 判定列 -->
            <template v-else-if="column.key?.endsWith('Result')">
              <a-tag :color="getResultColor(record[column.key])">
                {{ record[column.key] || '-' }}
              </a-tag>
            </template>

            <!-- 录入人/编辑人 -->
            <template v-else-if="column.key === 'creatorUserName' || column.key?.includes('EditorName')">
              {{ record[column.key] || '-' }}
            </template>

            <!-- 其他数值列 -->
            <template v-else>
              {{ formatValue(record[column.key]) }}
            </template>
          </template>
        </BasicTable>

        <!-- 生成数据弹窗 -->
        <GenerateModal
          v-model:visible="generateModalVisible"
          :product-spec-id="selectedProductSpecId"
          :product-spec-options="productSpecOptions"
          @success="handleGenerateSuccess"
        />
      </div>
    </div>
  </div>
</template>

<script lang="ts" setup>
  import { ref, computed, onMounted } from 'vue';
  import { BasicTable, useTable, BasicColumn } from '/@/components/Table';
  import { useMessage } from '/@/hooks/web/useMessage';
  import { SyncOutlined } from '@ant-design/icons-vue';
  import {
    getIntermediateDataList,
    getProductSpecOptions,
    updatePerformance,
    updateAppearance,
    updateBaseInfo,
  } from '/@/api/lab/intermediateData';
  import ThicknessSubTable from './components/ThicknessSubTable.vue';
  import LaminationSubTable from './components/LaminationSubTable.vue';
  import EditableCell from './components/EditableCell.vue';
  import GenerateModal from './components/GenerateModal.vue';

  defineOptions({ name: 'IntermediateData' });

  const { createMessage } = useMessage();

  // 产品规格选项
  const productSpecOptions = ref<any[]>([]);
  const selectedProductSpecId = ref<string>('');

  // 编辑状态
  const editingCell = ref<{ id: string; field: string } | null>(null);
  const editingValue = ref<any>(null);

  // 生成数据
  const generating = ref(false);
  const generateModalVisible = ref(false);

  // 基础列配置（固定列部分）
  const baseFixedColumns: BasicColumn[] = [
    { title: '日期', dataIndex: 'dateMonth', key: 'dateMonth', width: 90, fixed: 'left' },
    { title: '喷次', dataIndex: 'sprayNo', key: 'sprayNo', width: 150, fixed: 'left' },
    { title: '炉号', dataIndex: 'furnaceNo', key: 'furnaceNo', width: 150, fixed: 'left' },
  ];

  // 基础列配置（非固定列部分）
  const baseColumns: BasicColumn[] = [
    { title: '一米重(g)', dataIndex: 'oneMeterWeight', key: 'oneMeterWeight', width: 130, align: 'right' },
    { title: '带宽(mm)', dataIndex: 'stripWidth', key: 'stripWidth', width: 110, align: 'right' },
    { title: '带厚分布', dataIndex: 'thicknessDistList', key: 'thicknessDistList', width: 180 },
    { title: '带厚范围', dataIndex: 'thicknessRange', key: 'thicknessRange', width: 110 },
    { title: '带厚极差', dataIndex: 'thicknessDiff', key: 'thicknessDiff', width: 90, align: 'right' },
    { title: '平均厚度', dataIndex: 'avgThickness', key: 'avgThickness', width: 90, align: 'right' },
    { title: '密度', dataIndex: 'density', key: 'density', width: 70, align: 'right' },
    { title: '叠片系数', dataIndex: 'laminationFactor', key: 'laminationFactor', width: 110, align: 'right' },
    { title: '叠片系数分布', dataIndex: 'laminationDistList', key: 'laminationDistList', width: 180 },
    { title: '四米重(g)', dataIndex: 'fourMeterWeight', key: 'fourMeterWeight', width: 130, align: 'right' },
    { title: '最大厚度', dataIndex: 'maxThicknessRaw', key: 'maxThicknessRaw', width: 110, align: 'right' },
    { title: '带型', dataIndex: 'stripType', key: 'stripType', width: 70, align: 'right' },
    { title: '分段', dataIndex: 'segmentType', key: 'segmentType', width: 70, align: 'center' },
  ];

  // 性能数据列（使用分组表头，固定列）
  const performanceColumns: BasicColumn[] = [
    {
      title: '1.35T',
      key: 'perf_1_35t',
      align: 'center',
      fixed: 'left',
      children: [
        {
          title: 'Ss激磁功率',
          dataIndex: 'perfSsPower',
          key: 'perfSsPower',
          width: 120,
          align: 'right',
          fixed: 'left',
        },
      ],
    },
    {
      title: '50Hz',
      key: 'perf_50hz',
      align: 'center',
      fixed: 'left',
      children: [
        {
          title: 'Ps铁损',
          dataIndex: 'perfPsLoss',
          key: 'perfPsLoss',
          width: 100,
          align: 'right',
          fixed: 'left',
        },
      ],
    },
    {
      title: 'Hc',
      key: 'perf_hc',
      align: 'center',
      fixed: 'left',
      children: [
        {
          title: 'Hc',
          dataIndex: 'perfHc',
          key: 'perfHc',
          width: 90,
          align: 'right',
          fixed: 'left',
        },
      ],
    },
    {
      title: '刻痕后性能',
      key: 'perf_after',
      align: 'center',
      fixed: 'left',
      children: [
        {
          title: 'Ss激磁功率',
          dataIndex: 'perfAfterSsPower',
          key: 'perfAfterSsPower',
          width: 120,
          align: 'right',
          fixed: 'left',
        },
        {
          title: 'Ps铁损',
          dataIndex: 'perfAfterPsLoss',
          key: 'perfAfterPsLoss',
          width: 100,
          align: 'right',
          fixed: 'left',
        },
        {
          title: 'Hc',
          dataIndex: 'perfAfterHc',
          key: 'perfAfterHc',
          width: 90,
          align: 'right',
          fixed: 'left',
        },
      ],
    },
    { title: '性能判定人', dataIndex: 'perfJudgeName', key: 'perfJudgeName', width: 90, fixed: 'left' },
    { title: '性能编辑人', dataIndex: 'perfEditorName', key: 'perfEditorName', width: 90, fixed: 'left' },
  ];

  // 判定列
  const resultColumns: BasicColumn[] = [
    { title: '磁性能判定', dataIndex: 'magneticResult', key: 'magneticResult', width: 90, align: 'center' },
    { title: '厚度判定', dataIndex: 'thicknessResult', key: 'thicknessResult', width: 90, align: 'center' },
    { title: '叠片系数判定', dataIndex: 'laminationResult', key: 'laminationResult', width: 100, align: 'center' },
  ];

  // 外观特性列
  const appearanceColumns: BasicColumn[] = [
    { title: '外观特性', dataIndex: 'appearanceFeature', key: 'appearanceFeature', width: 100 },
    { title: '韧性', dataIndex: 'toughness', key: 'toughness', width: 70, align: 'center' },
    { title: '鱼鳞纹', dataIndex: 'fishScale', key: 'fishScale', width: 70, align: 'center' },
    { title: '中Si', dataIndex: 'midSi', key: 'midSi', width: 70, align: 'center' },
    { title: '中B', dataIndex: 'midB', key: 'midB', width: 70, align: 'center' },
    { title: '左花纹', dataIndex: 'leftPattern', key: 'leftPattern', width: 70, align: 'center' },
    { title: '中花纹', dataIndex: 'midPattern', key: 'midPattern', width: 70, align: 'center' },
    { title: '右花纹', dataIndex: 'rightPattern', key: 'rightPattern', width: 70, align: 'center' },
    { title: '断头数', dataIndex: 'breakCount', key: 'breakCount', width: 70, align: 'right' },
    { title: '单卷重(kg)', dataIndex: 'coilWeightKg', key: 'coilWeightKg', width: 130, align: 'right' },
    { title: '外观检验员', dataIndex: 'appearJudgeName', key: 'appearJudgeName', width: 90 },
    { title: '外观编辑人', dataIndex: 'appearEditorName', key: 'appearEditorName', width: 90 },
  ];

  // 其他列
  const otherColumns: BasicColumn[] = [
    { title: '录入人', dataIndex: 'creatorUserName', key: 'creatorUserName', width: 90 },
    { title: '备注', dataIndex: 'remark', key: 'remark', width: 120 },
  ];

  // 合并所有列（性能列放在炉号之后，固定列）
  const allColumns = computed(() => [
    ...baseFixedColumns,
    ...performanceColumns,
    ...baseColumns,
    ...resultColumns,
    ...appearanceColumns,
    ...otherColumns,
  ]);

  const [registerTable, { reload }] = useTable({
    api: getIntermediateDataList,
    columns: allColumns.value,
    useSearchForm: true,
    immediate: false,
    scroll: { x: 3000 },
    formConfig: {
      baseColProps: { span: 6 },
      labelWidth: 100,
      showAdvancedButton: false,
      schemas: [
        {
          field: 'keyword',
          label: '关键词',
          component: 'Input',
          colProps: { span: 6 },
          componentProps: {
            placeholder: '炉号、产线等',
            submitOnPressEnter: true,
          },
        },
        {
          field: 'prodDateRange',
          label: '生产日期',
          component: 'DateRange',
          colProps: { span: 8 },
          componentProps: {
            placeholder: ['开始日期', '结束日期'],
          },
        },
        {
          field: 'dateMonth',
          label: '月份',
          component: 'MonthPicker',
          colProps: { span: 4 },
          componentProps: {
            placeholder: '选择月份',
            format: 'YYYY-MM',
            valueFormat: 'YYYY-MM',
          },
        },
      ],
      fieldMapToTime: [['prodDateRange', ['startDate', 'endDate'], 'YYYY-MM-DD']],
    },
    beforeFetch: params => {
      params.productSpecId = selectedProductSpecId.value;
      return params;
    },
  });

  // 外观特性字段
  const appearanceFields = [
    'toughness',
    'fishScale',
    'midSi',
    'midB',
    'leftPattern',
    'midPattern',
    'rightPattern',
    'breakCount',
    'coilWeightKg',
    'appearJudgeName',
  ];

  function isAppearanceColumn(key: string) {
    return appearanceFields.includes(key);
  }

  function getAppearanceFieldType(key: string) {
    if (key === 'breakCount') return 'number';
    if (key === 'coilWeightKg') return 'number';
    return 'text';
  }

  // 加载产品规格选项
  async function loadProductSpecOptions() {
    try {
      const result = await getProductSpecOptions();
      productSpecOptions.value = result.data || result || [];
      if (productSpecOptions.value.length > 0 && !selectedProductSpecId.value) {
        selectedProductSpecId.value = productSpecOptions.value[0].id;
        reload();
      }
    } catch (error) {
      console.error('加载产品规格失败', error);
    }
  }

  // 产品规格变化
  function handleProductSpecChange() {
    reload();
  }

  // 生成数据
  function handleGenerateData() {
    if (!selectedProductSpecId.value) {
      createMessage.warning('请先选择产品规格');
      return;
    }
    generateModalVisible.value = true;
  }

  // 生成成功
  function handleGenerateSuccess() {
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

  // 判定结果颜色
  function getResultColor(result: string) {
    if (!result) return 'default';
    if (result === '合格' || result === '通过') return 'success';
    if (result === '不合格' || result === '不通过') return 'error';
    return 'warning';
  }

  // 格式化值
  function formatValue(value: any) {
    if (value === null || value === undefined) return '-';
    if (typeof value === 'number') {
      return value.toFixed(2);
    }
    return value;
  }

  onMounted(() => {
    loadProductSpecOptions();
  });
</script>

<style scoped>
  .product-spec-selector {
    display: flex;
    align-items: center;
    margin-bottom: 16px;
    padding: 12px 16px;
    background: #fafafa;
    border-radius: 4px;
  }

  .product-spec-selector .label {
    font-weight: 500;
    margin-right: 8px;
  }

  .editable-cell {
    cursor: pointer;
    padding: 4px 8px;
    border-radius: 4px;
  }

  .editable-cell:hover {
    background: #f0f0f0;
  }
</style>
