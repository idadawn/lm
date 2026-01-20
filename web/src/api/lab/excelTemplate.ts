import { defHttp } from '/@/utils/http/axios';

enum Api {
    Prefix = '/api/lab/excel-templates',
}

// 获取模板列表
export function getExcelTemplates() {
    return defHttp.get({ url: Api.Prefix });
}

// 根据产品规格ID获取模板列表
export function getTemplatesByProductSpecId(productSpecId: string) {
    return defHttp.get({ url: Api.Prefix + '/by-product-spec/' + productSpecId });
}

// 获取默认模板
export function getDefaultTemplate() {
    return defHttp.get({ url: Api.Prefix + '/default' });
}

// 根据ID获取模板
export function getExcelTemplateById(id: string) {
    return defHttp.get({ url: Api.Prefix + '/' + id });
}

// 创建模板
export function createExcelTemplate(data: any) {
    return defHttp.post({ url: Api.Prefix, data });
}

// 更新模板
export function updateExcelTemplate(id: string, data: any) {
    return defHttp.put({ url: Api.Prefix + '/' + id, data });
}

// 删除模板
export function deleteExcelTemplate(id: string) {
    return defHttp.delete({ url: Api.Prefix + '/' + id });
}

// 设置默认模板
export function setTemplateAsDefault(id: string) {
    return defHttp.put({ url: Api.Prefix + '/' + id + '/set-default' });
}

// 验证模板配置
export function validateTemplateConfig(configJson: string) {
    return defHttp.post({ url: Api.Prefix + '/validate-config', data: { configJson } });
}

// 解析Excel表头
export function parseExcelHeaders(data: any) {
    return defHttp.post({ url: Api.Prefix + '/parse-headers', data });
}

// 获取系统默认字段
export function getSystemFields(templateCode: string) {
    return defHttp.get({ url: Api.Prefix + '/system-fields', params: { templateCode } });
}

// 验证Excel文件与模板配置
export function validateExcelAgainstTemplate(data: {
    templateCode: string;
    fileName: string;
    fileData: string;
}) {
    return defHttp.post({ url: Api.Prefix + '/validate-excel', data });
}