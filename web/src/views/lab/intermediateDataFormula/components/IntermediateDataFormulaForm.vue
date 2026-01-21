<template>
  <BasicModal
    v-bind="$attrs"
    @register="registerModal"
    :title="getTitle"
    @ok="handleSubmit"
    @cancel="handleCancel"
    :width="1200"
    :minHeight="600"
    :bodyStyle="{ height: 'calc(100vh - 200px)', padding: '16px' }">
    <BasicForm @register="registerForm"></BasicForm>
    <FormulaBuilder @register="registerFormulaBuilderModal" @save="handleFormulaBuilderSave" />
  </BasicModal>
</template>

<script lang="ts" setup>
import { ref, computed, h, nextTick, watch } from 'vue';
import { BasicForm, useForm } from '/@/components/Form';
import { useMessage } from '/@/hooks/web/useMessage';
import type { IntermediateDataFormula } from '/@/api/lab/types/intermediateDataFormula';
import { validateFormula, updateIntermediateDataFormula, getAvailableColumns } from '/@/api/lab/intermediateDataFormula';
import { getUnitById } from '/@/utils/lab/unit';
import UnitSelect from '/@/components/Lab/UnitSelect.vue';
import AdvancedJudgmentEditor from './AdvancedJudgmentEditor.vue';
import FormulaBuilder from './FormulaBuilder.vue';
import { BasicModal, useModalInner, useModal } from '/@/components/Modal';

const emit = defineEmits(['register', 'reload']);

const recordId = ref<string>('');
const formMode = ref<'Attributes' | 'Formula'>('Attributes');
const originalRecord = ref<IntermediateDataFormula | null>(null);
const availableFields = ref<any[]>([]);
const fieldsRefreshKey = ref(0); // 用于强制刷新表单
const isFormReady = ref(false); // 跟踪表单是否已准备好

// 监听 availableFields 变化，确保表单能响应式更新
watch(() => availableFields.value, async (newFields) => {
  if (newFields && newFields.length > 0 && isFormReady.value) {
    await nextTick();
    fieldsRefreshKey.value++;
    
    // 强制更新 formula 字段，触发重新渲染
    // 确保表单已经注册后再调用 updateSchema
    await nextTick();
    try {
      await updateSchema({
        field: 'formula',
        componentProps: {
          _updateTime: new Date().getTime()
        }
      });
    } catch (error) {
      // 如果表单还未准备好，延迟重试
      setTimeout(async () => {
        try {
          await nextTick();
          await updateSchema({
            field: 'formula',
            componentProps: {
              _updateTime: new Date().getTime()
            }
          });
        } catch (e) {
          // 忽略错误，表单可能还未完全初始化
        }
      }, 100);
    }
  }
}, { deep: true });

async function refreshAvailableFields() {
  try {
    const response = await getAvailableColumns(true);
    
    // 检查 response 的结构 - 可能需要访问 response.data
    // 使用 any 类型来处理可能的嵌套结构
    let dataArray: any = response;
    if (response && (response as any).data && Array.isArray((response as any).data)) {
      dataArray = (response as any).data;
    }
    
    if (dataArray && Array.isArray(dataArray)) {
      availableFields.value = dataArray.map(item => ({
        id: item.columnName,
        name: item.displayName || item.columnName,
        code: item.columnName,
        columnName: item.columnName,
        displayName: item.displayName
      }));
      
    } else {
      console.error('[Form] 返回数据格式不正确:', response);
    }
  } catch (e) {
    console.error('[Form] 获取可用字段失败:', e);
  }
}

const [registerFormulaBuilderModal, { openModal: openFormulaBuilderModal }] = useModal();
let currentFormulaCallback: ((formula: string) => void) | null = null;

// 处理打开公式编辑器
function handleOpenFormulaEditor(data: {
  currentValue: string;
  conditionId: string;
  onSave: (formula: string) => void;
}) {
  currentFormulaCallback = data.onSave;
  openFormulaBuilderModal(true, {
    record: {
      formula: data.currentValue || '',
      formulaName: '条件表达式',
      columnName: `condition_${data.conditionId}`,
    }
  });
}

function handleFormulaBuilderSave(data: { id: string; formula: string }) {
  if (currentFormulaCallback) {
    currentFormulaCallback(data.formula);
    currentFormulaCallback = null;
  }
}

