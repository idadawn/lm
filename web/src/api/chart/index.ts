import { defHttp } from '/@/utils/http/axios';
const mock = import.meta.env.VITE_MOCK_SERVER;
const dev = '';
const fastmock = 'https://www.fastmock.site/mock/a69a88ff42173df1cb56573550e37842/mock';
const Url = {
  getDashTreeList: `${dev}/api/kpi/v1/metricgot/list/dash`,
  createDash: `${dev}/api/kpi/v1/metricgot`,
  editDash: `${dev}/api/kpi/v1/metricgot/`,
  deleteDash: `${dev}/api/kpi/v1/metricgot/`,
  getLayout: `${dev}/api/kpi/v1/metricdash/`,
  saveLayout: `${dev}/api/kpi/v1/metricdash`,
  getChartData: `${fastmock}/api/visualdev/getChartData`,
  getFilterData: `${dev}/api/kpi/v1/metric-dimension/data/`,
  getDimensions: `${dev}/api/kpi/v1/metric-dimension/options`,
  getMetricsDimensions:`${fastmock}/api/getMetricsDimensions`,
  getMarkAreaData:`${fastmock}/api/getMarkAreaData`,
};


// 获取仪表盘列表
export function getDashTreeList(params: any): Promise<any> {
  return defHttp.post({ url: Url.getDashTreeList, params });
}

//新建仪表盘
export function createDash(params: any): Promise<any> {
  params.metricTag = params.metricTag.join(',');
  return defHttp.post({ url: Url.createDash, params });
}
// 编辑仪表盘
export function editDash(params: any): Promise<any> {
  params.metricTag = params.metricTag.join(',');
  return defHttp.put({ url: Url.editDash + params.id, params });
}
// 删除仪表盘
export function deleteDash(id: string): Promise<any> {
  return defHttp.delete({ url: Url.deleteDash + id });
}

// 存储仪表盘详情
export function saveLayout(params: any): Promise<any> {
  return defHttp.post({ url: Url.saveLayout, params });
}

// 获取仪表盘详情
export function getLayout(params: any): Promise<any> {
  return defHttp.get({ url: Url.getLayout + `${params.id}` });
}

export function getChartData(params: API.GetChartDataParams): Promise<API.GetWarningNodeElementsResult> {
  return defHttp.post({ url: Url.getChartData, params });
}

// 获取公共维度
export function getDimensions(): Promise<any> {
  return defHttp.get({ url: Url.getDimensions });
}
// 根据维度筛选器数据
export function getFilterData(id: string): Promise<API.GetFilterDataResult> {
  return defHttp.get({ url: Url.getFilterData + id });
}

// 根据指标查询维度
export function getMetricsDimensions(params: any): Promise<any> {
  return defHttp.post({ url: Url.getMetricsDimensions,params });
}

// 获取markArea数据
export function getMarkAreaData(params: any): Promise<any> {
  return defHttp.post({ url: Url.getMarkAreaData, params });
}



