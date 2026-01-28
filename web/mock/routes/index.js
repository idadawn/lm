/**
 * 路由配置模块.
 * @module routes
 */

const Router = require('koa-router');
const router = new Router();
const helper = require('../helper');
const responses = require('./response.json');

// 第一种方式
responses.forEach(item => {
  router[item.method.toLowerCase()](item.url, async ctx => {
    await helper.sleep(item.delay || 0);
    ctx.body = item.response;
  });
});

// 第二种方式
const configCommon = require('./common');
configCommon(router);

const configCreateModel = require('./createModel');
configCreateModel(router);

const configChart = require('./chart');
configChart(router);

// router.post(url.upload, async (ctx) => {
//   const file = ctx.request.files.file;
//   const readStream = fs.createReadStream(file.path);
//   const filePath = path.resolve(__dirname, `../static/uploads/${file.name}`);
//   
//   const writeStream = fs.createWriteStream(filePath);
//   readStream.pipe(writeStream);
//   ctx.body = {
//     code: '200',
//     msg: '上传成功',
//   };
// });

module.exports = router;
