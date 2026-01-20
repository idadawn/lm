export type ComparisonOperator = '=' | '<>' | '>' | '>=' | '<' | '<=' | 'IS_NULL' | 'NOT_NULL';

export interface JudgmentCondition {
    id: string;
    fieldId: string;
    operator: ComparisonOperator;
    value: string;
}

export interface ConditionGroup {
    id: string;
    logic: 'AND' | 'OR';
    conditions: JudgmentCondition[];
    subGroups?: ConditionGroup[];
}

export interface JudgmentRule {
    id: string;
    resultValue: string;
    rootGroup: ConditionGroup;
}

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
