<template>
  <BasicModal v-bind="$attrs" @register="registerModal" :title="getTitle" showOkBtn @ok="handleSubmit">
    <BasicForm @register="registerForm" />
  </BasicModal>
</template>
<script lang="ts" setup>
  import { unref, computed, reactive } from 'vue';
  import { BasicModal, useModalInner } from '/@/components/Modal';
  import { BasicForm, useForm, FormSchema } from '/@/components/Form';
  import { useMessage } from '/@/hooks/web/useMessage';
  import { getInfo, createProject, updateProject } from '/@/api/extend/projectGantt';

  interface State {
    dataForm: any;
  }

  const state = reactive<State>({
    dataForm: {},
  });
  const checkStartTime = async (_rule, value) => {
    if (!getFieldsValue().endTime) return Promise.resolve();
    if (getFieldsValue().endTime < value) return Promise.reject('开始时间应该小于结束时间');
    validate(['endTime']);
    return Promise.resolve();
  };
  const checkEndTime = (_rule, value) => {
    if (!getFieldsValue().startTime || !value) return Promise.resolve();
    if (getFieldsValue().startTime > value) return Promise.reject('结束日期应该大于开始日期');
    return Promise.resolve();
  };
  const schemas: FormSchema[] = [
    {
      field: 'fullName',
      label: '项目名称',
      component: 'Input',
      componentProps: { placeholder: '项目名称', maxlength: 50 },
      rules: [{ required: true, trigger: 'blur', message: '项目名称不能为空' }],
    },
    {
      field: 'enCode',
      label: '项目编码',
      component: 'Input',
      componentProps: { placeholder: '项目编码', maxlength: 50 },
      rules: [
        { required: true, trigger: 'blur', message: '项目编码不能为空' },
        { pattern: /^\w+$/, message: '请输入正确的项目编码', trigger: 'blur' },
      ],
    },
    {
      field: 'managerIds',
      label: '参与人员',
      component: 'UserSelect',
      componentProps: { placeholder: '参与人员', multiple: true },
      rules: [{ required: true, trigger: 'blur', message: '参与人员不能为空', type: 'array' }],
    },
    {
      field: 'startTime',
      label: '开始日期',
      component: 'DatePicker',
      componentProps: { placeholder: '选择日期' },
      rules: [
        { required: true, trigger: 'blur', message: '开始日期不能为空' },
        { validator: checkStartTime, trigger: 'change' },
      ],
    },
    {
      field: 'endTime',
      label: '结束日期',
      component: 'DatePicker',
      componentProps: { placeholder: '选择日期' },
      rules: [
        { required: true, trigger: 'blur', message: '结束日期不能为空' },
        { validator: checkEndTime, trigger: 'change' },
      ],
    },
    {
      field: 'timeLimit',
      label: '项目工期',
      component: 'InputNumber',
      defaultValue: 0,
      componentProps: { placeholder: '项目工期', step: 1, min: 0, 'addon-after': '天' },
      rules: [{ required: true, trigger: 'blur', message: '项目工期不能为空' }],
    },
    {
      field: 'schedule',
      label: '完成进度',
      component: 'InputNumber',
      defaultValue: 0,
      componentProps: { placeholder: '完成进度', step: 1, min: 0, max: 100, 'addon-after': '%' },
      rules: [{ required: true, trigger: 'blur', message: '完成进度不能为空' }],
    },
    {
      field: 'state',
      label: '状态',
      component: 'Select',
      defaultValue: 1,
      componentProps: {
        placeholder: '请选择',
        options: [
          { fullName: '进行中', id: 1 },
          { fullName: '已暂停', id: 2 },
        ],
      },
    },
    {
      field: 'description',
      label: '项目描述',
      component: 'Textarea',
      componentProps: { placeholder: '项目描述', rows: 3 },
    },
  ];
  const getTitle = computed(() => (!unref(state.dataForm.id) ? '新建项目' : '编辑项目'));
  const emit = defineEmits(['register', 'reload']);
  const { createMessage } = useMessage();
  const [registerForm, { setFieldsValue, getFieldsValue, validate, resetFields }] = useForm({ labelWidth: 80, schemas: schemas });
  const [registerModal, { closeModal, changeLoading, changeOkLoading }] = useModalInner(init);

  function init(data) {
    resetFields();
    state.dataForm.id = data.id;
    if (state.dataForm.id) {
      changeLoading(true);
      getInfo(state.dataForm.id).then(res => {
        let data = res.data;
        data.schedule = data.schedule ? Number(data.schedule) : 0;
        data.timeLimit = data.timeLimit ? Number(data.timeLimit) : 0;
        data.managerIds = data.managerIds ? data.managerIds.split(',') : [];
        state.dataForm = data;
        setFieldsValue(data);
        changeLoading(false);
      });
    }
  }
  async function handleSubmit() {
    const values = await validate();
    if (!values) return;
    changeOkLoading(true);
    const query = {
      ...values,
      managerIds: values.managerIds.join(','),
      id: state.dataForm.id,
    };
    const formMethod = state.dataForm.id ? updateProject : createProject;
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
