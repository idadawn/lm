import { defHttp } from '/@/utils/http/axios';

export interface ReportConfig {
    id: string;
    name: string;
    levelNames: string[];
    isSystem: boolean;
    sortOrder: number;
    description: string;
    isHeader: boolean;
    isPercentage: boolean;
    isShowInReport: boolean;
    isShowRatio: boolean;
    formulaId: string;
}

export interface ReportConfigInput {
    id?: string;
    name: string;
    levelNames: string[];
    sortOrder: number;
    description?: string;
}

enum Api {
    GetList = '/api/lab/report-config',
    Add = '/api/lab/report-config',
    Update = '/api/lab/report-config',
    Delete = '/api/lab/report-config',
}

/**
 * 获取配置列表
 */
export const getReportConfigList = () => {
    return defHttp.get<ReportConfig[]>({ url: Api.GetList });
};

/**
 * 添加配置
 */
export const addReportConfig = (params: ReportConfigInput) => {
    return defHttp.post({ url: Api.Add, params });
};

/**
 * 更新配置
 */
export const updateReportConfig = (params: ReportConfigInput) => {
    return defHttp.put({ url: Api.Update, params });
};

/**
 * 删除配置
 */
export const deleteReportConfig = (id: string) => {
    return defHttp.delete({ url: `${Api.Delete}/${id}` });
};
