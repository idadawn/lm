import { defHttp } from '/@/utils/http/axios';
const mock = import.meta.env.VITE_MOCK_SERVER;
const dev = '';
const Url = {
  getStatusList: `${dev}/api/kpi/v1/metric-covstatus/list`, //
  deleteStatus: `${dev}/api/kpi/v1/metric-covstatus/`, //
  getStatusOptionsList: `${dev}/api/kpi/v1/metric-covstatus/options`,
  addStatus: `${dev}/api/kpi/v1/metric-covstatus`,
  updateStatus: `${dev}/api/kpi/v1/metric-covstatus/`,
  getStatusDetail: `${dev}/api/kpi/v1/metric-covstatus/`,
};

// 获取列表
export function getStatusList(data: any): Promise<any> {
  return defHttp.post({ url: Url.getStatusList, data });
}
//删除列中的某一条
export function deleteStatus(id: string): Promise<any> {
  return defHttp.delete({ url: Url.deleteStatus + `${id}` });
}
// 获取价值链状态下拉选项
export function getStatusOptionsList(): Promise<any> {
  return defHttp.get({ url: Url.getStatusOptionsList });
}
//新建公共维度
export function addStatus(data: any): Promise<any> {
  return defHttp.post({ url: Url.addStatus, data });
}
//编辑公共维度
export function updateStatus(data: any): Promise<any> {
  return defHttp.put({ url: `${Url.updateStatus}${data.id}`, data });
}
//查询详情
export function getStatusDetail(id: string): Promise<any> {
  return defHttp.get({ url: `${Url.getStatusDetail}${id}` });
}
