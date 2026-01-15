<template>
  <div class="chartbox">
    <div class="page-content-wrapper">
      <div class="page-content-wrapper-center">
        <div class="page-content-wrapper-content">
          <a-row>
            <a-col :xs="{ span: 15, offset: 0 }" :lg="{ span: 15, offset: 0 }">
              <div class="itemBox">
                <chartsModel
                  :chartsFlag="state.chartsFlag"
                  :chartsData="state.elements"
                  :gtData="indexParams.gtData"
                  :ltData="indexParams.ltData"
                  :random="random" />
              </div>
            </a-col>
            <a-col :xs="{ span: 8, offset: 1 }" :lg="{ span: 8, offset: 1 }">
              <div class="itemBox2">
                <a-form-item label="时间范围" name="leaveEndTime">
                  <jnpf-date-range
                    v-model:value="value1"
                    format="YYYY-MM-DD"
                    @change="timeChange"
                    allowClear />
                </a-form-item>
                <a-form-item label="分析维度" name="type">
                  <a-select
                    v-model:value="value"
                    mode="multiple"
                    style="width: 100%"
                    placeholder="Select Item..."
                    :max-tag-count="maxTagCount"
                    :options="options">
                    <template #maxTagPlaceholder="omittedValues">
                      <span style="color: red">+ {{ omittedValues.length }} ...</span>
                    </template>
                  </a-select>
                </a-form-item>
                <a-button type="primary" size="large" class="buttonStyle" @click="getIndexChartsDataListData"
                  >分析</a-button
                >
              </div>
            </a-col>
          </a-row>
        </div>
      </div>
    </div>
  </div>
</template>
<script lang="ts" setup>
  import VisitAnalysisBar from '/@/views/basic/home/components/VisitAxis.vue';
  import { reactive, ref, toRefs, watch, computed, nextTick, onMounted } from 'vue';
  import { useRouter } from 'vue-router';
  import type { SelectProps } from 'ant-design-vue';
  import chartsModel from './chartsModel.vue';
  import { getAnalysisChartsData } from '/@/api/basic/charts';

  const options = ref<SelectProps['options']>([]);

  for (let i = 10; i < 36; i++) {
    const value = i.toString(36) + i;
    options.value.push({
      label: `Long Label: ${value}`,
      value,
    });
  }
  const random = ref();
  const state = reactive<State>({
    chartsFlag: 'mapLine',
    timeStart: '',
    timeEnd: '',
    ChartsObj: {
      // legendData: ['直接', '最优值'],
      xAxisData: [],
      visualMap: {
        type: 'piecewise',
        show: false,
        dimension: 0,
        seriesIndex: 0,
        pieces: [
          {
            gt: 1,
            lt: 100,
            color: 'red',
          },
        ],
      },
      seriesData: [
        {
          name: '',
          type: 'pie',
          stack: 'Total',
          data: [],
        },
      ],
    },
  });
  const maxTagCount = ref(6);
  const maxTagTextLength = ref(10);
  import { Dayjs } from 'dayjs';

  type RangeValue = [Dayjs, Dayjs];
  const value = ref(['a10', 'c12', 'h17', 'j19', 'k20']);
  const value1 = ref([]);
  const indexParams = reactive<State>({
    timeStart: '',
    timeEnd: '',
    ltData: '',
    gtData: '',
  });
  // const value = ref(['a10', 'c12', 'h17', 'j19', 'k20']);

  onMounted(() => {
    getAnalysisDataList();
  });
  async function getAnalysisDataList() {
    try {
      const res = await getAnalysisChartsData({ nodeId: '1', userId: '1' });

      state.elements = []; //置空
      state.ChartsObj.seriesData[0].name = res.data.metric_names[0];
      state.elements = res.data?.data;
    } catch (_) {
      //
    }
  }
  function timeChange(e) {
    indexParams.timeStart = getData(e[0]).slice(0, 10);
    indexParams.timeEnd = getData(e[1]).slice(0, 10);
    //如果用户选了时间，就去做阴影面积索引设定
    if (indexParams.timeStart && indexParams.timeEnd) {
      state.elements.map((v, index) => {
        if (v[0] == indexParams.timeStart) {
          console.log('vvvvv', v[0], index);
          indexParams.gtData = index;
        } else if (v[0] == indexParams.timeEnd) {
          indexParams.ltData = index;
          console.log('vvvvv', v[0], index);
        }
      });
    }
    // state.elements['gtData'] = indexParams.gtData;
    // state.elements['ltData'] = indexParams.ltData;
    console.log('-----', indexParams.gtData, indexParams.ltData);
    random.value = crypto.randomUUID();
  }
  function getData(n) {
    let now = new Date(n),
      y = now.getFullYear(),
      m = now.getMonth() + 1,
      d = now.getDate();
    return y + '-' + (m < 10 ? '0' + m : m) + '-' + (d < 10 ? '0' + d : d) + ' ' + now.toTimeString().substr(0, 8);
  }
  //'2022-1-18 10:09:06'
</script>
<style>
  .page-content-wrapper-content {
    background: #fff;
  }

  .chartbox {
    width: 100%;
    display: flex;
    flex-wrap: wrap;
  }

  .flex100 {
    width: 100%;
  }

  .itemBox {
  }

  .itemBox2 {
    height: 280px;
  }

  .ant-select-selector {
    height: 128px;
  }

  .buttonStyle {
    width: 100%;
    margin: 0 auto;
    margin-bottom: 10px;
  }
</style>
