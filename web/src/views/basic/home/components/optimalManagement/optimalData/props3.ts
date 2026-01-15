import { PropType } from 'vue';

export interface BasicProps {
  width: string;
  text: string;
  height: string;
}
export const basicProps3 = {
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
      series: [
        {
          type: 'gauge',
          axisLine: {
            lineStyle: {
              width: 30,
              color: [
                [0.3, '#67e0e3'],
                [0.7, '#37a2da'],
                [1, '#fd666d'],
              ],
            },
          },
          pointer: {
            itemStyle: {
              color: 'auto',
            },
          },
          axisTick: {
            distance: -30,
            length: 8,
            lineStyle: {
              color: '#fff',
              width: 2,
            },
          },
          splitLine: {
            distance: -30,
            length: 30,
            lineStyle: {
              color: '#fff',
              width: 4,
            },
          },
          axisLabel: {
            color: 'inherit',
            distance: 40,
            fontSize: 20,
          },
          detail: {
            valueAnimation: true,
            formatter: '{value} km/h',
            color: 'inherit',
          },
          data: [
            {
              value: '',
            },
          ],
        },
      ],
    },
  },
};
