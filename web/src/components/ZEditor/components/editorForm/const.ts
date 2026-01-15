export const enum AddRuleTypeEnum {
  GreaterThan = 'GreaterThan',
  Between = 'Between',
  LessThan = 'LessThan',
}

export enum AddRuleThresholdEnum {
  Value = '数值',
  Percent = '百分比',
}

/**
 * @description 通知来源
 */
export const AddRuleThresholdList = [
  {
    fullName: '数值',
    id: 'Value',
  },
  {
    fullName: '百分比',
    id: 'Percent',
  },
];

/**
 * @description 通知来源
 */
export const enum NoticeSourceEnum {
  /**
   * @description 节点
   * */
  Node = 'Node',
  /**
   * @description 规则
   * */
  Rule = 'Rule',
}

/**
 * @description 节点Tabs
 */
export const enum NodeTabsEnum {
  /**
   * 建模
   * */
  Model = 1,
  /**
   * 预警
   * */
  Warning = 2,
  /**
   * 基线
   * */
  Baseline = 3,
  /**
   * 指标数据
   * */
  Data = 4,
  /**
   * 创建分级
   * */
  Grading = 5,
  /**
   * 创建通知
   * */
  Notice = 6,
}

/**
 * @description 操作状态
 */
export const enum OptTypeEnum {
  Add = 'Add',
  Edit = 'Edit ',
  Delete = 'Delete'
}

/**
 * @description 分级类型
 */
export const enum GradingTypeEnum {
  /**
   * @description 数值
   * */
  Value = 1,
  /**
   * @description 趋势
   * */
  Trend
}