const [registerForm, { setFieldsValue, resetFields, validate, getFieldsValue, updateSchema }] = useForm({
  schemas: [
    {
      field: 'tableName',
      label: '表名',
      component: 'Select',
      componentProps: {
        disabled: true,
        options: [
          { label: '中间数据表', value: 'INTERMEDIATE_DATA' },
        ],
      },
      defaultValue: 'INTERMEDIATE_DATA',
      ifShow: () => formMode.value === 'Attributes',
    },
    {
      field: 'columnName',
      label: '中间数据表列',
      component: 'Input',
      componentProps: {
        disabled: true,
      },
      ifShow: () => formMode.value === 'Attributes',
    },
    {
      field: 'formulaName',
      label: '公式名称',
      component: 'Input',
      componentProps: {
        disabled: true,
      },
      ifShow: () => formMode.value === 'Attributes',
    },
    // --- 属性 / 公式 通用字段 ---
    {
      field: 'formulaType',
      label: '类型',
      component: 'Select',
      componentProps: {
        options: [
          { label: 'CALC-计算公式', value: 'CALC' },
          { label: 'JUDGE-判定公式', value: 'JUDGE' },
          { label: 'NO-只展示', value: 'NO' },
        ],
        fieldNames: { label: 'label', value: 'value' },
        onChange: (value: string) => {
          // 当切换为判定公式时，强制使用 EXCEL 语言，并隐藏公式语言下拉
          if (value === 'JUDGE') {
            setFieldsValue({ formulaLanguage: 'EXCEL', defaultValue: '', formula: '' });
          } else if (value === 'NO') {
            // 只展示类型不需要公式
            setFieldsValue({ defaultValue: '', formula: '' });
          } else {
            // 计算公式默认值为 0
            setFieldsValue({ defaultValue: '0', formula: '' });
          }
        },
      },
      defaultValue: 'CALC',
      rules: [{ required: true, trigger: 'change', message: '必填' }],
      ifShow: () => formMode.value === 'Attributes',
    },
    // --- 属性模式字段 ---
    {
      field: 'unitId',
      label: '单位',
      component: 'Input',
      ifShow: () => formMode.value === 'Attributes',
      render: (values) => {
        const unitId = values.values?.unitId || values.model?.unitId;
        return h(UnitSelect, {
          modelValue: unitId || undefined,
          placeholder: '请选择单位（可选）',
          'onUpdate:modelValue': (value: string | undefined) => {
            setFieldsValue({ unitId: value || undefined });
            if (value) {
              getUnitById(value).then(unit => {
                if (unit) {
                  setFieldsValue({ unitName: unit.symbol });
                }
              });
            } else {
              setFieldsValue({ unitName: undefined });
            }
          },
        });
      },
    },
    {
      field: 'precision',
      label: '小数点保留位数',
      component: 'InputNumber',
      ifShow: () => formMode.value === 'Attributes',
      componentProps: {
        placeholder: '请输入小数位数',
        min: 0,
        max: 10,
      },
    },
    {
      field: 'sortOrder',
      label: '计算优先级',
      component: 'InputNumber',
      ifShow: () => formMode.value === 'Attributes',
      componentProps: {
        placeholder: '数值越小优先级越高',
        min: 0,
      },
      helpMessage: '数值越小优先级越高，按顺序计算',
      defaultValue: 0,
    },
    {
      field: 'isEnabled',
      label: '是否启用',
      component: 'Switch',
      ifShow: () => formMode.value === 'Attributes',
      defaultValue: true,
    },
    {
      field: 'defaultValue',
      label: '默认值',
      component: 'Input',
      ifShow: () => formMode.value === 'Attributes',
      componentProps: {
        placeholder: () => {
          const formulaType = getFieldsValue().formulaType;
          return formulaType === 'JUDGE' ? '判定公式默认为空' : '计算公式默认为0';
        },
      },
      helpMessage: '计算公式默认值为0，判定公式默认值为空',
    },
    {
      field: 'remark',
      label: '备注',
      component: 'Input',
      ifShow: () => formMode.value === 'Attributes',
      componentProps: {
        placeholder: '请输入备注信息',
        type: 'textarea',
        rows: 2,
      },
    },
    // --- 公式模式字段 ---
    {
      field: 'formula',
      label: '',
      component: 'Input',
      ifShow: () => formMode.value === 'Formula' && getFieldsValue().formulaType !== 'NO',
      colProps: { span: 24 },
      labelWidth: 0,
      wrapperCol: { span: 24 },
      render: ({ model, field }) => {
        const formulaType = model['formulaType'] || originalRecord.value?.formulaType || 'CALC';
        if (formulaType === 'JUDGE') {
          // 直接访问 availableFields.value 确保响应式更新
          // 使用 fieldsRefreshKey 作为 key 的一部分，确保数据更新时组件重新渲染
          return h('div', { 
            style: { 
              width: '100%',
              maxWidth: '100%',
              boxSizing: 'border-box'
            } 
          }, [
            h(AdvancedJudgmentEditor, {
              key: `judgment-editor-${fieldsRefreshKey.value}-${availableFields.value.length}`,
              value: model[field],
              defaultValue: model['defaultValue'],
              fields: availableFields.value,
              'onUpdate:value': (val: string) => {
                  model[field] = val;
                  setFieldsValue({ formula: val });
              },
              'onUpdate:defaultValue': (val: string) => {
                  model['defaultValue'] = val;
                  setFieldsValue({ defaultValue: val });
              },
              onOpenFormulaEditor: handleOpenFormulaEditor,
            })
          ]);
        }
        
         return h('div', [
             h('textarea', {
                 value: model[field],
                 class: 'ant-input',
                 rows: 8,
                 placeholder: '请输入公式表达式',
                 onInput: (e: any) => {
                     model[field] = e.target.value;
                     setFieldsValue({ formula: e.target.value });
                 }
             })
         ]);
      },
      rules: [
        {
          validator: async (_rule, value) => {
            const formulaType = getFieldsValue().formulaType || originalRecord.value?.formulaType;
            
            // 对于判定公式，如果没有值也不报错（允许为空）
            if (!value) {
              if (formulaType === 'JUDGE') {
                return Promise.resolve();
              }
              return Promise.reject('必填');
            }
            
            if (formulaType === 'JUDGE') {
              try {
                const rules = JSON.parse(value);
                if (!Array.isArray(rules)) {
                  return Promise.reject('判定规则格式错误');
                }
              } catch {
                return Promise.reject('判定规则格式错误');
              }
              return Promise.resolve();
            }

            try {
              const result = await validateFormula({
                formula: value,
                formulaLanguage: getFieldsValue().formulaLanguage || 'EXCEL',
                columnName: getFieldsValue().columnName || originalRecord.value?.columnName,
              });
              if (!result.isValid) {
                return Promise.reject(result.errorMessage || '公式验证失败');
              }
            } catch (error: any) {
              return Promise.reject(error.message || '公式验证失败');
            }
          },
          trigger: 'blur',
        },
      ],
    },
    {
      field: 'formulaLanguage',
      label: '公式语言',
      component: 'Select',
      ifShow: () =>
        formMode.value === 'Formula' &&
        getFieldsValue().formulaType === 'CALC' &&
        getFieldsValue().formulaType !== 'NO',
      componentProps: {
        placeholder: '请选择公式语言',
        options: [
          { label: 'EXCEL', value: 'EXCEL' },
          { label: 'MATH', value: 'MATH' },
        ],
      },
      defaultValue: 'EXCEL',
      rules: [{ required: true, trigger: 'change', message: '必填' }],
    },
  ],
  labelWidth: 120,
  wrapperCol: { span: 18 },
  showActionButtonGroup: false,
});

