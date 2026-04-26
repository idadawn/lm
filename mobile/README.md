# 检测室数据分析 - 移动端 App

基于 **uni-app Vue3** 开发的跨平台移动应用，支持 Android / iOS / H5 / 小程序。

> 注：最初按 uni-app-x 规划，为兼容 HBuilderX 稳定版与 MuMu 模拟器，已调整为标准 uni-app Vue3（`.vue` + `.js`），功能完全一致。

## 功能模块

| 模块 | 说明 |
|------|------|
| 登录 | 参考 Web 端登录页面，复用 `/api/oauth/Login` 接口，**origin 标识为 `app`**，密码使用 MD5 加密 |
| 生产驾驶舱 | 参考 `web/src/views/lab/monthly-dashboard/index.vue`，适配移动端布局，包含 KPI 卡片、质量分布、叠片系数趋势、不合格 Top5、班次对比、厚度-叠片系数关联 |
| 自动升级 | App 启动时检测新版本，支持 wgt/apk 热更新与整包升级，支持强制更新 |

## 项目结构

```
mobile/
├── manifest.json          # 应用配置（appid、版本、权限等）
├── pages.json             # 页面路由与全局样式
├── main.js                # 应用入口（Vue3 + uni-app）
├── App.vue                # 根组件（启动时检查登录态与自动升级）
├── uni.scss               # 全局 SCSS 变量
├── pages/
│   ├── login/
│   │   └── login.vue      # 登录页
│   └── dashboard/
│       └── dashboard.vue  # 生产驾驶舱
├── api/
│   ├── user.js            # 用户相关 API（登录、用户信息、配置）
│   └── dashboard.js       # Dashboard 数据 API
├── utils/
│   ├── http.js            # HTTP 请求封装（Token 拦截、401 跳转）
│   ├── storage.js         # 本地存储封装（Token、用户信息）
│   ├── date.js            # 日期格式化工具
│   ├── md5.js             # MD5 加密（与 Web 端对齐）
│   └── update.js          # 自动升级检测与安装
└── static/
    └── logo.png           # App Logo（请替换为正式图片）
```

## 后端接口复用说明

| 功能 | 接口 | 备注 |
|------|------|------|
| 登录 | `POST /api/oauth/Login` | form-urlencoded，新增 `origin=app` 字段 |
| 获取用户信息 | `GET /api/oauth/CurrentUser` | 复用 Web 端接口 |
| 退出登录 | `GET /api/oauth/Logout` | 复用 Web 端接口 |
| 登录配置 | `GET /api/oauth/getConfig/{account}` | 复用 Web 端接口 |
| 月度质量报表 | `GET /api/lab/monthly-quality-report` | 复用 Web 端接口 |
| 叠片系数趋势 | `GET /api/lab/dashboard/lamination-trend` | 复用 Web 端接口 |
| 厚度-叠片系数关联 | `GET /api/lab/dashboard/thickness-correlation` | 复用 Web 端接口 |
| 今日产量 | `GET /api/lab/dashboard/daily-production` | 复用 Web 端接口 |
| 版本检查 | `GET /api/app/version` | **需后端新增**（见下方说明） |

## 配置项

### 1. API 基础地址

修改 `utils/http.js` 中的 `API_BASE_URL`：

```js
const API_BASE_URL = 'http://192.168.1.100:10089'
```

### 2. App Logo

替换 `static/logo.png` 为正式应用图标。

### 3. manifest.json

- 修改 `appid` 为 DCloud 开发者中心申请的正式 AppID
- 修改 `versionName` / `versionCode` 控制版本号（与自动升级逻辑关联）
- 根据需要在 `app-plus.distribute` 中配置 Android / iOS 证书、包名等

## 自动升级 - 后端接口约定

后端需提供 `GET /api/app/version` 接口，返回示例：

```json
{
  "code": 200,
  "data": {
    "hasUpdate": true,
    "latestVersion": "1.1.0",
    "downloadUrl": "https://your-domain.com/app/__UNI__LM2026.wgt",
    "forceUpdate": false,
    "updateLog": "修复已知问题，优化性能"
  },
  "msg": "success"
}
```

| 字段 | 说明 |
|------|------|
| `hasUpdate` | 是否有新版本 |
| `latestVersion` | 最新版本号（展示用） |
| `downloadUrl` | 升级包下载地址（`.wgt` 热更新包或 `.apk` 整包） |
| `forceUpdate` | 是否强制更新（`true` 时用户无法取消） |
| `updateLog` | 更新日志 |

## 运行方式

### HBuilderX（推荐）

1. 安装 [HBuilderX](https://www.dcloud.io/hbuilderx.html)（3.7+ 版本）
2. 在 HBuilderX 中打开 `mobile/` 目录
3. **运行到 MuMu 模拟器**：
   - 菜单 **运行 → 运行到手机或模拟器**
   - 如果列表中没有 MuMu，先执行 adb 连接：
     ```bash
     adb connect 127.0.0.1:7555   # MuMu 12 默认端口
     # 或
     adb connect 127.0.0.1:22471  # MuMu 6 默认端口
     ```
   - 重新打开 HBuilderX 的运行菜单，选择 MuMu 模拟器
4. 首次运行会自动安装调试基座，稍等 1-3 分钟即可

### CLI（可选）

```bash
cd mobile
# 如需 H5 预览
npm run dev:h5
# 如需 App
npm run dev:app
```

## 注意事项

1. **App 端标识**：登录时固定传入 `origin: 'app'`，后端可据此区分 Web 端与 App 端登录，做差异化处理（如不同的 token 有效期、权限控制等）。
2. **Canvas 图表**：Dashboard 中的趋势图与散点图使用原生 Canvas 绘制，无需引入第三方图表库，减少包体积。
3. **安全区域**：页面已适配 iPhone 底部安全区（`env(safe-area-inset-bottom)`）。
4. **MD5 加密**：`utils/md5.js` 为纯 JS 实现，与 Web 端 `crypto-js/md5` 结果一致，保证登录密码加密方式相同。
