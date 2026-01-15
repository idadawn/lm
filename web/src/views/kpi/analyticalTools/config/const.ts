export enum DataSetKeyEnum {
  length = '总长度',
  sum = '总和',
  average = '平均值',
  max = '最大值',
  min = '最小值',
  variance = '方差',
  standardDeviation = '标准差',
  standarDevRangeOfOne = '一倍标准差范围',
  standarDevRangeOfTwo = '二倍标准差范围',
  standarDevRangeOfThree = '三倍标准差范围',
}

/**
 * 小数保留位数
 */
export const Reservedfixed = 4;

/**
 * @description SPC Chart Key名称
 */
export enum ChartTypeEnum {
  /**
   * 单值-移动极差图
   */
  individualmovingRange = 'individual-moving-range',
  /**
   * 均值-标准差图
   */
  meanStandard = 'mean-standard',
  /**
   * 均值-极差图
   */
  meanExtreme = 'mean-extreme',
  /**
   * 中位数-极差图
   */
  medianExtreme = 'median-extreme',
}

export const SpcChartKey = {
  [ChartTypeEnum.individualmovingRange]: '单值-移动极差图',
  [ChartTypeEnum.meanStandard]: '均值-标准差图',
  [ChartTypeEnum.meanExtreme]: '均值-极差图',
  [ChartTypeEnum.medianExtreme]: '中位数-极差图',
};

/**
 * @description 选择相应的组合
 * @description 单值-移动极差图
 * @description 均值-标准差图
 * @description 均值-极差图
 * @description 中位数-极差图
 */
export const SpcColumn = {
  mean: {
    title: '均值',
    dataIndex: 'mean',
  },
  range: {
    title: '极差值',
    dataIndex: 'range',
  },
  standard: {
    title: '标准差',
    dataIndex: 'standard',
  },
  median: {
    title: '中位数',
    dataIndex: 'median',
  },
  individual: {
    title: '单值',
    dataIndex: 'individual',
  },
  movingRange: {
    title: '移动极差值',
    dataIndex: 'movingRange',
  },
};

/**
 * @description Spc A2系数;
 * @description index = 0, n = 2 (n表示样本数量)
 * */
export const SpcA2 = [1.88, 1.02, 0.73, 0.58, 0.48, 0.42, 0.34, 0.34, 0.31];

/**
 * @description Spc D2系数;
 * @description index = 0, n = 2 (n表示样本数量)
 * */
export const SpcD2 = [1.13, 1.69, 2.06, 2.33, 2.53, 2.7, 2.85, 2.97, 3.08];

/**
 * @description Spc D3系数;
 * @description index = 0, n = 2 (n表示样本数量)
 * */
export const SpcD3 = [0, 0, 0, 0, 0, 0.08, 0.14, 0.18, 0.22];

/**
 * @description Spc D4系数;
 * @description index = 0, n = 2 (n表示样本数量)
 * */
export const SpcD4 = [3.27, 2.57, 2.28, 2.11, 2.0, 1.92, 1.86, 1.82, 1.78];

/**
 * @description Spc A3系数;
 * @description index = 0, n = 2 (n表示样本数量)
 * */
export const SpcA3 = [2.66, 1.95, 1.63, 1.43, 1.29, 1.18, 1.1, 1.03, 0.98];

/**
 * @description Spc B3系数;
 * @description index = 0, n = 2 (n表示样本数量)
 * */
export const SpcB3 = [0, 0, 0, 0, 0.03, 0.12, 0.19, 0.24, 0.28];

/**
 * @description Spc B4系数;
 * @description index = 0, n = 2 (n表示样本数量)
 * */
export const SpcB4 = [3.27, 2.57, 2.27, 2.09, 1.97, 1.88, 1.82, 1.76, 1.72];

/**
 * @description Spc A4系数;
 * @description index = 0, n = 2 (n表示样本数量)
 * */
export const SpcA4 = [2.66, 1.95, 1.63, 1.43, 1.29, 1.18, 1.1, 1.03, 0.98];

/**
 * @description Spc E2系数;
 * @description index = 0, n = 2 (n表示样本数量)
 * */
export const SpcE2 = [2.66, 1.77, 1.46, 1.29, 1.18, 1.11, 1.05, 1.01, 0.98];

/**
 * SPC Line Chart MarkLine Data 样式
 */
export const MarkLineDataLineStyle = {
  /**
   * 过程规范的上限
   * */
  USL: {
    color: '#090',
    type: 'solid',
    width: 1,
  },
  /**
   * 单边工程规范的极限
   * */
  SL: {
    color: '#00841F',
    type: 'solid',
    width: 1,
  },
  /**
   * 工程规范的下限
   * */
  LSL: {
    color: '#090',
    type: 'solid',
    width: 1,
  },
  /**
   * 上控制限
   * */
  UCL: {
    color: '#931313',
    type: 'solid',
    width: 1,
  },
  /**
   * 均值
   * */
  CL: {
    color: '#00841F',
    type: 'solid',
    width: 1,
  },
  /**
   * 下控制限
   * */
  LCL: {
    color: '#931313',
    type: 'solid',
    width: 1,
  },
};
