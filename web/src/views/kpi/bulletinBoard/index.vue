<template>
  <div class="page-content-wrapper">
    <div class="page-content-wrapper-center">
      <div class="page-content-wrapper-content">
        <div class="tagBox">
          <a-tag
            class="tag"
            :class="activeName == item.name ? 'activeTag' : ''"
            color="#2db7f5"
            @click="getTagInfo(item)"
            :key="index"
            v-for="(item, index) in tagOptions"
            >{{ item.name }}</a-tag
          >
        </div>
        <a-tabs class="tabContainer" v-model:activeKey="activeKey" @change="tabChanges">
          <a-tab-pane :tab="`价值链${index + 1}`" v-for="(item, index) in sourceList" :key="index">
            <!-- 价值链组件 -->
            <a-spin :spinning="loading">
              <div class="mindMapBox">
                <mindMap v-if="!isEmpty(item)" :source="item"> </mindMap>
              </div>
            </a-spin>
          </a-tab-pane>
        </a-tabs>
      </div>
    </div>
  </div>
</template>
<script lang="ts" setup>
  //获取标签接口
  import { getTagSelectorList, getTagMsg, postMetricData, getMetricData } from '/@/api/createModel/model';
  import { useMessage } from '/@/hooks/web/useMessage';
  import { MindMap } from '/@/components/MindMap';

  import { onMounted, ref, reactive } from 'vue';
  import { GotTypeEnum } from '/@/enums/publicEnum';
  import { find, isEmpty } from 'lodash-es';
  import { ResultEnum } from '/@/enums/httpEnum';
  import { Spin as ASpin } from 'ant-design-vue';

  defineOptions({ name: 'kpi-bulletinBoard' });
  const { createMessage } = useMessage();
  const tagOptions = ref([]);
  const loading = ref();
  const activeName = ref();
  const sourceList = ref([]); //接口获取的所有的信息
  const state = reactive<any>({
    source: {},
    statusOptions: [],
  });
  const activeKey = ref(0);

  /**
   * 初始化
   */
  function init() {}
  /**
   * 挂载完成
   */
  onMounted(() => {
    init();
  });
  getTagSelectorList();
  getTagSelectorList().then(res => {
    tagOptions.value = res.data;
    activeName.value = res.data[0].name;
    getTagMsgList(res.data[0].id);
  });
  function getTagInfo(item) {
    activeName.value = item.name;
    // 
    getTagMsgList(item.id);
  }
  function tabChanges(key) {
    loading.value = true;
    state.source = sourceList.value[key];
    loading.value = false;
    loopMetric(state.source);
  }
  //获取KPI看板信息
  function getTagMsgList(id) {
    // 获取建模数据
    loading.value = true;
    getTagMsg(id).then(async res => {
      if (res.data && res.data.length > 0) {
        sourceList.value = res.data;
        state.source = res.data[0];
        loading.value = false;
        loopMetric(state.source);
      } else {
        state.source = {};
        loading.value = false;
        sourceList.value = []; //二级价值链tab清空
        createMessage.warn('当前标签下没有数据');
      }
    });
  }
  /**
   * @description 获取指标，拼接到数据源
   * */
  function loopMetric(arr) {
    const metricIdList = {};
    const loop = async data => {
      if (Object.prototype.toString.call(data) === '[object Array]') {
        data.forEach(item => loop(item));
      }

      if (Object.prototype.toString.call(data) === '[object Object]') {
        // 如果当前相同的指标ID已经请求，就不重新请求
        if (data.metricId) {
          if (metricIdList[data.metricId]) {
            const [trendData, metricName, metricGrade, currentValue] = metricIdList[data.metricId];
            data.trendData = trendData;
            data.metricName = metricName;
            data.metricGrade = metricGrade;
            data.currentValue = currentValue;
          } else {
            const res = (await getTrendData(data)) as any;
            const { trendData, metricName, metricGrade, currentValue } = res;
            data.trendData = trendData;
            data.metricName = metricName;
            data.metricGrade = metricGrade;
            data.currentValue = currentValue;
            metricIdList[data.metricId] = [trendData, metricName, metricGrade, currentValue];
          }
        }

        if (data.status) {
          data.statusColor = find(state.statusOptions, { id: data.status })?.color;
        } else {
          data.statusColor = '';
        }

        if (data.children) {
          await loop(data.children);
        }
      }
    };
    loop(arr);
  }
  /**
   * @description 获取指标数据
   */
  function getTrendData(query) {
    const limit = 20;
    const params = {
      metricId: query.metricId,
      limit,
    };

    let trendData: { item: string; value: number | string }[] = [],
      metricName = '',
      metricGrade = [],
      currentValue = '';
    return new Promise((resolve, reject) => {
      const p1 = postMetricData(params),
        p2 = getMetricData(query.metricId);
      Promise.all([p1, p2])
        .then(res => {
          const [r1, r2] = res;
          if (r1.code === ResultEnum.SUCCESS) {
            const list = r1.data.data.data.slice(0, limit);

            metricGrade = r1.data.metric_grade;
            for (let i = 0; i < list.length; i++) {
              const [item, value] = list[i];
              trendData.push({ item, value });
            }
          }

          if (r2.code === ResultEnum.SUCCESS) {
            currentValue = r2.data.data.data;
            metricName = r2.data.metricInfo.name;
          }
          resolve({ trendData, metricName, metricGrade, currentValue });
        })
        .catch(err => {
          reject(err);
        });
    });
  }
</script>
<style lang="less" scoped>
  .tagBox {
    // height: 100px;
    padding: 15px 0 15px 15px;
    box-sizing: border-box;
    cursor: pointer;

    .tag {
      padding: 4px 10px;
      font-size: 20px;
      background: #d1d1d4 !important;
    }
    .activeTag {
      background: #2db7f5 !important;
    }
  }
  .tabContainer {
    padding: 0px 20px;
    box-sizing: border-box;
    // border: 1px solid red;
  }
  .mindMapBox {
    width: calc(100vw - 20px);
    height: calc(100vh - 200px);
    text-align: center;
    // background: rgba(0, 0, 0, 0.05);
    border-radius: 4px;
    margin-bottom: 20px;
    // padding: 0px 0px;
    // margin: 20px 0;
  }

  .page-content-wrapper-content {
    background: #fff;
  }
</style>
<style>
  /* //价值链放大缩小框往下移，以免挡到标签 */
  .mindMapBox .g6-component-toolbar {
    /* margin-top: 44px !important; */
  }
</style>
