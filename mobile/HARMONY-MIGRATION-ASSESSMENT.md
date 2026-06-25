# 原生鸿蒙（HarmonyOS Next）迁移评估 — 检测室数据分析移动端

> 评估对象：`mobile/`（经典 uni-app · Vue3 · JS）→ 鸿蒙 Next。
> 结论基于代码实读 + DCloud 官方文档核实（2026-06，HBuilderX 5.07 / DevEco 26）。
> 本机已就绪：HBuilderX 5.07（含 launcher-harmony/uts-development-harmony 插件）、DevEco Studio 26.0.0。

## 结论速览

- **鸿蒙有两条路，先分清楚：**
  - **路径 A（过渡发版，低风险）：经典 uni-app 直接编译鸿蒙 `.hap`（WebView/JS 运行时）。** 业务代码近乎零改动，`@qiun/ucharts`、`mp-html`、`marked`、SSE 客户端、`v-html` **全部原样复用**。约 **1–3 人周**（主要是 manifest 鸿蒙配置、`plus.*` OTA 替换、签名上架）。代价：WebView 渲染，首屏/滚动性能弱于原生。
  - **路径 B（真·原生 ArkTS，你选的方向）：迁移到 uni-app x（uvue/uts），编译为 HarmonyOS 原生。** 经核实**没有硬阻塞**（canvas、rich-text、流式请求在鸿蒙均已支持），但本质是**换引擎 + 换语言的大改**，约 **14–24 人周**。
- **建议：先走 A 上架鸿蒙，再按页面价值用 B 渐进原生化（混合方案）。** 对话页/图表页等性能敏感处逐步换 uvue 原生，其余留 WebView。避免一次性投入 14–24 人周却阻塞发版。
- **重要修正：** 早先的自动评估因调研 agent 掉线，把图表/富文本/SSE 误判为"硬阻塞"。实际 uni-app x 在鸿蒙侧均有对应能力（见下表），它们是"高工作量重写"，不是"做不到"。

## 两条路径对比

| 维度 | 路径 A：经典 uni-app → .hap | 路径 B：uni-app x → 原生 ArkTS |
|---|---|---|
| 渲染 | WebView / JS 运行时 | 原生 ArkUI/ArkTS（uvue），无 DOM/WebView |
| 语言 | 现有 vue + js 不变 | 改写为 uvue + uts |
| 业务改动量 | 近乎零 | 大（见下两节） |
| 现有依赖复用 | ucharts/mp-html/marked/dompurify/SSE **全可用** | 均需替换为原生实现 |
| 工具链门槛 | HBuilderX 4.27+ | HBuilderX 4.61+、DevEco、鸿蒙 API 14+ |
| 性能 | 中（WebView） | 高（接近原生） |
| 工作量 | 1–3 人周 | 14–24 人周 |
| 适合时机 | 现在就要上架鸿蒙 | 对性能/原生体验有明确诉求时 |

## uni-app x 原生能力现状（已核实，破除"硬阻塞"误判）

| 能力 | 鸿蒙支持情况 | 来源 |
|---|---|---|
| Canvas | `canvas` 组件（W3C 语法，全平台；鸿蒙用其原生 canvas 实现）+ `DrawableContext`（App 端，鸿蒙自 4.61） | uni-app x canvas / drawablecontext 文档 |
| 富文本 | `rich-text` 组件，uni-app x 4.7+ 三端 App（含鸿蒙）统一用 web-view 实现渲染 HTML | uni-app x rich-text 文档 |
| 流式请求(SSE) | `uni.request` 自 HBuilderX 5.07 在 Web+App（含鸿蒙）支持 `onHeadersReceived`/`onChunkReceived`；官方 AI 流式范式：POST + `responseType:'arraybuffer'` + `onChunkReceived` + `TextDecoder` | uni-app x request 文档 / 5.07 release notes |
| 鸿蒙原生句柄 | `getHarmonyAbility()` 取 `UIAbility`（4.61） | uni-app x get-app 文档 |

