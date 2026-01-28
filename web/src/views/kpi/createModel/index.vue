<template>
  <div class="page-content-wrapper">
    <div class="page-content-wrapper-center">
      <div class="page-content-wrapper-content">
        <BasicTable @register="registerTable" :searchInfo="searchInfo">
          <template #tableTitle>
            <a-button type="primary" preIcon="icon-ym icon-ym-btn-add" @click="handleAdd('add')">新建价值链</a-button>
          </template>
          <template #bodyCell="{ column, record }">
            <template v-if="column.key === 'name'">
              <div @click="editHandleClick(record)" class="font-semibold cursor-pointer text-rose-600">
                {{ record?.name }}
              </div>
            </template>
            <template v-if="column.key === 'action'">
              <TableAction :actions="getTableActions(record)" />
            </template>
          </template>
        </BasicTable>
      </div>
    </div>
    <!-- 新建思维导图 -->
    <Form @register="registerForm" @reload="reload" />
    <!-- <FormMore @register="registerForm" @reload="reload" /> -->
    <!-- 新建/编辑列表 -->
    <DepForm @register="registerDepForm" @reload="reload" />
  </div>
</template>
<script lang="ts" setup>
  import { getIndicatorTreeList, deleteIndicator } from '/@/api/createModel/model';
  import { BasicTable, useTable, TableAction, BasicColumn, FormProps, ActionItem } from '/@/components/Table';
  import { useMessage } from '/@/hooks/web/useMessage';
  import { useI18n } from '/@/hooks/web/useI18n';
  import { useModal } from '/@/components/Modal';
  import { usePopup } from '/@/components/Popup';
  import { useOrganizeStore } from '/@/store/modules/organize';
  import Form from './Form.vue';
  // import FormMore from './FormMore.vue';
  import DepForm from './DepForm.vue';
  import { reactive } from 'vue';

  defineOptions({ name: 'permission-organize' });

  // 查询列表入参
  const searchInfo = reactive({
    keyword: '',
    currentPage: 1,
    pageSize: 10,
  });

  const { createMessage } = useMessage();
  const { t } = useI18n();
  const organizeStore = useOrganizeStore();
  const [registerDepForm, { openModal: openDepFormModal }] = useModal();
  const [registerForm, { openPopup: openFormPopup }] = usePopup();

  const columns: BasicColumn[] = [
    { title: '名称', dataIndex: 'name' },
    { title: '类型', dataIndex: 'typeStr' },
    { title: '创建时间', dataIndex: 'createdTime', format: 'date|YYYY-MM-DD HH:mm' },
  ];
  const [registerTable, { reload }] = useTable({
    api: getIndicatorTreeList,
    columns,
    isTreeTable: true,
    useSearchForm: true,
    pagination: false,
    formConfig: getFormConfig(),
    actionColumn: {
      width: 150,
      title: '操作',
      dataIndex: 'action',
    },
    afterFetch: data => setTableIndex(data),
  });
  // 树形列表index层级
  function setTableIndex(arr, index = 0) {
    arr.forEach(item => {
      item.index = 1;
      if (index) item.index = index + 1;
      if (item.children) setTableIndex(item.children, item.index);
    });
  }
  function getFormConfig(): Partial<FormProps> {
    return {
      schemas: [
        {
          field: 'keyword',
          label: t('common.keyword'),
          component: 'Input',
          componentProps: {
            placeholder: t('common.enterKeyword'),
            submitOnPressEnter: true,
          },
        },
      ],
    };
  }
  function getTableActions(record): ActionItem[] {
    return [
      {
        label: t('common.editText'),
        onClick: addOrUpdateHandle.bind(null, record.id, record.type),
      },
      {
        label: t('common.delText'),
        color: 'error',
        modelConfirm: {
          onOk: handleDelete.bind(null, record.id),
        },
      },
    ];
  }
  function handleAdd(type) {
    addOrUpdateHandle('', type);
  }
  function addOrUpdateHandle(id?, type?) {
    openDepFormModal(true, { id, type });
  }

  function editHandleClick(record) {
    openFormPopup(true, record);
  }

  function handleDelete(id) {
    deleteIndicator(id)
      .then(res => {
        createMessage.success(res.msg);
        organizeStore.resetState();
        reload();
      })
      .catch(() => {
      });
  }
</script>
<style scoped></style>
