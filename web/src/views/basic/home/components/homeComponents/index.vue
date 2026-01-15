<template>
  <div class="chartbox">
    <div
      v-for="(item, index) in state.basicParamsList"
      :key="item.id"
      @click="jumpSecond"
      :class="state.flag[index] == 50 ? 'md:w-1/2 w-ful' : 'flex100'">
      <div class="md:flex enter-y middleBox">
        <chartsManageMent :basicProps="item" />
      </div>
    </div>
  </div>
</template>
<script lang="ts" setup>
  import { reactive, ref, toRefs, watch, computed, nextTick, onMounted } from 'vue';
  import { useRouter } from 'vue-router';
  import chartsManageMent from '../optimalManagement/index2.vue';
  import { basicProps } from '../optimalManagement/optimalData/props.ts';
  import { basicProps2 } from '../optimalManagement/optimalData/props2.ts';
  import { basicProps3 } from '../optimalManagement/optimalData/props3.ts';
  import { basicProps4 } from '../optimalManagement/optimalData/props4.ts';
  import { getHomeChartsDataList } from '/@/api/basic/charts';

  const state = reactive<any>({
    form: {},
    basicParams: {},
    elements: [],
    basicParamsList: [],
    basicParamsObj: {},
    flag: [],
  });
  onMounted(() => {
    getHomeChartsDataListData();
  });
  async function getHomeChartsDataListData() {
    try {
      const res = await getHomeChartsDataList({ nodeId: '1', userId: '1' });
      state.elements = res;
      state.flag = [];
      state.elements.data.list.map((v: any, index) => {
        //实现如果是饼状图和折线图就占二分之一的逻辑
        if (v.type == 'pie' || v.type == 'gudge') {
          state.flag.push(50);
        } else {
          state.flag.push(100);
        }
        var basicPropsClone: any = {};
        switch (v.type) {
          case 'axis' /* 折线图 */:
            basicPropsClone = JSON.parse(JSON.stringify(basicProps));
            basicPropsClone.OptionsData.default.title.text = v.TitletextName;
            basicPropsClone.OptionsData.default.legend.data = v.legendData;
            basicPropsClone.OptionsData.default.xAxis.data = v.xAxisData;
            basicPropsClone.OptionsData.default.yAxis.data = v.yAxisData;
            basicPropsClone.OptionsData.default.series = v.seriesData;
            break;
          case 'hengaxis' /* 横过来的柱状图 */:
            basicPropsClone = JSON.parse(JSON.stringify(basicProps2));
            basicPropsClone.OptionsData.default.title.text = v.TitletextName;
            basicPropsClone.OptionsData.default.legend.data = v.legendData;
            basicPropsClone.OptionsData.default.xAxis.data = v.xAxisData;
            basicPropsClone.OptionsData.default.yAxis.data = v.yAxisData;
            basicPropsClone.OptionsData.default.series = v.seriesData;
            break;
          case 'gudge' /* 仪表盘 */:
            basicPropsClone = JSON.parse(JSON.stringify(basicProps3));
            basicPropsClone.OptionsData.default.series[0].data[0].value = v.value;

            break;
          case 'pie' /* 饼图 */:
            basicPropsClone = JSON.parse(JSON.stringify(basicProps4));
            basicPropsClone.OptionsData.default.series[0].data = v.valueData;
            break;
          default: {
          }
        }

        state.basicParamsObj = basicPropsClone;
        state.basicParamsList.push(state.basicParamsObj);
      });
    } catch (_) {
      //
    }
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
