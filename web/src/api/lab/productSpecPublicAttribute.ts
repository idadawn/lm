import { defHttp } from '/@/utils/http/axios';

enum Api {
    Prefix = '/api/lab/product-spec-public-attributes',
}

// 获取公共属性列表
export function getPublicAttributes() {
    return defHttp.get({ url: Api.Prefix });
}

// 创建公共属性
export function createPublicAttribute(data) {
    return defHttp.post({ url: Api.Prefix, data });
}

// 更新公共属性
export function updatePublicAttribute(id, data) {
    return defHttp.put({ url: Api.Prefix + '/' + id, data });
}

// 删除公共属性
export function deletePublicAttribute(id) {
    return defHttp.delete({ url: Api.Prefix + '/' + id });
}

// 将公共属性应用到所有产品
export function applyPublicAttributeToAllProducts(publicAttributeId) {
    return defHttp.post({ url: Api.Prefix + '/apply-to-all/' + publicAttributeId });
}
