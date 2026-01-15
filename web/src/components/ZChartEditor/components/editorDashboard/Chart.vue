<style lang="less">
  .chart-wrapper {
    position: relative;

    .loading {
      position: absolute;
      top: 50%;
      left: 50%;
      transform: translate(-50%, -50%);
    }
  }
</style>
<template>
  <div
    ref="chartWrapperRef"
    class="chart-wrapper"
    :style="{ width, height }"
    :loading="loading"
    @dragleave="dragleave"
    @dragover="dragover"
    @drop="drop">
    <spin v-if="!options.type" class="loading" />
    <div ref="chartRef" :style="{ width, height }"></div>
  </div>
</template>
<script lang="ts" setup>
  import { Ref, ref, watch } from 'vue';
  import { useECharts } from '/@/hooks/web/useECharts';
  import { Spin } from 'ant-design-vue';
  const props = defineProps({
    loading: {
      type: Boolean as PropType<boolean>,
      default: false,
    },
    width: {
      type: String as PropType<string>,
      default: '100%',
    },
    height: {
      type: String as PropType<string>,
      default: '300px',
    },
    options: {
      type: Object as PropType<any>,
      default: {
        // title: {
        //   text: '',
        // },
        // tooltip: {
        //   trigger: 'axis',
        //   axisPointer: {
        //     type: 'cross',
        //     label: {
        //       backgroundColor: '#6a7985',
        //     },
        //   },
        // },
        // toolbox: {
        //   feature: {
        //     saveAsImage: {},
        //   },
        // },
        // legend: {
        //   data: ['净利润', '总销售额', '件单利润'],
        // },
        // grid: {
        //   left: '3%',
        //   right: '4%',
        //   bottom: '3%',
        //   containLabel: true,
        // },
        // xAxis: [
        //   {
        //     type: 'category',
        //     boundaryGap: false,
        //     data: ['1', '2', '3', '4', '5', '6', '7'],
        //   },
        // ],
        // yAxis: [
        //   {
        //     type: 'value',
        //     name: '件',
        //   },
        // ],
        // series: [
        //   {
        //     name: '净利润',
        //     type: 'line',
        //     stack: '总量',
        //     areaStyle: {},
        //     data: [320, 332, 301, 98, 390, 330, 320],
        //   },
        //   {
        //     name: '总销售额',
        //     type: 'line',
        //     stack: '总量',
        //     areaStyle: {},
        //     data: [220, 182, 191, 234, 290, 330, 310],
        //   },
        //   {
        //     name: '件单利润',
        //     type: 'line',
        //     stack: '总量',
        //     areaStyle: {},
        //     data: [120, 132, 101, 134, 90, 230, 210],
        //   },
        // ],
      },
    },
  });
  const chartRef = ref<HTMLDivElement | null>(null);
  const chartWrapperRef = ref<HTMLDivElement | null>(null);
  const { setOptions, resizeFn } = useECharts(chartRef as Ref<HTMLDivElement>);

  const dragleave = () => {
    // 鼠标移出元素上时chartWrapperRef背景色变恢复原色
    chartWrapperRef.value!.style.background = 'transparent';
  };
  const dragover = () => {
    // 鼠标移动到元素上时chartWrapperRef背景色变为浅蓝色
    chartWrapperRef.value!.style.background = 'lightblue';
  };
  const drop = () => {
    chartWrapperRef.value!.style.background = 'transparent';
  };
  watch(
    () => props.options,
    () => {
      setOptions(props.options, true);
    },
    { immediate: true, deep: true },
  );
  // 监听width和height的变化
  watch(
    [() => props.width, () => props.height],
    () => {
      resizeFn();
    },
    { immediate: true, deep: true },
  );
</script>
