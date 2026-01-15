import { defHttp } from '/@/utils/http/axios';

enum Api {
    Prefix = '/api/lab/public-dimensions',
}

// 获取公共维度列表
export function getPublicDimensions() {
    return defHttp.get({ url: Api.Prefix });
}

// 根据ID获取公共维度
export function getPublicDimensionById(id: string) {
    return defHttp.get({ url: Api.Prefix + '/' + id });
}

// 创建公共维度
export function createPublicDimension(data) {
    return defHttp.post({ url: Api.Prefix, data });
}

// 更新公共维度
export function updatePublicDimension(id: string, data) {
    return defHttp.put({ url: Api.Prefix + '/' + id, data });
}

// 删除公共维度
export function deletePublicDimension(id: string) {
    return defHttp.delete({ url: Api.Prefix + '/' + id });
}

// 获取当前版本号
export function getCurrentVersion(dimensionId: string) {
    return defHttp.get({ url: Api.Prefix + '/current-version', params: { dimensionId } });
}

// 创建新版本
export function createNewVersion(dimensionId: string, versionDescription?: string) {
    return defHttp.post({ 
        url: Api.Prefix + '/create-version', 
        params: { 
            dimensionId, 
            ...(versionDescription ? { versionDescription } : {})
        } 
    });
}

// 获取维度版本列表
export function getVersionList(dimensionId: string) {
    return defHttp.get({ url: Api.Prefix + '/version-list', params: { dimensionId } });
}
