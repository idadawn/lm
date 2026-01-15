const createModel = require('../data/createModel');
const url = require('./url');
module.exports = router => {
  router.post(url.getIndicatorTreeList, async ctx => {
    ctx.body = createModel.getIndicatorTreeList;
  });
  router.get(url.getIndicatorValueChainList, async ctx => {
    ctx.body = createModel.getIndicatorValueChainList;
  });
  router.get(url.getAllIndicatorList, async ctx => {
    ctx.body = createModel.getAllIndicatorList;
  });
};