> 结论：图表、Markdown、SSE 在 uni-app x + 鸿蒙下都有落地路径，故路径 B "可行但贵"。

## 路径 B 迁移改造点与工作量

| 项 | 当前实现（代码） | uni-app x 对应能力 | 改造内容 | 人周 |
|---|---|---|---|---|
| 图表 | `components/u-chart/u-chart.vue` 用 `createCanvasContext`+`ctx.draw`（canvas2d:false）；`pages/index/index.vue` 641/727 手绘 2D canvas；`@qiun/ucharts` | uvue `canvas`/`DrawableContext` | 换 uni-app x 兼容图表库或基于原生 canvas 重写；`chat-chart-bubble.vue` 的纯 flex 柱状是可复用范式 | 3–5 |
| Markdown/富文本 | `chat.vue` 的 `v-html`+`mp-html`，`utils/markdown.js` 走 `marked`+`dompurify` | `rich-text`（web-view 渲染 HTML）或内嵌 `web-view` | `marked.lexer` → rich-text 节点；或对话页整页暂留 web-view | 3–5 |
| SSE 客户端（核心） | `utils/sse-client.js`：非 H5 用 `uni.request({enableChunked})`+`onChunkReceived`，H5 用 `fetch`+`ReadableStream`，带整包回落 | `uni.request`+`onChunkReceived`（5.07，含鸿蒙） | 按官方 arraybuffer+TextDecoder 范式重写，保持 `onText/onChart/onReasoningStep` 回调契约不变 | 2–4 |
| OTA 自更新 | `utils/update.js` 依赖 `plus.runtime.install/restart/quit`、`plus.downloader` | uvue 无 `plus.*` | 改为"应用市场版本检测 + 引导跳转"，放弃 `plus.runtime.install` 强装 | 1–2 |
| 受限 CSS | grid（index.vue 938/1239/493）、`@keyframes`（5 文件 20 处）、`::before/::after`（kg-reasoning-chain 117-142）、`linear-gradient`、`100vh`/`env()` | uvue 仅 flex 子集 + 原生 transition/animation | grid→flex；伪元素→真实 `view`；动画→uvue 动画 API；渐变/vh/env→纯色+安全区 API | 2–3 |
| Options API 组件 | `u-chart`、`kg-reasoning-chain`、`chat-chart-bubble`、`kg-demo`、`App.vue` | uvue 组件 | 改写为 uvue；`u-chart` 随图表一并重写 | 2–3 |
| 打包/签名/上架/真机回归 | — | — | 证书、权限映射、灰度 | 1–2 |
| **合计（整体原生迁移）** | | | | **约 14–24 人周** |

> `package.json` 的 `patch-marked.cjs`（postinstall 改写 node_modules）在路径 B 下随 `marked` 一并废弃。

## 分阶段迁移建议

- **阶段 0 · 过渡发版（1–3 人周，立即可做）：** 不动渲染栈，经典 uni-app 编译 App-Harmony `.hap` 先上架鸿蒙。仅处理打包必需项（见下节）。
- **阶段 1 · 原生地基与试点（2–4 人周）：** 搭 uni-app x 工程，先迁 1–2 个纯 flex 简单页（`profile`/`mine`）跑通工具链；同步做两项硬验证 ——① `onChunkReceived` 在 uvue 鸿蒙真机的流式表现；② uvue canvas 绘图边界。**设硬 gate：验证不过则停在路径 A，不进入大规模重写。**
- **阶段 2 · 图表原生化（3–5 人周）：** 以 `chat-chart-bubble` flex 范式建原生图表组件，替换 `u-chart`/手绘 canvas。
- **阶段 3 · Markdown 原生化（3–5 人周，最难，关键路径）：** 自写 `marked.lexer → rich-text 节点`，支持流式增量，对话页从 web-view 切原生。
- **阶段 4 · 收尾（2–4 人周）：** 剩余 Options API 组件、受限 CSS、uts 工具层、OTA 改造，全量真机回归与灰度。

