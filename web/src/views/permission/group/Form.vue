<template>
  <BasicModal v-bind="$attrs" @register="registerModal" :title="getTitle" @ok="handleSubmit">
    <BasicForm @register="registerForm" />
  </BasicModal>
</template>
<script lang="ts" setup>
  import { create, update, getInfo } from '/@/api/permission/group';
  import { ref, unref, computed, reactive } from 'vue';
  import { BasicModal, useModalInner } from '/@/components/Modal';
  import { BasicForm, useForm } from '/@/components/Form';
  import { useMessage } from '/@/hooks/web/useMessage';
  import { useBaseStore } from '/@/store/modules/base';
  import { useOrganizeStore } from '/@/store/modules/organize';
  import formValidate from '/@/utils/formValidate';

  interface State {
    dataForm: any;
  }

  const emit = defineEmits(['register', 'reload']);
  const state = reactive<State>({
    dataForm: {
      id: '',
      fullName: '',
      enCode: '',
      type: '',
      enabledMark: 1,
      globalMark: null,
      organizeIdsTree: [],
      sortCode: 0,
      description: '',
    },
  });
  const [registerForm, { setFieldsValue, resetFields, validate, updateSchema }] = useForm({
    schemas: [
      {
        field: 'fullName',
        label: '分组名称',
        component: 'Input',
        componentProps: { placeholder: '输入名称', maxlength: 50 },
        rules: [
          { required: true, trigger: 'blur', message: '请输入分组名称' },
          { validator: formValidate('fullName', '分组名称不能含有特殊符号'), trigger: 'blur' },
        ],
      },
      {
        field: 'enCode',
        label: '分组编码',
        component: 'Input',
        componentProps: { placeholder: '输入编码', maxlength: 50 },
        rules: [
          { required: true, trigger: 'blur', message: '请输入分组编码' },
          { validator: formValidate('enCode', '分组编码只能输入英文、数字和小数点且小数点不能放在首尾'), trigger: 'blur' },
        ],
      },
      {
        field: 'type',
        label: '分组类型',
        component: 'Select',
        componentProps: { placeholder: '请选择分组类型', showSearch: true, allowClear: false },
        rules: [{ required: true, trigger: 'change', message: '请选择分组类型' }],
      },
      {
        field: 'sortCode',
        label: '排序',
        defaultValue: 0,
        component: 'InputNumber',
        componentProps: { min: 0, max: 999999 },
      },
      {
        field: 'enabledMark',
        label: '状态',
        defaultValue: 1,
        component: 'Switch',
      },
      {
        field: 'description',
        label: '说明',
        component: 'Textarea',
        componentProps: { placeholder: '说明' },
      },
    ],
  });
  const [registerModal, { closeModal, changeLoading, changeOkLoading }] = useModalInner(init);
  const id = ref('');
  const { createMessage } = useMessage();
  const organizeStore = useOrganizeStore();
  const baseStore = useBaseStore();

  const getTitle = computed(() => (!unref(id) ? '新建分组' : '编辑分组'));

  function init(data) {
    resetFields();
    state.dataForm.id = data.id;
    getOptions();
    if (state.dataForm.id) {
      changeLoading(true);
      getInfo(state.dataForm.id).then(res => {
        setFieldsValue(res.data);
        changeLoading(false);
      });
    }
  }
  async function getOptions() {
    const typeRes = await baseStore.getDictionaryData('groupType');
    updateSchema({ field: 'type', componentProps: { options: typeRes } });
  }
  async function handleSubmit() {
    const values = await validate();
    if (!values) return;
    changeOkLoading(true);
    const query = { ...state.dataForm, ...values };
    const formMethod = state.dataForm.id ? update : create;
    formMethod(query)
      .then(res => {
        createMessage.success(res.msg);
        changeOkLoading(false);
        organizeStore.resetState();
        closeModal();
        emit('reload');
      })
      .catch(() => {
        changeOkLoading(false);
      });
  }
</script>
