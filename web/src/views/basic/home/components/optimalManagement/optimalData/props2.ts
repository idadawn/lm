import { PropType } from 'vue';

export interface BasicProps {
  width: string;
  text: string;
  height: string;
}
export const basicProps2 = {
  width: {
    type: String as PropType<string>,
    default: '100%',
  },
  height: {
    type: String as PropType<string>,
    default: '280px',
  },
  text: {
    type: String as PropType<string>,
    default: 'Stacked Line',
  },
  //组件思路===30dai--
  //1，options的里面固定的重复的写死传入组件
  //2，数据以及什么形式，接口获取
  OptionsData: {
    type: Object as PropType<object>,
    default: {
      title: {
        text: '',
      },
      tooltip: {
        trigger: 'axis',
        axisPointer: {
          type: 'shadow',
        },
      },
      legend: {},
      grid: {
        left: '3%',
        right: '4%',
        bottom: '3%',
        containLabel: true,
      },
      xAxis: {
        type: 'value',
        boundaryGap: [0, 0.01],
      },
      yAxis: {
        type: 'category',
        data: [],
      },
      series: [
        {
          name: '2011',
          type: 'bar',
          data: [
            18203,
            {
              value: 3500,
              itemStyle: {
                color: '#a90000',
              },
            },
            23489,
            29034,
            104970,
            131744,
            630230,
          ],
        },
        {
          name: '2012',
          type: 'bar',
          data: [19325, 23438, 31000, 121594, 134141, 681807],
        },
      ],
    },
  },
};
