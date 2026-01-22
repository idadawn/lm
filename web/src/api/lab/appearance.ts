import { defHttp } from '/@/utils/http/axios';
import { getEnabledSeverityLevels as getEnabledSeverityLevelsFromApi, type SeverityLevelInfo } from './severityLevel';

enum Api {
    Prefix = '/api/lab/appearance-features',
    Match = '/api/lab/appearance-features/match',
}

// Model interfaces

export interface AppearanceFeatureInfo {
    id: string;
    categoryId: string;            // 特性大类ID（数据库存储）
    category?: string;             // 特性大类名称（前端展示）
    name: string;                  // 特性名称
    severityLevelId: string;       // 特性等级ID（数据库存储）
    severityLevel?: string;        // 特性等级名称（前端展示）
    keywords?: string;             // 关键词列表（JSON数组或逗号分隔）
    description?: string;
    sortCode?: number;
    creatorTime?: string;
    variantList?: Array<{          // 特性等级变体列表
        name: string;
        severity?: string;
        keywords?: string;
    }>;
    // 匹配结果信息
    matchMethod?: string;          // 匹配方式（name/keyword/ai/fuzzy）
    degreeWord?: string;           // 识别到的程度词
    requiresSeverityConfirmation?: boolean;  // 是否需要确认等级
    suggestedSeverity?: string;    // 建议的等级名称
    categoryPath?: string;         // 分类路径（从顶级分类到当前分类，用➡️分隔，如："韧性➡️脆"）
    rootCategory?: string;         // 最顶级的分类名称（用于前端展示特征大类）
    featureExists?: boolean;        // 是否存在该特性（名称+等级组合）
    existingFeatureId?: string;   // 已存在的特性ID（当FeatureExists为true时）
    suggestedFeatureName?: string; // 建议的特性名称（用于创建新特性，如"严重脆"）
    severityLevelExists?: boolean;  // 建议的等级是否存在（如果不存在，需要先创建等级）
    matchedVariant?: {             // 匹配到的变体
        name: string;
        severity?: string;
    };
}

export interface AppearanceFeatureListQuery {
    keyword?: string;
    categoryId?: string;
    name?: string;
    severityLevelId?: string;
    currentPage?: number;
    pageSize?: number;
}

export interface AppearanceFeatureInput {
    id?: string;
    categoryId: string;
    name: string;
    severityLevelId: string;
    description?: string;
    keywords?: string;
    sortCode?: number;
}

export interface MatchInput {
    text: string;
}

// ============== 批量匹配相关接口 ==============

/** 批量匹配输入项 */
export interface MatchItemInput {
    id: string;      // 唯一标识
    query: string;   // 输入文字
}

/** 批量匹配输入 */
export interface BatchMatchInput {
    items: MatchItemInput[];
}

/** 人工修正选项（需要用户确认） */
export interface ManualCorrectionOption {
    featureId: string;        // 特性ID
    category: string;         // 特性大类
    categoryPath: string;     // 特性分类路径
    featureName: string;      // 特性名称
    severityLevel: string;    // 特性等级
    actionType: 'add_keyword' | 'select_existing';  // 建议操作类型
    suggestion: string;       // 建议说明
    correctionId?: string;    // 关联的修正记录ID
}

/** 匹配结果项 */
export interface MatchItemOutput {
    id: string;               // 唯一标识（对应输入）
    query: string;            // 输入文字（对应输入）
    category?: string;        // 特性大类
    categoryPath?: string;    // 特性分类路径
    featureName?: string;     // 特性名称
    severityLevel?: string;   // 特性等级
    matchMethod: 'name' | 'keyword' | 'ai' | 'none';  // 匹配方式
    isPerfectMatch: boolean;  // 是否100%匹配
    manualCorrections?: ManualCorrectionOption[];    // 人工修正列表（需用户确认）
}

/** 批量匹配（AI匹配结果需要用户确认后才能添加关键词） */
export function batchMatchAppearanceFeature(data: BatchMatchInput) {
    return defHttp.post<MatchItemOutput[]>({ url: Api.Prefix + '/batch-match', data });
}

// ============================================

// 获取列表
export function getAppearanceFeatureList(params: AppearanceFeatureListQuery) {
    return defHttp.get<AppearanceFeatureInfo[]>({ url: Api.Prefix, params });
}

// 获取详情
export function getAppearanceFeatureInfo(id: string) {
    return defHttp.get<AppearanceFeatureInfo>({ url: Api.Prefix + '/' + id });
}

