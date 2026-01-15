<template>
  <div class="page-content-wrapper">
    <div class="page-content-wrapper-center">
      <div class="page-content-wrapper-search-box">
        <BasicForm @register="registerForm" @submit="handleSubmit" @reset="handleReset" />
      </div>
      <div class="page-content-wrapper-content bg-white">
        <a-tabs v-model:activeKey="activeKey" type="card" class="page-content-wrapper-tabs" destroyInactiveTabPane>
          <a-tab-pane key="1" tab="登录日志">
            <BasicTable @register="registerLoginTable" :columns="loginTableColumns" :searchInfo="getSearchInfo">
              <template #tableTitle>
                <a-button type="error" preIcon="icon-ym icon-ym-btn-clearn" @click="handleDelete">删除</a-button>
                <a-button type="link" danger @click="handleDelAll">一键清空</a-button>
              </template>
            </BasicTable>
          </a-tab-pane>
          <a-tab-pane key="5" tab="请求日志">
            <BasicTable @register="registerRequestTable" :columns="requestTableColumns" :searchInfo="getSearchInfo">
              <template #tableTitle>
                <a-button type="error" preIcon="icon-ym icon-ym-btn-clearn" @click="handleDelete">删除</a-button>
                <a-button type="link" danger @click="handleDelAll">一键清空</a-button>
              </template>
            </BasicTable>
          </a-tab-pane>
          <a-tab-pane key="3" tab="操作日志">
            <BasicTable @register="registerOperationTable" :columns="operationTableColumns" :searchInfo="getSearchInfo">
              <template #tableTitle>
                <a-button type="error" preIcon="icon-ym icon-ym-btn-clearn" @click="handleDelete">删除</a-button>
                <a-button type="link" danger @click="handleDelAll">一键清空</a-button>
              </template>
              <template #bodyCell="{ column, record }">
                <template v-if="column.key === 'json'">
                  <a :title="record.json" @click="handleView(record.json, '操作记录')">{{ record.json }}</a>
                </template>
              </template>
            </BasicTable>
          </a-tab-pane>
          <a-tab-pane key="4" tab="异常日志">
            <BasicTable @register="registerErrorTable" :columns="errorTableColumns" :searchInfo="getSearchInfo">
              <template #tableTitle>
                <a-button type="error" preIcon="icon-ym icon-ym-btn-clearn" @click="handleDelete">删除</a-button>
                <a-button type="link" danger @click="handleDelAll">一键清空</a-button>
              </template>
              <template #bodyCell="{ column, record }">
                <template v-if="column.key === 'json'">
                  <a :title="record.json" @click="handleView(record.json, '异常描述')">{{ record.json }}</a>
                </template>
              </template>
            </BasicTable>
          </a-tab-pane>
        </a-tabs>
      </div>
    </div>
    <Form @register="registerDetailForm" />
  </div>
