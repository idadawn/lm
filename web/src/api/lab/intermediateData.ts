import { defHttp } from '/@/utils/http/axios';

enum Api {
    Prefix = '/api/lab/intermediate-data',
}

// 获取中间数据列表
export function getIntermediateDataList(params) {
    return defHttp.get({ url: Api.Prefix + '/list', params });
}

// 获取中间数据详情
export function getIntermediateDataInfo(id: string) {
    return defHttp.get({ url: Api.Prefix + '/' + id });
}

// 从原始数据生成中间数据
export function generateIntermediateData(data) {
    return defHttp.post({ url: Api.Prefix + '/generate', data });
}

// 更新性能数据
export function updatePerformance(data) {
    return defHttp.put({ url: Api.Prefix + '/performance', data });
}

// 更新外观特性
export function updateAppearance(data) {
    return defHttp.put({ url: Api.Prefix + '/appearance', data });
}

// 更新基础信息
export function updateBaseInfo(data) {
    return defHttp.put({ url: Api.Prefix + '/base-info', data });
}

// 删除中间数据
export function deleteIntermediateData(id: string) {
    return defHttp.delete({ url: Api.Prefix + '/' + id });
}

// 批量删除中间数据
export function batchDeleteIntermediateData(ids: string[]) {
    return defHttp.delete({ url: Api.Prefix + '/batch', data: ids });
}

// 获取产品规格选项
export function getProductSpecOptions() {
    return defHttp.get({ url: Api.Prefix + '/product-spec-options' });
}
