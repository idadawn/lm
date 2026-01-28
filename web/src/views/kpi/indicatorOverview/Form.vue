<template>
  <BasicPopup
    class="popup-wrapper"
    v-bind="$attrs"
    @register="registerPopup"
    :title="getTitle"
    showOkBtn
    @ok="handleSubmit"
    @close="handleCancel">
    <z-chart-editor
      v-if="state.dashboardId"
      :source="state.source"
      :drag-item="state.nodeList"
      :params="{ id: state.dashboardId, type: state.editType }" />
  </BasicPopup>
</template>
<script lang="ts" setup>
  import { computed, reactive } from 'vue';
  import { BasicPopup, usePopupInner } from '/@/components/Popup';
  import { ZChartEditor } from '/@/components/ZChartEditor';
  import { getAllIndicatorList } from '/@/api/createModel/model';
  import { getDimensions } from '/@/api/chart';
  import { useChartStore } from '/@/store/modules/chart';

  const emit = defineEmits(['register', 'reload']);
  const chartStore = useChartStore();
  const [registerPopup, { closePopup, changeLoading, changeOkLoading }] = usePopupInner(init);
  const getTitle = computed(() => (state.editType == 'edit' ? '编辑' : '预览'));

  const state = reactive<any>({
    nodeList: [
      {
        id: 'node1',
        name: '指标',
        children: [
          {
            id: 'node2-1',
            name: '净利润',
            level: '2',
            type: 'line',
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
        id: 'node2',
        name: '公共维度',
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
    dashboardId: '',
    editType: '',
  });
  const getNodelist = async () => {
    const indicatorRes = await getAllIndicatorList();
    const dimensionsRes = await getDimensions();
    state.nodeList[0].children = indicatorRes.data.map(item => {
      return {
        id: item.id,
        name: item.name,
        level: 2,
        type: 'line',
        class: 'chart',
      };
    });
    state.nodeList[1].children = dimensionsRes.data?.map(item => {
      return {
        id: item.id,
        name: item.name,
        level: 2,
        type: 'select',
        class: 'filter',
      };
    });
    chartStore.setMetrics(state.nodeList[0].children);
  };
  function init(data) {
    changeLoading(false);
    getNodelist();
    state.dashboardId = data.id;
    state.editType = data.type;
  }
  async function handleSubmit() {
    changeOkLoading(true);
    if (state.editType == 'edit') {
      await chartStore.saveLayout(state.dashboardId);
    }
    state.dashboardId = '';
    changeOkLoading(false);
    closePopup();
    emit('reload');
  }
  const handleCancel = () => {
    state.dashboardId = '';
  };
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
