<template>
  <BasicModal
    v-bind="$attrs"
    :title="getTitle"
    @register="register"
    @ok="handleSubmit"
    :width="600">
    <BasicForm @register="registerForm" />
  </BasicModal>
</template>

<script lang="ts" setup>
import { ref, computed } from 'vue';
import { BasicModal, useModalInner } from '/@/components/Modal';
import { BasicForm, useForm } from '/@/components/Form';
import { useMessage } from '/@/hooks/web/useMessage';
import { createUnitCategory, updateUnitCategory, getUnitCategoryInfo } from '/@/api/lab/unit';

const emit = defineEmits(['register', 'success']);

const { createMessage } = useMessage();
const isUpdate = ref(false);
const recordId = ref<string>('');

const [registerForm, { setFieldsValue, resetFields, validate }] = useForm({
  labelWidth: 100,
  schemas: [
    {
      field: 'name',
      label: '维度名称',
      component: 'Input',
      componentProps: { placeholder: '如：长度、质量、密度' },
      rules: [{ required: true, message: '请输入维度名称' }],
    },
    {
      field: 'code',
      label: '唯一编码',
      component: 'Input',
      componentProps: { placeholder: '如：LENGTH, MASS, DENSITY' },
      rules: [{ required: true, message: '请输入唯一编码' }],
    },
    {
      field: 'description',
      label: '描述',
      component: 'InputTextArea',
      componentProps: { placeholder: '请输入描述', rows: 3 },
    },
    {
      field: 'sortCode',
      label: '排序码',
      component: 'InputNumber',
      componentProps: { min: 0, placeholder: '排序码' },
    },
  ],
});

const [register, { setModalProps, closeModal }] = useModalInner(async (data) => {
  resetFields();
  setModalProps({ confirmLoading: false });
  isUpdate.value = !!data?.isUpdate;

  if (isUpdate.value) {
    recordId.value = data.record.id;
    try {
      const res: any = await getUnitCategoryInfo(data.record.id);
      const info = res.data || res;
      setFieldsValue({
        name: info.name,
        code: info.code,
        description: info.description,
        sortCode: info.sortCode,
      });
    } catch (error) {
      console.error('加载单位维度详情失败:', error);
    }
  } else {
    recordId.value = '';
  }
});

const getTitle = computed(() => (!isUpdate.value ? '新增单位维度' : '编辑单位维度'));

async function handleSubmit() {
  try {
    const values = await validate();
    setModalProps({ confirmLoading: true });

    if (isUpdate.value) {
      await updateUnitCategory(recordId.value, values);
      createMessage.success('更新成功');
    } else {
      await createUnitCategory(values);
      createMessage.success('创建成功');
    }

    closeModal();
    emit('success');
  } catch (error: any) {
    console.error('保存失败:', error);
    const errorMsg = error?.response?.data?.msg || error?.message || '保存失败，请稍后重试';
    createMessage.error(errorMsg);
  } finally {
    setModalProps({ confirmLoading: false });
  }
}
</script>
