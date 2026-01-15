/**
 * 所有的路径集合
 */
module.exports = {
  getLoginConfig: `/api/oauth/getLoginConfig`,
  admin: `/api/oauth/getConfig/admin`,
  Login: `/api/oauth/Login`,
  CurrentUser: `/api/oauth/CurrentUser`,
  updatePasswordMessage: `/api/oauth/updatePasswordMessage`,
  SysConfig: `/api/system/SysConfig`,
  All: `/system/DictionaryData/All`,
  Base: `/api/visualdev/Base`,
  message: `/api/message`,
  imreply: `/message/imreply`,
  // 获取nodes节点，用于拖拽: `/api/visualdev/getNodes`,
  getNodes: `/api/visualdev/getNodes`,
  getNodeElements: `/api/visualdev/getNodeElements`,
  getOptimalNodeElements: `/api/visualdev/getOptimalNodeElements`,
  getWarningNodeElements: `/api/visualdev/getWarningNodeElements`,
  getNoticeElements: `/api/visualdev/getNoticeElements`,
  getIndexDataElements: `/api/visualdev/getIndexDataElements`,
  getRulesElements: `/api/visualdev/getRulesElements`,
  //预警table表格的获取
  getWarningTableList: `/api/extend/TableExample`,
  //charts图表
  getChartsDataList: `/api/extend/getChartsDataList`,
  //charts图表
  getHomeChartsDataList: `/api/extend/getHomeChartsDataList`,
  //归因分析charts图表
  getAnalysisChartsData: `/api/extend/getAnalysisChartsData`,
  //正式格式
  getChartsFormatData: `/api/extend/getChartsFormatData`,

  configTemplate: `/collector/configTemplate`,

  getLayout: `/api/kpi/v1/metricdash/:id`,
  getChartData: `/api/visualdev/getChartData`,
  getFilterData: `/api/visualdev/getFilterData`,
  getDimensions: `/api/kpi/v1/metric-dimension/list`,
  getDashTreeList: `/api/kpi/v1/metricgot/list/dash`,
  getAllIndicatorList: `/api/kpi/v1/metric/all`,

  // 问题树
  getIndicatorTreeList: `/api/kpi/v1/metricgot/list/cov`,
  // 获取指标价值链列表
  getIndicatorValueChainList: `/api/kpi/v1/metriccov/list/:id`,
  // 获取所有指标信息
  getAllIndicatorList: '/api/kpi/v1/metric/all',
  getMetricSchema: 'api/kpi/v1/metric/schema',
};
