<template>
  <div ref="chartRef" :style="{ height: height.default, width: width.default }"></div>
</template>
<script lang="ts" setup>
  import { onMounted, ref, Ref, toRefs, watchEffect, watch, nextTick } from 'vue';
  import { useECharts } from '/@/hooks/web/useECharts';
  import { getHeight } from '@antv/dom-util';

  defineOptions({
    name: 'ChartsManageMent',
  });

  const props = defineProps(['basicProps']);
  const { basicProps } = toRefs(props);
  const chartRef = ref<HTMLDivElement | null>(null);

  const { setOptions } = useECharts(chartRef as Ref<HTMLDivElement>);
  const { height, width, OptionsData } = toRefs(basicProps?.value);

  watchEffect(() => {
    // console.log('9999999999', props.basicProps);
    setOptions(props.basicProps.OptionsData.default);
  });
</script>
