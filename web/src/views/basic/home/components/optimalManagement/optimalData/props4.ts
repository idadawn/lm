import { PropType } from 'vue';

export interface BasicProps {
  width: string;
  text: string;
  height: string;
}
export const basicProps4 = {
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
  //组件思路===30dai--
  //1，options的里面固定的重复的写死传入组件
  //2，数据以及什么形式，接口获取
  OptionsData: {
    type: Object as PropType<object>,
    default: {
      tooltip: {
        trigger: 'item',
      },
      legend: {
        top: '5%',
        left: 'center',
      },
      series: [
        {
          name: '直接',
          type: 'pie',
          radius: ['40%', '70%'],
          avoidLabelOverlap: false,
          itemStyle: {
            borderRadius: 10,
            borderColor: '#fff',
            borderWidth: 2,
          },
          label: {
            show: false,
            position: 'center',
          },
          emphasis: {
            label: {
              show: true,
              fontSize: 40,
              fontWeight: 'bold',
            },
          },
          labelLine: {
            show: false,
          },
          data: [],
        },
      ],
    },
  },
};
