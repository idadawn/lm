export type TokenType = 'field' | 'operator' | 'function' | 'value' | 'parenthesis' | 'separator';

export interface FormulaToken {
  id: string;
  type: TokenType;
  value: string;
  label?: string; // For fields (e.g., "Revenue" instead of "col_1")
  editable?: boolean;
}

export interface FieldDefinition {
  id: string;
  name: string;
  code: string; // The actual value used in formula (e.g., M3)
  dataType: 'number' | 'string' | 'date';
}

export interface FormulaSnippet {
  name: string;
  description: string;
  tokens: Omit<FormulaToken, 'id'>[];
}

// 新增列表相关类型
export type FormulaType = '判定' | '计算';
export type FormulaStatus = '启用' | '停用';

export interface FormulaRow {
  id: string;
  sort: number;
  name: string;
  column: string;
  type: FormulaType;
  formulaDisplay: string;
  unit?: string;
  precision?: number;
  status: FormulaStatus;
}

// --- 判定规则相关类型 ---

export type ComparisonOperator = '=' | '<>' | '>' | '>=' | '<' | '<=' | 'IS_NULL' | 'NOT_NULL';

export interface JudgmentCondition {
  id: string;
  fieldId: string; // 关联的字段ID
  operator: ComparisonOperator;
  value: string; // 比较值，如果是 IS_NULL/NOT_NULL 则为空
}

export interface ConditionGroup {
  id: string;
  logic: 'AND' | 'OR'; // 组内条件是 且 还是 或
  conditions: JudgmentCondition[];
  subGroups?: ConditionGroup[]; // 支持简单的嵌套
}

export interface JudgmentRule {
  id: string;
  resultValue: string; // 满足条件时返回的值，例如 "A", "性能不合"
  rootGroup: ConditionGroup; // 根条件组
}