</template>
<script lang="ts" setup>
  import { reactive, toRefs, watch, onMounted, computed, nextTick } from 'vue';
  import { getLogList, delLog, batchDelLog } from '/@/api/system/log';
  import { BasicForm, useForm } from '/@/components/Form';
  import { useMessage } from '/@/hooks/web/useMessage';
  import { useI18n } from '/@/hooks/web/useI18n';
  import { BasicTable, useTable, BasicColumn } from '/@/components/Table';
  import { usePopup } from '/@/components/Popup';
  import Form from './Form.vue';

  defineOptions({ name: 'system-log' });

  interface State {
    activeKey: string;
    keyword: string;
    startTime: number;
    endTime: number;
  }

  const { createMessage, createConfirm } = useMessage();
  const { t } = useI18n();
  const state = reactive<State>({
    activeKey: '1',
    keyword: '',
    startTime: 0,
    endTime: 0,
  });
  const { activeKey } = toRefs(state);

  const getSearchInfo = computed(() => ({ keyword: state.keyword, type: state.activeKey, startTime: state.startTime || null, endTime: state.endTime || null }));

  const [registerForm] = useForm({
    baseColProps: { span: 6 },
    showActionButtonGroup: true,
    showAdvancedButton: true,
    compact: true,
    labelAlign: 'left',
    labelWidth: 60,
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
        field: 'pickerVal',
        label: '开始时间',
        component: 'DateRange',
      },
    ],
    fieldMapToTime: [['pickerVal', ['startTime', 'endTime']]],
  });
  const loginTableColumns: BasicColumn[] = [
    { title: '登录时间', dataIndex: 'creatorTime', width: 150, format: 'date|YYYY-MM-DD HH:mm' },
    { title: '登录用户', dataIndex: 'userName', width: 120 },
    { title: '登录IP', dataIndex: 'ipaddress', width: 120 },
    { title: '登录设备', dataIndex: 'platForm' },
  ];
  const requestTableColumns: BasicColumn[] = [
    { title: '请求用户', dataIndex: 'userName', width: 120 },
    { title: '请求IP', dataIndex: 'ipaddress', width: 120 },
    { title: '请求设备', dataIndex: 'platForm', width: 200 },
    { title: '请求地址', dataIndex: 'requestURL', width: 200 },
    { title: '请求类型', dataIndex: 'requestMethod', width: 80, align: 'center' },
    { title: '耗时(毫秒)', dataIndex: 'requestDuration', width: 100 },
  ];
  const operationTableColumns: BasicColumn[] = [
    { title: '操作时间', dataIndex: 'creatorTime', width: 150, format: 'date|YYYY-MM-DD HH:mm' },
    { title: '操作用户', dataIndex: 'userName', width: 120 },
    { title: '操作IP', dataIndex: 'ipaddress', width: 160 },
    { title: '操作模块', dataIndex: 'moduleName', width: 80 },
    { title: '操作类型', dataIndex: 'requestMethod', width: 80, align: 'center' },
    { title: '耗时(毫秒)', dataIndex: 'requestDuration', width: 100 },
    { title: '操作记录', dataIndex: 'json', maxWidth: 200 },
  ];
  const errorTableColumns: BasicColumn[] = [
    { title: '创建时间', dataIndex: 'creatorTime', width: 150, format: 'date|YYYY-MM-DD HH:mm' },
    { title: '创建用户', dataIndex: 'userName', width: 120 },
    { title: '异常IP', dataIndex: 'ipaddress', width: 120 },
    { title: '异常功能', dataIndex: 'moduleName', width: 120 },
    { title: '异常描述', dataIndex: 'json' },
  ];
  const [registerLoginTable, { reload: reloadLoginTable, getSelectRows: getLoginSelectRows }] = useTable({
    api: getLogList,
    rowSelection: { type: 'checkbox' },
    immediate: false,
    clickToRowSelect: false,
  });
  const [registerRequestTable, { reload: reloadRequestTable, getSelectRows: getRequestSelectRows }] = useTable({
    api: getLogList,
    rowSelection: { type: 'checkbox' },
    immediate: false,
    clickToRowSelect: false,
  });
  const [registerOperationTable, { reload: reloadOperationTable, getSelectRows: getOperationSelectRows }] = useTable({
    api: getLogList,
    rowSelection: { type: 'checkbox' },
    immediate: false,
    clickToRowSelect: false,
  });
  const [registerErrorTable, { reload: reloadErrorTable, getSelectRows: getErrorSelectRows }] = useTable({
    api: getLogList,
    rowSelection: { type: 'checkbox' },
    immediate: false,
    clickToRowSelect: false,
  });
  const [registerDetailForm, { openPopup: openFormPopup }] = usePopup();

  watch(
    () => state.activeKey,
    () => {
      nextTick(() => reload());
    },
  );

  function handleSubmit(values) {
    state.keyword = values?.keyword || '';
    state.startTime = values?.startTime || 0;
    state.endTime = values?.endTime || 0;
    handleSearch();
  }
  function handleReset() {
    state.keyword = '';
    state.startTime = 0;
    state.endTime = 0;
    handleSearch();
  }
  function handleSearch() {
    nextTick(() => {
      reload();
    });
  }
  function handleDelete() {
    const list: any[] = getSelectData() || [];
    if (!list.length) return createMessage.error('请选择一条数据');
    const query = {
      ids: list.map(item => item.id),
    };
    delLog(query).then(res => {
      createMessage.success(res.msg);
      reload();
    });
  }
  function handleDelAll() {
    createConfirm({
      iconType: 'warning',
      title: t('common.tipTitle'),
      content: '此操作会将所有日志删除，是否继续?',
      onOk: () => {
        batchDelLog(state.activeKey).then(res => {
          createMessage.success(res.msg);
          reload();
        });
      },
    });
  }
  function handleView(json, title) {
    openFormPopup(true, { json, title });
  }
  function reload() {
    if (state.activeKey === '1') reloadLoginTable({ page: 1 });
    if (state.activeKey === '5') reloadRequestTable({ page: 1 });
    if (state.activeKey === '3') reloadOperationTable({ page: 1 });
    if (state.activeKey === '4') reloadErrorTable({ page: 1 });
  }
  function getSelectData() {
    if (state.activeKey === '1') return getLoginSelectRows();
    if (state.activeKey === '5') return getRequestSelectRows();
    if (state.activeKey === '3') return getOperationSelectRows();
    if (state.activeKey === '4') return getErrorSelectRows();
  }

  onMounted(() => {
    reload();
  });
</script>
