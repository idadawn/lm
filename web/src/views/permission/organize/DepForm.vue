<template>
  <BasicModal v-bind="$attrs" @register="registerModal" :title="getTitle" @ok="handleSubmit">
    <BasicForm @register="registerForm">
      <template #parentId>
        <jnpfOrganizeSelect v-model:value="organizeIdTree" placeholder="选择所属组织" auth @change="onOrganizeChange" :currOrgId="id || '0'" />
      </template>
    </BasicForm>
  </BasicModal>
</template>
<script lang="ts" setup>
  import { createDepartment, updateDepartment, getDepartmentInfo } from '/@/api/permission/organize';
  import { ref, unref, computed } from 'vue';
  import { BasicModal, useModalInner } from '/@/components/Modal';
  import { BasicForm, useForm } from '/@/components/Form';
  import { useMessage } from '/@/hooks/web/useMessage';
  import { useOrganizeStore } from '/@/store/modules/organize';
  import formValidate from '/@/utils/formValidate';

  const emit = defineEmits(['register', 'reload']);
  const [registerForm, { setFieldsValue, resetFields, validate, clearValidate }] = useForm({
    schemas: [
      {
        field: 'parentId',
        label: '所属组织',
        component: 'TreeSelect',
        slot: 'parentId',
        rules: [{ required: true, trigger: 'change', message: '所属组织不能为空' }],
      },
      {
        field: 'fullName',
        label: '部门名称',
        component: 'Input',
        componentProps: { placeholder: '输入名称', maxlength: 50 },
        rules: [
          { required: true, trigger: 'blur', message: '请输入部门名称' },
          { validator: formValidate('fullName', '部门名称不能含有特殊符号'), trigger: 'blur' },
        ],
      },
      {
        field: 'enCode',
        label: '部门编码',
        component: 'Input',
        componentProps: { placeholder: '输入编码', maxlength: 50 },
        rules: [
          { required: true, trigger: 'blur', message: '请输入部门编码' },
          { validator: formValidate('enCode', '部门编码只能输入英文、数字和小数点且小数点不能放在首尾'), trigger: 'blur' },
        ],
      },
      {
        field: 'managerId',
        label: '部门主管',
        component: 'UserSelect',
        componentProps: { placeholder: '选择部门主管' },
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
  const organizeStore = useOrganizeStore();

  const getTitle = computed(() => (!unref(id) ? '新建部门' : '编辑部门'));

  function init(data) {
    resetFields();
    id.value = data.id;
    organizeIdTree.value = [];
    if (id.value) {
      changeLoading(true);
      getDepartmentInfo(id.value).then(res => {
        organizeIdTree.value = res.data.organizeIdTree || [];
        setFieldsValue(res.data);
        changeLoading(false);
      });
    }
  }
  function onOrganizeChange(val) {
    setFieldsValue({ parentId: !val || !val.length ? '' : val[val.length - 1] });
    if (!val || !val.length) return;
    clearValidate('parentId');
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
    const formMethod = id.value ? updateDepartment : createDepartment;
    formMethod(query)
      .then(res => {
        createMessage.success(res.msg);
        changeOkLoading(false);
        organizeStore.resetState();
        closeModal();
        setTimeout(() => {
          emit('reload');
        }, 300);
      })
      .catch(() => {
        changeOkLoading(false);
      });
  }
</script>