// 新建
export function createAppearanceFeature(data: AppearanceFeatureInput) {
    return defHttp.post({ url: Api.Prefix, data });
}

// 修改
export function updateAppearanceFeature(data: AppearanceFeatureInput) {
    return defHttp.put({ url: Api.Prefix + '/' + data.id, data });
}

// 删除
export function delAppearanceFeature(id: string) {
    return defHttp.delete({ url: Api.Prefix + '/' + id });
}

// 语义匹配
export function matchAppearanceFeature(data: MatchInput) {
    return defHttp.post<AppearanceFeatureInfo[]>({ url: Api.Match, data });
}

// 获取启用的严重程度等级列表（用于下拉选择）
// 注意：此函数已移至 severityLevel.ts，保留此导出以保持向后兼容
export function getEnabledSeverityLevels() {
    return defHttp.get<SeverityLevelInfo[]>({
        url: '/api/lab/appearance-feature-levels/enabled'
    });
}

// 新增严重程度等级
export function createSeverityLevel(data: { name: string; description: string; sortCode?: number; enabled?: boolean }) {
    return defHttp.post({ url: '/api/lab/appearance-feature-levels', data });
}

// 保存人工修正记录
export interface AppearanceFeatureCorrectionInput {
    inputText: string;
    autoMatchedFeatureId?: string;
    correctedFeatureId: string;
    matchMode: 'auto' | 'manual' | 'create';
    scenario: 'test' | 'import';
    remark?: string;
}

export function saveAppearanceFeatureCorrection(data: AppearanceFeatureCorrectionInput) {
    return defHttp.post({ url: Api.Prefix + '/save-correction', data });
}

// 添加带等级的特性
export interface AddWithSeverityInput {
    categoryId: string;
    name: string;
    severityLevelId: string;
    description?: string;
    keywords?: string;
    sortCode?: number;
}

export function addFeatureWithSeverity(data: AddWithSeverityInput) {
    return defHttp.post<AppearanceFeatureInfo>({ url: Api.Prefix + '/add-with-severity', data });
}

// 添加关键字到特性
export interface AddKeywordInput {
    featureId: string;
    keyword: string;
}

export function addKeywordToFeature(data: AddKeywordInput) {
    return defHttp.post({ url: Api.Prefix + '/add-keyword', data });
}

// 创建或添加关键词（智能处理）
export interface CreateOrAddKeywordInput {
    inputText: string;              // 输入文本（如"严重脆"）
    autoMatchedFeatureId?: string;  // 自动匹配的特征ID（如果有）
    categoryId: string;            // 特性大类ID
    featureName: string;           // 特性名称（如"脆"）
    severityLevelName: string;      // 特性等级名称（如"严重"）
    description?: string;           // 描述（可选）
    sortCode?: number;             // 排序码（可选）
    scenario?: string;             // 使用场景（test/import）
}

export interface CreateOrAddKeywordOutput {
    feature: AppearanceFeatureInfo;
    action: string;                 // 执行的操作（create/add_keyword）
    message: string;                // 操作结果消息
}

export function createOrAddKeyword(data: CreateOrAddKeywordInput) {
    return defHttp.post<CreateOrAddKeywordOutput>({ url: Api.Prefix + '/create-or-add-keyword', data });
}

// 为现有特性添加新等级变体
export interface AddFeatureVariantInput {
    severity: string;  // 严重程度名称
    name?: string;     // 特性名称（可选，如果未提供则使用原特性名称）
}

export async function addFeatureVariant(featureId: string, data: AddFeatureVariantInput) {
    // 获取现有特性信息
    const feature = await getAppearanceFeatureInfo(featureId);

    // 获取所有启用的严重程度等级
    const severityLevelsRes = await getEnabledSeverityLevelsFromApi();
    const severityLevels = Array.isArray(severityLevelsRes) ? severityLevelsRes : (severityLevelsRes as any)?.data || [];
    const severityLevel = severityLevels.find((level: SeverityLevelInfo) => level.name === data.severity);

    if (!severityLevel) {
        throw new Error(`未找到严重程度等级 "${data.severity}"`);
    }

    // 使用现有特性的信息创建新变体
    return addFeatureWithSeverity({
        categoryId: feature.categoryId,
        name: data.name || feature.name,
        severityLevelId: severityLevel.id,
        description: feature.description,
        keywords: feature.keywords,
        sortCode: feature.sortCode,
    });
}
