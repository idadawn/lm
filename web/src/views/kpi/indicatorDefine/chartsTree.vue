```
<template>
  <div class="w-full bg-gray-50 flex flex-col overflow-hidden" style="height: calc(100vh - 110px)">
    <!-- Top Header & Tabs -->
    <div class="bg-white border-b border-gray-200 px-6 py-4 flex items-center justify-between shrink-0 shadow-sm z-10">
      <div class="flex items-center gap-4">
        <div class="flex bg-gray-100 p-1 rounded-lg">
          <div v-for="item in tabListTitle" :key="item.key" @click="onTabChange(item.key)"
            class="cursor-pointer px-6 py-2 rounded-md transition-all duration-300 text-sm font-medium select-none"
            :class="activeKey === item.key
              ? 'bg-white text-blue-600 shadow-md transform scale-105'
              : 'text-gray-500 hover:text-gray-700 hover:bg-gray-200'
              ">
            {{ item.tab }}
          </div>
        </div>
      </div>
      <div class="flex items-center gap-2">
        <!-- Optional: Right side controls or info -->
        <a-button type="link" @click="handleBack"> 返回列表 </a-button>
      </div>
    </div>

    <!-- Main Content Area -->
    <div class="flex-1 overflow-auto p-6 relative">
      <!-- Tab 1: Charts -->
      <transition name="fade" mode="out-in">
        <div v-show="activeKey === 'tab1'" class="h-full flex flex-col gap-6">
          <!-- Filter Bar -->
          <div class="bg-white rounded-xl shadow-sm p-4 border border-gray-100 transition-all hover:shadow-md">
            <BasicForm @register="registerForm" @submit="handleSubmit" @reset="handleReset" class="!mb-0" />
          </div>

          <!-- Charts Display -->
          <div class="flex-1 bg-white rounded-xl shadow-sm border border-gray-100 p-6 relative min-h-[400px]">
            <div v-if="state.elementsLength != '0'" class="w-full h-full">
              <chartsModel :chartsFlag="state.chartsFlag" :chartsData="state.ChartsObj" gtData="" ltData="" />
            </div>
            <!-- Empty State -->
            <div v-else class="absolute inset-0 flex flex-col items-center justify-center text-gray-400">
              <img src="../../../assets/images/dashboard-nodata.png" alt="No Data"
                class="w-64 h-64 object-contain opacity-80 mb-4" />
              <p class="text-lg font-medium text-gray-500">暂无数据分析</p>
              <p class="text-sm text-gray-400 mt-2">请尝试调整筛选条件</p>
              <!-- Debug info (hidden) -->
              <!-- {{ state.ChartsDataparams }} -->
            </div>
          </div>
        </div>
      </transition>

      <!-- Tab 2: Attribution Analysis -->
      <transition name="fade" mode="out-in">
        <div v-if="activeKey === 'tab2'" class="h-full bg-white rounded-xl shadow-sm border border-gray-100 p-6">
          <AttributionAnalysis :recordId="route.query.recordId" :dimensionOptionsList="dimensionOptionsList" />
        </div>
      </transition>
    </div>

    <!-- <MessageDrawer @register="registerMessageDrawer" @readMsg="readMsg" /> -->
  </div>
</template>
<script lang="ts" setup>
import { reactive, ref, onMounted } from 'vue';
import { useRouter, useRoute } from 'vue-router';

import chartsModel from './components/chartsModel.vue';
import AttributionAnalysis from './components/AttributionAnalysis.vue';

import { BasicForm, useForm } from '/@/components/Form';
// import { useI18n } from '/@/hooks/web/useI18n';
// import { getChartsFormatData } from '/@/api/basic/charts';
// import { isArray } from '@vue/shared';

import { getChartsData, getBasicInfo, getDeriveInfo, getCoppositeInfo } from '/@/api/targetDirectory';
// import { getDimensionOptionsList } from '/@/api/dimension/model';

// const { t } = useI18n();
const activeKey = ref('tab1');

interface ChartsObjType {
  xAxisData: any[];
  seriesData: any[];
  valueData: any[];
}

type LocationQueryValue = string | null;

interface State {
  activeKey: string;
  keyword: string;
  chartsFlag: string;
  rowLimit: string;
  ChartsObj: ChartsObjType;
  ChartsDataparams: {
    metricId: string | LocationQueryValue | LocationQueryValue[];
    dimensions: any;
    limit: number | string;
  };
  elements: any[];
  elementsLength: string;
}

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
        type: 'pie', // This might need to check logic if 'axis' is default
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
  elements: [],
  elementsLength: '0',
});
const dimensionOptionsList = ref<any[]>([]);

const router = useRouter();
const route = useRoute();
// const Dates = [];
// const onChange = (time: '', timeString: string) => {
//   // 
// };

const tabListTitle = [
  {
    key: 'tab1',
    tab: '图表分析',
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
      field: 'dateRange',
      label: '时间',
      component: 'RangePicker',
      componentProps: {
        picker: 'month',
        valueFormat: 'YYYY-MM',
      },
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
          { fullName: '数据表格', id: 'gudge' },
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
// const apiMethod = ref();

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
onMounted(() => { });
function onTabChange(key) {
  activeKey.value = key;
  if (key === 'tab1') {
    getChartsDataList();
  }
}

function handleBack() {
  router.go(-1);
}

function handleSubmit(values) {
  state.ChartsDataparams.dimensions = values?.dimensions || '';
  state.ChartsDataparams.limit = values?.limit || '0';
  state.chartsFlag = values?.chartsFlag || '';

  // Handle Date Range
  if (values.dateRange && values.dateRange.length === 2) {
    // Assuming param names are startTime/endTime based on typical patterns, 
    // but since I can't see the exact backend DTO, I'll add them to params.
    state.ChartsDataparams['startTime'] = values.dateRange[0];
    state.ChartsDataparams['endTime'] = values.dateRange[1];
  } else {
    state.ChartsDataparams['startTime'] = '';
    state.ChartsDataparams['endTime'] = '';
  }

  dimensionOptionsList.value.map(item => {
    // Revert to item.field for safety, consistent with original code
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
    const res = await getBasicInfo(route.query.recordId as string);
    res.data.dimensions.map(item => {
      item['fullName'] = item.fieldName;
      item['id'] = item.field;
    });
    dimensionOptionsList.value = res.data.dimensions;
    state.ChartsDataparams.metricId = route.query.recordId as string;

    getChartsDataList();
  } catch (_) { }
}
async function getcompositeRecordInfo() {
  try {
    const res = await getCoppositeInfo(route.query.recordId as string);

    res.data.dimensions.map(item => {
      item['fullName'] = item.fieldName;
      item['id'] = item.field;
    });
    dimensionOptionsList.value = res.data.dimensions;
    state.ChartsDataparams.metricId = route.query.recordId as string;

    getChartsDataList();
  } catch (_) { }
}
async function getDeriveRecordInfo() {
  try {
    const res = await getDeriveInfo(route.query.recordId as string);
    res.data.dimensions.map(item => {
      item['fullName'] = item.fieldName;
      item['id'] = item.field;
    });
    dimensionOptionsList.value = res.data.dimensions;
    state.ChartsDataparams.metricId = route.query.recordId as string;

    getChartsDataList();
  } catch (_) { }
}
async function getChartsDataList() {
  state.ChartsDataparams.metricId = route.query.recordId as string;

  // Ensure we use the correct ID property (id was mapped to field)
  const currentDimId = state.ChartsDataparams.dimensions
    ? state.ChartsDataparams.dimensions.id
    : dimensionOptionsList.value[0]?.id;

  setFieldsValue({
    dimensions: currentDimId,
  });

  if (!state.ChartsDataparams.dimensions) {
    //初始化进来
    state.ChartsDataparams.dimensions = dimensionOptionsList.value[0];
  }
  try {
    console.log('Fetching charts data with params:', JSON.stringify(state.ChartsDataparams));
    const res = await getChartsData({
      ...state.ChartsDataparams,
    });
    console.log('Charts data response:', res);
    state.elements = res.data.data.data;
    state.elementsLength = res.data.data.data.length ? '1' : '0';
    if (state.chartsFlag == 'pie') {
      let valueData: any[] = [];
      state.elements.map(item => {
        let valueDataObj: any = {};
        valueDataObj['value'] = item[1];
        valueDataObj['name'] = item[0];
        valueData.push(valueDataObj);
      });

      state.ChartsObj.valueData = valueData;
      // state.ChartsObj.seriesData[0].type='pie'
    } else if (state.chartsFlag == 'gudge') {
      //图表区域
      let valueData: any[] = [];
      state.elements.map(item => {
        let valueDataObj: any = {};
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
  } catch (e) {
    console.error('Error fetching charts data:', e);
  }
}
</script>
<style scoped>
/* Add any custom transitions here */
.fade-enter-active,
.fade-leave-active {
  transition: opacity 0.3s ease;
}

.fade-enter-from,
.fade-leave-to {
  opacity: 0;
}
</style>
