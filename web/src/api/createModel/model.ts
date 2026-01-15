import { defHttp } from '/@/utils/http/axios';
const mock = import.meta.env.VITE_MOCK_SERVER;
const dev = '';
const Url = {
  getNodes: `${mock}/api/visualdev/getNodes`,
  getNodeElements: `${mock}/api/visualdev/getNodeElements`,
  getOptimalNodeElements: `${mock}/api/visualdev/getOptimalNodeElements`,
  getWarningNodeElements: `${mock}/api/visualdev/getWarningNodeElements`,
  getNoticeElements: `${mock}/api/visualdev/getNoticeElements`,
  getIndexDataElements: `${mock}/api/visualdev/getIndexDataElements`,
  getRulesElements: `${mock}/api/visualdev/getRulesElements`,

  // 问题树列表新增接口
  addIndicator: `${dev}/api/kpi/v1/metricgot`,
  // 问题树列表编辑接口
  putIndicator: `${dev}/api/kpi/v1/metricgot/`,
  // 获取指标思维图信息详情
  getMetricgotDetail: `${dev}/api/kpi/v1/metricgot/`,
  // 问题树列表删除接口
  deleteIndicator: `${dev}/api/kpi/v1/metricgot/`,
  // 获取问题树列表
  getIndicatorTreeList: `${dev}/api/kpi/v1/metricgot/list/cov`,
  // 获取指标价值链列表
  getIndicatorValueChainList: `${dev}/api/kpi/v1/metriccov/list/`,
  // 新建指标价值链
  addIndicatorValueChain: `${dev}/api/kpi/v1/metriccov`,
  // 更新指标价值链
  updateIndicatorValueChain: `${dev}/api/kpi/v1/metriccov/`,
  // 删除指标价值链
  deleteIndicatorValueChain: `${dev}/api/kpi/v1/metriccov/`,
  // 获取所有指标列表
  getAllIndicatorList: `${dev}/api/kpi/v1/metric/all`,
  // 获取指标值
  getMetricData: `${dev}/api/kpi/v1/metric_data/`,
  // 获取问题树标签下拉数据
  getTagSelectorList: `${dev}/api/kpi/v1/metrictag/selector`,
  ///获取指标链看板信息
  getTagMsg: `${dev}/api/kpi/v1/metriccov/kpi/`,
  // 获取指标价值链规则列表
  getMetriccovruleList: `${dev}/api/kpi/v1/metriccovrule/list/`,
  // 获取价值链状态所有选项
  getMetricCovstatusOptions: `${dev}/api/kpi/v1/metric-covstatus/options`,
  // 删除指标价值链规则
  deleteMetriccovrule: `${dev}/api/kpi/v1/metriccovrule/`,
  // 更新指标价值链规则
  putMetriccovrule: `${dev}/api/kpi/v1/metriccovrule/`,
  // 新建指标价值链规则
  postMetriccovrule: `${dev}/api/kpi/v1/metriccovrule`,
  // 获取通知模板
  getMetricNoticeTemplateList: `${dev}/api/kpi/v1/metric-notice/template/list`,
  // 获取通知列表
  getmetricNotice: `${dev}/api/kpi/v1/metric-notice/`,
  // 新建指标通知
  postMetricNotice: `${dev}/api/kpi/v1/metric-notice`,
  // 删除通知
  deleteMetricNotice: `${dev}/api/kpi/v1/metric-notice/`,
  // 获取指标数据
  postMetricData: `${dev}/api/kpi/v1/metric_data`,
  // 获取父节点列表
  getMetriccovSelector: `${dev}/api/kpi/v1/metriccov/selector`,
};

export function getNodes(data: API.GetNodesParams): Promise<API.GetNodesResult> {
  return defHttp.post({ url: Url.getNodes, data });
}
export function getNodeElements(data: API.GetNodeElementsParams): Promise<API.GetNodeElementsResult> {
  return defHttp.post({ url: Url.getNodeElements, data });
}
export function getOptimalNodeElements(
  data: API.GetOptimalNodeElementsParams,
): Promise<API.GetOptimalNodeElementsResult> {
  return defHttp.post({ url: Url.getOptimalNodeElements, data });
}
export function getWarningNodeElements(
  data: API.GetWarningNodeElementsParams,
): Promise<API.GetWarningNodeElementsResult> {
  return defHttp.post({ url: Url.getWarningNodeElements, data });
}
export function getNoticeElements(data: API.GetNoticeElementsParams): Promise<API.GetNoticeElementsResult> {
  return defHttp.post({ url: Url.getNoticeElements, data });
}
export function getIndexDataElements(data: API.GetNoticeElementsParams): Promise<API.GetNoticeElementsResult> {
  return defHttp.post({ url: Url.getIndexDataElements, data });
}
export function getRulesElements(data: API.GetNoticeElementsParams): Promise<API.GetNoticeElementsResult> {
  return defHttp.post({ url: Url.getRulesElements, data });
}

