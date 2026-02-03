<template>
    <BasicModal v-bind="$attrs" @register="registerModal" title="新增特征等级" @ok="handleSubmit" :width="500">
        <BasicForm @register="registerForm" />
    </BasicModal>
</template>

<script lang="ts" setup>
import { ref, unref } from 'vue';
import { BasicModal, useModalInner } from '/@/components/Modal';
import { BasicForm, useForm } from '/@/components/Form';
import { createSeverityLevel } from '/@/api/lab/severityLevel';
import { useMessage } from '/@/hooks/web/useMessage';

const emit = defineEmits(['success', 'register']);
const { createMessage } = useMessage();

const [registerForm, { resetFields, validate, setFieldsValue }] = useForm({
    labelWidth: 80,
    baseColProps: { span: 24 },
    schemas: [
        {
            field: 'name',
            label: '等级名称',
            component: 'Input',
            required: true,
            componentProps: {
                placeholder: '例如: 轻微',
            },
        },
        {
            field: 'description',
            label: '描述',
            component: 'InputTextArea',
            componentProps: {
                placeholder: '请输入描述',
                rows: 3,
            },
        },
        {
            field: 'sortCode',
            label: '排序',
            component: 'InputNumber',
            defaultValue: 0,
            componentProps: {
                min: 0,
            },
        },
        {
            field: 'enabled',
            label: '有效',
            component: 'Switch',
            defaultValue: true,
        },
    ],
    showActionButtonGroup: false,
});

const [registerModal, { setModalProps, closeModal }] = useModalInner(async () => {
    resetFields();
    setModalProps({ confirmLoading: false });
});

async function handleSubmit() {
    try {
        const values = await validate();
        setModalProps({ confirmLoading: true });

        await createSeverityLevel(values);

        createMessage.success('新增成功');
        closeModal();
        emit('success');
    } catch (error: any) {
        console.error('保存失败:', error);
        const errorMsg = error?.response?.data?.msg || error?.message || '新增失败';
        createMessage.error(errorMsg);
    } finally {
        setModalProps({ confirmLoading: false });
    }
}
</script>
