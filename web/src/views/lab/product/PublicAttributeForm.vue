<template>
  <BasicModal v-bind="$attrs" @register="registerModal" :title="getTitle" @ok="handleSubmit" @cancel="handleCancel"
    :width="600">
    <BasicForm @register="registerForm"></BasicForm>
  </BasicModal>
</template>

<script lang="ts" setup>
import { createPublicAttribute, updatePublicAttribute, getPublicAttributes } from '/@/api/lab/productSpecPublicAttribute';
import { ref, unref, computed, h } from 'vue';
import { BasicModal, useModalInner } from '/@/components/Modal';
import { BasicForm, useForm } from '/@/components/Form';
import { useMessage } from '/@/hooks/web/useMessage';
import UnitSelect from '/@/components/Lab/UnitSelect.vue';
import { getUnitById, loadAllUnits } from '/@/utils/lab/unit';

const emit = defineEmits(['register', 'reload']);

const [registerForm, { setFieldsValue, resetFields, validate, getFieldsValue }] = useForm({
  schemas: [
    {
      field: 'attributeName',
      label: '属性名称',
      component: 'Input',
      componentProps: {
        placeholder: '如：宽度、厚度、重量',
        onChange: (e: any) => {
          const name = e.target?.value || e;
          if (name && isNew.value) {
            // 仅在新建模式下自动生成键名
            const currentKey = getFieldsValue().attributeKey;
            if (!currentKey) {
              generateKey(name);
            }
          }
        }
      },
      rules: [{ required: true, trigger: 'blur', message: '必填' }],
    },
    {
      field: 'attributeKey',
      label: '属性键名',
      component: 'Input',
      componentProps: { placeholder: '如：width、thickness、weight（自动生成）' },
      rules: [{ required: true, trigger: 'blur', message: '必填' }],
    },
    {
      field: 'valueType',
      label: '属性类型',
      component: 'Select',
      componentProps: {
        placeholder: '请选择属性类型',
        options: [
          { label: '小数', value: 'decimal' },
          { label: '整数', value: 'int' },
          { label: '文本', value: 'text' },
        ],
        fieldNames: { label: 'label', value: 'value' },
      },
      rules: [{ required: true, trigger: 'change', message: '必填' }],
    },
    {
      field: 'unitId',
      label: '单位',
      component: 'Input', // 使用 Input 作为占位，实际通过 render 渲染
      render: (values) => {
        // values 包含 { field, model, values, schema }
        // 使用 values.values.unitId 或 values.model.unitId 来访问表单值
        const unitId = values.values?.unitId || values.model?.unitId;
        return h(UnitSelect, {
          modelValue: unitId || undefined, // 确保空值时为 undefined
          placeholder: '请选择单位（留空表示无单位）',
          'onUpdate:modelValue': (value: string | undefined) => {
            setFieldsValue({ unitId: value || undefined });
            // 根据单位ID获取单位符号和精度，用于向后兼容
            if (value) {
              getUnitById(value).then(unit => {
                if (unit) {
                  setFieldsValue({ unit: unit.symbol });
                  // 如果选择了单位且当前精度为空或未设置，自动使用单位的精度
                  const currentPrecision = getFieldsValue().precision;
                  if (values.values?.valueType === 'decimal' && (currentPrecision === undefined || currentPrecision === null)) {
                    setFieldsValue({ precision: unit.precision });
                  }
                }
              });
            } else {
              setFieldsValue({ unit: undefined });
            }
          },
          onChange: (_value: string | undefined, unit: any) => {
            if (unit) {
              setFieldsValue({ unit: unit.symbol });
              // 如果选择了单位且当前精度为空或未设置，自动使用单位的精度
              const currentPrecision = getFieldsValue().precision;
              if (values.values?.valueType === 'decimal' && (currentPrecision === undefined || currentPrecision === null)) {
                setFieldsValue({ precision: unit.precision });
              }
            } else {
              setFieldsValue({ unit: undefined });
            }
          }
        });
      },
    },
    {
      field: 'precision',
      label: '精度',
      component: 'InputNumber',
      componentProps: {
        placeholder: '小数位数（仅用于小数类型，选择单位后会自动填充单位精度）',
        min: 0,
        max: 10,
      },
      ifShow: ({ values }) => values.valueType === 'decimal',
      helpMessage: '如果已选择单位，将优先使用单位的推荐精度，您也可以手动修改',
    },
    {
      field: 'defaultValue',
      label: '默认值',
      component: 'Input',
      componentProps: { placeholder: '请输入默认值（可选）' },
    },
    {
      field: 'sortCode',
      label: '排序码',
      component: 'InputNumber',
      componentProps: {
        placeholder: '用于排序，数字越小越靠前',
        min: 0,
      },
    },
  ],
});

