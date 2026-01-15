// 重新导出 appearance.ts 和 appearanceCategory.ts 中的函数
import {
    getAppearanceFeatureList as getAppearanceFeatureListOriginal,
    getAppearanceFeatureInfo,
    createAppearanceFeature,
    updateAppearanceFeature,
    delAppearanceFeature,
    matchAppearanceFeature,
    batchMatchAppearanceFeature,
    getEnabledSeverityLevels,
    createSeverityLevel,
    saveAppearanceFeatureCorrection,
    addFeatureWithSeverity,
    addKeywordToFeature,
    createOrAddKeyword,
    addFeatureVariant,
    type AppearanceFeatureInfo,
    type AppearanceFeatureListQuery,
    type AppearanceFeatureInput,
    type MatchInput,
    type BatchMatchInput,
    type MatchItemInput,
    type MatchItemOutput,
    type ManualCorrectionOption,
    type AppearanceFeatureCorrectionInput,
    type AddWithSeverityInput,
    type AddKeywordInput,
    type CreateOrAddKeywordInput,
    type CreateOrAddKeywordOutput,
    type AddFeatureVariantInput,
} from './appearance';

import {
    getAppearanceFeatureCategoryList as getAppearanceFeatureCategoryListOriginal,
    getAllAppearanceFeatureCategories,
    getAppearanceFeatureCategoryInfo,
    createAppearanceFeatureCategory,
    updateAppearanceFeatureCategory,
    delAppearanceFeatureCategory,
    type AppearanceFeatureCategoryInfo,
    type AppearanceFeatureCategoryInput,
    type AppearanceFeatureCategoryListQuery,
} from './appearanceCategory';

// 包装函数以统一返回格式
export async function getAppearanceFeatureList(params: AppearanceFeatureListQuery) {
    const response = await getAppearanceFeatureListOriginal(params);
    // 处理不同的返回格式
    if (Array.isArray(response)) {
        return { list: response };
    } else if (response?.data) {
        const data = response.data;
        if (Array.isArray(data)) {
            return { list: data };
        } else if (data?.list) {
            return { list: data.list };
        }
    } else if (response?.list) {
        return { list: response.list };
    }
    return { list: [] };
}

export async function getAppearanceFeatureCategoryList(params: AppearanceFeatureCategoryListQuery) {
    const response = await getAppearanceFeatureCategoryListOriginal(params);
    // 处理不同的返回格式
    if (Array.isArray(response)) {
        return { list: response };
    } else if (response?.data) {
        const data = response.data;
        if (Array.isArray(data)) {
            return { list: data };
        } else if (data?.list) {
            return { list: data.list };
        }
    } else if (response?.list) {
        return { list: response.list };
    }
    return { list: [] };
}

// 重新导出其他函数和类型
export {
    getAppearanceFeatureInfo,
    createAppearanceFeature,
    updateAppearanceFeature,
    delAppearanceFeature,
    matchAppearanceFeature,
    batchMatchAppearanceFeature,
    getEnabledSeverityLevels,
    createSeverityLevel,
    saveAppearanceFeatureCorrection,
    addFeatureWithSeverity,
    addKeywordToFeature,
    createOrAddKeyword,
    addFeatureVariant,
    getAllAppearanceFeatureCategories,
    getAppearanceFeatureCategoryInfo,
    createAppearanceFeatureCategory,
    updateAppearanceFeatureCategory,
    delAppearanceFeatureCategory,
    type AppearanceFeatureInfo,
    type AppearanceFeatureListQuery,
    type AppearanceFeatureInput,
    type MatchInput,
    type BatchMatchInput,
    type MatchItemInput,
    type MatchItemOutput,
    type ManualCorrectionOption,
    type AppearanceFeatureCorrectionInput,
    type AddWithSeverityInput,
    type AddKeywordInput,
    type CreateOrAddKeywordInput,
    type CreateOrAddKeywordOutput,
    type AddFeatureVariantInput,
    type AppearanceFeatureCategoryInfo,
    type AppearanceFeatureCategoryInput,
    type AppearanceFeatureCategoryListQuery,
};
