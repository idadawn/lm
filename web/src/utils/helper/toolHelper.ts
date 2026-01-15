import { Theme, themes } from './themes';
import { getChartData, getFilterData, getMarkAreaData, getMetricsDimensions } from '/@/api/chart';
import { ChartTypeEnum } from '/@/enums/chartEnum';

/**
 * 生成唯一guid
 * @returns {*} string
 */
export const guid = () =>
  'xxxxxxxx-xxxx-4xxx-yxxx-xxxxxxxxxxxx'.replace(/[xy]/g, c => {
    const r = (Math.random() * 16) | 0,
      v = c == 'x' ? r : (r & 0x3) | 0x8;
    return v.toString(16);
  });

// echarts结构数据
const chartOptions = {
  type: 'line',
  chartType: '',
  backgroundColor: 'transparent', // 设置背景透明
  title: {
    text: '',
  },
  tooltip: {
    trigger: 'axis',
    axisPointer: {
      // 坐标轴指示器，坐标轴触发有效
      type: 'shadow', // 默认为直线，可选为：'line' | 'shadow'
    },
  },
  toolbox: {
    show: true,
    orient: 'vertical',
    left: 'right',
    top: 'center',
    feature: {
      mark: { show: true },
      dataView: { show: true, readOnly: false },
      magicType: { show: true, type: ['stack', 'tiled'] },
      restore: { show: false },
      saveAsImage: { show: true },
    },
  },
  legend: {
    data: ['直接访问', '邮件营销'],
  },
  grid: {
    left: '30px',
    right: '30px',
    bottom: '30px',
    containLabel: true,

  },
  xAxis: [
    {
      type: 'category',
      data: ['周一', '周二', '周三', '周四', '周五', '周六', '周日'],
    },
  ],
  yAxis: [
    {
      name: '',
      type: 'value',
    },
  ],
  series: [
    {
      name: '直接访问',
      type: 'bar',
      data: [320, 332, 301, 334, 390, 330, 320],
    },
    {
      name: '邮件营销',
      type: 'bar',
      stack: '广告',
      data: [120, 132, 101, 134, 90, 230, 210],
    },
  ],
};

/**
 * 拖拽元素数据转换器
 * @param option
 * @param data
 * @returns
 */
export async function optionAdapter(option, data) {
  console.log('optionAdapter');
  const { type, chartTheme } = option;
  switch (type) {
    case 'date':
      return dateFilterAdapter(data);
    case 'select':
      return selectFilterAdapter(data);
    case ChartTypeEnum.MARKAREA:
      const res = await getMarkAreaData({ metrics: [] });
      const markArea = {
        tooltip: { show: false },
        silent: true,
        label: { position: 'insideLeft' },
        data: res.data,
      };
      data.markArea = markArea;
    default:
      data.type = type;
      return chartAdapter(data, chartTheme);
  }
}

/**
 * 图表数据转换器
 * @param data
 * @param theme
 * @returns
 */
export function chartAdapter(data, theme = '明亮') {
  const colorObj = themes.find(item => item.name == theme) as Theme;
  // legend
  chartOptions.legend.data = data.display_names;
  // series
  const arr = data.data;
  const series: any[] = [];
  const xAxisData: any[] = [];
  data.display_names.forEach((name, index) => {
    const item: any = {
      name: name,
      type: data.type == ChartTypeEnum.MARKAREA ? 'line' : data.type,
      // stack: '总量',
      // areaStyle: {},
      data: [],
    };
    arr.forEach((target: string, aIndex: number) => {
      if (index == 0) {
        xAxisData.push(target[0]);
      }
      item.data.push(target[index + 1]);
    });
    item.itemStyle = {
      color: colorObj.color[index],
    };
    item.markArea = data.markArea;
    series.push(item);
  });

  chartOptions.xAxis[0].data = xAxisData;
  chartOptions.series = series;
  chartOptions.type = data.type;
  chartOptions.chartType = data.chartType;

  console.log('chartOptions', chartOptions);
  return JSON.parse(JSON.stringify(chartOptions));
}
/**
 * 日期类型数据转换器
 * @param data
 * @returns
 */
export function dateFilterAdapter(data) {
  return {
    type: 'date',
  };
}
/**
 * 筛选条件数据转换器
 * @param data
 * @returns
 */
export function selectFilterAdapter(data) {
  return {
    type: 'select',
    data,
  };
}

/**
 * 图表类型切换
 * @param option
 * @param type
 * @param layout
 * @returns
 */
export async function changeTypeAdapter(option: any, type: ChartTypeEnum, layout: any) {
  const params = layout.query.metrics;
  let markArea = {
    tooltip: { show: false },
    silent: true,
    label: { position: 'insideLeft' },
    data: [],
  };
  if (type == ChartTypeEnum.MARKAREA) {
    const res = await getMarkAreaData(params);
    markArea.data = res.data;
  }
  option.series.forEach(item => {
    item.type = type == ChartTypeEnum.MARKAREA ? 'line' : type;
    item.markArea = markArea;
  });
  return JSON.parse(JSON.stringify(option));
}

/**
 * 获取公共filters
 * @param items
 * @returns
 */
export function getFilters(items) {
  return items.filter(item => item.class == 'filter').map(item => item.filter);
}

/**
 * 获取画布所有拖拽原色的数据
 * @param layout
 * @param flag
 * @returns
 */
export async function getOptionMap(layout, flag) {
  const ids = layout.canvas.layouts[0].layout;
  const chartTheme = layout.canvas.layoutsStyle.chartTheme;
  if (!Array.isArray(ids) || ids.length < 1) {
    return {};
  }
  let optionMap = {};
  let optionData;
  const items = layout.canvas.layouts[0].layout;
  const commonFilters = getFilters(items);
  for (let i = 0; i < ids.length; i++) {
    const item = items[i];
    const { id, type, key } = item;
    switch (type) {
      case ChartTypeEnum.SELECT:
        if (flag == 'init') {
          const filterData = await getFilterData(id);
          optionData = filterData.data;
          optionMap[key] = await optionAdapter({ type }, optionData.data);
        }
        break;
      case ChartTypeEnum.LINE:
      case ChartTypeEnum.MARKAREA:
      case ChartTypeEnum.BAR:
      case ChartTypeEnum.PIE:
        console.log(ChartTypeEnum, 'ChartTypeEnum');
        item.class = 'chart';
        const query = getChartParams(commonFilters, item);
        optionData = await getChartData(query);
        optionMap[key] = await optionAdapter({ type, chartTheme }, optionData.data);
        break;
      default:
        break;
    }
  }
  return optionMap;
}
/**
 * 获取图表查询条件
 * @param filters 公共查询条件
 * @param target 当前节点
 * @returns
 */
export function getChartParams(filters, target) {
  const arr = filters ?? [];
  return {
    filters: [...arr, target.filter],
    metrics: target.query.metrics,
    dimension: target.query.dimension,
  };
}

/**
 * 获取维度
 * @param layout
 * @returns
 */
export async function getDimensionMap(layout) {
  const res = {};
  const ids = layout.canvas.layouts[0].layout.filter(item => item.class == 'chart');
  if (!Array.isArray(ids) || ids.length < 1) {
    return {};
  }
  const metrics = ids.map(item => item.query.metrics);
  const data = await getMetricsDimensions(metrics);
  for (const item of ids) {
    res[item.key] = { dimensions: data.data };
  }

  return res;
}
