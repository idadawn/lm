import { defHttp } from '/@/utils/http/axios';

// const server = import.meta.env.VITE_KPI_V1_SERVER + '/api/kpi/v1';
const server = '/api/kpi/v1';

// 获取指标定义信息
export function getMetrickinship(id) {
  return defHttp.get({ url: server + '/metrickinship/' + id });
}

// 更新指标定义
export function putMetrickinship(data) {
  // console.log('data-------', data);
  // let obj = {
  //   fullName: data.fullName,
  //   description: data.description,
  //   ownId: data.ownId,
  //   parentId: data.parentId,
  // };
  return defHttp.put({ url: server + '/metrickinship/' + data.id, data });
}

// 删除指标定义
export function deleteMetrickinship(id) {
  return defHttp.delete({ url: server + '/metrickinship/' + id });
}

// 获取指标定义列表
export function postMetrickinshipList(data) {
  return defHttp.post({ url: server + '/metric/list', data });
}

// 新建指标定义
export function postMetrickinship(data) {
  return defHttp.post({ url: server + '/metrickinship', data });
}

// 获取标签
export function getMetrictagSelector() {
  return defHttp.get({ url: server + '/metrictag/selector' });
}

// -------------原子指标-----------------

// DB数据源列表
export function getMetricDB() {
  return defHttp.get({ url: server + '/metric/db' });
}
// DB-Scheme列表
export function getMetricSchema() {
  return defHttp.get({ url: server + '/metric/schema' });
}
// 获取Scheme信息
export function getMetricLinkIdSchemaSchemaName(data) {
  return defHttp.get({ url: server + '/metric/' + data.linkId + '/schema/' + data.schemaName });
}
// 获取筛选数据
export function postMetricFilter_model_data(data) {
  return defHttp.post({ url: server + '/metric/filter_model_data', data });
}
// 获取聚合方式
export function postMetricAgg_type(data) {
  return defHttp.post({ url: server + '/metric/agg_type', data });
}
// 当数据类型为实时数据时，获取列的信息调这个接口
export function getMetriRcreal_time(id) {
  return defHttp.get({ url: server + '/metric/real-time/' + id });
}
// 指标定义提交
export function postMetric(data) {
  return defHttp.post({ url: server + '/metric', data });
}
// 获取指标定义的详情信息
export function getMetric(id) {
  return defHttp.get({ url: server + '/metric/' + id });
}
// 更新指标定义
export function putMetric(id, data) {
  return defHttp.put({ url: server + '/metric/' + id, data });
}
// 删除指标定义
export function deleteMetric(data) {
  return defHttp.delete({ url: server + '/metric', data });
}
// 指标在线
export function putMetricOnline(data) {
  return defHttp.put({ url: server + '/metric/online', data });
}
// 指标下线
export function putMetricOffline(data) {
  return defHttp.put({ url: server + '/metric/offline', data });
}

// ---------派生指标----------

//获取衍生自
export function getMetricAllDerive() {
  return defHttp.get({ url: server + '/metric/all-derive' });
}
// 新建指标定义
export function postMetricDerive(data) {
  return defHttp.post({ url: server + '/metric-derive', data });
}
// 更新指标定义
export function putMetricDerive(id, data) {
  return defHttp.put({ url: server + '/metric-derive/' + id, data });
}
// 获取详情
export function getMetricDerive(id) {
  return defHttp.get({ url: server + '/metric-derive/' + id });
}

// ---------复合指标----------

// 获取所有指标信息
export function getMetricAll() {
  return defHttp.get({ url: server + '/metric/all' });
}
// 新建指标定义
export function postMetricComposite(data) {
  return defHttp.post({ url: server + '/metric-composite', data });
}
// 更新指标定义
export function putMetricComposite(id, data) {
  return defHttp.put({ url: server + '/metric-composite/' + id, data });
}
// 获取详情
export function getMetricComposite(id) {
  return defHttp.get({ url: server + '/metric-composite/' + id });
}
// 公式检查
export function postMetricCompositeFormulaCheck(data) {
  return defHttp.post({ url: server + '/metric-composite/formula-check', data });
}
// 获取所有维度信息
export function postMetricCompositeDims(data) {
  return defHttp.post({ url: server + '/metric-composite/dims', data });
}
export function postMetricDims(data) {
  return defHttp.post({ url: server + '/metric/dims', data });
}
// 筛选选择维度获取信息
export function postFilterMetricData(data) {
  return defHttp.post({ url: server + '/metric/filter_metric_data', data });
}

// 创建分级-状态
export function getMetricCovstatusOptions() {
  return defHttp.get({ url: server + '/metric-covstatus/options' });
}

// 克隆指标
export function postMetricCopy(id) {
  return defHttp.post({ url: server + '/metric/copy/' + id });
}

// ---------指标分级----------
// 获取指标分级信息
export function getMetricGraded(id) {
  return defHttp.get({ url: server + '/metric-graded/' + id });
}
// 更新指标分级
export function putMetricGraded(id, data) {
  return defHttp.put({ url: server + '/metric-graded/' + id, data });
}
// 删除指标分级
export function deleteMetricGraded(id) {
  return defHttp.delete({ url: server + '/metric-graded/' + id });
}
// 获取指标分级列表
export function postMetricGradedList(metricId) {
  return defHttp.post({ url: server + '/metric-graded/list/' + metricId });
}
// 新建指标分级
export function postMetricGraded(data) {
  return defHttp.post({ url: server + '/metric-graded', data });
}
