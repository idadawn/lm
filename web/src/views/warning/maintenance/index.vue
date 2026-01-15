<template>
  <div class="page-content-wrapper">
    <div class="page-content-wrapper-center bg-white">
      <z-editor :source="state.source" :drag-item="state.nodeList"></z-editor>
    </div>
  </div>
</template>
<script lang="ts" setup>
  // import { reactive } from 'vue';
  import { ZEditor } from '/@/components/ZEditor';
  import { getNodes } from '/@/api/createModel/model';
  import { reactive, toRefs, onMounted, computed, nextTick } from 'vue';
  import { useRoute } from 'vue-router';

  const state = reactive<any>({
    kpiModel: {
      name: 'kpi',
    },
    source: {
      // 点集
      nodes: [
        {
          id: 'node1',
          x: 100,
          y: 200,
        },
        {
          id: 'node2',
          x: 300,
          y: 200,
        },
      ],
      // 边集
      edges: [
        // 表示一条从 node1 节点连接到 node2 节点的边
        {
          source: 'node1',
          target: 'node2',
          // label: 'default arrow',
          style: {
            endArrow: true,
          },
        },
      ],
    },
    nodeList: [
      {
        id: 'node1',
        name: '实例1',
        children: [
          {
            id: 'node1-1',
            name: '指标1-1',
            level: '2',
          },
          {
            id: 'node1-2',
            name: '指标1-2',
            level: '2',
          },
        ],
      },
      {
        id: 'node2',
        name: '实例2',
        children: [
          {
            id: 'node2-1',
            name: '指标2-1',
            level: '2',
          },
          {
            id: 'node2-2',
            name: '指标2-2',
            level: '2',
          },
        ],
      },
    ],
  });

  const init = async () => {
    const res = await getNodes({ userId: '1' });
    state.source = res.data;
    console.log(res);
  };
  init();
  onMounted(() => {
    const route = useRoute();
    console.log('传递过来的指标编码-------', route.query.config);
    if (route.query.config) {
      state.activeKey = route.query.config == '1' ? '1' : '2';
      nextTick(() => {
        state.activeKey == '1' ? reloadDelegateTable({ page: 1 }) : reloadDelegateTable1({ page: 1 });
      });
    } else {
    }
  });
</script>
<style lang="less" scoped>
  .drag-item {
    width: 100px;
    height: 100px;
    background: grey;
  }
</style>
