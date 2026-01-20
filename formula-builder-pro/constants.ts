import { FieldDefinition, FormulaSnippet, FormulaRow } from './types';

export const MOCK_FIELDS: FieldDefinition[] = [
  { id: 'f1', name: '列表价 (L3)', code: 'L3', dataType: 'number' },
  { id: 'f2', name: '乘数 (M3)', code: 'M3', dataType: 'number' },
  { id: 'f3', name: '调整值 (AV3)', code: 'AV3', dataType: 'number' },
  { id: 'f4', name: '税率', code: 'TAX', dataType: 'number' },
  { id: 'f5', name: '折扣', code: 'DISC', dataType: 'number' },
  // 增加判定公式中出现的字段 Mock
  { id: 'f_F3', name: 'F3 (厚度)', code: 'F3', dataType: 'number' },
  { id: 'f_I3', name: 'I3 (备用厚度)', code: 'I3', dataType: 'number' },
  { id: 'f_AI3', name: 'AI3 (表面缺陷)', code: 'AI3', dataType: 'string' },
  { id: 'f_AJ3', name: 'AJ3 (边缘缺陷)', code: 'AJ3', dataType: 'string' },
];

// 分类导出运算符，提升UI易用性
export const BASIC_OPERATORS = [
  { label: '+', value: '+', type: 'operator' },
  { label: '-', value: '-', type: 'operator' },
  { label: '×', value: '*', type: 'operator' },
  { label: '÷', value: '/', type: 'operator' },
];

export const COMPARISON_OPERATORS = [
  { label: '=', value: '=', type: 'operator' },
  { label: '<>', value: '<>', type: 'operator' },
  { label: '>', value: '>', type: 'operator' },
  { label: '<', value: '<', type: 'operator' },
  { label: '≥', value: '>=', type: 'operator' },
  { label: '≤', value: '<=', type: 'operator' },
];

export const SYNTAX_TOKENS = [
  { label: '(', value: '(', type: 'parenthesis' },
  { label: ')', value: ')', type: 'parenthesis' },
  { label: ',', value: ',', type: 'separator' }, // 逗号特别重要，用于函数参数分隔
];

// 判定规则专用的运算符选项
export const RULE_OPERATORS = [
    { label: '等于', value: '=' },
    { label: '不等于', value: '<>' },
    { label: '大于', value: '>' },
    { label: '大于等于', value: '>=' },
    { label: '小于', value: '<' },
    { label: '小于等于', value: '<=' },
    { label: '为空', value: 'IS_NULL' },
    { label: '不为空', value: 'NOT_NULL' },
];

// 保持旧的导出以防有遗漏引用（可选）
export const OPERATORS = [
  ...BASIC_OPERATORS,
  ...SYNTAX_TOKENS,
  ...COMPARISON_OPERATORS
];

export const FUNCTIONS = [
  { label: 'SUM', value: 'SUM', desc: '求和 (字段1, 字段2, ...)' },
  { label: 'AVG', value: 'AVERAGE', desc: '平均值 (字段1, 字段2, ...)' },
  { label: 'MAX', value: 'MAX', desc: '最大值' },
  { label: 'MIN', value: 'MIN', desc: '最小值' },
  { label: 'IF', value: 'IF', desc: '条件判断 (条件, 真值, 假值)' },
];

export const SNIPPETS: FormulaSnippet[] = [
  {
    name: '多字段求和 (SUM)',
    description: '计算多个字段的总和',
    tokens: [
      { type: 'function', value: 'SUM' },
      { type: 'parenthesis', value: '(' },
      { type: 'field', value: 'field1', label: '字段1' },
      { type: 'separator', value: ',' },
      { type: 'field', value: 'field2', label: '字段2' },
      { type: 'parenthesis', value: ')' },
    ]
  },
  {
    name: '安全除法 (IF)',
    description: '防止除以零的错误',
    tokens: [
      { type: 'function', value: 'IF' },
      { type: 'parenthesis', value: '(' },
      { type: 'field', value: 'divisor', label: '分母' },
      { type: 'operator', value: '<>' },
      { type: 'value', value: '0', editable: true },
      { type: 'separator', value: ',' },
      { type: 'field', value: 'numerator', label: '分子' },
      { type: 'operator', value: '/' },
      { type: 'field', value: 'divisor', label: '分母' },
      { type: 'separator', value: ',' },
      { type: 'value', value: '0', editable: true },
      { type: 'parenthesis', value: ')' },
    ]
  }
];

export const MOCK_FORMULA_ROWS: FormulaRow[] = [
  { id: '1', sort: 1, name: '贴标', column: 'Labeling', type: '判定', formulaDisplay: 'IF(AND(F3>0...), "A", ...)', status: '启用' },
  { id: '8', sort: 8, name: '一米带材重量', column: 'OneMeterWeight', type: '计算', formulaDisplay: 'L3 * M3', unit: 'g', precision: 2, status: '启用' },
  { id: '32', sort: 32, name: '带厚最小值', column: 'ThicknessMin', type: '计算', formulaDisplay: 'MIN(Data)', precision: 2, status: '启用' },
  { id: '33', sort: 33, name: '带厚最大值', column: 'ThicknessMax', type: '计算', formulaDisplay: 'MAX(Data)', precision: 2, status: '启用' },
];
