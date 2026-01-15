# venus-web

## 浏览器支持

本地开发推荐使用`Chrome 90+` 浏览器

支持现代浏览器, 不支持 IE

| [<img src="https://raw.githubusercontent.com/alrra/browser-logos/master/src/edge/edge_48x48.png" alt=" Edge" width="24px" height="24px" />](http://godban.github.io/browsers-support-badges/)IE | [<img src="https://raw.githubusercontent.com/alrra/browser-logos/master/src/edge/edge_48x48.png" alt=" Edge" width="24px" height="24px" />](http://godban.github.io/browsers-support-badges/)Edge | [<img src="https://raw.githubusercontent.com/alrra/browser-logos/master/src/firefox/firefox_48x48.png" alt="Firefox" width="24px" height="24px" />](http://godban.github.io/browsers-support-badges/)Firefox | [<img src="https://raw.githubusercontent.com/alrra/browser-logos/master/src/chrome/chrome_48x48.png" alt="Chrome" width="24px" height="24px" />](http://godban.github.io/browsers-support-badges/)Chrome | [<img src="https://raw.githubusercontent.com/alrra/browser-logos/master/src/safari/safari_48x48.png" alt="Safari" width="24px" height="24px" />](http://godban.github.io/browsers-support-badges/)Safari |
| :-: | :-: | :-: | :-: | :-: |
| not support | last 2 versions | last 2 versions | last 2 versions | last 2 versions |

## 环境要求

- Node.js （版本要求 Node 16.15.0 版本以上）
- pnpm （版本要求 pnpm 8.1.0 版本以上）

## 使用说明--

### 安装 pnpm

```bash
npm install pnpm -g
```

> 若已经安装 pnpm ，可跳过此步骤

> 建议使用 pnpm，如果使用 yarn,请用 Yarn1.x 版本，否则依赖可能安装不上。

### 安装依赖

```bash
pnpm install
```

### 开发环境

- 运行前端项目(目前后端服务没有部署，需要本地运行 mock 才可以打开页面，账号密码随便填)

```bash
pnpm dev
```

- 运行 mock 项目

```bash
cd mock
npm i
npm run mock
```

- 配置接口地址 `.env.development`

```bash
VITE_PROXY = [["/dev","http://localhost:30000"]]
VITE_GLOB_WEBSOCKET_URL='ws://localhost:30000'
```

> 说明：把`localhost:30000` 换成自己地址。

### 生产发布

```bash
# 构建生产环境，对应.env.production文件配置
pnpm build
```

### 命名规范

- 组件命名：大驼峰，如：`UserList`
- 变量命名：小驼峰，如：`userList`
- vue 文件命名：除 index.vue 外，其余大驼峰，如：`UserList`
- ts 文件命名：小驼峰 class 命名：蛇形，如：`user-list-page`

禁止写行内样式

### git 提交注释规范

- type 类型可以根据具体情况进行适当的增删改，下面是一些常用的 type 类型：

- （1）feat：新功/修改（feature）。
- （2）fix：修复 bug。
- （3）docs：文档（documentation）修改。
- （4）style：格式（不影响代码运行的变动）。
- （5）refactor：重构（即不是新增功能，也不是修改 bug 的代码变动）。
- （6）perf：优化相关，比如提升性能、体验。
- （7）test：增加测试。
- （8）chore：构建过程或辅助工具的变动。
- （9）revert：回滚到上一个版本。
- （10）merge：代码合并。
- （11）sync：同步主线或分支的 Bug。
- （12）remove: 移除功能或模块。
-  (13) clean: 清理代码