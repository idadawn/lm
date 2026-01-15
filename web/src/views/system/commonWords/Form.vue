<template>
  <BasicModal
    v-bind="$attrs"
    @register="registerModal"
    :title="getTitle"
    @ok="handleSubmit(0)"
    :showContinueBtn="!id"
    @continue="handleSubmit(1)"
    @cancel="handleCancel">
    <BasicForm @register="registerForm"></BasicForm>
  </BasicModal>
</template>
<script lang="ts" setup>
  import { create, update, getInfo } from '/@/api/system/commonWords';
  import { getSystemList } from '/@/api/system/system';
  import { ref, unref, computed } from 'vue';
  import { BasicModal, useModalInner } from '/@/components/Modal';
  import { BasicForm, useForm } from '/@/components/Form';
  import { useMessage } from '/@/hooks/web/useMessage';

  const emit = defineEmits(['register', 'reload']);

  const [registerForm, { setFieldsValue, resetFields, validate, updateSchema }] = useForm({
    schemas: [
      {
        field: 'systemIds',
        label: '所属应用',
        component: 'Select',
        componentProps: { placeholder: '请选择', multiple: true, showSearch: true, onChange: onSystemIdsChange },
        rules: [{ required: true, trigger: 'change', message: '所属应用不能为空', type: 'array' }],
        ifShow: ({ values }) => values.commonWordsType === 0,
      },
      {
        field: 'commonWordsText',
        label: '常用语',
        component: 'Input',
        componentProps: { placeholder: '常用语', maxlength: 50 },
        rules: [{ required: true, trigger: 'blur', message: '常用语不能为空' }],
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
        ifShow: ({ values }) => values.commonWordsType === 0,
      },
      {
        field: 'commonWordsType',
        label: '类型',
        defaultValue: 0,
        component: 'InputNumber',
        ifShow: false,
      },
    ],
  });
  const [registerModal, { closeModal, changeLoading, changeOkLoading, changeContinueLoading }] = useModalInner(init);
  const { createMessage } = useMessage();
  const id = ref('');
  const systemNames = ref<string[]>([]);
  const commonWordsType = ref<number>(0);
  const isContinue = ref<boolean>(false);

  const getTitle = computed(() => (!unref(id) ? '新建审批常用语' : '编辑审批常用语'));

  function init(data) {
    changeLoading(true);
    changeOkLoading(false);
    changeContinueLoading(false);
    systemNames.value = [];
    isContinue.value = false;
    resetFields();
    id.value = data.id;
    getSystemList({ enableMark: '1' }).then(res => {
      updateSchema({ field: 'systemIds', componentProps: { options: res.data.list || [] } });
      if (id.value) {
        getInfo(id.value).then(res => {
          setFieldsValue(res.data);
          systemNames.value = res.data.systemNames || [];
          commonWordsType.value = res.data.commonWordsType || 0;
          changeLoading(false);
        });
      } else {
        setFieldsValue({ commonWordsType: data.commonWordsType || 0 });
        commonWordsType.value = data.commonWordsType || 0;
        changeLoading(false);
      }
    });
  }
  function onSystemIdsChange(val, data) {
    if (!val) return (systemNames.value = []);
    systemNames.value = data.map(o => o.fullName);
  }
  async function handleSubmit(type) {
    const changeLoadingMethod = type == 1 ? changeContinueLoading : changeOkLoading;
    const values = await validate();
    if (!values) return;
    changeLoadingMethod(true);
    const query = {
      ...values,
      id: id.value,
      systemNames: systemNames.value,
    };
    if (commonWordsType.value === 1) {
      query.enabledMark = 1;
      query.systemIds = [];
    }
    const formMethod = id.value ? update : create;
    formMethod(query)
      .then(res => {
        createMessage.success(res.msg);
        changeLoadingMethod(false);
        if (type == 1) {
          resetFields();
          setFieldsValue({ commonWordsType: commonWordsType.value || 0 });
          isContinue.value = true;
        } else {
          closeModal();
          emit('reload');
        }
      })
      .catch(() => {
        changeLoadingMethod(false);
      });
  }
  function handleCancel() {
    if (isContinue.value == true) emit('reload');
  }
</script>
