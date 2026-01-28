<template>
  <BasicPopup
    class="popup-wrapper"
    v-bind="$attrs"
    @register="registerPopup"
    :title="getTitle"
    :loading="loading"
    @ok="handleSubmit"
    @close="handleClose">
    <z-editor v-if="!isEmpty(state.source)" :source="state.source" :status-options="state.statusOptions"></z-editor>
  </BasicPopup>
</template>
<script lang="ts" setup>
  import { reactive, ref } from 'vue';
  import { BasicPopup, usePopupInner } from '/@/components/Popup';
  import { ZEditor } from '/@/components/ZEditor';
  import {
    addIndicatorValueChain,
    getIndicatorValueChainList,
    getMetricCovStatusOptions,
    postMetricData,
    getMetricData,
  } from '/@/api/createModel/model';
  import { GotTypeEnum } from '/@/enums/publicEnum';
  import { find, isEmpty } from 'lodash-es';
  import { ResultEnum } from '/@/enums/httpEnum';

  defineEmits(['register', 'reload']);

  const loading = ref(true);

  const state = reactive<any>({
    source: {
      // id: 'root',
      // parentId: '-1'
      // name: '净利润达到去年的1.5倍',
      // metric: 'met5cq5eq8ig',
      // valueName: '利润率',
      // value: '$3.25M',
      // trendData: [
      //   { item: 'Sports', value: 1 },
      //   { item: 'Strategy', value: 15 },
      // ],
      // pieData: [
      //   { item: '已完成', value: 8 },
      //   { item: '未完成', value: 2 },
      // ],
      // children: null
    },
    statusOptions: [],
  });

  const [registerPopup, { closePopup, changeOkLoading }] = usePopupInner(init);
  const getTitle = ref('价值链');

  function init(data) {
    loading.value = true;
    if (data.name) getTitle.value = data.name;

    getMetricCovStatusOptionsList();
    getEditorData(data);
  }

  /**
   * @description 获取指标状态
   * */
  const getMetricCovStatusOptionsList = async () => {
    try {
      const res = (await getMetricCovStatusOptions()) as unknown as { code: number; data: any[] };
      if (res.code === ResultEnum.SUCCESS) {
        state.statusOptions = res.data;
      }
    } catch (err) {
    }
  };

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
            const list = (r1.data.data.data || []).slice(0, limit);

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

  function getEditorData(data) {
    // 获取建模数据
    getIndicatorValueChainList(data.id).then(async res => {
      if (res.data && res.data.length > 0) {
        state.source = res.data[0];
        loopMetric(state.source);
        loading.value = false;
      } else {
        const params = {
          name: '初始节点',
          gotType: GotTypeEnum.cov,
          gotId: String(data.id),
          metricId: '',
          currentValue: '',
          parentId: -1,
          status: '',
          is_root: true,
        };
        addIndicatorValueChain(params)
          .then(res => {
            if (res.code === ResultEnum.SUCCESS) {
              state.source = res.data;
            }
          })
          .catch(() => {
            state.source = {
              id: 'root',
              name: '初始节点',
              parentId: '-1',
              gotId: data.id,
              trendData: [],
              is_root: true,
            };
          })
          .finally(() => {
            loading.value = false;
          });
      }
    });
  }

  /**
   * @description 清除缓存
   */
  const handleClose = () => {
    state.source = {};
  };

  /**
   * @description 提交表单
   */
  async function handleSubmit() {
    changeOkLoading(true);
    setTimeout(() => {
      changeOkLoading(false);
      state.source = {};
      closePopup();
    }, 2000);
  }
</script>

<style lang="less" scoped>
  .popup-wrapper {
    height: 100%;
    position: relative;

    :deep(.scrollbar__view) {
      height: 100%;

      .popup-body-warapper {
        height: 100%;
      }
    }
  }

  .editor-wrapper {
    background: grey;
    height: 100%;
  }
</style>
