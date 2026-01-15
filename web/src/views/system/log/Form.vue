<template>
  <BasicPopup v-bind="$attrs" @register="registerPopup" :title="title">
    <BasicForm @register="registerForm" class="!px-10px" />
  </BasicPopup>
</template>
<script lang="ts" setup>
  import { ref } from 'vue';
  import { BasicPopup, usePopupInner } from '/@/components/Popup';
  import { BasicForm, useForm, FormSchema } from '/@/components/Form';

  const title = ref('');
  const schemas: FormSchema[] = [
    {
      field: 'value',
      label: '',
      component: 'Textarea',
      componentProps: { placeholder: 'VALUE', rows: 33, readonly: true },
    },
  ];
  const [registerForm, { setFieldsValue, resetFields }] = useForm({ labelWidth: 50, schemas: schemas });
  const [registerPopup] = usePopupInner(init);

  function init(data) {
    resetFields();
    title.value = data.title;
    setFieldsValue({ value: data.json });
  }
</script>
