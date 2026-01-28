<template>
  <div class="page-content-wrapper">
    <div class="page-content-wrapper-center">
      <div class="page-content-wrapper-content">
        <BasicTable @register="registerTable">
          <template #tableTitle>
            <a-button type="primary" preIcon="icon-ym icon-ym-btn-add" @click="handleAdd()">新建维度</a-button>
          </template>

          <template #bodyCell="{ column, record }">
            <template v-if="column.key === 'action'">
              <TableAction :actions="getTableActions(record)" />
            </template>
          </template>
        </BasicTable>
      </div>
    </div>
    <!-- 新建维度弹框 -->

    <DepForm @register="registerDepForm" @reload="reload" />
  </div>
</template>
<script lang="ts" setup>
  import { getDimensionList, deleteDimension, getDimensionOptionsList } from '/@/api/dimension/model';
  import { BasicTable, useTable, TableAction, BasicColumn, FormProps, ActionItem } from '/@/components/Table';
  import { useMessage } from '/@/hooks/web/useMessage';
  import { useModal } from '/@/components/Modal';
  import DepForm from './DepForm.vue';
  import { onMounted } from 'vue';
  import { useI18n } from '/@/hooks/web/useI18n';

  defineOptions({ name: 'permission-organize' });
  const { t } = useI18n();
  const { createMessage } = useMessage();
  const [registerDepForm, { openModal: openDepFormModal }] = useModal();
  const [registerMember, { openModal: openMemberModal }] = useModal();

  const columns: BasicColumn[] = [
    { title: '公共维度名', dataIndex: 'name' },
    { title: '数据类型', dataIndex: 'dataType' },
    { title: '最后更新时间', dataIndex: 'createdTime', width: 150, format: 'date|YYYY-MM-DD HH:mm' },
  ];
  const [registerTable, { reload }] = useTable({
    api: getDimensionList,
    columns,
    isTreeTable: true,
    useSearchForm: true,
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
  /**
   * 搜索表单
   */
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
        label: t('common.editText'),
        onClick: addOrUpdateHandle.bind(null, record.id, record.type, record.parentId),
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

  function handleAdd() {
    addOrUpdateHandle();
  }
  function addOrUpdateHandle(id = '', parentId = '') {
    const openMethod = openDepFormModal;
    openMethod(true, { id, parentId });
  }

  function handleDelete(id) {
    deleteDimension(id)
      .then(res => {
        createMessage.success(res.msg);
        reload();
      })
      .catch(() => {
      });
  }
  function viewMember(id, fullName) {
    openMemberModal(true, { id, fullName });
  }

  /**
   * 表格初始化
   */
  function init() {
    reload();
  }
  /**
   * 挂载完成
   */
  onMounted(() => {
    init();
  });
</script>
