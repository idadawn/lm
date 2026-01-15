import { PropType } from 'vue';

export interface BasicProps {
  width: string;
  text: string;
  height: string;
}
export const histogramProps = {
  width: {
    type: String as PropType<string>,
    default: '100%',
  },
  height: {
    type: String as PropType<string>,
    default: '350px',
  },
  text: {
    type: String as PropType<string>,
    default: 'Stacked Line',
  },
  //柱状图的参数
  OptionsData: {
    type: Object as PropType<object>,
    default: {
      title: {
        text: '****率00',
      },
      tooltip: {
        trigger: 'axis',
      },
      grid: {
        left: '3%',
        right: '4%',
        bottom: '3%',
        containLabel: true,
      },
      // toolbox: {
      //   //工具类型
      //   feature: {
      //     dataView: { show: true, readOnly: false },
      //     magicType: { show: true, type: ['line', 'bar'] },
      //     restore: { show: true },
      //     saveAsImage: { show: true },

      //   },
      // },
      legend: {
        data: [],
      },
      xAxis: {
        type: 'category',
        data: [...new Array(12)].map((_item, index) => `${index + 1}月`),
      },
      yAxis: [
        {
          type: 'value',
          data: [],
          // name: 'Precipitation',
          // min: 0,
          // max: 250,
          // interval: 50,
          // axisLabel: {
          //   formatter: '{value} ml',
          // },
        },
        {
          type: 'value',
          // name: 'Precipitation',
          // min: 0,
          // max: 250,
          // interval: 50,
          // axisLabel: {
          //   formatter: '{value} ml',
          // },
        },
      ],
      series: [],
    },
  },
};
