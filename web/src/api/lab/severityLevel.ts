import { defHttp } from '/@/utils/http/axios';

enum Api {
    Prefix = '/api/lab/appearance-feature-levels',
    Enabled = '/api/lab/appearance-feature-levels/enabled',
}

// Model interfaces
export interface SeverityLevelInfo {
    id: string;
    name: string;
    description?: string;
    sortCode?: number;
    enabled: boolean;
    isDefault?: boolean;
    creatorTime?: string;
}

export interface SeverityLevelListQuery {
    keyword?: string;
    enabled?: boolean;
    currentPage?: number;
    pageSize?: number;
}

export interface SeverityLevelInput {
    id?: string;
    name: string;
    description?: string;
    enabled?: boolean;
    isDefault?: boolean;
    sortCode?: number;
}

// 获取列表
export function getSeverityLevelList(params: SeverityLevelListQuery) {
    return defHttp.get<SeverityLevelInfo[]>({ url: Api.Prefix, params });
}

// 获取详情
export function getSeverityLevelInfo(id: string) {
    return defHttp.get<SeverityLevelInfo>({ url: Api.Prefix + '/' + id });
}

// 新建
export function createSeverityLevel(data: SeverityLevelInput) {
    return defHttp.post({ url: Api.Prefix, data });
}

// 修改
export function updateSeverityLevel(data: SeverityLevelInput) {
    return defHttp.put({ url: Api.Prefix + '/' + data.id, data });
}

// 删除
export function delSeverityLevel(id: string) {
    return defHttp.delete({ url: Api.Prefix + '/' + id });
}

// 获取所有启用的等级
export function getEnabledSeverityLevels() {
    return defHttp.get<SeverityLevelInfo[]>({ url: Api.Enabled });
}
