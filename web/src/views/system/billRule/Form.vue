<template>
  <BasicModal v-bind="$attrs" @register="registerModal" :title="getTitle" showOkBtn @ok="handleSubmit">
    <BasicForm @register="registerForm" />
  </BasicModal>
</template>
<script lang="ts" setup>
  import { ref, unref, computed, reactive } from 'vue';
  import { BasicModal, useModalInner } from '/@/components/Modal';
  import { BasicForm, useForm, FormSchema } from '/@/components/Form';
  import { getBillRuleInfo, createBillRule, updateBillRule } from '/@/api/system/billRule';
  import { useMessage } from '/@/hooks/web/useMessage';
  import dayjs from 'dayjs';

  interface State {
    dataForm: any;
  }

  const id = ref('');
  const state = reactive<State>({
    dataForm: {},
  });
  const validateZero = (_rule, value) => {
    let str = value && value.replace(/0/g, '');
    if (!str) return Promise.reject(`流水起始不能为${value}`);
    return Promise.resolve();
  };
  const schemas: FormSchema[] = [
    {
      field: 'fullName',
      label: '业务名称',
      component: 'Input',
      componentProps: { placeholder: '输入名称' },
      rules: [
        { required: true, trigger: 'blur', message: '请输入业务名称' },
        { max: 50, message: '业务名称最多为50个字符！', trigger: 'blur' },
      ],
    },
    {
      field: 'enCode',
      label: '业务编码',
      component: 'Input',
      componentProps: { placeholder: '业务编码', maxlength: 50 },
      rules: [
        { required: true, trigger: 'blur', message: '请输入业务编码' },
        { max: 50, message: '业务编码最多为50个字符！', trigger: 'blur' },
      ],
    },
    {
      field: 'category',
      label: '业务分类',
      component: 'Select',
      componentProps: { placeholder: '请选择' },
      rules: [{ required: true, trigger: 'change', message: '请选择业务分类' }],
    },
    {
      field: 'prefix',
      label: '流水前辍',
      component: 'Input',
      componentProps: { placeholder: '输入前缀', onChange: handlePrefixChange },
      rules: [{ required: true, trigger: 'blur', message: '请输入流水前缀' }],
    },
    {
      field: 'dateFormat',
      label: '流水日期',
      component: 'Select',
      componentProps: {
        placeholder: '请选择',
        options: [
          { fullName: 'yyyymmdd', id: 'yyyyMMdd' },
          { fullName: 'yyyymm', id: 'yyyyMM' },
          { fullName: 'yyyy', id: 'yyyy' },
          { fullName: 'no', id: 'no' },
        ],
        onChange: handleDateFormatChange,
      },
      rules: [{ required: true, trigger: 'change', message: '请输入选择流水日期格式' }],
    },
    {
      field: 'digit',
      label: '流水位数',
      component: 'InputNumber',
      componentProps: { placeholder: '流水位数', min: 1, max: 9, precision: 0, onChange: handleDigitChange },
      rules: [{ required: true, trigger: 'blur', message: '请输入流水位数', type: 'number' }],
    },
    {
      field: 'startNumber',
      label: '流水起始',
      component: 'Input',
      componentProps: { placeholder: '不允许输入0或特殊字符', onChange: handleStartNumberChange },
      rules: [
        { required: true, trigger: 'blur', message: '只能输入数字' },
        { pattern: /^[0-9]*$/, message: '只能输入数字', trigger: 'blur' },
        { validator: validateZero },
      ],
    },
    {
      field: 'example',
      label: '流水范例',
      component: 'Input',
      componentProps: { disabled: true },
    },
    {
      field: 'sortCode',
      label: '排序',
      component: 'InputNumber',
      defaultValue: 0,
      componentProps: { min: '0', max: '999999', placeholder: '排序' },
    },
    {
      field: 'enabledMark',
      label: '状态',
      component: 'Switch',
      defaultValue: 1,
    },
    {
      field: 'description',
      label: '说明',
      component: 'Textarea',
      componentProps: { rows: 3 },
    },
  ];
  const getTitle = computed(() => (!unref(id) ? '新建单据' : '编辑单据'));
  const emit = defineEmits(['register', 'reload']);
  const { createMessage } = useMessage();
  const [registerForm, { setFieldsValue, validate, resetFields, updateSchema }] = useForm({ labelWidth: 80, schemas: schemas });
  const [registerModal, { closeModal, changeLoading, changeOkLoading }] = useModalInner(init);

  function init(data) {
    resetFields();
    id.value = data.id;
    if (data.categoryList) updateSchema([{ field: 'category', componentProps: { options: data.categoryList } }]);
    if (id.value) {
      changeLoading(true);
      getBillRuleInfo(id.value).then(res => {
        const data = res.data;
        state.dataForm = data;
        setFieldsValue(data);
        changeLoading(false);
      });
    }
  }
  function handlePrefixChange(e) {
    state.dataForm.prefix = e;
    handleChange();
  }
  function handleDateFormatChange(e) {
    state.dataForm.dateFormat = e;
    handleChange();
  }
  function handleDigitChange(e) {
    state.dataForm.digit = e;
    handleChange();
  }
  function handleStartNumberChange(e) {
    state.dataForm.startNumber = e;
    handleChange();
  }
  function handleChange() {
    // 流水前缀
    const prefix = state.dataForm.prefix;
    // 流水日期格式
    const dateFormat = state.dataForm.dateFormat || '';
    let dateVal = '';
    if (dateFormat && dateFormat !== 'no') {
      dateVal = dayjs().format(dateFormat.toUpperCase());
    }
    // 流水位数
    let digitVal = state.dataForm.digit || '';
    if (digitVal != '') digitVal = Array(digitVal > 0 ? digitVal - ('' + 0).length + 1 : 0).join('0') + 0;
    // 流水起始
    const startNumber = state.dataForm.startNumber || '';
    let startNumberVal = '';
    if (startNumber != '') {
      startNumberVal = digitVal + startNumber;
      digitVal = startNumberVal.substring(startNumberVal.length - digitVal.length, startNumberVal.length);
      state.dataForm.startNumber = digitVal;
      setFieldsValue({ startNumber: digitVal });
    }
    // 流水范例
    const example = prefix + dateVal + digitVal;
    setFieldsValue({ example: example });
  }
  async function handleSubmit() {
    const values = await validate();
    if (!values) return;
    changeOkLoading(true);
    const query = {
      ...values,
      id: id.value,
    };
    const formMethod = id.value ? updateBillRule : createBillRule;
    formMethod(query)
      .then(res => {
        createMessage.success(res.msg);
        changeOkLoading(false);
        closeModal();
        emit('reload');
      })
      .catch(() => {
        changeOkLoading(false);
      });
  }
</script>
