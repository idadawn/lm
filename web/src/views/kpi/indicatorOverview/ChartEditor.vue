<template>
  <div class="page-content-wrapper">
    <div class="page-content-wrapper-center bg-white">
      <z-chart-editor :source="state.source" :drag-item="state.nodeList" :params="{ id: '1', type: 'edit' }" />
    </div>
  </div>
</template>
<script lang="ts" setup>
  import { reactive } from 'vue';
  import { ZChartEditor } from '/@/components/ZChartEditor';
  import { useRoute } from 'vue-router';
  import { getAllIndicatorList } from '/@/api/createModel/model';
  defineOptions({
    name: 'config',
  });
  const route = useRoute();
  console.log(route.params.id, 'route.params.id');
  const state = reactive<any>({
    kpiModel: {
      name: 'kpi',
    },
    nodeList: [
      {
        id: 'node2',
        name: '指标',
        children: [
          {
            id: 'node2-1',
            name: '净利润',
            level: '2',
            type: 'bar',
            class: 'chart',
          },
          {
            id: 'node2-2',
            name: '总销售额',
            level: '2',
            type: 'line',
            class: 'chart',
          },
        ],
      },
      {
        id: 'node1',
        name: '过滤条件',
        children: [
          {
            id: 'node1-1',
            name: '销售日期',
            level: '2',
            type: 'date',
            class: 'filter',
          },
          {
            id: 'node1-2',
            name: '销售区域',
            level: '2',
            type: 'select',
            class: 'filter',
          },
        ],
      },
    ],
    items: [],
  });
  const getNodelist = async () => {
    const res = await getAllIndicatorList();
    const arr = res.data.map((item: any) => {
      return {
        id: item.id,
        name: item.name,
        level: '2',
        type: 'line',
        class: 'chart',
      };
    });
    state.nodeList[0].children = arr;
  };
  const init = () => {
    getNodelist();
  };
  init();
</script>
<style lang="less" scoped></style>
