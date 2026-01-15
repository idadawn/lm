import { defHttp } from '/@/utils/http/axios';

enum Api {
    Prefix = '/api/lab/product-specs',
}

// 获取列表
export function getProductSpecList(data) {
    return defHttp.get({ url: Api.Prefix, data });
}

// 获取详情
export function getProductSpecInfo(id) {
    return defHttp.get({ url: Api.Prefix + '/' + id });
}

// 新建
export function createProductSpec(data) {
    return defHttp.post({ url: Api.Prefix, data });
}

// 修改
export function updateProductSpec(data) {
    return defHttp.put({ url: Api.Prefix + '/' + data.id, data });
}

// 删除
export function delProductSpec(id) {
    return defHttp.delete({ url: Api.Prefix + '/' + id });
}

// 获取版本列表
export function getProductSpecVersionList(productSpecId) {
    return defHttp.get({ url: '/api/lab/product-spec-versions/version-list?productSpecId=' + productSpecId });
}

// 获取指定版本的扩展属性
export function getProductSpecVersionAttributes(params) {
    return defHttp.get({ url: '/api/lab/product-spec-versions/attributes-by-version', params });
}

// 对比两个版本的差异
export function compareProductSpecVersions(params) {
    return defHttp.get({ url: '/api/lab/product-spec-versions/compare', params });
}
