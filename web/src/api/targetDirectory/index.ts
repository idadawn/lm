import { defHttp } from '/@/utils/http/axios';

// const server = import.meta.env.VITE_KPI_V1_SERVER + '/api/kpi/v1';
const server = '/api/kpi/v1';
// 分类就是目录
// 新建指标分类
export function postMetriccategory(data) {
  return defHttp.post({ url: server + '/metriccategory', data });
}
// 获取指标分类信息
export function getMetriccategoryList(data) {
  return defHttp.get({ url: server + '/metriccategory/list', data });
}
// 获取指标分类信息
export function getMetriccategory(id) {
  return defHttp.get({ url: server + '/metriccategory/' + id });
}
// 更新指标分类信息
export function putMetriccategory(data) {
  // console.log('data-------', data);
  let obj = {
    fullName: data.fullName,
    description: data.description,
    ownId: data.ownId,
    parentId: data.parentId,
  };
  return defHttp.put({ url: server + '/metriccategory/' + data.id, data: obj });
}
// 删除指标分类信息
export function deleteMetriccategory(id) {
  return defHttp.delete({ url: server + '/metriccategory/' + id });
}
//二级页面的图形参数
export function getChartsData(data) {
  return defHttp.post({ url: server + '/metric_data', data });
}
//根因分析的文字数据
export function getAnalysisData(data) {
  return defHttp.post({ url: server + `/metric/analysis/summary/${data}`, data: {} });
}
//根因分析的图表数据
export function getAnalysisResult(data) {
  return defHttp.post({ url: server + `/metric/analysis/result/${data}`, data: {} });
  // return defHttp.post({ url: server + '/metric/analysis/result/', data });
}
//创建分析数据
export function createAnalysis(data) {
  return defHttp.post({ url: server + '/metric/analysis/task', data });
}
// 获取派生指标详情
export function getDeriveInfo(id) {
  return defHttp.get({ url: server + '/metric-derive/' + id });
}
// 获取基础指标详情
export function getBasicInfo(id) {
  return defHttp.get({ url: server + '/metric/' + id });
}
// 获取复合指标详情
export function getCoppositeInfo(id) {
  return defHttp.get({ url: server + '/metric-composite/' + id });
}
