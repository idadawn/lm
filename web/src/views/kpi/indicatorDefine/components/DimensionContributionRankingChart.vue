<template>
  <div class="dcr-container-chart">
    <div class="dcr-title">
      <ArrowDownOutlined v-if="type === ContributionTypeEnum.Down" :class="['px-1', 'mr-2', 'down']" />
      <ArrowUpOutlined v-if="type === ContributionTypeEnum.Up" :class="['px-1', 'mr-2', 'up']" />
      <span class="pr-2">{{ type === ContributionTypeEnum.Down ? '同向贡献' : '反向贡献' }}</span>
      <a-tooltip>
        <template #title>
          {{ type === ContributionTypeEnum.Down ? '引起下降的因素排名' : '引起上升的因素排名' }}
        </template>
        <ExclamationCircleOutlined class="dcr-icon" />
      </a-tooltip>
    </div>
    <div class="dcr-chart" ref="chartRef"></div>
  </div>
</template>

<script setup lang="ts">
  import { ref, toRefs, onMounted, Ref, watch, reactive } from 'vue';
  import { ArrowUpOutlined, ArrowDownOutlined, ExclamationCircleOutlined } from '@ant-design/icons-vue';
  import { ContributionTypeEnum } from '/@/enums/publicEnum';
  import { useECharts } from '/@/hooks/web/useECharts';

  defineOptions({
    name: 'DimensionContributionRankingChart',
  });

  const props = defineProps<{
    type: string;
    chartData: any;
  }>();

  const { type, chartData } = toRefs(props);

  const chartRef = ref<HTMLDivElement | null>(null);

  const options = ref();

  const { tableData } = toRefs(props);
  const state = reactive<State>({
    tableDataMsg: '',
  });

  onMounted(() => {
    const { setOptions } = useECharts(chartRef as Ref<HTMLDivElement>);
    options.value = {
      tooltip: {
        trigger: 'axis',
        axisPointer: {
          type: 'shadow',
        },
        textStyle: {
          fontSize: 12,
        },
        formatter: function (params, _ticket, _callback) {
          var res = `
            <span style="color: #a5b2c5; padding-right: 6px;">维度值</span> ${params[0].name}<br/>
            <span style="color: #a5b2c5; padding-right: 6px;">贡献值</span> ${params[0].name}<br/>
            <span style="color: #a5b2c5; padding-right: 6px;">变化值</span> 
              <span style="color: ${params[0].color}">${params[0].name}</span>`;
          return res;
        },
      },
      legend: {
        show: false,
      },
      grid: {
        top: '1%',
        left: '3%',
        right: '4%',
        bottom: '3%',
        containLabel: true,
      },
      xAxis: {
        type: 'value',
        splitLine: {
          show: false,
        },
        axisLabel: {
          show: false,
        },
      },
      yAxis: {
        type: 'category',
        axisTick: {
          show: false,
        },
        axisLine: {
          lineStyle: {
            color: '#ecf0f8',
          },
        },
        axisLabel: {
          color: '#2f374c',
        },
        data: chartData.value.xAxis,
      },
      series: [
        {
          name: '维度值',
          type: 'bar',
          stack: 'total',
          label: {
            show: true,
            formatter: function (params) {
              if (type.value === ContributionTypeEnum.Down) {
                return `-${params.value}`;
              } else {
                return `+${params.value}`;
              }
            },
          },
          emphasis: {
            focus: 'series',
          },
          itemStyle: {
            color: type.value === ContributionTypeEnum.Down ? '#FECB9A' : '#B0CCFB',
          },
          data: chartData.value.yAxis,
        },
      ],
    };
    setOptions(options.value);
  });
  watch(
    () => props,
    (newValue, oldValue) => {
      // console.log('========999', newValue);
      state.tableDataMsg = newValue;
    },
    { deep: true },
  );
</script>

<style lang="less" scoped>
  .dcr-container-chart {
    .dcr-title {
      span {
        font-weight: 600;
        font-size: 12px;
        line-height: 22px;
        color: #546174;
        border-radius: 6px;
      }

      .down {
        color: rgb(254, 185, 120);
        background-color: rgb(255, 242, 229);
      }

      .up {
        color: rgb(59, 130, 252);
        background-color: rgb(241, 246, 254);
      }

      .dcr-icon {
        color: #a5b2c5;
      }
    }

    .dcr-chart {
      width: 100%;
      height: 200px;
    }
  }
</style>
