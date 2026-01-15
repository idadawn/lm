<template>
  <div class="editor-wrapper">
    <z-editor :source="state.source" :drag-item="state.nodeList"></z-editor>
  </div>
</template>
<script lang="ts" setup>
  import { reactive } from 'vue';
  import { ZEditor } from '/@/components/ZEditor';
  import { getNodes } from '/@/api/createModel/model';

  const emit = defineEmits(['register', 'reload']);
  const state = reactive<any>({
    source: {
      id: 'root',
      label: '净利润达到去年的1.5倍',
      metric: 'met5cq5eq8ig',
      valueName: '利润率',
      value: '$3.25M',
      trendData: [
        { item: 'Sports', value: 1 },
        { item: 'Strategy', value: 15 },
        { item: 'Action', value: 2 },
        { item: 'Shooter', value: 18 },
        { item: 'Other', value: 13 },
        { item: 'Other1', value: 23 },
        { item: 'Other2', value: 43 },
        { item: 'Other3', value: 13 },
      ],
      pieData: [
        { item: '已完成', value: 8 },
        { item: '未完成', value: 2 },
      ],
      children: [
        {
          id: 'c1',
          label: '净利润达到去年的1.5倍',
          valueName: '利润率',
          value: '$3.25M',
          metric: 'met5cq5eq8ig',

          trendData: [
            { item: 'Sports', value: 1 },
            { item: 'Strategy', value: 15 },
            { item: 'Action', value: 2 },
            { item: 'Shooter', value: 18 },
            { item: 'Other', value: 13 },
            { item: 'Other1', value: 23 },
            { item: 'Other2', value: 43 },
            { item: 'Other3', value: 13 },
          ],
          pieData: [
            { item: '已完成', value: 8 },
            { item: '未完成', value: 2 },
          ],
          children: [
            {
              id: 'c1-1',
              label: '净利润达到去年的2倍',
              valueName: '利润率',
              value: '$3.25M',
              metric: 'met5cq5eq8ig',

              pieData: [
                { item: '已完成', value: 8 },
                { item: '未完成', value: 2 },
              ],
            },
            {
              id: 'c1-2',
              label: '净利润达到去年的1.5倍',
              valueName: '利润率',
              value: '$3.25M',
              metric: 'met5cq5eq8ig',

              pieData: [
                { item: '已完成', value: 8 },
                { item: '未完成', value: 2 },
              ],
            },
          ],
        },
        {
          id: 'c2',
          label: '净利润达到去年的1.5倍',
          valueName: '利润率',
          value: '$3.25M',
          metric: 'met5cq5eq8ig',

          trendData: [
            { item: 'Sports', value: 1 },
            { item: 'Strategy', value: 15 },
            { item: 'Action', value: 2 },
            { item: 'Shooter', value: 18 },
            { item: 'Other', value: 13 },
            { item: 'Other1', value: 23 },
            { item: 'Other2', value: 43 },
            { item: 'Other3', value: 13 },
          ],
          pieData: [
            { item: '已完成', value: 8 },
            { item: '未完成', value: 2 },
          ],
        },
        {
          id: 'c3',
          label: '净利润达到去年的1.5倍',
          valueName: '利润率',
          value: '$3.25M',
          metric: 'met5cq5eq8ig',

          trendData: [
            { item: 'Sports', value: 1 },
            { item: 'Strategy', value: 15 },
            { item: 'Action', value: 2 },
            { item: 'Shooter', value: 18 },
            { item: 'Other', value: 13 },
            { item: 'Other1', value: 23 },
            { item: 'Other2', value: 43 },
            { item: 'Other3', value: 13 },
          ],
          pieData: [
            { item: '已完成', value: 8 },
            { item: '未完成', value: 2 },
          ],
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

  function init(data) {
    getEditorData(data);
  }
  init({ id: 'root' });
  async function getEditorData(data) {
    // 获取建模数据
    const res = await getNodes({ userId: '', nodeId: data.id });
    if (res.data) {
      console.log(res.data, '建模数据');
      state.source = res.data;
    }
  }
</script>

<style lang="less" scoped>
  .editor-wrapper {
    background: rgb(255 255 255);
    height: 100%;
  }
</style>
