export interface MetricDefinition {
    id: string;
    name: string;
    code: string;
    description?: string;
    formula: string;
    formulaLanguage?: string;
    metricType?: string;
    returnType?: string;
    unitId?: string;
    unitName?: string;
    category?: string;
    isEnabled: boolean;
    isSystem?: boolean;
    sortOrder: number;
    version?: number;
    isVersioned?: boolean;
    hasVariables?: boolean;
    storeResults?: boolean;
    precision?: number;
    calculationOrder?: number;
    remark?: string;
    createTime?: string;
    creatorUserName?: string;
    lastModifyUserName?: string;
    variables?: MetricVariable[];
    versions?: MetricVersion[];
}

export interface MetricVariable {
    id?: string;
    metricId?: string;
    variableName: string;
    sourceType: string;
    sourceId?: string;
    dataType: string;
    isRequired?: boolean;
    defaultValue?: string;
    sortOrder?: number;
}

export interface MetricVersion {
    metricId?: string;
    version: number;
    versionName?: string;
    formula: string;
    changeReason?: string;
    isCurrent?: boolean;
    createTime?: string;
    creatorUserName?: string;
}

export interface MetricDefinitionQuery {
    keyword?: string;
    category?: string;
    isEnabled?: boolean;
    metricType?: string;
    returnType?: string;
    isVersioned?: boolean;
    hasVariables?: boolean;
    storeResults?: boolean;
    isSystem?: boolean;
    currentPage?: number;
    pageSize?: number;
    sidx?: string;
    sort?: string;
    createTimeStart?: string;
    createTimeEnd?: string;
    lastModifyTimeStart?: string;
    lastModifyTimeEnd?: string;
}

export interface MetricDefinitionInput {
    name: string;
    code: string;
    description?: string;
    formula: string;
    formulaLanguage?: string;
    metricType?: string;
    returnType?: string;
    unitId?: string;
    category?: string;
    isEnabled?: boolean;
    isVersioned?: boolean;
    hasVariables?: boolean;
    storeResults?: boolean;
    sortOrder?: number;
    precision?: number;
    calculationOrder?: number;
    remark?: string;
    variables?: MetricVariableInput[];
}

export interface MetricVariableInput {
    variableName: string;
    sourceType: string;
    sourceId?: string;
    dataType: string;
    isRequired?: boolean;
    defaultValue?: string;
    sortOrder?: number;
}

export interface UpdateStatusInput {
    isEnabled: boolean;
}

export interface BatchUpdateStatusInput {
    ids: string[];
    isEnabled: boolean;
}

export interface BatchDeleteInput {
    ids: string[];
}

export interface PaginatedResponse<T> {
    total: number;
    list: T[];
    currentPage: number;
    pageSize: number;
    totalPages: number;
}

export interface FormulaValidationInput {
    formula: string;
    formulaLanguage?: string;
    variables?: MetricVariableInput[];
}

export interface FormulaValidationResult {
    isValid: boolean;
    errorMessage?: string;
    variables?: string[];
}

export interface MetricCalculationInput {
    metricId?: string;
    metricCode?: string;
    contextData: Record<string, any>;
    sourceDataId?: string;
}

export interface MetricCalculationResult {
    value: any;
    unit?: string;
    errorMessage?: string;
    calculationTime?: string;
}
