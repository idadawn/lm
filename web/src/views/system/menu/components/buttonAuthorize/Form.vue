<template>
  <BasicModal v-bind="$attrs" @register="registerModal" :title="getTitle" showOkBtn @ok="handleSubmit">
    <BasicForm @register="registerForm" />
  </BasicModal>
</template>
<script lang="ts" setup>
  import { computed, reactive } from 'vue';
  import { BasicModal, useModalInner } from '/@/components/Modal';
  import { BasicForm, useForm, FormSchema } from '/@/components/Form';
  import { getButtonAuthorizeSelector, create, update, getInfo } from '/@/api/system/buttonAuthorize';
  import { useMessage } from '/@/hooks/web/useMessage';

  interface State {
    moduleId: string;
    id: string;
  }

  const state = reactive<State>({
    moduleId: '',
    id: '',
  });
  const schemas: FormSchema[] = [
    {
      field: 'parentId',
      label: '上级',
      component: 'TreeSelect',
      componentProps: { placeholder: '选择上级菜单' },
      rules: [{ required: true, trigger: 'change', message: '上级不能为空' }],
    },
    {
      field: 'fullName',
      label: '名称',
      component: 'Input',
      componentProps: { placeholder: '输入名称', maxlength: 50 },
      rules: [{ required: true, trigger: 'blur', message: '按钮名称不能为空' }],
    },
    {
      field: 'enCode',
      label: '编码',
      component: 'Input',
      componentProps: { placeholder: '输入编码', maxlength: 50 },
      rules: [{ required: true, trigger: 'blur', message: '按钮编码不能为空' }],
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
  const getTitle = computed(() => (!state.id ? '新建按钮' : '编辑按钮'));
  const emit = defineEmits(['register', 'reload']);
  const { createMessage } = useMessage();
  const [registerForm, { setFieldsValue, validate, resetFields, updateSchema }] = useForm({ labelWidth: 80, schemas: schemas });
  const [registerModal, { closeModal, changeLoading, changeOkLoading }] = useModalInner(init);

  function init(data) {
    resetFields();
    state.id = data.id;
    state.moduleId = data.moduleId;
    changeLoading(true);
    getButtonAuthorizeSelector(state.moduleId)
      .then(res => {
        let topItem = {
          fullName: '顶级节点',
          hasChildren: true,
          id: '-1',
          children: res.data.list,
        };
        updateSchema({ field: 'parentId', componentProps: { options: [topItem] } });
        changeLoading(false);
      })
      .then(() => {
        changeLoading(false);
      });
    if (state.id) {
      changeLoading(true);
      getInfo(state.id)
        .then(res => {
          const data = res.data;
          setFieldsValue(data);
          changeLoading(false);
        })
        .then(() => {
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
      moduleId: state.moduleId,
      id: state.id,
    };
    const formMethod = state.id ? update : create;
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
