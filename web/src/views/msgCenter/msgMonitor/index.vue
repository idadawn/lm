<template>
  <div class="page-content-wrapper">
    <div class="page-content-wrapper-center">
      <div class="page-content-wrapper-content">
        <BasicTable @register="registerTable">
          <template #tableTitle>
            <a-button type="error" preIcon="icon-ym icon-ym-btn-clearn" @click="handleDel">删除</a-button>
            <a-button type="link" danger @click="handleDelAll">一键清空 </a-button>
          </template>
          <template #bodyCell="{ column, record }">
            <template v-if="column.key === 'action'">
              <TableAction :actions="getTableActions(record)" />
            </template>
          </template>
        </BasicTable>
      </div>
    </div>
    <Detail @register="registerForm" @reload="reload" />
  </div>
</template>
<script lang="ts" setup>
  import { onMounted } from 'vue';
  import { BasicTable, useTable, TableAction, BasicColumn, ActionItem } from '/@/components/Table';
  import { useI18n } from '/@/hooks/web/useI18n';
  import { getMsgMonitorList, delMsgMonitor, emptyMsgMonitor } from '/@/api/msgCenter/msgMonitor';
  import { getMsgTypeList } from '/@/api/msgCenter/msgTemplate';
  import { useMessage } from '/@/hooks/web/useMessage';
  import { useModal } from '/@/components/Modal';
  import { useBaseStore } from '/@/store/modules/base';
  import Detail from './Detail.vue';

  defineOptions({ name: 'msgCenter-msgMonitor' });

  const { t } = useI18n();
  const baseStore = useBaseStore();
  const { createMessage, createConfirm } = useMessage();
  const columns: BasicColumn[] = [
    { title: '消息类型', dataIndex: 'messageType', width: 100 },
    { title: '消息来源', dataIndex: 'messageSource', width: 100 },
    { title: '消息标题', dataIndex: 'title' },
    { title: '发送时间', dataIndex: 'sendTime', width: 150, format: 'date|YYYY-MM-DD HH:mm' },
  ];
  const [registerForm, { openModal: openFormModal }] = useModal();
  const [registerTable, { reload, getSelectRows, clearSelectedRowKeys, getForm }] = useTable({
    api: getMsgMonitorList,
    columns,
    useSearchForm: true,
    formConfig: {
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
        {
          field: 'messageType',
          label: '消息类型',
          component: 'Select',
          componentProps: {
            placeholder: '请选择消息类型',
          },
        },
        {
          field: 'pickerVal',
          label: '发送时间',
          component: 'DateRange',
        },
        {
          field: 'messageSource',
          label: '消息类型',
          component: 'Select',
          componentProps: {
            placeholder: '请选择消息类型',
          },
        },
      ],
      fieldMapToTime: [['pickerVal', ['startTime', 'endTime']]],
    },
    actionColumn: {
      width: 50,
      title: '操作',
      dataIndex: 'action',
    },
    rowSelection: { type: 'checkbox' },
    clickToRowSelect: false,
  });

  function getTableActions(record): ActionItem[] {
    return [
      {
        label: '查看',
        onClick: handleDetail.bind(null, record.id),
      },
    ];
  }
  function handleDel() {
    const selectData = getSelectRows() || [];
    if (!selectData.length) return createMessage.error('请选择一条数据');
    createConfirm({
      iconType: 'warning',
      title: t('common.tipTitle'),
      content: '您确定要删除这些数据吗, 是否继续?',
      onOk: () => {
        const query = {
          ids: selectData.map(item => item.id),
        };
        delMsgMonitor(query).then(res => {
          createMessage.success(res.msg);
          clearSelectedRowKeys();
          reload();
        });
      },
    });
  }
  function handleDelAll() {
    createConfirm({
      iconType: 'warning',
      title: t('common.tipTitle'),
      content: '您确定要清空全部数据吗, 是否继续?',
      onOk: () => {
        emptyMsgMonitor().then(res => {
          createMessage.success(res.msg);
          reload();
        });
      },
    });
  }
  function handleDetail(id) {
    openFormModal(true, { id });
  }
  async function init() {
    const data = await baseStore.getMsgTypeData();
    data.map(o => (o.id = o.enCode));
    getForm().updateSchema([{ field: 'messageType', componentProps: { options: data } }]);
    getMsgTypeList(4).then(res => {
      const messageSourceList = res.data || [];
      messageSourceList.map(o => (o.id = o.enCode));
      getForm().updateSchema([{ field: 'messageSource', componentProps: { options: messageSourceList } }]);
    });
  }

  onMounted(() => {
    init();
  });
</script>
