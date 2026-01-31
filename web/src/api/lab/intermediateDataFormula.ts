import { defHttp } from '/@/utils/http/axios';
import {
    IntermediateDataFormula,
    IntermediateDataColumnInfo,
    FormulaVariableSource,
    IntermediateDataFormulaInput,
    FormulaValidationRequest,
    FormulaValidationResult,
} from './types/intermediateDataFormula';

enum Api {
    Base = '/api/lab/intermediate-data-formula',
    AvailableColumns = '/api/lab/intermediate-data-formula/available-columns',
    VariableSources = '/api/lab/intermediate-data-formula/variable-sources',
    Validate = '/api/lab/intermediate-data-formula/validate',
}

/**
 * 获取公式列表
 */
export const getIntermediateDataFormulaList = () => {
    return defHttp.get<IntermediateDataFormula[]>({ url: Api.Base });
};

/**
 * 根据ID获取公式
 */
export const getIntermediateDataFormula = (id: string) => {
    return defHttp.get<IntermediateDataFormula>({ url: `${Api.Base}/${id}` });
};

/**
 * 创建公式
 */
export const createIntermediateDataFormula = (params: IntermediateDataFormulaInput) => {
    return defHttp.post<IntermediateDataFormula>({ url: Api.Base, params });
};

/**
 * 更新公式
 */
export const updateIntermediateDataFormula = (id: string, params: IntermediateDataFormulaInput) => {
    return defHttp.put<IntermediateDataFormula>({ url: `${Api.Base}/${id}`, params });
};

/**
 * 仅更新公式内容
 */
export const updateFormula = (id: string, formula: string) => {
    return defHttp.put<IntermediateDataFormula>({
        url: `${Api.Base}/${id}/formula`,
        data: { formula } // 会作为 JSON Body 发送: { "formula": "..." }
    });
};

/**
 * 删除公式
 */
export const deleteIntermediateDataFormula = (id: string) => {
    return defHttp.delete<void>({ url: `${Api.Base}/${id}` });
};

/**
 * 获取中间数据表可用列列表
 */
/**
 * 获取中间数据表可用列列表
 */
export const getAvailableColumns = (includeHidden = false) => {
    return defHttp.get<IntermediateDataColumnInfo[]>({ url: Api.AvailableColumns, params: { includeHidden } });
};

/**
 * 初始化公式列表
 */
export const initializeIntermediateDataFormula = () => {
    return defHttp.post<void>({ url: `${Api.Base}/initialize` });
};

/**
 * 获取公式变量来源列表
 */
export const getVariableSources = () => {
    return defHttp.get<FormulaVariableSource[]>({ url: Api.VariableSources });
};

/**
 * 验证公式
 */
export const validateFormula = (params: FormulaValidationRequest) => {
    return defHttp.post<FormulaValidationResult>({ url: Api.Validate, params });
};

/**
 * 生成判定规则
 */
export const generateJudgmentRule = (id: string) => {
    return defHttp.post<void>({ url: `${Api.Base}/${id}/generate-judgment` });
};
