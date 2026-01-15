<template>
  <div class="chartbox">
    <!-- 表格---- -->
    <div v-if="chartsFlag == 'gudge'">
      <BasicTable @register="registerTable">
        <template #bodyCell="{ column, record }">
          <template v-if="column.key === 'fullName'"
            ><i :class="'mr-6px ' + record.icon"></i>{{ record.fullName }}</template
          >
        </template>
      </BasicTable>
    </div>
    <div
      v-else
      v-for="(item, index) in state.basicParamsList"
      :key="item.id"
      @click="jumpSecond"
      :class="state.flag[index] == 50 ? 'md:w-1/2 w-ful' : 'flex100'">
      <div class="md:flex enter-y middleBox">
        <charts-manage-ment :basicProps="item" :random="random" />
      </div>
    </div>
  </div>
</template>
<script lang="ts">
  type ChartsDataType = {
    seriesData: Array<any>;
    valueData: Array<any>;
    xAxisData: Array<any>;
  };
</script>
<script lang="ts" setup>
  import { reactive, ref, toRefs, watch, computed, nextTick, onMounted } from 'vue';
  import { useRouter } from 'vue-router';
  import ChartsManageMent from '/@/views/basic/home/components/optimalManagement/index2.vue';
  import { basicProps } from '/@/views/basic/home/components/optimalManagement/optimalData/props';
  import { mapProps } from '/@/views/basic/home/components/optimalManagement/optimalData/mapProps';
  // import { basicProps3 } from '/@/views/basic/home/components/optimalManagement/optimalData/props3';
  import { basicProps4 } from '/@/views/basic/home/components/optimalManagement/optimalData/props4';
  import { histogramProps } from '/@/views/basic/home/components/optimalManagement/optimalData/histogramProps';

  import { BasicTable, useTable, BasicColumn } from '/@/components/Table';
  import { getIndexChartsDataList } from '/@/api/basic/charts';

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
  const random = ref();
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
    { title: 'X轴', dataIndex: 'value' },
    { title: 'Y轴', dataIndex: 'name' },
  ];
  onMounted(() => {
    console.log('onMounted---------', props);
  });
  watch(
    () => props,
    (newValue, oldValue) => {
      random.value = crypto.randomUUID();
      initData();
    },
    { deep: true },
  );
  watch(
    () => props.gtData,
    (newValue, oldValue) => {
      console.log('7777777');
    },
    { deep: true },
  );
  watch(
    () => props.ltData,
    (newValue, oldValue) => {
      console.log('3333', props.ltData);
    },
    { deep: true },
  );

  function initData() {
    const itemOptions = props.chartsData;
    //都占取100%的位置
    state.flag.push(100);
    var basicPropsClone: any = {};
    switch (props.chartsFlag) {
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

        console.log('tttttt', props.gtData);
        console.log('ffffff', props.ltData);
        // basicPropsClone.OptionsData.default.series[0].data = [
        //   ['2019-10-10', 200],
        //   ['2019-10-11', 560],
        //   ['2019-10-12', 750],
        //   ['2019-10-13', 580],
        //   ['2019-10-14', 250],
        //   ['2019-10-15', 300],
        // ];
        break;

      default: {
      }
    }

    state.basicParamsObj = basicPropsClone;
    state.basicParamsList = [];
    state.basicParamsList.push(state.basicParamsObj);
    console.log('---------', state.basicParamsObj);
  }
  const [registerTable, { reload }] = useTable({
    api: () => {
      return Promise.resolve({
        code: 200,
        msg: '操作成功',
        data: {
          list: props.chartsData.valueData,
        },
        extras: null,
        timestamp: new Date().valueOf(),
      });
    },
    columns,
    isTreeTable: true,
    useSearchForm: false,
    pagination: false,
    // formConfig: getFormConfig(),
    afterFetch: data => {
      // console.log('444444', data);
    },
  });

  function getFormConfig(): Partial<FormProps> {
    return {
      schemas: false,
    };
  }
</script>
<style>
  .chartbox {
    width: 100%;
    display: flex;
    flex-wrap: wrap;
  }
  .flex100 {
    width: 100%;
  }
</style>
