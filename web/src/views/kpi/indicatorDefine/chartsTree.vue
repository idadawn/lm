<template>
  <div :class="activeKey == 'tab1' ? 'chartContainer' : 'chartContainer02'">
    <!-- <div class="page-content-wrapper">
      <div class="page-content-wrapper-center">
        <div class="page-content-wrapper-content"> -->
    <Card :tab-list="tabListTitle" v-bind="$attrs" :active-tab-key="activeKey" @tab-change="onTabChange">
      <div v-show="activeKey === 'tab1'">
        <!-- 时间筛选框   -->
        <div class="searchBox">
          <BasicForm @register="registerForm" @submit="handleSubmit" @reset="handleReset" />
        </div>
        <div class="md:flex enter-y middleBox" v-if="state.elementsLength != 0">
          <chartsModel :chartsFlag="state.chartsFlag" :chartsData="state.ChartsObj" />
        </div>
        <div class="chartbox" v-else-if="state.elementsLength == 0">
          <div class="portal-layout-nodata">
            <img src="../../../assets/images/dashboard-nodata.png" alt="" class="layout-nodata-img" />
            <p class="layout-nodata-txt">暂无数据</p>
          </div>
        </div>
      </div>
      <div v-if="activeKey === 'tab2'">
        <!-- 归因分析模块 -->
        <AttributionAnalysis :recordId="route.query.recordId" :dimensionOptionsList="dimensionOptionsList" />
      </div>
    </Card>
    <MessageDrawer @register="registerMessageDrawer" @readMsg="readMsg" />
    <!-- </div>
      </div>
    </div> -->
  </div>
