import { defHttp } from '/@/utils/http/axios';

enum Api {
    GetSuggestedKeywords = '/api/lab/feature-learning/suggested-keywords',
    ApplyKeywords = '/api/lab/feature-learning/apply-keywords',
    GetLearningStats = '/api/lab/feature-learning/learning-stats',
    GetCorrectionList = '/api/lab/feature-learning/corrections',
    DeleteCorrection = '/api/lab/feature-learning/corrections',
    ConfirmCorrection = '/api/lab/feature-learning/corrections',
}

export interface KeywordSuggestion {
    featureId: string;
    featureName: string;
    featureCategory: string;
    suggestedKeyword: string;
    frequency: number;
    status: string;
}

export interface AppearanceFeatureCorrection {
    id: string;
    inputText: string;
    autoMatchedFeatureId: string;
    correctedFeatureId: string;
    matchMode: string;
    scenario: string;
    remark: string;
    creatorTime: string;
    status?: string;
    correctedFeatureName?: string;
    autoMatchedFeatureName?: string;
    matchModeText?: string;
}

export const getSuggestedKeywords = () => {
    return defHttp.get<KeywordSuggestion[]>({ url: Api.GetSuggestedKeywords });
};

export const applyKeywordSuggestions = (input: { suggestions: KeywordSuggestion[] }) => {
    return defHttp.post({ url: Api.ApplyKeywords, params: input });
};

export const getLearningStats = () => {
    return defHttp.get<any>({ url: Api.GetLearningStats });
};

export const getCorrectionList = () => {
    return defHttp.get<AppearanceFeatureCorrection[]>({ url: Api.GetCorrectionList });
};

export const deleteCorrection = (id: string) => {
    return defHttp.delete({ url: `${Api.DeleteCorrection}/${id}` });
};

export const confirmCorrection = (id: string) => {
    return defHttp.post({ url: `${Api.ConfirmCorrection}/${id}/confirm` });
};

export const updateCorrection = (
    id: string,
    input: { featureId?: string; status?: string; remark?: string }
) => {
    return defHttp.put({
        url: `${Api.GetCorrectionList}/${id}`,
        params: input,
    });
};

export interface UploadExcelResult {
    totalRows: number;
    uniqueFeatureTexts: number;
    perfectMatches: number;
    newCorrections: number;
    message: string;
}

export const uploadExcelForCorrection = (file: File) => {
    const formData = new FormData();
    formData.append('file', file);
    return defHttp.post<UploadExcelResult>({
        url: '/api/lab/feature-learning/upload-excel',
        params: formData,
        headers: { 'Content-Type': 'multipart/form-data' },
    });
};
