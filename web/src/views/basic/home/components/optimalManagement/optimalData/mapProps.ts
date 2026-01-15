import { PropType } from 'vue';

export interface BasicProps {
  width: string;
  text: string;
  height: string;
}
export const mapProps = {
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
      // toolbox: {
      //   //工具类型
      //   feature: {
      //     dataView: { show: true, readOnly: false },
      //     magicType: { show: true, type: ['line', 'bar'] },
      //     restore: { show: true },
      //     saveAsImage: { show: true },
      //   },
      // },

      xAxis: {
        type: 'category',
        boundaryGap: [0, 0.01],
      },
      yAxis: {
        type: 'value',
        data: [],
      },
      dataZoom: [
        {
          type: 'inside',
          start: 0,
          end: 20,
        },
        {
          start: 0,
          end: 20,
        },
      ],
      visualMap: {
        type: 'piecewise',
        show: false,
        dimension: 0,
        seriesIndex: 0,
        pieces: [
          {
            gt: 1,
            lt: 2,
            color: 'rgba(0, 0, 180, 0.4)',
          },
        ],
      },
      series: [
        {
          type: 'line',

          symbol: 'none',
          lineStyle: {
            color: '#5470C6',
            width: 3,
          },
          areaStyle: {},
          data: [],
        },
      ],
    },
  },
};
