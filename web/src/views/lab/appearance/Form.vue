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
  import { createAppearanceFeature, updateAppearanceFeature, getAppearanceFeatureInfo } from '/@/api/lab/appearance';
  import { ref, unref, computed } from 'vue';
  import { BasicModal, useModalInner } from '/@/components/Modal';
  import { BasicForm, useForm } from '/@/components/Form';
  import { useMessage } from '/@/hooks/web/useMessage';

  const emit = defineEmits(['register', 'reload']);

  const [registerForm, { setFieldsValue, resetFields, validate }] = useForm({
    schemas: [
      {
        field: 'category',
        label: '外观大类',
        component: 'Input',
        componentProps: { placeholder: '如 韧性' },
        rules: [{ required: true, trigger: 'blur', message: '必填' }],
      },
      {
        field: 'name',
        label: '特性名称',
        component: 'Input',
        componentProps: { placeholder: '如 脆' },
        rules: [{ required: true, trigger: 'blur', message: '必填' }],
      },
      {
        field: 'description',
        label: '描述',
        component: 'InputTextArea',
      },
    ],
  });
  const [registerModal, { closeModal, changeLoading, changeOkLoading, changeContinueLoading }] = useModalInner(init);
  const { createMessage } = useMessage();
  const id = ref('');
  const isContinue = ref<boolean>(false);

  const getTitle = computed(() => (!unref(id) ? '新建外观特性' : '编辑外观特性'));

  function init(data) {
    changeLoading(true);
    changeOkLoading(false);
    changeContinueLoading(false);
    isContinue.value = false;
    resetFields();
    id.value = data.id;
    if (id.value) {
      getAppearanceFeatureInfo(id.value).then(res => {
        setFieldsValue(res.data);
        changeLoading(false);
      });
    } else {
      changeLoading(false);
    }
  }

  async function handleSubmit(type) {
    const changeLoadingMethod = type == 1 ? changeContinueLoading : changeOkLoading;
    const values = await validate();
    if (!values) return;
    changeLoadingMethod(true);
    const query = {
      ...values,
      id: id.value,
    };
    const formMethod = id.value ? updateAppearanceFeature : createAppearanceFeature;
    formMethod(query)
      .then(res => {
        createMessage.success(res.msg);
        changeLoadingMethod(false);
        if (type == 1) {
          resetFields();
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
