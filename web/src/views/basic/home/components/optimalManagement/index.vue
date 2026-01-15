<template>
  <div class="md:flex enter-y middleBox" v-for="item in state.basicParamsList" :key="item.id" @click="jumpSecond">
    <chartsManageMent :basicProps="item" />
  </div>
</template>
<script lang="ts" setup>
  import { reactive, ref, toRefs, watch, computed, nextTick, onMounted } from 'vue';
  import { useRouter } from 'vue-router';
  import chartsManageMent from './index2.vue';
  import { basicProps } from './optimalData/props';
  import { getChartsDataList } from '/@/api/basic/charts';

  const state = reactive<any>({
    form: {},
    basicParams: {},
    elements: [],
    basicParamsList: [],
    basicParamsObj: {},
    zhuelements: [],
    currTab: 1,
    tabs: [
      { key: 1, name: '建模' },
      { key: 2, name: '预警' },
      { key: 3, name: '基线' },
    ],
  });
  onMounted(() => {
    getChartsDataListData();
  });
  async function getChartsDataListData() {
    try {
      const res = await getChartsDataList({ nodeId: '1', userId: '1' });
      state.elements = res;
      state.elements.data.list.map((v, index) => {
        //把一个接口里的数据依次加入props，传给组件，生成charts
        const basicPropsClone = JSON.parse(JSON.stringify(basicProps));
        basicPropsClone.OptionsData.default.title.text = v.TitletextName;
        basicPropsClone.OptionsData.default.legend.data = v.legendData;
        basicPropsClone.OptionsData.default.xAxis.data = v.xAxisData;
        basicPropsClone.OptionsData.default.series = v.seriesData;
        state.basicParamsObj = basicPropsClone;
        state.basicParamsList.push(state.basicParamsObj);
      });
    } catch (_) {}
  }
</script>
<style></style>
