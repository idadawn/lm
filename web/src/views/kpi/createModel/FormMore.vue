<template>
  <BasicPopup
    class="popup-wrapper"
    v-bind="$attrs"
    @register="registerPopup"
    :title="getTitle"
    :loading="loading"
    @ok="handleSubmit"
    @close="handleClose">
    <z-editor
      v-if="!isEmpty(state.source.nodes)"
      :source="state.source"
      :status-options="state.statusOptions"></z-editor>
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
  import { find, isEmpty, isEqual, omit, uniqWith } from 'lodash-es';
  import { ResultEnum } from '/@/enums/httpEnum';
  import { Edge, SourceInterface, Node } from '/@/components/MindMap/hooks/MindMapSourceType';

  defineEmits(['register', 'reload']);

  const loading = ref(true);

  const state = reactive<any>({
    source: {
      // id: 'root',
      // parentId: '-1'
      // name: '净利润达到去年的1.5倍',
      // metricId: 'met5cq5eq8ig',
      // trendData: [
      //   { item: 'Sports', value: 1 },
      //   { item: 'Strategy', value: 15 },
      // ],
      // children: null
      nodes: [],
      edges: [],
      gotId: '',
    } as SourceInterface,
    statusOptions: [],
  });

  const getTitle = ref('价值链');

  function init(data) {
    loading.value = true;
    state.source.gotId = data.id;
    if (data.name) getTitle.value = data.name;

    getMetricCovStatusOptionsList();
    getEditorData(data);
  }

  const [registerPopup, { closePopup, changeOkLoading }] = usePopupInner(init);

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
      console.log(err);
    }
  };

  /**
   * @description 获取指标数据
   */
  async function getTrendData(node, list) {
    const limit = 20;
    const params = {
      metricId: node.metricId,
      limit,
    };
    // 如果存在则直接取list里的值对node赋值，list[metricId]固定四个值
    if (list[node.metricId]) {
      node.metricGrade = list[node.metricId][0];
      node.trendData = list[node.metricId][1];
      node.currentValue = list[node.metricId][2];
      node.metricName = list[node.metricId][3];
    } else {
      const res = await Promise.all([postMetricData(params), getMetricData(params.metricId)]);
      const [r1, r2] = res;
      list[node.metricId] = [] as any[];
      if (r1.code === ResultEnum.SUCCESS) {
        const list = (r1?.data?.data?.data || []).slice(0, limit);
        const trendData: { item: string; value: number | string }[] = [];
        for (let i = 0; i < list.length; i++) {
          const [item, value] = list[i];
          trendData.push({ item, value });
        }
        node.metricGrade = r1.data.metric_grade;
        node.trendData = trendData;
      }

      if (r2.code === ResultEnum.SUCCESS) {
        node.currentValue = r2?.data?.data?.data;
        node.metricName = r2.data.metricInfo.name;
      }
      list[node.metricId] = [node?.metricGrade, node?.trendData, node?.currentValue, node?.metricName];
    }
  }

  /**
   * @description 获取指标，拼接到数据源
   * */
  function loopMetric(arr) {
    const metricIdList = {};
    const edgesInit = [];
    const loop = arr => {
      const run = initial => {
        if (Array.isArray(initial)) {
          for (let i = 0; i < initial.length; i++) {
            run(initial[i]);
          }
        } else {
          const node: Node = omit(initial, ['children']);

          state.source.nodes.push(node);

          if (initial.covTreeIds) {
            edgesInit.push(initial.covTreeIds);
          } else {
            if (initial.covTreeId) {
              edgesInit.push(initial.covTreeId.split(','));
            } else {
              edgesInit.push([]);
            }
          }

          if (initial.children && initial.children.length > 0) {
            run(initial.children);
          }
        }
      };
      run(arr);

      // 格式化edgesInit给edges
      const edges: Edge[] = [];
      for (let i = 0; i < edgesInit.length; i++) {
        const edgesItem = edgesInit[i];
        for (let j = 0; j < edgesItem.length; j++) {
          const start = edgesItem[j];
          const next = j + 1;
          if (next <= edgesItem.length - 1) {
            const end = edgesItem[next];
            edges.push({
              source: start,
              target: end,
            });
          }
        }
      }
      state.source.edges = uniqWith(edges, isEqual);

      // 数据拼接
      const nodesSplicing = async () => {
        let nodes = state.source.nodes;
        for (let i = 0; i < nodes.length; i++) {
          // 数据拼接
          const node = nodes[i];
          if (node.metricId && !metricIdList[node.metricId]) {
            await getTrendData(node, metricIdList);
          }
        }
      };
      nodesSplicing();
    };
    loop(arr);
  }

  function getEditorData(data) {
    // 获取建模数据
    getIndicatorValueChainList(data.id)
      .then(res => {
        if (res.data && res.data.length > 0) {
          loopMetric(res.data);
        } else {
          const params = {
            name: '初始节点',
            gotType: GotTypeEnum.cov,
            gotId: String(data.id),
            parentId: -1,
            is_root: true,
          };
          addIndicatorValueChain(params)
            .then(resp => {
              if (resp.code === ResultEnum.SUCCESS) {
                loopMetric(resp.data);
              }
            })
            .catch(() => {
              state.source = {
                nodes: [
                  {
                    id: 'root',
                    name: '初始节点',
                    parentId: '-1',
                    gotId: data.id,
                    trendData: [],
                    is_root: true,
                  },
                ],
                edges: [],
              };
            })
            .finally(() => {
              loading.value = false;
            });
        }
      })
      .finally(() => {
        loading.value = false;
      });
  }

  /**
   * @description 清除缓存
   */
  const handleClose = () => {
    state.source = {
      nodes: [],
      edges: [],
    };
  };

  /**
   * @description 提交表单
   */
  async function handleSubmit() {
    changeOkLoading(true);
    setTimeout(() => {
      changeOkLoading(false);
      handleClose()
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