## 立即可做：App-Harmony 过渡发版（路径 A）

**前置条件**
- HBuilderX 4.27+（本机 5.07 ✅）；DevEco + 鸿蒙 SDK（本机 DevEco 26 ✅）；华为 AGC 签名证书（`.p12` + Profile）与开发者账号。

**manifest 鸿蒙配置（当前缺失）**
- 现 `manifest.json` 只有 `app-plus`（Android/iOS），**无鸿蒙块**。需在 HBuilderX manifest 界面新增 App-Harmony 配置：
  - 鸿蒙 Bundle Name（鸿蒙侧通常独立申请，对齐 `cn.emergen.lm` 命名）；版本映射 `1.3.3 / 133`；签名证书与 Profile。
  - 权限映射：现有 Android 权限（`INTERNET`/`ACCESS_NETWORK_STATE`/`RECORD_AUDIO`/读写存储，manifest 27-32 行）→ 鸿蒙 `ohos.permission.*`，写入 `harmony-configs/entry/src/main/module.json5` 的 `requestPermissions`。
  - 图标/启动页：补 `unpackage/res/icons` 的鸿蒙所需尺寸。
- 打包：【发行】→【App-Harmony 本地打包】→ 产物在 `unpackage/dist/build/harmony/`（`app.hap`/`app-release.hap`/`app-release.zip`）。

**必须实测/替换的运行时能力**
- **OTA 自更新（`update.js`）：** `plus.runtime.install` 在鸿蒙受市场政策限制，过渡期降级为"版本检测 + 引导应用市场更新"。
- **分发渠道：** `.hap` 一般通过**华为应用市场（AGC）** 或自建服务器分发，**蒲公英主要面向 APK/IPA，对 .hap 支持有限**——鸿蒙包不要指望沿用现在的蒲公英流程，需走 AGC 或自托管。
- **音频/录音：** `RECORD_AUDIO` + `speech` 需映射鸿蒙对应 API/权限。
- **图表/Markdown/SSE：** WebView 方案下 `@qiun/ucharts`、`mp-html`、`marked`、`dompurify`、`sse-client.js` **零改动复用**——这是路径 A 相对原生迁移的最大优势。

## 关键不确定性与下一步验证

1. `onChunkReceived` 在 uvue 鸿蒙真机的流式稳定性（NLQ 对话核心）——阶段 1 必验。
2. uvue canvas 对复杂图表交互/动画的还原度——阶段 1 必验。
3. App-Harmony 下 `plus.*` 的可用子集与 `enableChunked` 行为——以目标 HBuilderX 真机实测为准。
4. `.hap` 的灰度/内测分发方案（AGC 内测 vs 自托管）——影响过渡期测试效率。

## 参考来源

- uni-app x · Harmony 开发指南：https://doc.dcloud.net.cn/uni-app-x/app-harmony/
- uni-app x · canvas / DrawableContext：https://doc.dcloud.net.cn/uni-app-x/component/canvas.html
- uni-app x · rich-text：https://doc.dcloud.net.cn/uni-app-x/component/rich-text.html
- uni-app x · request（onChunkReceived）：https://doc.dcloud.net.cn/uni-app-x/api/request.html
- 经典 uni-app · 鸿蒙运行和发行：https://uniapp.dcloud.net.cn/tutorial/harmony/runbuild.html
- HBuilderX 5.07 release notes（onChunkReceived）：https://uniapp.dcloud.io/release

> 局限：本评估的代码审计为实读，鸿蒙原生 API 与 App-Harmony 打包细节以官方文档为准、部分需真机复核；自动评估流程中"运行时 API 审计"与"能力调研"两个 agent 曾因网络中断失败，相关结论已由本文档据官方文档补齐并修正。
