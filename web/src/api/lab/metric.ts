import { defHttp } from '/@/utils/http/axios';
import { MetricDefinition, MetricDefinitionQuery, MetricDefinitionInput } from './types/metric';

enum Api {
    Base = '/api/lab/metric-definitions',
}

export const getMetricDefinitionList = (params: MetricDefinitionQuery) => {
    return defHttp.get<{ list: MetricDefinition[]; total: number }>({ url: Api.Base, params });
};

export const getMetricDefinition = (id: string) => {
    return defHttp.get<MetricDefinition>({ url: `${Api.Base}/${id}` });
};

export const createMetricDefinition = (params: MetricDefinitionInput) => {
    return defHttp.post<string>({ url: Api.Base, params });
};

export const updateMetricDefinition = (id: string, params: MetricDefinitionInput) => {
    return defHttp.put<void>({ url: `${Api.Base}/${id}`, params });
};

export const deleteMetricDefinition = (id: string) => {
    return defHttp.delete<void>({ url: `${Api.Base}/${id}` });
};

// Update Status
export const updateMetricStatus = (id: string, status: number) => {
    return defHttp.put<void>({ url: `${Api.Base}/${id}/status`, data: { status } });
};
