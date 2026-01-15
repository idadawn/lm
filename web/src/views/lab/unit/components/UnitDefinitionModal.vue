<template>
  <BasicModal
    v-bind="$attrs"
    :title="getTitle"
    @register="register"
    @ok="handleSubmit"
    :width="700">
    <BasicForm @register="registerForm" />
  </BasicModal>
</template>

<script lang="ts" setup>
import { ref, computed, watch } from 'vue';
import { BasicModal, useModalInner } from '/@/components/Modal';
import { BasicForm, useForm } from '/@/components/Form';
import { useMessage } from '/@/hooks/web/useMessage';
import {
  createUnitDefinition,
  updateUnitDefinition,
  getUnitDefinitionInfo,
} from '/@/api/lab/unit';

const emit = defineEmits(['register', 'success']);

const { createMessage } = useMessage();
const isUpdate = ref(false);
const recordId = ref<string>('');
const categoryList = ref<any[]>([]);

const [registerForm, { setFieldsValue, resetFields, validate, updateSchema }] = useForm({
  labelWidth: 120,
  schemas: [
    {
      field: 'categoryId',
      label: '单位维度',
      component: 'Select',
      componentProps: {
        placeholder: '请选择单位维度',
        options: [],
      },
      rules: [{ required: true, message: '请选择单位维度' }],
    },
    {
      field: 'name',
      label: '单位全称',
      component: 'Input',
      componentProps: { placeholder: '如：毫米、微米' },
      rules: [{ required: true, message: '请输入单位全称' }],
    },
    {
      field: 'symbol',
      label: '单位符号',
      component: 'Input',
      componentProps: { placeholder: '如：mm, μm' },
      rules: [{ required: true, message: '请输入单位符号' }],
    },
    {
      field: 'isBase',
      label: '基准单位',
      component: 'Switch',
      componentProps: {
        checkedChildren: '是',
        unCheckedChildren: '否',
      },
      defaultValue: false,
    },
    {
      field: 'scaleToBase',
      label: '换算比例',
      component: 'InputNumber',
      componentProps: {
        placeholder: '换算至基准单位的比例系数',
        min: 0,
        step: 0.0000000001,
        precision: 10,
        style: 'width: 100%',
      },
      rules: [{ required: true, message: '请输入换算比例' }],
      helpMessage: '例如：1mm = 0.001m，则比例为 0.001',
    },
    {
      field: 'offset',
      label: '换算偏移量',
      component: 'InputNumber',
      componentProps: {
        placeholder: '默认0，用于摄氏度/华氏度等',
        step: 0.0000000001,
        precision: 10,
        style: 'width: 100%',
      },
      defaultValue: 0,
    },
    {
      field: 'precision',
      label: '显示精度',
      component: 'InputNumber',
      componentProps: {
        placeholder: '小数位数',
        min: 0,
        max: 10,
        style: 'width: 100%',
      },
      rules: [{ required: true, message: '请输入显示精度' }],
      defaultValue: 2,
      helpMessage: '该单位推荐的显示精度（小数位数）',
    },
    {
      field: 'sortCode',
      label: '排序码',
      component: 'InputNumber',
      componentProps: {
        min: 0,
        placeholder: '排序码',
        style: 'width: 100%',
      },
    },
  ],
});

const [register, { setModalProps, closeModal }] = useModalInner(async (data) => {
  resetFields();
  setModalProps({ confirmLoading: false });
  isUpdate.value = !!data?.isUpdate;
  categoryList.value = data?.categoryList || [];

  // 更新维度选项
  updateSchema({
    field: 'categoryId',
    componentProps: {
      options: categoryList.value.map(cat => ({
        label: cat.name,
        value: cat.id,
      })),
    },
  });

  if (isUpdate.value) {
    recordId.value = data.record.id;
    try {
      const res: any = await getUnitDefinitionInfo(data.record.id);
      const info = res.data || res;
      setFieldsValue({
        categoryId: info.categoryId,
        name: info.name,
        symbol: info.symbol,
        isBase: info.isBase,
        scaleToBase: info.scaleToBase,
        offset: info.offset,
        precision: info.precision,
        sortCode: info.sortCode,
      });
    } catch (error) {
      console.error('加载单位定义详情失败:', error);
    }
  } else {
    recordId.value = '';
    // 如果有默认维度，设置它
    if (categoryList.value.length > 0 && data?.defaultCategoryId) {
      setFieldsValue({ categoryId: data.defaultCategoryId });
    }
  }
});

const getTitle = computed(() => (!isUpdate.value ? '新增单位定义' : '编辑单位定义'));

async function handleSubmit() {
  try {
    const values = await validate();
    setModalProps({ confirmLoading: true });

    if (isUpdate.value) {
      await updateUnitDefinition(recordId.value, values);
      createMessage.success('更新成功');
    } else {
      await createUnitDefinition(values);
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
