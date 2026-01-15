<template>
  <BasicModal v-bind="$attrs" @register="registerModal" title="协管流程" @ok="handleSubmit">
    <BasicForm @register="registerForm" />
  </BasicModal>
</template>
<script lang="ts" setup>
  import { ref } from 'vue';
  import { assist, getAssistList } from '/@/api/workFlow/flowEngine';
  import { BasicModal, useModalInner } from '/@/components/Modal';
  import { BasicForm, useForm } from '/@/components/Form';
  import { useMessage } from '/@/hooks/web/useMessage';

  defineEmits(['register']);
  const [registerForm, { setFieldsValue, resetFields, validate }] = useForm({
    schemas: [
      {
        field: 'managementUserId',
        label: '协管人员',
        component: 'UsersSelect',
        componentProps: { placeholder: '请选择该流程协管人员', multiple: true },
      },
    ],
  });
  const [registerModal, { closeModal, changeOkLoading, changeLoading }] = useModalInner(init);
  const id = ref('');
  const { createMessage } = useMessage();

  function init(data) {
    changeLoading(true);
    resetFields();
    id.value = data.id;
    getAssistList(data.id).then(res => {
      const managementUserId = res.data.list || [];
      setFieldsValue({ managementUserId });
      changeLoading(false);
    });
  }
  async function handleSubmit() {
    const values = await validate();
    if (!values) return;
    changeOkLoading(true);
    const query = {
      templateId: id.value,
      list: values.managementUserId,
    };
    assist(query)
      .then(res => {
        createMessage.success(res.msg);
        changeOkLoading(false);
        closeModal();
      })
      .catch(() => {
        changeOkLoading(false);
      });
  }
</script>
