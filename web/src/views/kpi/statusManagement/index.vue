<template>
  <div class="page-content-wrapper">
    <div class="page-content-wrapper-center">
      <div class="page-content-wrapper-content">
        <BasicTable @register="registerTable">
          <template #tableTitle>
            <a-button type="primary" preIcon="icon-ym icon-ym-btn-add" @click="handleAdd()">新建状态</a-button>
          </template>
          <template #bodyCell="{ column, record }">
            <template v-if="column.key === 'action'">
              <TableAction :actions="getTableActions(record)" />
            </template>
            <template v-if="column.key === 'color'">
              <div :style="{ background: record.color }" class="w-[70px] h-[20px]"></div>
            </template>
          </template>
        </BasicTable>
      </div>
    </div>
    <!-- 新建目录 -->
    <DepForm @register="registerDepForm" @reload="reload" />
  </div>
</template>
<script lang="ts" setup>
  import { onMounted } from 'vue';
  import { getStatusList, deleteStatus } from '/@/api/status/model';
  import { BasicTable, useTable, TableAction, BasicColumn, FormProps, ActionItem } from '/@/components/Table';
  import { useMessage } from '/@/hooks/web/useMessage';
  import { useModal } from '/@/components/Modal';
  import DepForm from './DepForm.vue';

  defineOptions({ name: 'permission-organize' });

  const { createMessage } = useMessage();
  const [registerDepForm, { openModal: openDepFormModal }] = useModal();

  const columns: BasicColumn[] = [
    { title: '名称', dataIndex: 'name' },
    { title: '颜色', dataIndex: 'color' },
    { title: '创建时间', dataIndex: 'createdTime', width: 200, format: 'date|YYYY-MM-DD HH:mm' },
  ];
  const [registerTable, { reload }] = useTable({
    api: getStatusList,
    columns,
    isTreeTable: true,
    useSearchForm: true,
    formConfig: getFormConfig(),
    actionColumn: {
      width: 150,
      title: '操作',
      dataIndex: 'action',
    },
    afterFetch: data => {
    },
  });

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

  /**
   * 表格操作列事件
   * @param record 当前行数据
   */
  function getTableActions(record): ActionItem[] {
    return [
      {
        label: '编辑',
        onClick: addOrUpdateHandle.bind(null, record.id, record.parentId),
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

  /**
   * 新建目录
   * @param params
   */
  function handleAdd() {
    addOrUpdateHandle();
  }

  /**
   * 新增 | 编辑
   * @param [id=string] id
   * @param [parentId=string] 父级id
   */
  function addOrUpdateHandle(id = '', parentId = '') {
    const openMethod = openDepFormModal;
    openMethod(true, { id, parentId });
  }

  /**
   * 删除
   * @param id
   */
  function handleDelete(id) {
    deleteStatus(id)
      .then(res => {
        createMessage.success(res.msg);
        reload();
      })
      .finally(() => {
      });
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