// 获取指标树列表
export function getIndicatorTreeList(data: any): Promise<any> {
  return defHttp.post({ url: Url.getIndicatorTreeList, data });
}

/**
 * @description 新增指标
 */
export function addIndicator(data: any): Promise<any> {
  return defHttp.post({ url: Url.addIndicator, data });
}

/**
 * @description 编辑指标
 */
export function putIndicator(data: any): Promise<any> {
  return defHttp.put({ url: `${Url.putIndicator}${data.id}`, data });
}

//删除指标列中的某一条
export function deleteIndicator(id: string): Promise<any> {
  return defHttp.delete({ url: Url.deleteIndicator + `${id}` });
}

// 获取指标价值链列表
export function getIndicatorValueChainList(id: string): Promise<any> {
  return defHttp.get({ url: Url.getIndicatorValueChainList + `${id}` });
}

// 新建指标价值链
export function addIndicatorValueChain(data: any): Promise<any> {
  return defHttp.post({ url: Url.addIndicatorValueChain, data });
}

// 更新指标价值链
export function updateIndicatorValueChain(data: any): Promise<any> {
  return defHttp.put({ url: Url.updateIndicatorValueChain + `${data.id}`, data });
}

// 删除指标价值链
export function deleteIndicatorValueChain(id: string): Promise<any> {
  return defHttp.delete({ url: Url.deleteIndicatorValueChain + `${id}` });
}

/**
 * 获取所有指标列表
 */
export function getAllIndicatorList(): Promise<any> {
  return defHttp.get({ url: Url.getAllIndicatorList });
}

/**
 * 获取指标值
 */
export function getMetricData<T>(id: T): Promise<any> {
  return defHttp.get({ url: `${Url.getMetricData}${id}` });
}

/**
 * @description 获取指标思维图信息详情
 */
export function getMetricgotDetail(id: string): Promise<any> {
  return defHttp.get({ url: `${Url.getMetricgotDetail}${id}` });
}

/**
 * @description 获取指标下拉数据
 */
export function getTagSelectorList(): Promise<any> {
  return defHttp.get({ url: Url.getTagSelectorList });
}
// 获取指标看板数据
export function getTagMsg(id: string): Promise<any> {
  return defHttp.get({ url: Url.getTagMsg + `${id}` });
}

/**
 * @description 获取指标价值链规则列表
 */
export function getMetriccovruleList(covId: string): Promise<any> {
  return defHttp.get({ url: `${Url.getMetriccovruleList}${covId}` });
}

/**
 * @description 获取价值链状态所有选项
 */
export function getMetricCovStatusOptions(): Promise<any> {
  return defHttp.get({ url: Url.getMetricCovstatusOptions });
}

/**
 * @description 删除指标价值链规则
 */
export function deleteMetriccovrule<T>(id: T): Promise<any> {
  return defHttp.delete({ url: Url.deleteMetriccovrule + `${id}` });
}

/**
 * @description 更新指标价值链规则
 */
export function putMetriccovrule(data: any): Promise<any> {
  return defHttp.put({ url: Url.putMetriccovrule + `${data.id}`, data });
}

/**
 * @description 新建指标价值链规则
 */
export function postMetriccovrule<T>(data: T): Promise<any> {
  return defHttp.post({ url: Url.postMetriccovrule, data });
}

/**
 * @description 获取通知模板
 * */
export function getMetricNoticeTemplateList(): Promise<any> {
  return defHttp.get({ url: `${Url.getMetricNoticeTemplateList}` });
}

/**
 * @description 获取通知列表
 * */
export function getmetricNotice<T>(data: T): Promise<any> {
  return defHttp.get({ url: `${Url.getmetricNotice}`, data });
}

/**
 * @description 新建指标通知
 * @description 新建规则通知
 */
export function postMetricNotice<T>(data: T): Promise<any> {
  return defHttp.post({ url: Url.postMetricNotice, data });
}

/**
 * @description 删除指标通知
 */
export function deleteMetricNotice<T>(id: T): Promise<any> {
  return defHttp.delete({ url: Url.deleteMetricNotice + `${id}` });
}

/**
 * @description 获取指标数据
 */
export function postMetricData<T>(data: T): Promise<any> {
  return defHttp.post({ url: Url.postMetricData, data });
}

/**
 * @description 获取父节点列表
 * */
export function getMetriccovSelector(): Promise<any> {
  return defHttp.get({ url: `${Url.getMetriccovSelector}` });
}
