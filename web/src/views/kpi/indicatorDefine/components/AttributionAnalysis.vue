<template>
  <div class="attributionContainer">
    <div class="page-content-wrapper">
      <div class="page-content-wrapper-center">
        <div class="page-content-wrapper-content">
          <a-row v-if="noDataFlag">
            <a-col :xs="{ span: 15, offset: 0 }" :lg="{ span: 15, offset: 0 }">
              <div class="itemBox" v-if="state.elementsLength != 0">
                <chartsModel
                  :chartsFlag="state.chartsFlag"
                  :chartsData="state.elements"
                  :gtData="indexParams.gtData"
                  :ltData="indexParams.ltData"
                  :random="random" />
              </div>

              <!-- //无数据的时候显示无数据 -->
              <div class="chartbox" v-else>
                <div class="portal-layout-nodata">
                  <img src="../../../../assets/images/dashboard-nodata.png" alt="" class="layout-nodata-img" />
                  <p class="layout-nodata-txt">暂无数据</p>
                </div>
              </div>
              <DimensionContributionRanking style="margin-top: 25px" :tableData="state.tableDataList" />
            </a-col>
            <a-col :xs="{ span: 8, offset: 1 }" :lg="{ span: 8, offset: 1 }">
              <div class="itemBox2">
                <a-form-item label="时间范围" name="leaveEndTime">
                  <jnpf-date-range
                    v-model:value="state.ChartsDataparams.value1"
                    format="YYYY-MM-DD"
                    @change="timeChange"
                    allowClear />
                </a-form-item>
                <a-form-item label="分析维度" name="type">
                  <!-- v-model:value="state.ChartsDataparams.dimensions" -->
                  <a-select
                    mode="multiple"
                    v-model:value="state.ChartsDataparams.dimensionsValue"
                    style="width: 100%"
                    placeholder="Select Item..."
                    :max-tag-count="maxTagCount"
                    :options="dimensionOptionsList"
                    @change="changeDimension"
                    :fieldNames="{
                      label: 'fieldName',
                      value: 'field',
                    }">
                    <!-- <a-select
                    v-model:value="state.ChartsDataparams.dimensionsValue"
                    style="width: 100%"
                    placeholder="Select Item..."
                    :max-tag-count="maxTagCount"
                    :options="dimensionOptionsList"
                    @change="changeDimension"
                    :fieldNames="{
                      label: 'fieldName',
                      value: 'field',
                    }"> -->
                  </a-select>
                </a-form-item>
                <a-button type="primary" size="large" class="buttonStyle" @click="goToAnalysis">分析</a-button>
              </div>
              <div class="analysisLeftBox gutter-box">
                <div class="dateTime">{{ state.dataRange.start_data }} 至 {{ state.dataRange.end_data }}</div>
                <div class="numChange"
                  >{{ state.dataRange.start }}<SwapRightOutlined style="color: #a5b2c5" />
                  {{ state.dataRange.end }}</div
                >
                <div class="percentNumBox"
                  ><ArrowDownOutlined
                    v-if="state.dataRange.trend == 'Down'"
                    style="color: #f29d41; font-weight: bold" />
                  <ArrowUpOutlined v-else style="color: #f29d41; font-weight: bold" />
                  {{ state.dataRange.value }} ({{ state.dataRange.percentage }})</div
                >
              </div>
              <div class="analysisRightBox gutter-box">
                <div class="analysisMsg">
                  <div>根据归因分析的结果，可以得出以下结论：</div>
                  {{ state.summaryMsg.summary_content }}
                </div>
                <div class="analysisPro">以上分析由 Copilot 自动生成 <GlobalOutlined /></div>
              </div>
            </a-col>
          </a-row>
          <!-- 整体显示无数据 -->
          <div v-else>
            <div class="nodataBox">
              <div class="portal-layout-nodata">
                <img src="../../../../assets/images/dashboard-nodata.png" alt="" class="layout-nodata-img" />
                <p class="layout-nodata-txt">暂无数据</p>
              </div>
            </div>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>
