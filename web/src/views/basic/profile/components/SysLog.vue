<template>
  <div class="page-content-wrapper bg-white sysLog">
    <div class="page-content-wrapper-center">
      <div class="page-content-wrapper-search-box">
        <BasicForm @register="registerForm" @submit="handleSubmit" @reset="handleReset" />
      </div>
      <div class="page-content-wrapper-content bg-white">
        <a-tabs v-model:activeKey="activeKey" class="page-content-wrapper-tabs" destroyInactiveTabPane>
          <a-tab-pane key="1" tab="登录日志">
            <BasicTable @register="registerTable" :columns="loginTableColumns" :searchInfo="getSearchInfo"></BasicTable>
          </a-tab-pane>
          <a-tab-pane key="5" tab="请求日志">
            <BasicTable @register="registerTable" :columns="requestTableColumns" :searchInfo="getSearchInfo"></BasicTable>
          </a-tab-pane>
        </a-tabs>
      </div>
    </div>
  </div>
</template>
<script lang="ts" setup>
  import { reactive, toRefs, watch, onMounted, computed, nextTick } from 'vue';
  import { getLogList } from '/@/api/permission/userSetting';
  import { BasicForm, useForm } from '/@/components/Form';
  import { useI18n } from '/@/hooks/web/useI18n';
  import { BasicTable, useTable, BasicColumn } from '/@/components/Table';

  interface State {
    activeKey: string;
    keyword: string;
    startTime: number;
    endTime: number;
  }

  const { t } = useI18n();
  const state = reactive<State>({
    activeKey: '1',
    keyword: '',
    startTime: 0,
    endTime: 0,
  });
  const { activeKey } = toRefs(state);

  const getSearchInfo = computed(() => ({
    keyword: state.keyword,
    category: state.activeKey,
    startTime: state.startTime || null,
    endTime: state.endTime || null,
  }));

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
    { title: '请求时间', dataIndex: 'creatorTime', width: 150, format: 'date|YYYY-MM-DD HH:mm' },
    { title: '请求用户', dataIndex: 'userName', width: 120 },
    { title: '请求IP', dataIndex: 'ipaddress', width: 120 },
    { title: '请求设备', dataIndex: 'platForm', width: 200 },
    { title: '请求地址', dataIndex: 'requestURL', width: 200 },
    { title: '请求类型', dataIndex: 'requestMethod', width: 80, align: 'center' },
    { title: '耗时(毫秒)', dataIndex: 'requestDuration', width: 100 },
  ];
  const [registerTable, { reload }] = useTable({
    api: getLogList,
    immediate: false,
    clickToRowSelect: false,
    showTableSetting: false,
  });

  watch(
    () => state.activeKey,
    () => {
      nextTick(() => resetFields());
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
      reload({ page: 1 });
    });
  }

  onMounted(() => {
    reload({ page: 1 });
  });
</script>
<style lang="less" scoped>
  .sysLog {
    .page-content-wrapper-tabs {
      :deep(.ant-tabs-nav) {
        margin-bottom: 0 !important;
      }
    }
  }
</style>
