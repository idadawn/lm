import { defHttp } from '/@/utils/http/axios';
const dev = '';
const Url = {
  getDimensionList: `${dev}/api/kpi/v1/metric-dimension/list`, //公共维度列表接口
  deleteDimension: `${dev}/api/kpi/v1/metric-dimension/`, //公共维度删除接口
  getDimensionOptionsList: `${dev}/api/kpi/v1/metric-composite/dims`,
  addDimension: `${dev}/api/kpi/v1/metric-dimension`,
  updateDimension: `${dev}/api/kpi/v1/metric-dimension/`,
  getMetricSchema: `${dev}/api/kpi/v1/metric/schema`,
  getDimensionDetail: `${dev}/api/kpi/v1/metric-dimension/`,
};

// 获取列表
export function getDimensionList(data: any): Promise<any> {
  return defHttp.post({ url: Url.getDimensionList, data });
}
//删除列中的某一条
export function deleteDimension(id: string): Promise<any> {
  return defHttp.delete({ url: Url.deleteDimension + `${id}` });
}

// 获取列表
export function getDimensionOptionsList(data: any): Promise<any> {
  return defHttp.post({ url: Url.getDimensionOptionsList, data });
}
//获取数据源选项
export function getMetricSchema() {
  return defHttp.get({ url: Url.getMetricSchema });
}
//新建公共维度
export function addDimension(data: any): Promise<any> {
  return defHttp.post({ url: Url.addDimension, data });
}
//编辑公共维度
export function updateDimension(data: any): Promise<any> {
  return defHttp.put({ url: `${Url.updateDimension}${data.id}`, data });
}
//查询详情
export function getDimensionDetail(id: string): Promise<any> {
  return defHttp.get({ url: `${Url.getDimensionDetail}${id}` });
}
