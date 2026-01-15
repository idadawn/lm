<template>
  <div class="page-content-wrapper">
    <div class="page-content-wrapper-center">
      <div class="page-content-wrapper-content">
        <BasicTable @register="registerTable">
          <template #tableTitle>
            <a-button type="primary" preIcon="icon-ym icon-ym-btn-add" @click="addOrUpdateHandle()">新建</a-button>
          </template>
          <template #bodyCell="{ column, record }">
            <template v-if="column.key === 'enabledMark'">
              <a-tag :color="record.enabledMark == 1 ? 'success' : 'error'">{{ record.enabledMark == 1 ? '启用' : '禁用' }}</a-tag>
            </template>
            <template v-if="column.key === 'messageType'">
              <span class="my-span-tag" :style="{ background: colorList[item.type] }" v-for="(item, index) in record.messageType" :key="index">
                {{ item.fullName }}
              </span>
            </template>
            <template v-if="column.key === 'action'">
              <TableAction :actions="getTableActions(record)" :dropDownActions="getDropDownActions(record)" />
            </template>
          </template>
        </BasicTable>
      </div>
    </div>
    <Form @register="registerForm" @reload="reload" />
    <Detail @register="registerDetail" />
    <TestSend @register="registerTestSend" />
  </div>
</template>
<script lang="ts" setup>
  import { onMounted, reactive } from 'vue';
  import { BasicTable, useTable, TableAction, BasicColumn, ActionItem } from '/@/components/Table';
  import { getSendConfigList, delSendConfig, copy } from '/@/api/msgCenter/sendConfig';
  import { getMsgTypeList } from '/@/api/msgCenter/msgTemplate';
  import { usePopup } from '/@/components/Popup';
  import { useI18n } from '/@/hooks/web/useI18n';
  import { useMessage } from '/@/hooks/web/useMessage';
  import Form from './Form.vue';
  import Detail from './Detail.vue';
  import TestSend from './components/TestSend.vue';

  defineOptions({ name: 'msgCenter-sendConfig' });

  const { t } = useI18n();
  const colorList = reactive([
    '',
    'rgba(40, 112, 248, 0.1)',
    'rgba(109, 224, 131, 0.1)',
    'rgba(244, 130, 130, 0.1)',
    'rgba(104, 147, 245, 0.1)',
    'rgba(100, 176, 244, 0.1)',
    'rgba(255, 150, 178, 0.1)',
    'rgba(156, 130, 244, 0.1)',
  ]);
  const { createMessage } = useMessage();
  const columns: BasicColumn[] = [
    { title: '名称', dataIndex: 'fullName', width: 180 },
    { title: '编码', dataIndex: 'enCode', width: 150 },
    { title: '消息来源', dataIndex: 'messageSource', width: 100 },
    {
      title: '模板类型',
      dataIndex: 'category',
      width: 100,
      customRender: ({ record }) => (record.templateType == '1' ? '系统模板' : '自定义模板'),
    },
    { title: '消息类型', dataIndex: 'messageType', width: 380 },
    { title: '创建人', dataIndex: 'creatorUser', width: 120 },
    { title: '创建时间', dataIndex: 'creatorTime', width: 150, format: 'date|YYYY-MM-DD HH:mm' },
    { title: '最后修改时间', dataIndex: 'lastModifyTime', width: 150, format: 'date|YYYY-MM-DD HH:mm' },
    { title: '排序', dataIndex: 'sortCode', width: 70, align: 'center' },
    { title: '状态', dataIndex: 'enabledMark', width: 70, align: 'center' },
  ];
  const [registerForm, { openPopup: openFormPopup }] = usePopup();
  const [registerDetail, { openPopup: openDetailPopup }] = usePopup();
  const [registerTestSend, { openPopup: openTestSendPopup }] = usePopup();
  const [registerTable, { reload, getForm }] = useTable({
    api: getSendConfigList,
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
          field: 'messageSource',
          label: '消息来源',
          component: 'Select',
          componentProps: {
            placeholder: '请选择消息来源',
          },
        },
        {
          field: 'templateType',
          label: '模板类型',
          component: 'Select',
          componentProps: {
            placeholder: '请选择模板类型',
            options: [
              { fullName: '系统模板', id: 1 },
              { fullName: '自定义模板', id: 0 },
            ],
          },
        },
        {
          field: 'enabledMark',
          label: '状态',
          component: 'Select',
          componentProps: {
            placeholder: '请选择状态',
            options: [
              { fullName: '启用', id: 1 },
              { fullName: '禁用', id: 0 },
            ],
          },
        },
      ],
    },
    actionColumn: {
      width: 150,
      title: '操作',
      dataIndex: 'action',
    },
  });

  function getTableActions(record): ActionItem[] {
    return [
      {
        label: t('common.editText'),
        disabled: record.templateType == 1,
        onClick: addOrUpdateHandle.bind(null, record.id),
      },
      {
        label: t('common.delText'),
        color: 'error',
        disabled: record.templateType == 1,
        modelConfirm: {
          onOk: handleDelete.bind(null, record.id),
        },
      },
    ];
  }
  function getDropDownActions(record): ActionItem[] {
    return [
      {
        label: '复制',
        modelConfirm: {
          content: '您确定要复制该功能模板, 是否继续?',
          onOk: handleCopy.bind(null, record.id),
        },
      },
      {
        label: '测试发送',
        onClick: handleTestSend.bind(null, record.id),
      },
      {
        label: '详情',
        onClick: handleDetail.bind(null, record),
      },
    ];
  }
  function addOrUpdateHandle(id = '') {
    openFormPopup(true, { id });
  }
  function handleCopy(id) {
    copy(id).then(res => {
      createMessage.success(res.msg);
      reload();
    });
  }
  function handleDetail(record) {
    openDetailPopup(true, { id: record.id, messageSourceName: record.messageSource });
  }
  function handleTestSend(id) {
    openTestSendPopup(true, { id });
  }
  function handleDelete(id) {
    delSendConfig(id).then(res => {
      createMessage.success(res.msg);
      reload();
    });
  }
  async function init() {
    let res = await getMsgTypeList(4);
    res.data.map(o => (o.id = o.enCode));
    getForm().updateSchema([{ field: 'messageSource', componentProps: { options: res.data } }]);
  }

  onMounted(() => {
    init();
  });
</script>
<style lang="less" scoped>
  .my-span-tag {
    margin: 3px 8px 3px 0;
    color: #606266;
    height: 24px;
    padding: 0 8px;
    line-height: 24px;
    border-radius: 4px;
    white-space: nowrap;
    display: inline-block;
    font-size: 12px;
  }
</style>
