export interface ExcelImportTemplate {
    id: string;
    templateName: string;
    templateCode: string;
    description: string;
    templateType: 'system' | 'user';
    ownerUserId: string;
    configJson: string;
    productSpecId: string;
    productSpecName: string;
    version: number;
    isDefault: number;
    sortCode: number | null;
    creatorTime: string;
    lastModifyTime: string;
}

export interface ExcelImportTemplateInput {
    templateName: string;
    templateCode: string;
    description: string;
    templateType: 'system' | 'user';
    ownerUserId: string;
    configJson: string;
    productSpecId: string;
    isDefault: number;
    sortCode: number | null;
}

// 模板配置类型（对应后端ExcelTemplateConfig）
export interface ExcelTemplateConfig {
    version: string;
    description: string;
    fieldMappings: TemplateColumnMapping[];
    detectionColumns: DetectionColumnConfig;
    validation: TemplateValidationConfig;
}

export interface TemplateColumnMapping {
    field: string;
    excelColumnNames: string[];
    required: boolean;
    dataType: 'datetime' | 'string' | 'decimal' | 'int';
    defaultValue?: string;
}

export interface DetectionColumnConfig {
    patterns: string[];
    minColumn: number;
    maxColumn: number;
}

export interface TemplateValidationConfig {
    requiredFields: string[];
    fieldRules: Record<string, FieldValidationRule>;
}

export interface FieldValidationRule {
    pattern?: string;
    errorMessage?: string;
    min?: number;
    max?: number;
}