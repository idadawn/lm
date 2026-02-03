<template>
  <div class="chartModalbox">
    <!-- 表格---- -->
    <div v-if="chartsFlag == 'gudge'">
      <BasicTable @register="registerTable" />
    </div>
    <div v-else v-for="(item, index) in state.basicParamsList" :key="item.id" @click="jumpSecond"
      :class="state.flag[index] == 50 ? 'md:w-1/2 w-ful' : 'flex100'">
      <div class="md:flex enter-y">
        <charts-manage-ment :basicProps="item" />
      </div>
    </div>
  </div>
</template>
<script lang="ts">
type ChartsDataType = {
  seriesData: Array<any>;
  valueData: Array<any>;
  xAxisData: Array<any>;
  TitletextName?: string;
  legendData?: Array<any>;
};
</script>
<script lang="ts" setup>
import { reactive, watch } from 'vue';

import ChartsManageMent from '/@/views/basic/home/components/optimalManagement/index2.vue';
import { basicProps } from '/@/views/basic/home/components/optimalManagement/optimalData/props';
import { mapProps } from '/@/views/basic/home/components/optimalManagement/optimalData/mapProps';
import { basicProps4 } from '/@/views/basic/home/components/optimalManagement/optimalData/props4';
import { BasicTable, useTable, BasicColumn } from '/@/components/Table';

//接收父组件的传参
// const props = defineProps({
//   chartsFlag: {
//     default: '',
//     type: String,
//   },
//   chartsData: {
//     default: [],
//     type: ChartsDataType,
//   },
// });
// const random = ref();
const props = defineProps<{
  chartsFlag: String;
  chartsData: ChartsDataType;
  gtData: String;
  ltData: String;
}>();
const state = reactive<any>({
  form: {},
  basicParams: {},
  elements: [],
  basicParamsList: [],
  basicParamsObj: {},
  flag: [],
});
const columns: BasicColumn[] = [
  { title: '维度', dataIndex: 'value' },
  { title: '指标值', dataIndex: 'name' },
];

watch(
  () => props,
  () => {
    initData();
  },
  { deep: true },
);
watch(
  () => (props.chartsFlag == 'gudge' ? props.chartsData.valueData : ''),
  () => {
    if (props.chartsFlag == 'gudge') {
      reload();
    }
  },
  { deep: true },
);

watch(
  () => props.gtData,
  () => { },
  { deep: true },
);
watch(
  () => props.ltData,
  () => {
  },
  { deep: true },
);

function initData() {
  const itemOptions = props.chartsData;
  //都占取100%的位置
  state.flag.push(100);
  var basicPropsClone: any = {};
  // 
  const chartsFlag = props.chartsFlag ? props.chartsFlag : 'axis';
  switch (chartsFlag) {
    case 'axis' /* 折线图 */:
    case 'histogram' /* 柱状图 */:
      basicPropsClone = JSON.parse(JSON.stringify(basicProps));
      basicPropsClone.OptionsData.default.title.text = itemOptions.TitletextName;
      basicPropsClone.OptionsData.default.legend.data = itemOptions.legendData;
      basicPropsClone.OptionsData.default.xAxis.data = itemOptions.xAxisData;
      basicPropsClone.OptionsData.default.series = itemOptions.seriesData;
      break;
    case 'pie' /* 饼图 */:
      basicPropsClone = JSON.parse(JSON.stringify(basicProps4));
      basicPropsClone.OptionsData.default.series[0].data = itemOptions.valueData;
      // basicPropsClone.OptionsData.default.series[0].data = [
      //   { value: 10, name: '11' },
      //   { value: 10, name: '12' },
      // ];
      break;
    case 'mapLine' /* 阴影面积 */:
      basicPropsClone = JSON.parse(JSON.stringify(mapProps));
      basicPropsClone.OptionsData.default.series[0].data = itemOptions;
      basicPropsClone.OptionsData.default.visualMap.pieces[0].gt = props.gtData ? props.gtData : 0;
      basicPropsClone.OptionsData.default.visualMap.pieces[0].lt = props.ltData ? props.ltData : 1;

      // basicPropsClone.OptionsData.default.visualMap.pieces[0].gt = 0;
      // basicPropsClone.OptionsData.default.visualMap.pieces[0].lt = 1;
      break;

    default: {
    }
  }

  state.basicParamsObj = basicPropsClone;
  state.basicParamsList = [];
  state.basicParamsList.push(state.basicParamsObj);
}
// function getTableList() {
const [registerTable, { reload }] = useTable({
  api: () =>
    Promise.resolve({
      code: 200,
      msg: '操作成功',
      data: {
        list: props.chartsData.valueData || [],
      },
      extras: null,
      timestamp: new Date().valueOf(),
    }),
  columns,
  isTreeTable: true,
  useSearchForm: false,
  pagination: false,
  // formConfig: getFormConfig(),
  afterFetch: _data => {
  },
});

function jumpSecond() {
  // 跳转逻辑待实现或按需处理
}
// }


</script>
<style lang="less" scoped>
.chartModalbox {
  width: 100%;
  display: flex;
  flex-wrap: wrap;
}

.flex100 {
  width: 100%;
}
</style>
