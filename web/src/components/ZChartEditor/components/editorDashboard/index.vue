<template>
  <div
    ref="areaRef"
    class="editor-chart-wrapper"
    :style="{ width: viewport.width + 'px', height: viewport.height + 'px', background: layout.background }"
    @dragover.prevent
    @drop="drop($event, '', 'outer')"
    @click="activateEv('')">
    <drag-resize
      class="drag-wrapper"
      v-for="(item, index) in chartItems"
      :key="item.key"
      :w="item.style.width"
      :h="item.style.height"
      :x="item.style.x"
      :y="item.style.y"
      :isActive="currentChartId == item.key"
      :gridX="snapToGridSize"
      :gridY="snapToGridSize"
      :snapToGrid="snapToGrid"
      :parentScaleX="scale.x"
      :parentScaleY="scale.x"
      :style="layoutsStyle"
      :isDraggable="editType == 'edit'"
      :isResizable="editType == 'edit'"
      @click.stop="activateEv(item.key, index)"
      @deactivated="deactivateEv(item.key)"
      @dragstop="changeDimensions($event, item)"
      @resizestop="changeDimensions($event, item)"
      @resizing="resizing"
      @dragover.stop.prevent
      @drop.stop="drop($event, item.key, 'inChart')">
      <!-- 工具栏 -->
      <tool-bar v-if="item.setting.titleShow" :title="item.setting.title"></tool-bar>
      <!-- x轴标题 -->
      <div class="x-axis" v-if="item.class == 'chart' && item.setting.yAxis?.titleShow">
        {{ item.setting.yAxis?.title }}
      </div>
      <!-- y轴标题 -->
      <div class="y-axis" v-if="item.class == 'chart' && item.setting.xAxis?.titleShow">
        {{ item.setting.xAxis?.title }}
      </div>
      <!-- 删除按钮 -->
      <span v-if="editType == 'edit'" class="del-btn" @click="delNode(item.key)">
        <CloseOutlined />
      </span>
      <!-- 过滤器 -->
      <select-filter
        v-if="state.selectTypes.includes(item.type)"
        :options="optionMap[item.key]"
        :currentLayout="index"
        :item="item"
        :width="item.style.width + 'px'"
        :height="getHeight(item)" />
      <!-- 图表 -->
      <Chart
        v-if="state.chartTypes.includes(item.type)"
        :options="optionMap[item.key]"
        :width="item.style.width + 'px'"
        :height="getHeight(item)" />
    </drag-resize>
  </div>
</template>
<script lang="ts" setup>
  import { computed, reactive, onMounted, onBeforeUnmount, ref } from 'vue';
  import { props as _props } from './props';
  import Chart from './Chart.vue';
  import DragResize from './DragResize.vue';
  import SelectFilter from './SelectFilter.vue';
  import ToolBar from './ToolBar.vue';
  import { useChartStore } from '/@/store/modules/chart';
  import { useEditor } from '../../hooks/useEditor';
  import { CloseOutlined } from '@ant-design/icons-vue';

  defineOptions({
    name: 'ZEditorDashboard',
  });

  const { addNode, delNode } = useEditor();
  defineExpose({ addNode });

  const emit = defineEmits(['dragendNode']);
  const chartStore = useChartStore();
  const areaRef = ref<HTMLElement>();
  const state = reactive({
    isOnChart: false,
    currentChartId: '',
    unitPx: 1,
    chartTypes: ['line', 'bar', 'markArea'],
    dateTypes: ['date', 'datetime', 'timestamp'],
    selectTypes: ['select', 'radio', 'checkbox', 'cascader'],
  });

  const chartItems = computed(() => {
    return chartStore.getLayout.canvas.layouts[0].layout || [{}];
  });
  const optionMap = computed(() => {
    return chartStore.getOptionMap || {};
  });
  const currentChartId = computed(() => {
    return chartStore.getCurrentChartId;
  });

  const viewport = computed(() => {
    return chartStore.getLayout.canvas.viewport;
  });
  const layoutsStyle = computed(() => {
    return chartStore.getLayout.canvas.layoutsStyle;
  });
  const layout = computed(() => {
    return chartStore.getLayout.canvas.layouts[0].style;
  });
  const scale = computed(() => {
    return chartStore.scale;
  });
  const snapToGrid = computed(() => {
    return chartStore.snapToGrid;
  });
  const snapToGridSize = computed(() => {
    return chartStore.snapToGridSize;
  });
  const editType = computed(() => {
    return chartStore.editType;
  });

  const toolbarHeight = 30;
  const getHeight = item => {
    return item.style.height - (item.setting.titleShow ? toolbarHeight : 0) + 'px';
  };

  const drop = (e, key, t) => {
    const drayDataString = e.dataTransfer.getData('chartData');
    const drayData = JSON.parse(drayDataString);
    state.isOnChart = drayData.class == 'chart' && t == 'inChart';
    const _key = state.isOnChart ? key : '';
    addNode({ e, item: drayData }, state.isOnChart, _key);
  };

  const changeDimensions = (newRect, item) => {
    const { style } = item;
    style.y = newRect.top;
    style.x = newRect.left;
    style.width = newRect.width;
    style.height = newRect.height;
  };

  const activateEv = (key?, index?) => {
    chartStore.setCurrentChartId(key, index);
  };

  const deactivateEv = index => {
    console.log(index);
  };

  const resizing = () => {
    console.log('resizing');
  };

  const init = () => {
    state.unitPx = ((areaRef.value?.clientWidth as number) / 100 / 17) * 1;
  };

  // TODO:暂时不用
  const handleKeyDown = e => {
    if (currentChartId.value) {
      // delete
      if (e.key == 'Backspace' && e.ctrlKey) {
        e.preventDefault();
        // delNode();
      }
    }
  };
  // TODO:暂时不用
  const handleResize = () => {
    state.unitPx = ((areaRef.value?.clientWidth as number) / 100 / 17) * 1;
  };

  onMounted(() => {
    init();
    document.addEventListener('keydown', handleKeyDown);
    window.addEventListener('resize', handleResize);
  });
  onBeforeUnmount(() => {
    document.removeEventListener('keydown', handleKeyDown);
    window.removeEventListener('resize', handleResize);
  });
</script>

<style lang="less" scoped>
  ::v-global(.scrollbar__view) {
    padding: 0 !important;
  }

  .editor-chart-wrapper {
    width: 100%;
    height: 100%;
    padding: 10px 8px;
    overflow: auto;
    position: relative;
    background: @chart-editor-wrapper-color;

    .drag-wrapper {
      // overflow: hidden;
      .x-axis {
        position: absolute;
        top: 50%;
        transform: translateY(-50%);
        left: 0;
        writing-mode: vertical-lr;
      }

      .y-axis {
        position: absolute;
        left: 50%;
        transform: translateX(-50%);
        bottom: 1%;
      }

      .del-btn {
        position: absolute;
        top: 8px;
        right: 8px;
        display: none;
        z-index: 9;
      }

      &:hover .del-btn {
        display: block;
        cursor: pointer;
      }
    }
  }
</style>
