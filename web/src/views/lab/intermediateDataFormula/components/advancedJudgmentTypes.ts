/**
 * 高级判定规则编辑器 - 类型定义 v2
 * 支持嵌套条件组：OR(AND(...), AND(...))
 */

// 单个条件
export interface Condition {
  id: string;
  leftExpr: string;      // 左侧表达式: "F3" 或 "(N3 - O3)"
  operator: string;      // 操作符
  rightValue: string;    // 右侧值
}

// 子条件组（最内层，只包含简单条件）
export interface SubConditionGroup {
  id: string;
  logic: 'AND' | 'OR';   // 子组内条件的关系
  conditions: Condition[];
}

// 条件组（可以包含简单条件 OR 子条件组）
export interface ConditionGroup {
  id: string;
  name?: string;                    // 组名，如 "厚度检测"
  mode: 'simple' | 'nested';        // simple: 简单条件列表, nested: 子条件组列表
  logic: 'AND' | 'OR';              // 组内逻辑

  // mode === 'simple' 时使用
  conditions: Condition[];

  // mode === 'nested' 时使用（子组之间的逻辑由 logic 决定）
  subGroups: SubConditionGroup[];
}

// 判定规则
export interface AdvancedJudgmentRule {
  id: string;
  resultValue: string;
  groups: ConditionGroup[];  // 组之间是 AND 关系
}

// 操作符选项
export const OPERATORS = [
  { label: '等于 (=)', value: '=' },
  { label: '不等于 (≠)', value: '<>' },
  { label: '大于 (>)', value: '>' },
  { label: '大于等于 (≥)', value: '>=' },
  { label: '小于 (<)', value: '<' },
  { label: '小于等于 (≤)', value: '<=' },
  { label: '包含任意 (CONTAINS ANY)', value: 'CONTAINS_ANY' },
  { label: '包含所有 (CONTAINS ALL)', value: 'CONTAINS_ALL' },
  { label: '为空', value: 'IS_NULL' },
  { label: '不为空', value: 'NOT_NULL' },
];