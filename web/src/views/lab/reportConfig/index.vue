<template>
    <div>
        <BasicTable @register="registerTable">
            <template #toolbar>
                <a-button type="primary" @click="handleCreate"> 新增配置 </a-button>
            </template>
            <template #bodyCell="{ column, record }">
                <template v-if="column.key === 'action' || column.dataIndex === 'action'">
                    <TableAction :actions="[
                        {
                            icon: 'clarity:note-edit-line',
                            label: '编辑',
                            tooltip: '编辑',
                            onClick: handleEdit.bind(null, record),
                        },
                        {
                            icon: 'ant-design:delete-outlined',
                            label: '删除',
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
        <ReportConfigDrawer @register="registerDrawer" @success="handleSuccess" />
    </div>
</template>

<script lang="ts" setup>
defineOptions({ name: 'ReportConfigList' });
import { BasicTable, useTable, TableAction } from '/@/components/Table';
import { getReportConfigList, deleteReportConfig } from '/@/api/lab/reportConfig';
import { useDrawer } from '/@/components/Drawer';
import { useMessage } from '/@/hooks/web/useMessage';
import ReportConfigDrawer from './components/ReportConfigDrawer.vue';

const { createMessage } = useMessage();
const [registerDrawer, { openDrawer }] = useDrawer();

const [registerTable, { reload }] = useTable({
    title: '统计配置列表',
    api: getReportConfigList,
    columns: [
        { title: '名称', dataIndex: 'name', align: 'center', width: 150 },
        { title: '排序', dataIndex: 'sortOrder', align: 'center', width: 80 },
        { title: '包含等级', dataIndex: 'levelNames', customRender: ({ text }) => Array.isArray(text) ? text.join(', ') : text },
        { title: '头部展示', dataIndex: 'isHeader', align: 'center', customRender: ({ text }) => text ? '是' : '否', width: 100 },
        { title: '百分比', dataIndex: 'isPercentage', align: 'center', customRender: ({ text }) => text ? '是' : '否', width: 80 },
        { title: '报表展示', dataIndex: 'isShowInReport', align: 'center', customRender: ({ text }) => text ? '是' : '否', width: 100 },
        { title: '显示占比', dataIndex: 'isShowRatio', align: 'center', customRender: ({ text }) => text ? '是' : '否', width: 100 },
        { title: '描述', dataIndex: 'description' },
    ],
    actionColumn: {
        width: 120,
        title: '操作',
        dataIndex: 'action',
        // key: 'action',
    },
    pagination: false,
    useSearchForm: false,
    showTableSetting: true,
    bordered: true,
    showIndexColumn: false,
});

function handleCreate() {
    openDrawer(true, {
        isUpdate: false,
    });
}

function handleEdit(record: Recordable) {
    openDrawer(true, {
        record,
        isUpdate: true,
    });
}

async function handleDelete(record: Recordable) {
    await deleteReportConfig(record.id);
    createMessage.success('删除成功');
    reload();
}

function handleSuccess() {
    reload();
}
</script>
