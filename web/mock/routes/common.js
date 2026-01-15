const common = require('../data/common');
const chart = require('../data/chart');
const url = require('./url');
module.exports = router => {
  router.get(url.getLoginConfig, async ctx => {
    ctx.body = common.getLoginConfig;
  });
  router.get(url.admin, async ctx => {
    ctx.body = common.admin;
  });
  router.post(url.Login, async ctx => {
    ctx.body = common.Login;
  });
  router.get(url.CurrentUser, async ctx => {
    ctx.body = common.CurrentUser;
  });
  router.post(url.updatePasswordMessage, async ctx => {
    ctx.body = common.updatePasswordMessage;
  });
  router.get(url.SysConfig, async ctx => {
    ctx.body = common.SysConfig;
  });
  router.get(url.All, async ctx => {
    ctx.body = common.All;
  });
  router.get(url.Base, async ctx => {
    ctx.body = common.Base;
  });

  router.get(url.message, async ctx => {
    ctx.body = common.message;
  });
  router.get(url.imreply, async ctx => {
    ctx.body = common.imreply;
  });

  router.post(url.getNodes, async ctx => {
    ctx.body = common.getNodes;
  });

  router.post(url.configTemplate, async ctx => {
    ctx.body = common.configTemplate;
  });

  router.post(url.getNodeElements, async ctx => {
    ctx.body = common.getNodeElements;
  });
  router.post(url.getOptimalNodeElements, async ctx => {
    ctx.body = common.getOptimalNodeElements;
  });
  router.post(url.getWarningNodeElements, async ctx => {
    ctx.body = common.getWarningNodeElements;
  });
  router.post(url.getIndexDataElements, async ctx => {
    ctx.body = common.getIndexDataElements;
  });
  router.post(url.getNoticeElements, async ctx => {
    ctx.body = common.getNoticeElements;
  });
  router.post(url.getRulesElements, async ctx => {
    ctx.body = common.getRulesElements;
  });
  router.get(url.getWarningTableList, async ctx => {
    ctx.body = common.getWarningTableList;
  });
  router.get(url.getChartsDataList, async ctx => {
    ctx.body = common.getChartsDataList;
  });
  router.get(url.getHomeChartsDataList, async ctx => {
    ctx.body = common.getHomeChartsDataList;
  });
  //获得指标分析的charts数据
  router.get(url.getAnalysisChartsData, async ctx => {
    ctx.body = common.getAnalysisChartsData;
  });
  //获得指标分析的charts数据---正式格式
  router.get(url.getChartsFormatData, async ctx => {
    ctx.body = common.getChartsFormatData;
  });
  router.get(url.getMetricSchema, async ctx => {
    ctx.body = common.getMetricSchema;
  });
};
