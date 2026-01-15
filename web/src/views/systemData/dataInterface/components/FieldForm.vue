<template>
  <BasicModal v-bind="$attrs" @register="registerModal" :title="getTitle" @ok="handleSubmit">
    <BasicForm @register="registerForm" />
  </BasicModal>
</template>

<script lang="ts" setup>
  import { computed, reactive } from 'vue';
  import { BasicModal, useModalInner } from '/@/components/Modal';
  import { BasicForm, useForm } from '/@/components/Form';
  import { buildBitUUID } from '/@/utils/uuid';

  const emit = defineEmits(['register', 'reload']);

  interface State {
    list: any[];
    type: string;
    dataForm: any;
  }

  const state = reactive<State>({
    list: [],
    type: 'add',
    dataForm: {
      id: '',
      defaultValue: '',
      field: '',
      dataType: '',
      required: 0,
      fieldName: '',
    },
  });
  const checkName = (_rule, value) => {
    let boo = true;
    for (let i = 0; i < state.list.length; i++) {
      if (value === state.list[i].field && state.dataForm.id !== state.list[i].id) {
        boo = false;
        break;
      }
    }
    if (boo) return Promise.resolve();
    return Promise.reject('参数名称重复');
  };
  const [registerForm, { setFieldsValue, resetFields, validate }] = useForm({
    labelWidth: 80,
    schemas: [
      {
        field: 'field',
        label: '参数名称',
        component: 'Input',
        componentProps: { placeholder: '参数名称' },
        rules: [
          { required: true, trigger: 'blur', message: '参数名称不能为空' },
          { validator: checkName, trigger: 'blur' },
        ],
      },
      {
        field: 'dataType',
        label: '参数类型',
        component: 'Select',
        componentProps: {
          placeholder: '请选择参数类型',
          options: [
            { fullName: '字符串', id: 'varchar' },
            { fullName: '整型', id: 'int' },
            { fullName: '日期时间', id: 'datetime' },
            { fullName: '浮点', id: 'decimal' },
          ],
        },
        rules: [{ required: true, trigger: 'blur', message: '请选择参数类型' }],
      },
      {
        field: 'defaultValue',
        label: '默认值',
        defaultValue: '',
        component: 'Input',
        componentProps: { placeholder: '默认值' },
      },
      {
        field: 'required',
        label: '必填',
        defaultValue: 0,
        component: 'Switch',
      },
      {
        field: 'fieldName',
        label: '参数说明',
        defaultValue: '',
        component: 'Input',
        componentProps: { placeholder: '参数说明' },
      },
    ],
  });
  const [registerModal, { closeModal }] = useModalInner(init);

  const getTitle = computed(() => (state.dataForm.id ? '新建参数' : '编辑参数'));

  function init(data) {
    resetFields();
    state.list = data.list;
    if (data.item) {
      state.dataForm = data.item;
      state.type = 'edit';
      setFieldsValue(data.item);
    } else {
      state.dataForm.id = buildBitUUID();
      state.type = 'add';
    }
  }
  async function handleSubmit() {
    const values = await validate();
    if (!values) return;
    emit('reload', state.type, { id: state.dataForm.id, ...values });
    closeModal();
  }
</script>