</template>
<script lang="ts" setup>
  import { reactive, ref, toRefs, toRef, watch, computed, nextTick, onMounted } from 'vue';
  import { Card, Button } from 'ant-design-vue';
  import { useRouter, useRoute } from 'vue-router';

  import chartsModel from './components/chartsModel.vue';
  import AttributionAnalysis from './components/AttributionAnalysis.vue';

  import { BasicForm, useForm } from '/@/components/Form';
  import { useI18n } from '/@/hooks/web/useI18n';
  import { getChartsFormatData } from '/@/api/basic/charts';
  import { isArray } from '@vue/shared';

  import { getChartsData, getBasicInfo, getDeriveInfo, getCoppositeInfo } from '/@/api/targetDirectory';
  import { getDimensionOptionsList } from '/@/api/dimension/model';

  const { t } = useI18n();
  const activeKey = ref('tab1');

  const state = reactive<State>({
    activeKey: '0',
    keyword: '',
    chartsFlag: 'axis',
    rowLimit: '',
    ChartsObj: {
      // legendData: ['直接', '最优值'],
      xAxisData: [],
      seriesData: [
        {
          name: '',
          type: 'pie',
          stack: 'Total',
          data: [],
        },
      ],
      valueData: [],
    },
    ChartsDataparams: {
      metricId: '',
      dimensions: '',
      limit: 0,
    },
  });
  const dimensionOptionsList = ref<{ fullName: string; id: number }[]>([]);

  const router = useRouter();
  const route = useRoute();
  const Dates = [];
  const onChange = (time: '', timeString: string) => {
    // 
  };

  const tabListTitle = [
    {
      key: 'tab1',
      tab: '图表',
    },
    {
      key: 'tab2',
      tab: '归因分析',
    },
    // {
    //   key: 'tab3',
    //   tab: '定义',
    // },
  ];
  const [registerForm, { setFieldsValue }] = useForm({
    baseColProps: { span: 6 },
    // actionColOptions: { span: 24 },
    showActionButtonGroup: true,
    showAdvancedButton: true,
    compact: true,
    labelAlign: 'left',
    labelWidth: 60,
    schemas: [
      {
        field: 'dimensions',
        label: '维度',
        component: 'Select',
        // defaultValue: '888',
        componentProps: { options: dimensionOptionsList },
      },
      {
        field: 'chartsFlag',
        label: '图表类型',
        component: 'Select',
        componentProps: {
          placeholder: '请选择类型',
          options: [
            { fullName: '折线图', id: 'axis' },
            { fullName: '饼状图', id: 'pie' },
            { fullName: '柱状图', id: 'histogram' },
            { fullName: '图表', id: 'gudge' },
          ],
        },
      },
      {
        field: 'limit',
        label: '行数限制',
        component: 'Select',
        componentProps: {
          placeholder: '请选择行数限制',
          options: [
            { fullName: '10', id: 10 },
            { fullName: '20', id: 20 },
          ],
        },
      },
    ],
  });
  const apiMethod = ref();
  switch (route.query.recordType) {
    case 'Basic':
      getBasicRecordInfo();
      break;
    case 'Derive':
      getDeriveRecordInfo();
      break;
    case 'Composite':
      getcompositeRecordInfo();
      break;
  }
  onMounted(() => {});
  function onTabChange(key) {
    activeKey.value = key;
    getChartsDataList();
  }
  function handleSubmit(values) {
    state.ChartsDataparams.dimensions = values?.dimensions || '';
    state.ChartsDataparams.limit = values?.limit || '0';
    state.chartsFlag = values?.chartsFlag || '';
    dimensionOptionsList.value.map(item => {
      if (values.dimensions == item.field) {
        state.ChartsDataparams.dimensions = item;
      }
    });
    getChartsDataList();
  }
  function handleReset() {
    handleSearch();
  }
  function handleSearch() {
    getChartsDataList();
  }
  async function getBasicRecordInfo() {
    try {
      const res = await getBasicInfo(route.query.recordId);
      res.data.dimensions.map(item => {
        item['fullName'] = item.fieldName;
        item['id'] = item.field;
      });
      dimensionOptionsList.value = res.data.dimensions;
      state.ChartsDataparams.metricId = route.query.recordId;

      getChartsDataList();
    } catch (_) {}
  }
  async function getcompositeRecordInfo() {
    try {
      const res = await getCoppositeInfo(route.query.recordId);

      res.data.dimensions.map(item => {
        item['fullName'] = item.fieldName;
        item['id'] = item.field;
      });
      dimensionOptionsList.value = res.data.dimensions;
      state.ChartsDataparams.metricId = route.query.recordId;

      getChartsDataList();
    } catch (_) {}
  }
  async function getDeriveRecordInfo() {
    try {
      const res = await getDeriveInfo(route.query.recordId);
      res.data.dimensions.map(item => {
        item['fullName'] = item.fieldName;
        item['id'] = item.field;
      });
      dimensionOptionsList.value = res.data.dimensions;
      state.ChartsDataparams.metricId = route.query.recordId;

      getChartsDataList();
    } catch (_) {}
  }
  async function getChartsDataList() {
    state.ChartsDataparams.metricId = route.query.recordId;
    setFieldsValue({
      dimensions: state.ChartsDataparams.dimensions
        ? state.ChartsDataparams.dimensions.id
        : dimensionOptionsList.value[0].id,
    });
    if (!state.ChartsDataparams.dimensions) {
      //初始化进来
      state.ChartsDataparams.dimensions = dimensionOptionsList.value[0];
    }
    try {
      const res = await getChartsData({
        ...state.ChartsDataparams,
      });
      state.elements = res.data.data.data;
      state.elementsLength = res.data.data.data.length ? '1' : '0';
      if (state.chartsFlag == 'pie') {
        let valueData = [];
        state.elements.map(item => {
          let valueDataObj = {};
          valueDataObj['value'] = item[1];
          valueDataObj['name'] = item[0];
          valueData.push(valueDataObj);
        });

        state.ChartsObj.valueData = valueData;
        // state.ChartsObj.seriesData[0].type='pie'
      } else if (state.chartsFlag == 'gudge') {
        //图表区域
        let valueData = [];
        state.elements.map(item => {
          let valueDataObj = {};
          valueDataObj['value'] = item[0];
          valueDataObj['name'] = item[1];
          valueData.push(valueDataObj);
        });
        state.ChartsObj.valueData = valueData;
      } else if (state.chartsFlag == 'histogram') {
        //柱状图
        //初始化XY轴数据
        state.ChartsObj.xAxisData = [];
        state.ChartsObj.seriesData[0].data = [];
        state.elements.map(item => {
          item[0] ? state.ChartsObj.xAxisData.push(item[0]) : state.ChartsObj.xAxisData.push('');
          item[1] ? state.ChartsObj.seriesData[0].data.push(item[1]) : state.ChartsObj.seriesData[0].data.push('');
        });
        state.ChartsObj.seriesData[0].type = 'bar';
      } else {
        //初始化XY轴数据
        state.ChartsObj.xAxisData = [];
        state.ChartsObj.seriesData[0].data = [];
        state.elements.map(item => {
          item[0] ? state.ChartsObj.xAxisData.push(item[0]) : state.ChartsObj.xAxisData.push('');
          item[1] ? state.ChartsObj.seriesData[0].data.push(item[1]) : state.ChartsObj.seriesData[0].data.push('');
        });
        state.ChartsObj.seriesData[0].type = 'line';
      }
    } catch (_) {}
  }
</script>
<style lang="less" scoped>
  .page-content-wrapper-content {
    background: #fff;
  }
  .chartContainer {
    height: calc(100vh - 180px);
    background: #fff;
  }
  .chartContainer02 {
    height: 100%;
    overflow-y: scroll;
    background: #fff;
  }
  .chartbox {
    width: 100%;
    height: calc(100vh - 340px);
    // border: 1px solid red;
    background: #fff;
  }
  .portal-layout-nodata {
    text-align: center;
    position: absolute;
    top: calc(50% - 200px);
    // top: 50%;
    left: calc(50% - 200px);

    .layout-nodata-img {
      width: 400px;
      height: 400px;
    }

    .layout-nodata-txt {
      margin-top: -60px;
      font-size: 20px;
      color: #909399;
      line-height: 30px;
    }
  }
</style>
<style>
  @keyframes blink {
    0% {
      opacity: 1;
    }

    100% {
      opacity: 0;
    }
  }

  .blink {
    animation: blink 0.3s linear infinite alternate;
  }

  .middleBox {
    margin-top: 40px;

    /* border: 1px solid red; */
  }

  /* .searchBox {
    display: flex;
    align-items: center;
  } */
</style>
