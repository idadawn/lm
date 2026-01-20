<template>
  <BasicModal
    v-bind="$attrs"
    @register="registerModal"
    :title="getTitle"
    @ok="handleSubmit"
    @cancel="handleCancel"
    :width="900">
    <BasicForm @register="registerForm"></BasicForm>
  </BasicModal>
</template>

<script lang="ts" setup>
import { ref, computed, h, onMounted } from 'vue';
import { BasicModal, useModalInner } from '/@/components/Modal';
import { BasicForm, useForm } from '/@/components/Form';
import { useMessage } from '/@/hooks/web/useMessage';
import type { IntermediateDataFormula } from '/@/api/lab/types/intermediateDataFormula';
import { getIntermediateDataFormulaList, validateFormula, updateIntermediateDataFormula } from '/@/api/lab/intermediateDataFormula';
import { getUnitById } from '/@/utils/lab/unit';
import JudgmentRuleEditor from './JudgmentRuleEditor.vue';

const emit = defineEmits(['register', 'reload']);

const recordId = ref<string>('');
const formMode = ref<'Attributes' | 'Formula'>('Attributes');
const originalRecord = ref<IntermediateDataFormula | null>(null);
const availableFields = ref<any[]>([]);

onMounted(() => {
    refreshAvailableFields();
});

async function refreshAvailableFields() {
   try {
     const result = await getIntermediateDataFormulaList();
     if (result && Array.isArray(result)) {
        availableFields.value = result.map(item => ({
             id: item.columnName, 
             name: item.formulaName || item.columnName,
             code: item.columnName
        }));
     }
   } catch (e) {
     console.error('Failed to fetch available fields', e);
   }
}

const [registerForm, { setFieldsValue, resetFields, validate, getFieldsValue }] = useForm({
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
    },
    {
      field: 'columnName',
      label: '中间数据表列',
      component: 'Input',
      componentProps: {
        disabled: true,
      },
    },
    {
      field: 'formulaName',
      label: '公式名称',
      component: 'Input',
      componentProps: {
        disabled: true,
      },
    },
    // --- 属性 / 公式 通用字段 ---
    {
      field: 'formulaType',
      label: '类型',
      component: 'Select',
      componentProps: {
        options: [
          { label: '计算公式', value: 'CALC' },
          { label: '判定公式', value: 'JUDGE' },
        ],
        fieldNames: { label: 'label', value: 'value' },
        onChange: (value: string) => {
          // 当切换为判定公式时，强制使用 EXCEL 语言，并隐藏公式语言下拉
          if (value === 'JUDGE') {
            setFieldsValue({ formulaLanguage: 'EXCEL', defaultValue: '', formula: '' });
          } else {
            // 计算公式默认值为 0
            setFieldsValue({ defaultValue: '0', formula: '' });
          }
        },
      },
      defaultValue: 'CALC',
      rules: [{ required: true, trigger: 'change', message: '必填' }],
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
      label: '公式',
      component: 'Input',
      ifShow: () => formMode.value === 'Formula',
      render: ({ model, field }) => {
        const formulaType = model['formulaType'] || 'CALC';
        if (formulaType === 'JUDGE') {
          return h(JudgmentRuleEditor, {
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
          });
        }
        // Fallback to normal input for CALC
        // Note: BasicForm render override completely replaces component.
        // We need to render default Input if not JUDGE. Not easy with `render`.
        // Better strategy: Use `slot`. But BasicForm slot usage is specific.
        // Simple way: If not JUDGE, render VNode for Input.TextArea or similar.
        // Or since we only care about JUDGE here? No, we need to support CALC too.
        // Actually, this render function is only called if ifShow is true.
        // Let's implement dynamic component rendering.
         return h('div', [
             h('textarea', {
                 value: model[field],
                 class: 'ant-input',
                 rows: 4,
                 placeholder: '请输入公式表达式',
                 onInput: (e: any) => {
                     model[field] = e.target.value;
                     setFieldsValue({ formula: e.target.value });
                 }
             })
         ]);
      },
      // When using render, componentProps are ignored.
      // We must implement validation logic manually or rely on `rules`.
      rules: [
        { required: true, trigger: 'blur', message: '必填' },
        {
          validator: async (_rule, value) => {
            if (!value) return;
            const formulaType = getFieldsValue().formulaType;
             if (formulaType === 'JUDGE') {
                  // Validate JSON
                  try {
                      const rules = JSON.parse(value);
                      if (!Array.isArray(rules) || rules.length === 0) {
                           // Allow empty rules? Maybe not.
                           // return Promise.reject('请至少添加一条判定规则');
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
                columnName: getFieldsValue().columnName,
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
        getFieldsValue().formulaType === 'CALC',
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

const getTitle = computed(() => formMode.value === 'Attributes' ? '编辑属性' : '编辑公式');

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
