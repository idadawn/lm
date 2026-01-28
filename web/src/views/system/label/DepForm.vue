<template>
  <BasicModal v-bind="$attrs" @register="registerModal" :title="getTitle" @ok="handleSubmit">
    <BasicForm @register="registerForm"> </BasicForm>
  </BasicModal>
</template>
<script lang="ts" setup>
  import { postMetrictag, getMetrictag, putMetrictag } from '/@/api/labelManagement';
  import { ref, unref, computed } from 'vue';
  import { BasicModal, useModalInner } from '/@/components/Modal';
  import { BasicForm, useForm } from '/@/components/Form';
  import { useMessage } from '/@/hooks/web/useMessage';

  const emit = defineEmits(['register', 'reload']);
  const [registerForm, { setFieldsValue, resetFields, validate, clearValidate }] = useForm({
    schemas: [
      {
        field: 'name',
        label: '标签名称',
        component: 'Input',
        componentProps: { placeholder: '输入名称', maxlength: 50 },
        rules: [{ required: true, trigger: 'blur', message: '请输入标签名称' }],
      },
      {
        field: 'description',
        label: '描述',
        component: 'Textarea',
        componentProps: { placeholder: '请输入内容述', rows: 3 },
      },
    ],
  });
  const [registerModal, { closeModal, changeLoading, changeOkLoading }] = useModalInner(init);
  const id = ref('');
  const { createMessage } = useMessage();

  const getTitle = computed(() => (!unref(id) ? '新建标签' : '编辑标签'));

  function init(data) {
    resetFields();
    id.value = data.id;

    if (id.value) {
      changeLoading(true);
      getMetrictag(id.value).then(res => {
        // 将数据回显到form表单
        setFieldsValue(res.data);
        changeLoading(false);
      });
    }
  }
  async function handleSubmit() {
    const values = await validate(); //获取详情当前所有的字段
    // 
    if (!values) return;
    changeOkLoading(true);
    const query = {
      ...values,
      id: id.value,
    };
    const formMethod = id.value ? putMetrictag : postMetrictag;
    formMethod(query)
      .then(res => {
        createMessage.success(res.msg);
        changeOkLoading(false);
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
