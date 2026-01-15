<template>
  <BasicModal v-bind="$attrs" @register="registerModal" :title="getTitle" @ok="handleSubmit">
    <BasicForm @register="registerForm" />
  </BasicModal>
</template>
<script lang="ts" setup>
  import { getCommonFieldsInfo as getInfo, createCommonFields as create, updateCommonFields as update } from '/@/api/systemData/commonFields';
  import { ref, unref, computed } from 'vue';
  import { BasicModal, useModalInner } from '/@/components/Modal';
  import { BasicForm, useForm } from '/@/components/Form';
  import { useMessage } from '/@/hooks/web/useMessage';
  import formValidate from '/@/utils/formValidate';

  const emit = defineEmits(['register', 'reload']);
  const typeOptions = [
    { fullName: '字符串', id: 'varchar' },
    { fullName: '整型', id: 'int' },
    { fullName: '日期时间', id: 'datetime' },
    { fullName: '浮点', id: 'decimal' },
    { fullName: '长整型', id: 'bigint' },
    { fullName: '文本', id: 'text' },
  ];
  const [registerForm, { setFieldsValue, resetFields, validate }] = useForm({
    labelWidth: 60,
    schemas: [
      {
        field: 'field',
        label: '列名',
        component: 'Input',
        componentProps: { placeholder: '输入列名', maxlength: 50 },
        rules: [
          { required: true, trigger: 'blur', message: '列名不能为空' },
          { pattern: /(^_([a-zA-Z0-9]_?)*$)|(^[a-zA-Z](_?[a-zA-Z0-9])*_?$)/, message: '请输入正确的列名', trigger: 'blur' },
        ],
      },
      {
        field: 'fieldName',
        label: '编码',
        component: 'Input',
        componentProps: { placeholder: '输入编码', maxlength: 50 },
        rules: [
          { required: true, trigger: 'blur', message: '编码不能为空' },
          { validator: formValidate('fullName', '编码不能含有特殊符号'), trigger: 'blur' },
        ],
      },
      {
        field: 'dataType',
        label: '类型',
        component: 'Select',
        componentProps: { options: typeOptions, placeholder: '请选择类型', showSearch: true },
        rules: [{ required: true, trigger: 'change', message: '类型不能为空' }],
      },
      {
        field: 'dataLength',
        label: '长度',
        defaultValue: 50,
        component: 'InputNumber',
        componentProps: { min: 1, max: 999999 },
        rules: [{ required: true, trigger: 'blur', message: '长度不能为空', type: 'number' }],
      },
      {
        field: 'allowNull',
        label: '允许空',
        defaultValue: 1,
        component: 'Switch',
        rules: [{ required: true, trigger: 'change', message: '允许空不能为空' }],
      },
    ],
  });
  const [registerModal, { closeModal, changeLoading, changeOkLoading }] = useModalInner(init);
  const id = ref('');
  const { createMessage } = useMessage();

  const getTitle = computed(() => (!unref(id) ? '新建字段' : '编辑字段'));

  function init(data) {
    changeLoading(true);
    resetFields();
    id.value = data.id;
    if (id.value) {
      getInfo(id.value).then(res => {
        res.data.dataLength = Number(res.data.dataLength);
        setFieldsValue(res.data);
        changeLoading(false);
      });
    } else {
      changeLoading(false);
    }
  }

  async function handleSubmit() {
    const values = await validate();
    if (!values) return;
    changeOkLoading(true);
    const query = {
      ...values,
      id: id.value,
    };
    const formMethod = id.value ? update : create;
    formMethod(query)
      .then(res => {
        createMessage.success(res.msg);
        changeOkLoading(false);
        closeModal();
        setTimeout(() => {
          emit('reload');
        }, 200);
      })
      .catch(() => {
        changeOkLoading(false);
      });
  }
</script>
