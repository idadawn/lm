<template>
    <BasicModal v-bind="$attrs" @register="registerModal" title="统计配置" width="800px">
        <div class="p-4">
            <div class="mb-4">
                <a-button type="primary" @click="handleAdd">新增配置</a-button>
            </div>
            <BasicTable @register="registerTable">
                <template #bodyCell="{ column, record }">
                    <template v-if="column.key === 'action'">
                        <TableAction :actions="[
                            {
                                icon: 'clarity:note-edit-line',
                                onClick: handleEdit.bind(null, record),
                            },
                            {
                                icon: 'ant-design:delete-outlined',
                                color: 'error',
                                popConfirm: {
                                    title: '是否确认删除',
                                    placement: 'left',
                                    confirm: handleDelete.bind(null, record),
                                },
                                ifShow: !record.isSystem,
                            },
                        ]" />
                    </template>
                </template>
            </BasicTable>
        </div>

        <BasicModal @register="registerEditModal" :title="isUpdate ? '编辑配置' : '新增配置'" @ok="handleSubmit">
            <BasicForm @register="registerForm" />
        </BasicModal>
    </BasicModal>
</template>

<script lang="ts" setup>
import { ref, unref, nextTick } from 'vue';
import { BasicModal, useModalInner, useModal } from '/@/components/Modal';
import { BasicTable, useTable, TableAction } from '/@/components/Table';
import { BasicForm, useForm } from '/@/components/Form';
import { getReportConfigList, addReportConfig, updateReportConfig, deleteReportConfig, ReportConfig } from '/@/api/lab/reportConfig';
import { getJudgmentLevelNames } from '/@/api/lab/intermediateDataJudgmentLevel';
import { useMessage } from '/@/hooks/web/useMessage';

const { createMessage } = useMessage();
const isUpdate = ref(true);
const currentId = ref('');
const emit = defineEmits(['success', 'register']);

const [registerModal, { closeModal: closeMainModal }] = useModalInner(async () => {
    reload();
});

const [registerTable, { reload }] = useTable({
    title: '配置列表',
    api: getReportConfigList,
    columns: [
        { title: '名称', dataIndex: 'name', width: 150 },
        { title: '排序', dataIndex: 'sortOrder', width: 80 },
        { title: '包含等级', dataIndex: 'levelNames', customRender: ({ text }) => Array.isArray(text) ? text.join(', ') : text },
        { title: '描述', dataIndex: 'description' },
    ],
    actionColumn: {
        width: 120,
        title: '操作',
        dataIndex: 'action',
    },
    pagination: false,
    resizeHeightOffset: 16,
});

const [registerEditModal, { openModal: openEditModal, closeModal: closeEditModal }] = useModal();

const [registerForm, { setFieldsValue, resetFields, validate }] = useForm({
    labelWidth: 100,
    schemas: [
        {
            field: 'name',
            label: '统计名称',
            component: 'Input',
            required: true,
            componentProps: {
                placeholder: '例如：A类占比',
            },
        },
        {
            field: 'sortOrder',
            label: '排序',
            component: 'InputNumber',
            defaultValue: 0,
            componentProps: {
                min: 0,
            },
        },
        {
            field: 'levelNames',
            label: '包含等级',
            component: 'ApiSelect',
            componentProps: {
                api: () => getJudgmentLevelNames(''),
                labelField: 'name',
                valueField: 'name',
                mode: 'multiple',
            },
            required: true,
        },
        {
            field: 'description',
            label: '描述',
            component: 'InputTextArea',
        },
    ],
    showActionButtonGroup: false,
});

function handleAdd() {
    isUpdate.value = false;
    currentId.value = '';
    openEditModal(true);
    nextTick(() => {
        resetFields();
    });
}

function handleEdit(record: ReportConfig) {
    isUpdate.value = true;
    currentId.value = record.id;
    openEditModal(true);
    nextTick(() => {
        setFieldsValue({
            name: record.name,
            sortOrder: record.sortOrder,
            levelNames: record.levelNames,
            description: record.description,
        });
    });
}

async function handleDelete(record: ReportConfig) {
    try {
        await deleteReportConfig(record.id);
        createMessage.success('删除成功');
        reload();
        emit('success');
    } catch (error) {
        console.error(error);
    }
}

async function handleSubmit() {
    try {
        const values = await validate();
        if (isUpdate.value) {
            await updateReportConfig({ ...values, id: unref(currentId) });
        } else {
            await addReportConfig(values);
        }
        closeEditModal();
        reload();
        createMessage.success('保存成功');
        emit('success');
    } catch (error) {
        console.error(error);
    }
}
</script>