<script lang="ts" setup>
  // import VisitAnalysisBar from '/@/views/basic/home/components/VisitAxis.vue';
  import { reactive, ref, toRefs, watch, computed, nextTick, onMounted } from 'vue';
  import { useRouter } from 'vue-router';
  import type { SelectProps } from 'ant-design-vue';
  import chartsModel from './chartsModel.vue';
  import { getAnalysisChartsData } from '/@/api/basic/charts';

  import { getChartsData, getAnalysisData, getAnalysisResult, createAnalysis } from '/@/api/targetDirectory';
  import { depSelect } from '/@/components/DepSelect';
  import { SwapRightOutlined, GlobalOutlined, ArrowDownOutlined } from '@ant-design/icons-vue';
  import DimensionContributionRanking from './DimensionContributionRanking.vue';

  const random = ref();
  const props = defineProps<{
    dimensionOptionsList: Array;
    recordId: '';
  }>();
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
    ChartsDataparams: {
      dimensions: '',
      dimensionsValue: [],
      value1: '',
      MetricId: '',
    },
    AnaLysisParams: {
      metric_id: '',
      time_dimensions: '',
      dimensions: '',
      filters: [],
      start_data: '',
      end_data: '',
    },
    tableDataList: { id: '111' },
    summaryMsg: {
      summary_content: '',
    },
    dataRange: {
      task_id: '',
    },
    dimensionTimeFlag: false,
    leftMenuArr: [],
    dimensionArr: [],
    attributionArr: [],
    basePeriodArr: [],
  });
  const targetColumnArrWeidu = ref([]);
  const targetCheckedArrWeidu = ref([]);
  const targetCheckedArrWeiduItem = ref<any[]>([]);
  const maxTagCount = ref(6);
  const maxTagTextLength = ref(10);
  const noDataFlag = ref(true);
  import { Dayjs } from 'dayjs';
  import { Console } from 'console';

  type RangeValue = [Dayjs, Dayjs];
  const indexParams = reactive<State>({
    timeStart: '',
    timeEnd: '',
    ltData: '',
    gtData: '',
  });
  const dimensionOptionsList = ref([]);
  onMounted(() => {
    dimensionOptionsList.value = props.dimensionOptionsList;
    noDataFlag.value = dimensionOptionsList.value.some(v => {
      return v.dataType == 'datetime';
    });
    getChartsDataList();
  });
  async function getChartsDataList() {
    const dimensionArr = [];
    if (!state.ChartsDataparams.dimensionsValue) {
      //默认进来
      // state.ChartsDataparams.dimensions = dimensionOptionsList.value[0];
      state.ChartsDataparams.dimensionsValue = dimensionOptionsList.value[0].field; //默认值带上第一条的值
    } else {
      //纬度多选逻辑处理
      // state.ChartsDataparams.dimensions = dimensionOptionsList.value.filter(v =>
      //   state.ChartsDataparams.dimensionsValue.includes(v.field),
      // );
      // 纬度单选逻辑处理
      // dimensionOptionsList.value.map(item => {
      //   if (state.ChartsDataparams.dimensionsValue == item.field) {
      //     state.ChartsDataparams.dimensions = item;
      //   }
      // });
    }
    state.ChartsDataparams.MetricId = props.recordId;
    try {
      const res = await getChartsData({
        ...state.ChartsDataparams,
      });
      state.elements = []; //置空
      state.ChartsObj.seriesData[0].name = 'mingcheng';
      state.elements = res.data.data.data;
      state.elementsLength = res.data.data.data.length ? '1' : '0';
    } catch (_) {}
  }
  async function goToAnalysis() {
    //请求接口获取taskID
    //得到taskId&&数字区间后去请求summary&&result
    //纬度多选逻辑处理

    if (!state.ChartsDataparams.dimensionsValue) {
      //默认进来
      state.AnaLysisParams.dimensions = dimensionOptionsList.value[0];
      state.ChartsDataparams.dimensionsValue = dimensionOptionsList.value[0].field; //默认值带上第一条的值
    } else {
      //纬度多选逻辑处理
      state.AnaLysisParams.dimensions = dimensionOptionsList.value.filter(v =>
        state.ChartsDataparams.dimensionsValue.includes(v.field),
      );
      // state.AnaLysisParams.dimensions = state.AnaLysisParams.dimensions[0];
    }
    dimensionOptionsList.value.map(v => {
      if (v.dataType == 'datetime') {
        state.AnaLysisParams.time_dimensions = v;
      }
    });
    state.AnaLysisParams.metric_id = props.recordId;
    try {
      const res = await createAnalysis({
        ...state.AnaLysisParams,
      });
      state.dataRange = res.data;
      getAnalysisSummary(state.dataRange.task_id);
      getAnalysisResultList(state.dataRange.task_id);
    } catch (_) {}
  }
  //获取文案分析接口
  async function getAnalysisSummary(taskId) {
    try {
      const res = await getAnalysisData({
        taskId: taskId,
      });
      state.summaryMsg = res.data;
      // state.summaryMsg = {
      //   task_id: 'anakqbikunti',
      //   task_status: 'FINISHED',
      //   summary_content:
      //     '根据归因分析结果，总交易量的数据变化趋势从4下降到了0，说明该指标在过去的时间内出现了下降趋势。通过归因分析，我们发现订单月份是与总交易量相关性最高的指标维度。从潜在引起指标下降的维度值排名列表中，我们可以看到维度值为10的订单月份对应的指标数据变化趋势为4~0，贡献度为100%。因此，可以推断出总交易量下降的原因是在订单月份为10的时候，总交易量出现了明显的下降。\n\n至于潜在引起指标上升的维度值排名列表，由于在问题描述中没有给出，因此无法得出结论。',
      //   msg: null,
      // };
    } catch (_) {}
  }
  //获取图表接口
  async function getAnalysisResultList(taskId) {
    try {
      const res = await getAnalysisResult({
        taskId: taskId,
      });
      state.tableDataList = res.data.analysis_result;
      state.leftMenuArr = [];
      state.dimensionArr = [];

      state.tableDataList.map(v => {
        state.leftMenuArr.push(v.dimension);
        v.attribution_list.map(item => {
          state.dimensionArr.push(item.dimension_value);
          state.attributionArr.push(item.attribution_value);
          state.basePeriodArr.push(item.base_period_value);
        });
      });

      // state.tableDataList = {
      //   task_id: 'anakqbikunti',
      //   start_time: 1704762425803,
      //   status: 'FINISHED',
      //   msg: 'Task finished.',
      //   base_period: '2015-10-18',
      //   base_period_value: 4,
      //   compared_period: '2018-12-30',
      //   compared_period_value: 0,
      //   analysis_result: [
      //     {
      //       dimension: '订单月份',
      //       coefficient: 1,
      //       attribution_list: [
      //         {
      //           dimension_value: '10',
      //           attribution_value: 1,
      //           base_period_value: 4,
      //           compared_period_value: 0,
      //         },
      //       ],
      //     },
      //     {
      //       dimension: '订单年份',
      //       coefficient: 1,
      //       attribution_list: [
      //         {
      //           dimension_value: '2015',
      //           attribution_value: 1,
      //           base_period_value: 4,
      //           compared_period_value: 0,
      //         },
      //       ],
      //     },
      //   ],
      // };
      // 
    } catch (_) {}
  }
  function timeChange(e) {
    indexParams.timeStart = getData(e[0]).slice(0, 10);
    indexParams.timeEnd = getData(e[1]).slice(0, 10);
    state.AnaLysisParams.start_data = indexParams.timeStart;
    state.AnaLysisParams.end_data = indexParams.timeEnd;

    //如果用户选了时间，就去做阴影面积索引设定
    if (indexParams.timeStart && indexParams.timeEnd) {
      // 
      state.elements.map((v, index) => {
        // 
        if (Date.parse(v[0].slice(0, 10)) == Date.parse(indexParams.timeStart)) {
          indexParams.gtData = index;
          // 
        } else if (Date.parse(v[0].slice(0, 10)) == Date.parse(indexParams.timeEnd)) {
          indexParams.ltData = index;
          // 
        }
      });
    }
    // state.elements['gtData'] = indexParams.gtData;
    // state.elements['ltData'] = indexParams.ltData;
    random.value = crypto.randomUUID();
  }
  function getData(n) {
    let now = new Date(n),
      y = now.getFullYear(),
      m = now.getMonth() + 1,
      d = now.getDate();
    // return y + '/' + (m < 10 ? '0' + m : m) + '/' + (d < 10 ? '0' + d : d) + ' ' + now.toTimeString().substr(0, 8);
    return y + '/' + m + '/' + d;
  }
