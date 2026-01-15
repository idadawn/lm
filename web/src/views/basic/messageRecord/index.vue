<template>
  <div class="page-content-wrapper message-record-wrapper">
    <div class="page-content-wrapper-center">
      <div class="page-content-wrapper-search-box">
        <BasicForm @register="registerForm" @submit="handleSubmit" @reset="handleReset" />
      </div>
      <div class="page-content-wrapper-content bg-white">
        <a-tabs v-model:activeKey="activeKey" type="card" class="page-content-wrapper-tabs" destroyInactiveTabPane>
          <a-tab-pane tab="全部" key="0"></a-tab-pane>
          <a-tab-pane tab="系统" key="3"></a-tab-pane>
          <a-tab-pane tab="流程" key="2"></a-tab-pane>
          <a-tab-pane tab="公告" key="1"></a-tab-pane>
          <a-tab-pane tab="日程" key="4"></a-tab-pane>
        </a-tabs>
        <BasicTable @register="registerTable" :columns="columns" :searchInfo="getSearchInfo">
          <template #tableTitle>
            <a-button type="error" preIcon="icon-ym icon-ym-btn-clearn" @click="handleDelete">删除</a-button>
          </template>
          <template #bodyCell="{ column, record }">
            <template v-if="column.key === 'title'">
              <a :title="record.name" @click="handleView(record)">{{ record.title }}</a>
            </template>
            <template v-if="column.key === 'type'">
              <span v-if="record.type == 1">公告</span>
              <span v-if="record.type == 2">流程</span>
              <span v-if="record.type == 3">系统</span>
              <span v-if="record.type == 4">日程</span>
            </template>
            <template v-if="column.key === 'isRead'">
              <a-tag :color="record.isRead == 1 ? 'success' : ''">{{ record.isRead == 1 ? '已读' : '未读' }}</a-tag>
            </template>
          </template>
        </BasicTable>
      </div>
    </div>
    <Detail @register="registerDetail" />
    <ScheduleDetail @register="registerScheduleDetail" />
  </div>
</template>
<script lang="ts" setup>
  import { reactive, toRefs, watch, computed, nextTick } from 'vue';
  import { getMessageList, readInfo, delMsgRecord } from '/@/api/system/message';
  import { BasicForm, useForm } from '/@/components/Form';
  import { useMessage } from '/@/hooks/web/useMessage';
  import { useI18n } from '/@/hooks/web/useI18n';
  import { BasicTable, useTable, BasicColumn } from '/@/components/Table';
  import { useModal } from '/@/components/Modal';
  import { useRouter } from 'vue-router';
  import { encryptByBase64 } from '/@/utils/cipher';
  import Detail from '/@/views/system/notice/Detail.vue';
  import ScheduleDetail from '/@/components/VisualPortal/Portal/HSchedule/Detail.vue';

  defineOptions({ name: 'messageRecord' });

  interface State {
    activeKey: string;
    keyword: string;
  }

  const router = useRouter();
  const { createMessage, createConfirm } = useMessage();
  const { t } = useI18n();
  const state = reactive<State>({
    activeKey: '0',
    keyword: '',
  });
  const { activeKey } = toRefs(state);

  const getSearchInfo = computed(() => ({ keyword: state.keyword, type: state.activeKey == '0' ? '' : state.activeKey }));

  const [registerForm, { resetFields }] = useForm({
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
    ],
  });
  const columns: BasicColumn[] = [
    { title: '消息标题', dataIndex: 'title' },
    { title: '消息类型', dataIndex: 'type', width: 120 },
    { title: '发送人员', dataIndex: 'releaseUser', width: 120 },
    { title: '发送时间', dataIndex: 'releaseTime', width: 150, format: 'date|YYYY-MM-DD HH:mm' },
    { title: '状态', dataIndex: 'isRead', width: 70 },
  ];
  const [registerTable, { reload, getSelectRows, clearSelectedRowKeys }] = useTable({
    api: getMessageList,
    rowSelection: { type: 'checkbox' },
    clickToRowSelect: false,
  });
  const [registerDetail, { openModal: openDetailModal }] = useModal();
  const [registerScheduleDetail, { openModal: openScheduleDetailModal }] = useModal();

  watch(
    () => state.activeKey,
    () => {
      resetFields();
    },
  );

  function handleSubmit(values) {
    state.keyword = values?.keyword || '';
    handleSearch();
  }
  function handleReset() {
    state.keyword = '';
    handleSearch();
  }
  function handleSearch() {
    nextTick(() => {
      reload({ page: 1 });
    });
  }
  function handleDelete() {
    const list: any[] = getSelectRows();
    if (!list.length) return createMessage.error('请选择一条数据');
    const query = { ids: list.map(item => item.id).join(',') };
    createConfirm({
      iconType: 'warning',
      title: t('common.tipTitle'),
      content: '您确定要删除这些数据吗, 是否继续?',
      onOk: () => {
        delMsgRecord(query).then(res => {
          createMessage.success(res.msg);
          clearSelectedRowKeys();
          reload();
        });
      },
    });
  }
  function handleView(item) {
    readInfo(item.id).then(res => {
      if (item.isRead == '0') item.isRead = '1';
      if (item.type == 4) {
        let bodyText = res.data.bodyText ? JSON.parse(res.data.bodyText) : {};
        if (bodyText.type == 3) return;
        openScheduleDetailModal(true, { id: bodyText.id, groupId: bodyText.groupId });
      } else if (item.type == 2 && item.flowType == 2) {
        const bodyText = JSON.parse(res.data.bodyText);
        router.push('/workFlow/entrust?config=' + bodyText.type);
      } else {
        if (item.type == 1 || item.type == 3) {
          openDetailModal(true, { id: item.id, type: 1 });
        } else {
          if (!res.data.bodyText) return;
          router.push('/workFlowDetail?config=' + encodeURIComponent(encryptByBase64(res.data.bodyText)));
        }
      }
    });
  }
</script>
<style lang="less" scoped>
  .message-record-wrapper {
    .page-content-wrapper-tabs {
      height: 40px !important;
    }
  }
</style>
