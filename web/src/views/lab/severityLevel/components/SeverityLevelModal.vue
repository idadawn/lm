<template>
  <BasicModal v-bind="$attrs" @register="registerModal" :title="getTitle" @ok="handleSubmit" :width="600">
    <BasicForm @register="registerForm" />
  </BasicModal>
</template>

<script lang="ts" setup>
  import { ref, computed, unref } from 'vue';
  import { BasicModal, useModalInner } from '/@/components/Modal';
  import { BasicForm, useForm } from '/@/components/Form';
  import { createSeverityLevel, updateSeverityLevel } from '/@/api/lab/severityLevel';
  import { useMessage } from '/@/hooks/web/useMessage';

  const emit = defineEmits(['success', 'register']);
  const isUpdate = ref(true);
  const rowId = ref('');
  const { createMessage } = useMessage();

  const [registerForm, { setFieldsValue, resetFields, validate, getFieldsValue }] = useForm({
    labelWidth: 100,
    schemas: [
      {
        field: 'name',
        label: '等级名称',
        component: 'Input',
        required: true,
        componentProps: {
          placeholder: '例如: 微、轻微、中等、严重、超级',
        },
        helpMessage: '等级的唯一标识名称，不能重复',
      },
      {
        field: 'description',
        label: '等级描述',
        component: 'InputTextArea',
        required: false,
        componentProps: {
          placeholder: '例如: 轻微、中等、严重',
          rows: 4,
        },
        helpMessage: '用于AI匹配的描述，可为空',
      },
      {
        field: 'isDefault',
        label: '是否默认',
        component: 'Switch',
        defaultValue: false,
        helpMessage: '设置为默认等级',
      },
      {
        field: 'enabled',
        label: '是否启用',
        component: 'Switch',
        defaultValue: true,
        helpMessage: '禁用的等级不会在匹配时使用',
      },
    ],
    showActionButtonGroup: false,
    actionColOptions: {
      span: 23,
    },
  });

  const [registerModal, { setModalProps, closeModal }] = useModalInner(async (data) => {
    resetFields();
    setModalProps({ confirmLoading: false });
    isUpdate.value = !!data?.isUpdate;

    if (unref(isUpdate)) {
      rowId.value = data.record.id;
      setFieldsValue({
        ...data.record,
      });
    } else {
      setFieldsValue({
        enabled: true,
        isDefault: false,
      });
    }
  });

  const getTitle = computed(() => (!unref(isUpdate) ? '新增特性等级' : '编辑特性等级'));

  async function handleSubmit() {
    try {
      const values = await validate();
      // 处理空字符串，将空字符串转换为 undefined
      if (values.description === '') {
        values.description = undefined;
      }
      // 移除 sortCode，由后台自动计算
      const { sortCode, ...submitValues } = values;
      // 确保 isDefault 和 enabled 字段始终被包含，即使为 false
      const finalValues = {
        name: values.name,
        description: values.description,
        enabled: values.enabled !== undefined ? Boolean(values.enabled) : true,
        isDefault: values.isDefault !== undefined ? Boolean(values.isDefault) : false,
      };
      setModalProps({ confirmLoading: true });
      if (unref(isUpdate)) {
        await updateSeverityLevel({ ...finalValues, id: rowId.value });
      } else {
        await createSeverityLevel(finalValues);
      }
      createMessage.success(unref(isUpdate) ? '编辑成功' : '新增成功');
      closeModal();
      emit('success');
    } catch (error: any) {
      console.error('保存失败:', error);
      const errorMsg =
        error?.response?.data?.msg ||
        error?.message ||
        (unref(isUpdate) ? '编辑失败' : '新增失败');
      createMessage.error(errorMsg);
    } finally {
      setModalProps({ confirmLoading: false });
    }
  }
</script>
