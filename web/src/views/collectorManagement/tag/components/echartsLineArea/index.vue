<template>
  <div class="page-content-wrapper">
    <div class="page-content-wrapper-center bg-white p-10px">
      <a-form
        ref="formRef"
        :model="formState"
        name="horizontal_login"
        layout="inline"
        autocomplete="off"
        style="margin-bottom: 20px">
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
      <a-tabs v-model:activeKey="activeKey" @change="tabsChange">
        <a-tab-pane key="1" tab="曲线">
          <Chart :options="options" height="500px" />
        </a-tab-pane>
        <a-tab-pane key="2" tab="表格" force-render>
          <BasicTable @register="registerTable" :searchInfo="searchInfo"></BasicTable>
        </a-tab-pane>
      </a-tabs>
    </div>
  </div>
</template>
<script lang="ts" setup>
  import { reactive, watch, ref } from 'vue';
  import { Chart } from '/@/components/Chart';
  import { RangePicker as ARangePicker } from 'ant-design-vue';
  import type { FormInstance } from 'ant-design-vue';
  import { tagHistory } from '/@/api/collector';
  import * as dayjs from 'dayjs';
  import { historyPage } from '/@/api/collector';
  import { BasicTable, useTable, TableAction, BasicColumn, FormProps, ActionItem } from '/@/components/Table';
  import { Tabs as ATabs } from 'ant-design-vue';

  defineOptions({ name: 'echartsLineArea' });

  const props = defineProps({
    dataEchart: {
      type: Object,
      default: {},
    },
  });

  const formRef = ref<FormInstance>();
  const activeKey = ref('1');
  const formState = reactive({
    datetime: props.dataEchart.date,
  });

  const options = reactive({
    title: {
      text: props.dataEchart.title,
    },
    tooltip: {
      trigger: 'axis',
      axisPointer: {
        type: 'cross',
        label: {
          backgroundColor: '#6a7985',
        },
      },
    },
    grid: {
      left: '6%',
      right: '4%',
      bottom: '3%',
      containLabel: true,
    },
    xAxis: [
      {
        type: 'category',
        boundaryGap: false,
        data: props.dataEchart.xAxisData,
        axisLabel: {
          rotate: 20,
        },
      },
    ],
    yAxis: [
      {
        type: 'value',
        axisLabel: {
          show: true,
          formatter: function (value) {
            if (value >= 10000) {
              return value / 10000 + 'w';
            } else {
              return value;
            }
          },
        },
      },
    ],
    series: [
      {
        smooth: true,
        type: 'line',
        data: props.dataEchart.seriesData,
      },
    ],
  });

  watch(
    () => props.dataEchart,
    () => {
      formState.datetime = props.dataEchart.date;
      options.title.text = props.dataEchart.title;
      options.xAxis[0].data = props.dataEchart.xAxisData;
      options.series[0].data = props.dataEchart.seriesData;
    },
    { immediate: true, deep: true },
  );
  // 重置
  const resetForm = () => {
    formState.datetime = props.dataEchart.date;
    submitForm();
  };
  // 搜索
  const submitForm = () => {
    // 曲线
    echartFun();
    // 表格
    reload();
  };
  // 曲线
  echartFun();
  function echartFun() {
    let data = {
      tagId: props.dataEchart.id,
      start: formState.datetime[0],
      end: formState.datetime[1],
    };
    tagHistory(data).then(res => {
      if (res.code === 200 && res.data.length > 0) {
        options.title.text = res.data[0].name;
        options.xAxis[0].data = Array.from(res.data, ({ timeStamp }) => dayjs(timeStamp).format('MM/DD HH:mm:ss'));
        options.series[0].data = Array.from(res.data, ({ value }) => value);
      } else {
        options.title.text = '';
        options.xAxis[0].data = [];
        options.series[0].data = [];
      }
    });
  }

  // 表格
  const columns: BasicColumn[] = [
    { title: 'ID', dataIndex: 'tagId' },
    { title: '名称', dataIndex: 'name' },
    { title: '状态', dataIndex: 'state' },
    { title: '值', dataIndex: 'value' },
    { title: '时间', dataIndex: 'timeStamp', width: 200, format: 'date|YYYY-MM-DD HH:mm' },
  ];
  const searchInfo = reactive({
    tagId: props.dataEchart.id, //关键字
    start: formState.datetime[0],
    end: formState.datetime[1],
  });

  const [registerTable, { reload }] = useTable({
    api: historyPage,
    columns,
  });

  reload();

  // // 切换曲线和表格
  // function tabsChange() {
  //   if (activeKey.value === '1') {
  //     // 曲线
  //     console.log('曲线');
  //     echartFun();
  //   } else {
  //     // 表格
  //     console.log('表格');
  //     reload();
  //   }
  // }
</script>
