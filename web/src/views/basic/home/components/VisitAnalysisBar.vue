<template>
  <div ref="chartRef" :style="{ height, width }"></div>
</template>
<script lang="ts">
  import { basicProps } from './props';
</script>
<script lang="ts" setup>
  import { onMounted, ref, Ref } from 'vue';
  import { useECharts } from '/@/hooks/web/useECharts';

  defineProps({
    ...basicProps,
  });

  const chartRef = ref<HTMLDivElement | null>(null);
  const { setOptions } = useECharts(chartRef as Ref<HTMLDivElement>);
  onMounted(() => {
    setOptions({
      tooltip: {
        trigger: 'axis',
        axisPointer: {
          lineStyle: {
            width: 1,
            color: '#019680',
          },
        },
      },
      grid: { left: '1%', right: '1%', top: '2  %', bottom: 0, containLabel: true },
      xAxis: {
        type: 'category',
        data: [...new Array(12)].map((_item, index) => `${index + 1}月`),
      },
      yAxis: {
        type: 'value',
        max: 8000,
        splitNumber: 4,
      },
      series: [
        {
          data: [
            3500,
            {
              value: 3500,
              itemStyle: {
                color: '#a90000',
              },
            },
            2000,
            1333,
            5000,
            {
              value: 5000,
              itemStyle: {
                color: '#a90000',
              },
            },
            2200,
            4200,
            {
              value: 4200,
              itemStyle: {
                color: '#a90000',
              },
            },
            1200,
            2100,
            3000,
            2100,
            1000,
            2200,
            2800,
          ],
          type: 'bar',
          barMaxWidth: 80,
        },
      ],
    });
  });
</script>
