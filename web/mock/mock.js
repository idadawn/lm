/**
 * 模拟后台服务器
 */
const Koa = require('koa');
const bodyParser = require('koa-body');
const ip = require('ip');
const router = require('./routes');
const app = new Koa();
const path = require('path');
const serve = require('koa-static');

app.use(serve(path.resolve(__dirname, 'static')));
const iosCordovaPath = path.resolve(
  __dirname,
  '../src-cordova/platforms/ios/platform_www',
  // "../src-cordova/platforms/android/platform_www"
);
app.use(serve(iosCordovaPath));

app.use(
  bodyParser({
    multipart: true,
  }),
);
app.use(async (ctx, next) => {
  const start = new Date();
  ctx.set({
    // 允许所有的域访问
    // "Access-Control-Allow-Origin": ctx.headers["referer"]
    //   ? ctx.headers["referer"].slice(0, -1)
    //   : "*",
    'Access-Control-Allow-Origin': '*',
    // "Access-Control-Allow-Credentials": true,
    'Access-Control-Allow-Methods': 'GET, POST, PUT,DELETE',
    // 根据需要添加头信息
    'Access-Control-Allow-Headers': ' Origin, X-Requested-With,Authorization,Content-Type, Accept,csrf-token,token',
  });
  try {
    await next();
    // token过期
    // ctx.body.code ='E1021'
    // 手机号未注册
    // ctx.body.code ='E1004'
    // 未设置初始密码
    // ctx.body.code ='E1009'

    const ms = new Date() - start;
    ctx.set({
      'X-Response-Time': `${ms}ms`,
    });
    console.log(`${ctx.url} ${ctx.method} ${ms}`);
  } catch (err) {
    console.log('捕获了程序的异常', new Date(), err);
  }
});

// 配置路由
app.use(router.routes());

app.use(router.allowedMethods());
app.listen(19003, () => {
  console.log(`mock 服务器已开启\nhttp://${ip.address()}:19003\nhttp://localhost:19003`);
});