const [registerModal, { setModalProps, closeModal }] = useModalInner(async (data) => {
  resetFields();
  setModalProps({ confirmLoading: false });
  recordId.value = data?.record?.id || '';
  formMode.value = data?.mode || 'Attributes';
  originalRecord.value = data?.record as IntermediateDataFormula | null;
  
  // 标记表单已准备好（resetFields 调用后表单应该已经注册）
  await nextTick();
  isFormReady.value = true;
  
  // Refresh fields when modal opens
  refreshAvailableFields();

  if (data?.record) {
    const record = data.record as IntermediateDataFormula;
    // 计算默认值的回退值：CALC类型默认为'0'，JUDGE类型默认为空
    const formulaType = record.formulaType || 'CALC';
    const defaultValueFallback = formulaType === 'CALC' ? '0' : '';
    
    setFieldsValue({
      tableName: record.tableName,
      columnName: record.columnName,
      formulaName: record.formulaName,
      
      // Attributes
      formulaType: formulaType,
      unitId: record.unitId,
      unitName: record.unitName,
      precision: record.precision,
      sortOrder: record.sortOrder,
      isEnabled: record.isEnabled,
      defaultValue: record.defaultValue ?? defaultValueFallback,
      remark: record.remark,

      // Formula
      formula: record.formula,
      formulaLanguage: record.formulaLanguage || 'EXCEL',
    });
  }
});

const { createMessage } = useMessage();

const getTitle = computed(() => {
  if (formMode.value === 'Attributes') {
    return '编辑属性';
  } else {
    // 公式模式下，显示列名或公式名称
    const columnName = originalRecord.value?.formulaName || originalRecord.value?.columnName || '';
    return columnName ? `编辑公式 - ${columnName}` : '编辑公式';
  }
});

const handleSubmit = async () => {
  try {
    const values = await validate();
    setModalProps({ confirmLoading: true });
    
    // sortOrder is now in values, no need to manually preserve
    
    await updateIntermediateDataFormula(recordId.value, values);
    createMessage.success('更新成功');

    closeModal();
    emit('reload');
  } catch (error: any) {
    console.error('提交失败:', error);
    createMessage.error(error.message || '提交失败');
  } finally {
    setModalProps({ confirmLoading: false });
  }
};

const handleCancel = () => {
  closeModal();
};
</script>

<style scoped lang="less">
:deep(.ant-form-item) {
  .ant-col.ant-form-item-control {
    &.ant-col-18 {
      max-width: 100%;
    }
  }
}

:deep(.ant-form-item-control-input-content) {
  width: 100%;
  
  > div {
    width: 100%;
  }
}

// 隐藏模态框的滚动条但保持滚动功能
:deep(.crollbar) {
  .crollbar__bar {
    display: none !important;
  }
  
  .crollbar__wrap {
    scrollbar-width: none; // Firefox
    -ms-overflow-style: none; // IE and Edge
    overflow: hidden !important; // 完全禁用滚动
    
    &::-webkit-scrollbar {
      display: none; // Chrome, Safari, Opera
    }
  }
  
  .crollbar__view {
    overflow: visible !important;
  }
}

// 确保模态框 body 不滚动
:deep(.ant-modal-body) {
  overflow: hidden !important;
  
  .crollbar {
    overflow: hidden !important;
  }
}
</style>