</script>
<style lang="less" scoped>
  :deep(.h-full) {
    background: #fff !important;
    // border: 1px solid red;
  }
  .attributionContainer {
    // width: 100%;
    // height: calc(100vh - 280px);
    // overflow: scroll;
  }
  .page-content-wrapper {
    // border: 1px solid green;
  }
  .page-content-wrapper-center {
    // border: 1px solid red;
  }
  .page-content-wrapper-content {
    background: #fff;
    // border: 1px solid red;
  }

  .chartContainer {
    width: 100%;
    display: flex;
    flex-wrap: wrap;
    // height: calc(100vh - 300px);
    // border: 1px solid red;
  }

  .flex100 {
    width: 100%;
  }

  .itemBox {
  }

  .itemBox2 {
    // height: 280px;
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
<style lang="less" scoped>
  .chartbox {
    width: 100%;
    // height: 420px;
    background: #fff;
  }
  .nodataBox {
    width: 100%;
    height: calc(100vh - 340px);
  }
  .portal-layout-nodata {
    text-align: center;
    position: absolute;
    top: calc(50% - 200px);
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
  .analysisLeftBox {
    height: 226px;
    margin-top: 25px;
  }
  .dateTime {
    font-weight: 400;
    font-size: 12px;
    line-height: 18px;
    color: #a5b2c5;
  }
  .numChange {
    max-width: 200px;
    font-weight: 600;
    font-size: 26px;
    line-height: 38px;
    color: #2f374c;
    overflow: hidden;
    text-overflow: ellipsis;
    white-space: nowrap;
  }
  .percentNumBox {
    background: #fff2e5;
    color: #f29d41;
    height: 34px;
    padding: 0 12px;
    font-style: normal;
    font-weight: 600;
    font-size: 20px;
    line-height: 34px;
    text-align: center;
    border-radius: 4px;
  }
  .analysisRightBox {
    // border: 1px solid red !important;
    height: 226px;
    position: relative;
    margin-top: 35px;
  }
  .gutter-box {
    display: flex;
    flex-direction: column;
    justify-content: center;
    align-items: center;
    grid-gap: 16px;
    gap: 16px;
    align-self: stretch;
    border-radius: 8px;
    border: 1px solid #e6ebf4;
    background: #fff;
    padding: 8px 0;
  }
  .analysisMsg {
    height: 194px;
    overflow: scroll;
    // border: 1px solid red;
    padding: 12px;
    overflow: auto;
    font-size: 13px;
    font-weight: 400;
    line-height: 20px;
    color: #2f374c;
  }
  .analysisPro {
    width: 100%;
    height: 32px;
    line-height: 32px;
    background: #f8f9fb;
    background: rgb(137, 133, 133);
    border-bottom-right-radius: 8px;
    border-bottom-left-radius: 8px;
    padding: 12px 0;
    position: absolute;
    bottom: 0;
    left: 0;
    // z-index: 9999;
    height: 32px;
    padding: 6px 8px;
    background-color: #f8f9fb;
    color: #a5b2c5;
    border-top: 1px solid #e6ebf4;
    display: flex;
    grid-gap: 4px;
    gap: 4px;
    align-items: center;
    justify-content: space-between;
    overflow: hidden;
  }
</style>
