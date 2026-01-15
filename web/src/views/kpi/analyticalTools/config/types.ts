import type { Dayjs } from 'dayjs';
import { ChartTypeEnum } from './const';

export type DataSet = {
  source: number[];
  dataAfterX: (string | number)[];
  dataAfterSpacing: (string | number)[];
  dataAfterY: number[];
  normalDistribution: number[];
  deviation: number[];
  length: number;
  sum: number;
  average: number;
  max: number;
  min: number;
  variance: number;
  standardDeviation: number;
  group: number;
  groupSpacing: number;
  range: number;
  standarDevRangeOfOne: string;
  standarDevRangeOfTwo: string;
  standarDevRangeOfThree: string;
};

export type RangeValue = [Dayjs, Dayjs];

export interface SpcFormState {
  dateTime: any;
  material: string;
  temperature: string;
  refiningTime: string;
  chartType: string;
}

export interface DataItem {
  key: string;
  group: number;
  name: string;
  data: number[];
  time: string;
  mean?: number;
  range?: number;
  standard?: number;
  [propName: string]: any;
}

export interface SpcLineType {
  label: string;
  value: ChartTypeEnum;
  data: string[];
}

/**
 * SPC图形数据范围控制线
 */
export interface MarkLineData {
  /**
   * 过程规范的上限
   * */
  USL?: number;
  /**
   * 单边工程规范的极限
   * */
  SL?: number;
  /**
   * 工程规范的下限
   * */
  LSL?: number;
  /**
   * 上控制限
   * */
  UCL?: number;
  /**
   * 均值
   * */
  CL?: number;
  /**
   * 下控制限
   * */
  LCL?: number;
}
