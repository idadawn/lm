<template>
  <BasicModal v-bind="$attrs" @register="registerModal" :title="getTitle" @ok="handleSubmit">
    <BasicForm @register="registerForm">
      <template #organizeId>
        <jnpfOrganizeSelect v-model:value="organizeIdTree" placeholder="选择所属组织" auth @change="onOrganizeChange" />
      </template>
    </BasicForm>
  </BasicModal>
</template>
<script lang="ts" setup>
  import { create, update, getInfo } from '/@/api/permission/position';
  import { ref, unref, computed } from 'vue';
  import { BasicModal, useModalInner } from '/@/components/Modal';
  import { BasicForm, useForm } from '/@/components/Form';
  import { useMessage } from '/@/hooks/web/useMessage';
  import { useBaseStore } from '/@/store/modules/base';
  import { useOrganizeStore } from '/@/store/modules/organize';
  import formValidate from '/@/utils/formValidate';

  const emit = defineEmits(['register', 'reload']);
  const [registerForm, { setFieldsValue, resetFields, validate, clearValidate, updateSchema }] = useForm({
    schemas: [
      {
        field: 'organizeId',
        label: '所属组织',
        component: 'TreeSelect',
        slot: 'organizeId',
        rules: [{ required: true, trigger: 'change', message: '所属组织不能为空' }],
      },
      {
        field: 'fullName',
        label: '岗位名称',
        component: 'Input',
        componentProps: { placeholder: '输入名称', maxlength: 50 },
        rules: [
          { required: true, trigger: 'blur', message: '请输入岗位名称' },
          { validator: formValidate('fullName', '岗位名称不能含有特殊符号'), trigger: 'blur' },
        ],
      },
      {
        field: 'enCode',
        label: '岗位编码',
        component: 'Input',
        componentProps: { placeholder: '输入编码', maxlength: 50 },
        rules: [
          { required: true, trigger: 'blur', message: '请输入岗位编码' },
          { validator: formValidate('enCode', '岗位编码只能输入英文、数字和小数点且小数点不能放在首尾'), trigger: 'blur' },
        ],
      },
      {
        field: 'type',
        label: '岗位类型',
        component: 'Select',
        componentProps: { placeholder: '请选择类型', fieldNames: { value: 'enCode' }, showSearch: true },
        rules: [{ required: true, trigger: 'change', message: '岗位类型不能为空' }],
      },
      {
        field: 'sortCode',
        label: '排序',
        defaultValue: 0,
        component: 'InputNumber',
        componentProps: { min: 0, max: 999999 },
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
  const organizeIdTree = ref([]);
  const { createMessage } = useMessage();
  const baseStore = useBaseStore();
  const organizeStore = useOrganizeStore();

  const getTitle = computed(() => (!unref(id) ? '新建岗位' : '编辑岗位'));

  function init(data) {
    changeLoading(true);
    resetFields();
    id.value = data.id;
    getOptions();
    if (id.value) {
      getInfo(id.value).then(res => {
        organizeIdTree.value = res.data.organizeIdTree || [];
        setFieldsValue(res.data);
        changeLoading(false);
      });
    } else {
      setFieldsValue({ organizeId: data.organizeId || '' });
      organizeIdTree.value = data.organizeIdTree || [];
      changeLoading(false);
    }
  }
  async function getOptions() {
    const typeRes = await baseStore.getDictionaryData('PositionType');
    updateSchema({ field: 'type', componentProps: { options: typeRes } });
  }
  function onOrganizeChange(val) {
    setFieldsValue({ organizeId: !val || !val.length ? '' : val[val.length - 1] });
    if (!val || !val.length) return;
    clearValidate('organizeId');
  }
  async function handleSubmit() {
    const values = await validate();
    if (!values) return;
    changeOkLoading(true);
    const query = {
      enabledMark: 1,
      ...values,
      id: id.value,
      organizeIdTree: organizeIdTree.value,
    };
    const formMethod = id.value ? update : create;
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
