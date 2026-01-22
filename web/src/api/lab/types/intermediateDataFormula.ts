/**
 * 公式类型枚举
 */
export enum FormulaType {
    /** 计算公式 */
    CALC = 'CALC',
    /** 判定公式 */
    JUDGE = 'JUDGE',
}

export interface IntermediateDataFormula {
    id: string;
    tableName: string;
    columnName: string;
    displayName?: string;
    formulaName: string;
    formula: string;
    formulaType?: string;
    formulaLanguage?: string;
    unitId?: string;
    unitName?: string;
    precision?: number;
    isEnabled: boolean;
    sortOrder: number;
    defaultValue?: string;
    remark?: string;
    creatorTime?: string;
    lastModifyTime?: string;
}

export interface IntermediateDataColumnInfo {
    columnName: string;
    displayName: string;
    dataType: string;
    isCalculable: boolean;
    decimalDigits?: number;
    description?: string;
    sort?: number;
    // 范围列属性
    isRange?: boolean;
    rangePrefix?: string;
    rangeStart?: number;
    rangeEnd?: number;
    // 外观特性分类与等级选项
    featureCategories?: Array<{ id: string; name: string }>;
    featureLevels?: Array<{ id: string; name: string }>;
}

export interface FormulaVariableSource {
    sourceType: string;
    tableName: string;
    variableKey: string;
    displayName: string;
    dataType: string;
    unit?: string;
}

export interface IntermediateDataFormulaInput {
    tableName: string;
    columnName: string;
    formulaName: string;
    formula: string;
    formulaType?: string;
    formulaLanguage?: string;
    unitId?: string;
    precision?: number;
    isEnabled?: boolean;
    sortOrder?: number;
    defaultValue?: string;
    remark?: string;
}

export interface FormulaValidationRequest {
    formula: string;
    formulaLanguage?: string;
    columnName?: string;
}

export interface FormulaValidationResult {
    isValid: boolean;
    errorMessage?: string;
    undefinedVariables?: string[];
}