const [registerModal, { closeModal, changeLoading, changeOkLoading }] = useModalInner(init);
const { createMessage } = useMessage();
const id = ref('');
const isNew = ref(true); // 标记是否是新建模式

const getTitle = computed(() => (!unref(id) ? '新建公共属性' : '编辑公共属性'));

async function init(data) {
  changeLoading(true);
  changeOkLoading(false);
  resetFields();
  id.value = data.id || '';
  isNew.value = !id.value;

  // 预加载所有单位信息
  await loadAllUnits();

  if (id.value) {
    // 编辑模式：从列表中查找数据
    getPublicAttributes().then(async res => {
      const list = res.data || res || [];
      const item = list.find((item: any) => item.id === id.value);
      if (item) {
        // 处理单位字段：优先使用 unitId，如果没有 unitId 但有 unit 符号，尝试查找
        let unitId = item.unitId;
        let unit = item.unit;

        // 只有当 unitId 为空且 unit 有值（非空字符串）时，才尝试根据符号查找单位ID
        if (!unitId && unit && unit.trim() !== '') {
          const allUnits = await loadAllUnits();
          const matchedUnit = allUnits.find(u => u.symbol === unit);
          if (matchedUnit) {
            unitId = matchedUnit.id;
          }
        }

        // 如果 unitId 为空，确保 unit 也为空（避免显示错误的单位）
        if (!unitId) {
          unit = null;
        }

        // 如果属性没有精度但有关联单位，尝试从单位获取精度
        let precision = item.precision;
        if ((precision === undefined || precision === null) && unitId && item.valueType === 'decimal') {
          const unitInfo = await getUnitById(unitId);
          if (unitInfo) {
            precision = unitInfo.precision;
          }
        }

        setFieldsValue({
          attributeName: item.attributeName,
          attributeKey: item.attributeKey,
          valueType: item.valueType,
          precision: precision,
          unitId: unitId || undefined, // 确保空值时为 undefined，而不是 null 或空字符串
          unit: unit || undefined, // 确保空值时为 undefined
          defaultValue: item.defaultValue,
          sortCode: item.sortCode,
        });
      }
      changeLoading(false);
    }).catch(() => {
      changeLoading(false);
    });
  } else {
    // 新建模式：设置默认值
    setFieldsValue({
      valueType: 'decimal',
      precision: 2,
      sortCode: 0,
    });
    changeLoading(false);
  }
}

async function handleSubmit() {
  const values = await validate();
  if (!values) return;
  changeOkLoading(true);

  try {
    // 如果选择了单位ID，确保单位符号也被设置（向后兼容）
    if (values.unitId && !values.unit) {
      const unit = await getUnitById(values.unitId);
      if (unit) {
        values.unit = unit.symbol;
      }
    }

    // 提交时同时包含 unitId 和 unit（向后兼容）
    const submitData = {
      ...values,
      unitId: values.unitId || null,
      unit: values.unit || null,
    };

    if (id.value) {
      await updatePublicAttribute(id.value, submitData);
      createMessage.success('更新成功');
    } else {
      await createPublicAttribute(submitData);
      createMessage.success('创建成功');
    }
    changeOkLoading(false);
    closeModal();
    emit('reload');
  } catch (error: any) {
    changeOkLoading(false);
    const errorMsg = error?.response?.data?.msg || error?.message || '操作失败';
    createMessage.error(errorMsg);
  }
}

function handleCancel() {
  closeModal();
}

// 根据属性名称生成键名（不设置到表单，仅用于比较）
function generateKeyFromName(attributeName: string): string {
  if (!attributeName) return '';

  // 简单的中文转拼音逻辑
  return attributeName
    .replace(/宽度/g, 'width')
    .replace(/厚度/g, 'thickness')
    .replace(/重量/g, 'weight')
    .replace(/长度/g, 'length')
    .replace(/层数/g, 'layers')
    .replace(/密度/g, 'density')
    .replace(/抗拉强度/g, 'tensileStrength')
    .replace(/屈服强度/g, 'yieldStrength')
    .replace(/延伸率/g, 'elongation')
    .replace(/强度/g, 'strength')
    .replace(/[\u4e00-\u9fa5]/g, '')
    .toLowerCase()
    .replace(/\s+/g, '_')
    .replace(/[^a-z0-9_]/g, '');
}

// 根据属性名称自动生成键名并设置到表单
function generateKey(attributeName: string) {
  const key = generateKeyFromName(attributeName);
  if (key) {
    setFieldsValue({ attributeKey: key });
  }
}
</script>
