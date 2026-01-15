const chart = require('../data/chart');
const url = require('./url');
module.exports = router => {
  router.post(url.getLayout, async ctx => {
    const res = {
      code: 200,
      msg: '操作成功',
      data: {
        code: 200,
        data: JSON.stringify(chart.getLayout.data.data),
      },
    };
    ctx.body = res;
  });
  router.post(url.getChartData, async ctx => {
    ctx.body = chart.getMetricChart;
  });
  router.post(url.getFilterData, async ctx => {
    ctx.body = chart.getFilterData;
  });
  router.post(url.getDimensions, async ctx => {
    ctx.body = chart.getDimensions;
  });
  router.post(url.getDashTreeList, async ctx => {
    ctx.body = chart.getDashTreeList;
  });
  router.post(url.getAllIndicatorList, async ctx => {
    ctx.body = chart.getAllIndicatorList;
  });
};
