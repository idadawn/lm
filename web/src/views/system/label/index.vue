<template>
  <div class="page-content-wrapper">
    <div class="page-content-wrapper-center">
      <div class="page-content-wrapper-content">
        <BasicTable @register="registerTable">
          <template #tableTitle>
            <a-button type="primary" preIcon="icon-ym icon-ym-btn-add" @click="handleAdd">新建标签</a-button>
          </template>
          <template #bodyCell="{ column, record }">
            <template v-if="column.key === 'action'">
              <TableAction :actions="getTableActions(record)" />
            </template>
          </template>
        </BasicTable>
      </div>
    </div>
    <DepForm @register="registerDepForm" @reload="reload" />
  </div>
</template>
<script lang="ts" setup>
  import { postMetrictagList, deleteMetrictag } from '/@/api/labelManagement';
  import { BasicTable, useTable, TableAction, BasicColumn, FormProps, ActionItem } from '/@/components/Table';
  import { useMessage } from '/@/hooks/web/useMessage';
  import { useModal } from '/@/components/Modal';
  import DepForm from './DepForm.vue';

  defineOptions({ name: 'label' });

  const { createMessage } = useMessage();
  const [registerDepForm, { openModal: openDepFormModal }] = useModal();

  const columns: BasicColumn[] = [
    { title: '名称', dataIndex: 'name' },
    { title: '描述', dataIndex: 'description' },
    { title: '创建时间', dataIndex: 'createdTime', width: 200, format: 'date|YYYY-MM-DD HH:mm' },
    { title: '更新时间', dataIndex: 'lastModifiedTime', width: 200, format: 'date|YYYY-MM-DD HH:mm' },
  ];

  const [registerTable, { reload }] = useTable({
    api: postMetrictagList,
    columns,
    useSearchForm: true, //开启搜索功能
    formConfig: getFormConfig(), //搜索的参数
    actionColumn: {
      width: 150,
      title: '操作',
      dataIndex: 'action',
    },
  });

  function getFormConfig(): Partial<FormProps> {
    return {
      schemas: [
        {
          field: 'name',
          label: '名称',
          component: 'Input',
          componentProps: {
            placeholder: '请输入名称',
            submitOnPressEnter: true,
          },
        },
      ],
    };
  }
  function getTableActions(record): ActionItem[] {
    return [
      {
        label: '编辑',
        onClick: addOrUpdateHandle.bind(null, record.id),
      },
      {
        label: '删除',
        color: 'error',
        modelConfirm: {
          onOk: handleDelete.bind(null, record.id),
        },
      },
    ];
  }
  function handleAdd() {
    addOrUpdateHandle();
  }
  // 新建和编辑
  function addOrUpdateHandle(id = '') {
    openDepFormModal(true, { id });
  }
  // 删除
  function handleDelete(id) {
    deleteMetrictag(id).then(res => {
      createMessage.success(res.msg);
      reload();
    });
  }
</script>
