<template>
  <div class="z-editor-page">
    <div class="editor-left" v-if="state.isEdit">
      <z-editor-tree :source="state.nodeList" @dragendNode="addNode" />
    </div>
    <div class="sketchpad" ref="sketchpadRef">
      <z-editor-dashboard
        :style="{ scale: layoutsStyle.x, transformOrigin: '0 0 0' }"
        class="sketchpad-cavans"
        ref="dashboardRef" />
    </div>
    <div class="editor-right" v-if="state.isEdit">
      <z-editor-elements></z-editor-elements>
    </div>
  </div>
</template>
<script lang="ts" setup>
  import { reactive, ref, watch, computed, onMounted } from 'vue';
  import { props as _props } from './props';
  import ZEditorDashboard from '../components/editorDashboard/index.vue';
  import ZEditorTree from '../components/editorTree/index.vue';
  import ZEditorElements from '../components/editorElements/index.vue';
  import { useChartStore } from '/@/store/modules/chart';

  defineOptions({
    name: 'ZEditorDashboard',
  });
  const props = defineProps(_props);
  const chartStore = useChartStore();
  const dashboardRef: any = ref(null); // 画布元素
  const sketchpadRef: any = ref(null);
  const state = reactive<any>({
    nodeList: computed(() => {
      return props.dragItem;
    }),
    isEdit: computed(() => {
      chartStore.setLayoutEditType(props.params.type);
      return props.params.type === 'edit';
    }),
  });
  const addNode = item => {
    console.log(item, 'dragendNode');
    dashboardRef.value.addNode(item);
  };
  const layoutsStyle = ref({
    x: 1,
    y: 1,
  });

  watch(
    () => props.source,
    () => {
      state.source = props.source;
    },
  );
  watch(
    () => chartStore.scale,
    () => {
      layoutsStyle.value.x = chartStore.scale.x;
    },
    { deep: true },
  );
  onMounted(() => {
    chartStore.setLayout({ type: 'init', id: props.params.id });
    setTimeout(() => {
      const ketchpad = sketchpadRef.value;
      const width = ketchpad?.offsetWidth;
      const height = ketchpad?.offsetHeight;
      const _scale = {
        x: parseFloat(width / 1920).toFixed(2),
        y: parseFloat(height / 1080).toFixed(2),
      };
      // chartStore.setScale(_scale);
    });
  });
</script>
<style lang="less" scoped>
  .z-editor-page {
    height: 100%;
    width: 100%;
    display: flex;
    justify-content: space-between;
    background: @chart-editor-wrapper-color;
    overflow: hidden;

    .editor-left {
      width: 200px;
      height: 100%;
      background: @chart-editor-left-color;
      position: relative;
      border-right: 1px solid @border-color-shallow-dark;
    }

    .sketchpad {
      height: 100%;
      flex: 1;
      background-image: linear-gradient(#eceef5 14px, transparent 0), linear-gradient(90deg, transparent 14px, #000 0);
      background-size: 15px 15px, 15px 15px;
      overflow: auto;

      .sketchpad-cavans {
        // border-radius: 8px;
      }
    }

    .editor-right {
      width: 300px;
      height: 100%;
      padding: 8px 10px;
      background: @chart-editor-right-color;
    }
  }
</style>
