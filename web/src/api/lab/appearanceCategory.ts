import { defHttp } from '/@/utils/http/axios';

enum Api {
    Prefix = '/api/lab/appearance-feature-categories',
}

export interface AppearanceFeatureCategoryInfo {
    id: string;
    name: string;
    description?: string;
    sortCode?: number;
    featureCount?: number;
    creatorTime?: string;
    parentId?: string;
    rootId?: string; // 根分类ID（最顶层分类的ID）
    path?: string; // 分类路径（从根分类到当前分类的完整路径，用逗号分隔ID）
    children?: AppearanceFeatureCategoryInfo[];
    hasChildren?: boolean;
    isLeaf?: boolean;
}

export interface AppearanceFeatureCategoryInput {
    id?: string;
    name: string;
    description?: string;
    sortCode?: number;
    parentId?: string;
}

export interface AppearanceFeatureCategoryListQuery {
    keyword?: string;
    currentPage?: number;
    pageSize?: number;
}

// 获取列表
export function getAppearanceFeatureCategoryList(params: AppearanceFeatureCategoryListQuery) {
    return defHttp.get<AppearanceFeatureCategoryInfo[]>({ url: Api.Prefix, params });
}

// 获取所有大类（用于下拉选择）
export function getAllAppearanceFeatureCategories() {
    return defHttp.get<AppearanceFeatureCategoryInfo[]>({ url: Api.Prefix + '/all' });
}

// 获取详情
export function getAppearanceFeatureCategoryInfo(id: string) {
    return defHttp.get<AppearanceFeatureCategoryInfo>({ url: Api.Prefix + '/' + id });
}

// 新建
export function createAppearanceFeatureCategory(data: AppearanceFeatureCategoryInput) {
    return defHttp.post({ url: Api.Prefix, data });
}

// 修改
export function updateAppearanceFeatureCategory(data: AppearanceFeatureCategoryInput) {
    return defHttp.put({ url: Api.Prefix + '/' + data.id, data });
}

// 删除
export function delAppearanceFeatureCategory(id: string) {
    return defHttp.delete({ url: Api.Prefix + '/' + id });
}
