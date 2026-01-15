<template>
  <BasicModal v-bind="$attrs" @register="registerModal" :title="getTitle" showOkBtn @ok="handleSubmit">
    <template #insertFooter>
      <a-button type="danger" @click="handleDelete" v-if="dataForm.id">删除</a-button>
    </template>
    <BasicForm @register="registerForm" />
  </BasicModal>
</template>
<script lang="ts" setup>
  import { computed, reactive, toRefs } from 'vue';
  import { BasicModal, useModalInner } from '/@/components/Modal';
  import { BasicForm, useForm, FormSchema } from '/@/components/Form';
  import { useMessage } from '/@/hooks/web/useMessage';
  import { getInfo, createSchedule, updateSchedule, delSchedule } from '/@/api/extend/schedule';

  interface State {
    dataForm: any;
    showLoading: boolean;
  }

  const checkEndTime = async (_rule, value) => {
    if (!getFieldsValue().startTime) return Promise.resolve();
    if (getFieldsValue().startTime > value) return Promise.reject('结束时间应该大于开始时间');
    return Promise.resolve();
  };
  const state = reactive<State>({
    dataForm: {
      id: 0,
      startTime: '',
      endTime: '',
      content: '',
      early: 1,
      appAlert: 0,
      weChatAlert: 0,
      mailAlert: 0,
      mobileAlert: 0,
      colour: '#188ae2',
    },
    showLoading: false,
  });
  const { dataForm } = toRefs(state);
  const schemas: FormSchema[] = [
    {
      field: 'startTime',
      label: '起始时间',
      component: 'DatePicker',
      componentProps: { placeholder: '选择日期时间', format: 'YYYY-MM-DD HH:mm:ss' },
      rules: [{ required: true, trigger: 'change', message: '起始时间不能为空' }],
    },
    {
      field: 'endTime',
      label: '结束时间',
      component: 'DatePicker',
      componentProps: { placeholder: '选择日期时间', format: 'YYYY-MM-DD HH:mm:ss' },
      rules: [
        { required: true, trigger: 'change', message: '结束时间不能为空' },
        { validator: checkEndTime, trigger: 'change' },
      ],
    },
    {
      field: 'content',
      label: '记录内容',
      component: 'Textarea',
      componentProps: { rows: 3, placeholder: '记录你将要做的一件事...' },
      rules: [{ required: true, trigger: 'change', message: '记录内容不能为空' }],
    },
    {
      field: 'early',
      label: '提醒设置',
      component: 'InputNumber',
      componentProps: { min: '0', max: '999999', placeholder: '默认1小时' },
    },
    {
      field: 'colour',
      label: '标签颜色',
      component: 'ColorPicker',
      defaultValue: '#188ae2',
      componentProps: { predefine: ['#188ae2', '#35b8e0', '#26bf8c', '#f9c851', '#ff5b5b', '#5b69bc', '#ff8acc', '#3b3e47', '#282828'] },
    },
  ];
  const getTitle = computed(() => (!state.dataForm.id ? '添加日程' : '编辑日程'));
  const emit = defineEmits(['register', 'refresh']);
  const { createMessage, createConfirm } = useMessage();
  const [registerForm, { setFieldsValue, validate, resetFields, getFieldsValue }] = useForm({ labelWidth: 80, schemas: schemas });
  const [registerModal, { closeModal, changeLoading, changeOkLoading }] = useModalInner(init);

  function init(data) {
    state.dataForm.id = data.id || 0;
    resetFields();
    if (state.dataForm.id) {
      changeLoading(true);
      getInfo(state.dataForm.id)
        .then(res => {
          state.dataForm = res.data;
          setFieldsValue(res.data);
          changeLoading(false);
        })
        .then(() => {
          changeLoading(false);
        });
    } else {
      state.dataForm.startTime = data.startTime || '';
      setFieldsValue({ startTime: state.dataForm.startTime });
    }
  }
  async function handleSubmit() {
    const values = await validate();
    if (!values) return;
    changeOkLoading(true);
    const formMethod = state.dataForm.id ? updateSchedule : createSchedule;
    const query = { ...dataForm, ...values };
    formMethod(query)
      .then(res => {
        createMessage.success(res.msg);
        emit('refresh');
        changeOkLoading(false);
        closeModal();
      })
      .catch(() => {
        changeOkLoading(false);
      });
  }
  function handleDelete() {
    createConfirm({
      iconType: 'warning',
      title: '提示',
      content: '您确定要删除当前日程吗, 是否继续?',
      onOk: () => {
        changeOkLoading(true);
        delSchedule(state.dataForm.id)
          .then(res => {
            createMessage.success(res.msg);
            emit('refresh');
            changeOkLoading(false);
            closeModal();
          })
          .catch(() => {
            changeOkLoading(false);
          });
      },
    });
  }
</script>
