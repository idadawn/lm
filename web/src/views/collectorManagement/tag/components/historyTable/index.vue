<template>
  <div class="page-content-wrapper">
    <div class="page-content-wrapper-center bg-white p-10px">
      <a-form ref="formRef" :model="formState" name="horizontal_login" layout="inline" autocomplete="off">
        <a-form-item label="时间" name="datetime">
          <a-range-picker
            v-model:value="formState.datetime"
            show-time
            :allowClear="false"
            format="YYYY/MM/DD HH:mm:ss"
            valueFormat="YYYY/MM/DD HH:mm:ss" />
        </a-form-item>

        <a-form-item>
          <a-button type="primary" class="mr-4" @click="submitForm">搜索</a-button>
          <a-button @click="resetForm">重置</a-button>
        </a-form-item>
      </a-form>
      <BasicTable @register="registerTable" :searchInfo="searchInfo"></BasicTable>
    </div>
    <DepForm @register="registerDepForm" @reload="reload" />
  </div>
</template>
<script lang="ts" setup>
  import { reactive, ref, onMounted, watch } from 'vue';
  import { historyPage } from '/@/api/collector';
  import { BasicTable, useTable, TableAction, BasicColumn, FormProps, ActionItem } from '/@/components/Table';
  import { useMessage } from '/@/hooks/web/useMessage';
  import { useModal } from '/@/components/Modal';
  import { RangePicker as ARangePicker } from 'ant-design-vue';

  defineOptions({ name: 'historyTable' });

  const props = defineProps({
    dataTable: {
      type: Object,
      default: {},
    },
  });
  const formState = reactive({
    datetime: props.dataTable.date,
  });

  const { createMessage } = useMessage();
  const [registerDepForm, { openModal: openDepFormModal }] = useModal();

  const columns: BasicColumn[] = [
    { title: 'ID', dataIndex: 'tagId' },
    { title: '名称', dataIndex: 'name' },
    { title: '状态', dataIndex: 'state' },
    { title: '值', dataIndex: 'value' },
    { title: '时间', dataIndex: 'timeStamp', width: 200, format: 'date|YYYY-MM-DD HH:mm' },
  ];
  const searchInfo = reactive({
    tagId: props.dataTable.id, //关键字
    start: formState.datetime[0],
    end: formState.datetime[1],
  });

  const [registerTable, { reload }] = useTable({
    api: historyPage,
    columns,
  });

  // 重置
  const resetForm = () => {
    formState.datetime = props.dataTable.date;
    submitForm();
  };
  // 搜索
  const submitForm = () => {
    searchInfo.start = formState.datetime[0];
    searchInfo.end = formState.datetime[1];
    reload();
  };
</script>
