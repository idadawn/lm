import { PropType } from 'vue';
export interface BasicProps {
  width: string;
  text: string;
  height: string;
}
export const basicProps = {
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
  OptionsData: {
    type: Object as PropType<object>,
    default: {
      title: {
        text: '****率',
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
        type: 'category', //'value' 数值轴，适用于连续数据。 'category' 类目轴，适用于离散的类目数据，为该类型时必须通过 data 设置类目数据。 'time' 时间轴，适用于连续的时序数据，与数值轴相比时间轴带有时间的格式化，在刻度计算上也有所不同，例如会根据跨度的范围来决定使用月，星期，日还是小时范围的刻度。 'log' 对数轴。适用于对数数据。
        boundaryGap: false, //刻度离纵轴有无间隙，默认true有间距
        // axisPointer: {
        //   //X轴上的阴影竖框（柱状图出现更为合适）
        //   type: 'shadow',
        // },
        data: [],
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
        },
      ],
      series: [],
    },
  },
};
function calc(arg0: number, arg1: number, px: any) {
  throw new Error('Function not implemented.');
}
