# 鸿蒙(HarmonyOS) 本地打包 + AppGallery Connect 上传

两个脚本，对齐 Android 的 `build-android.ps1` / `upload-to-pgyer.ps1` 风格：

| 脚本 | 作用 |
|------|------|
| `build-harmony.ps1` | 调 HBuilderX CLI 走完整链：编译 Vue3→ArkTS、在 `unpackage/dist/build/app-harmony` 生成 DevEco 工程、由 hvigorw 构建签名出 `.app`/`.hap` |
| `upload-to-agc.ps1` | 用 AGC 发布 API（v3）把 `.app`/`.hap` 上传到 AppGallery Connect |

> ⚠️ **诚实声明**：作者无法在本机验证鸿蒙打包链与 AGC 上传链。脚本基于华为/DCloud 官方文档编写，关键处（cli 鸿蒙 pack 子命令语法、产物落点、AGC 响应字段路径）做了**强校验 + 失败兜底提示**，但**首次使用务必逐步观察输出**，遇到字段/命令不符按提示调整。

---

## 为什么鸿蒙不走蒲公英

蒲公英只分发 `.apk`/`.ipa`，**不支持鸿蒙 `.hap`/`.app`**。鸿蒙必须走**华为 AppGallery Connect（AGC）**——这也是 App 内「检查更新」对鸿蒙引导去华为应用市场的原因。

---

## 一、一次性前置（脚本无法替代）

### 1. 工具链
- 安装 **DevEco Studio ≥ 5.1.0.849** 或 **HarmonyOS Command Line Tools**（华为开发者网站，需华为开发者账号；CLT 下载目前需中国区账号）。
- 在 **HBuilderX** [工具]→[设置]→[运行配置] 里配置 **鸿蒙 DevTools 路径** 指向上面的安装目录。
- 可选：在 `scripts/.env` 设 `DEVECO_HOME=安装目录`，方便 `build-harmony.ps1` 探测告警。

### 2. 签名证书
- 当前 `manifest.json` 的 `app-harmony.distribute.signingConfigs` 只有 **debug 的 `default`** 证书（HBuilderX 自动申请的调试证书，可本地安装调试）。
- **正式发布**需在 `signingConfigs` 增配 `release`：`.p12`/`.cer`/`.p7b` 需在 AGC 申请；`storePassword`/`keyPassword` 在 DevEco 工程的 `build-profile.json5` 里是**加密格式**，不能填明文。
- HBuilderX 4.61+ 打包时会把 manifest 的签名配置自动同步进 DevEco 工程的 `build-profile.json5`。

### 3. AGC 控制台（无 API 替代）
1. 创建 **HarmonyOS 应用**，`bundleName` 必须 = `com.emergen.lm`（与 manifest `app-harmony.distribute.bundleName` 一致，否则包解析失败）。
   > 注意：Android 包名是 `cn.emergen.lm`，鸿蒙是 `com.emergen.lm`，**两者不同是正常的**，建 AGC 应用时用 `com.emergen.lm`。
2. 拿到 **appId**（应用信息页）。
3. [用户与访问]→[API 客户端] 创建 **client_id / client_secret**：创建时 **Project 保持 N/A**（否则 403），至少 **运营人员(Operator)** 角色；secret 仅一次性显示，及时保存。
4. **首次正式发布**前在控制台填齐元数据（截图、隐私政策 URL、分类、版权等），否则 `app-submit` 报信息不完整。

### 4. 填写 `scripts/.env`
（`.env` 已被 `.gitignore` 忽略，勿提交；模板见 `.env.example`）
```
AGC_CLIENT_ID=你的clientId
AGC_CLIENT_SECRET=你的clientSecret
AGC_APP_ID=你的appId
AGC_REGION=cn            # cn/dre/dra/drru
AGC_RELEASE_TYPE=6       # 1=正式(需审核) / 6=测试(更快)
AGC_SUBMIT=false         # 仅上传不提审；true 才自动提交
HARMONY_DESCRIPTION=本次更新说明
DEVECO_HOME=             # 选填，DevEco/CLT 安装目录
```

---

## 二、使用

```powershell
cd D:\project\lm\mobile\scripts

# 1) 本地打包（HBuilderX CLI 全链）
.\build-harmony.ps1
#   产物在 ..\unpackage\dist\build\app-harmony\ 下（优先 .app）
#   工程已生成后只想重编：.\build-harmony.ps1 -UseHvigor

# 2) 上传到 AGC（默认仅上传+绑定，不提审）
.\upload-to-agc.ps1
#   提交测试发布：.\upload-to-agc.ps1 -ReleaseType 6 -Submit
#   指定包：      .\upload-to-agc.ps1 -HapPath "..\unpackage\dist\build\app-harmony\xxx.app"
```

打包失败时，`build-harmony.ps1` 会提示 GUI 兜底：HBuilderX [发行]→[原生App-本地打包]→鸿蒙，或用 DevEco 打开 `unpackage/dist/build/app-harmony` 后 Build → Build App(s)，出包后再单独跑 `upload-to-agc.ps1`。

---

## 三、关键限制与注意

- **`.app` vs `.hap`**：AGC 正式渠道只接收 `.app`（多 HAP 聚合包）。单 `.hap` 仅适合测试通道/本地安装。需 `.app` 时用 `hvigorw assembleApp` 或 HBuilderX 出包。
- **上传链时序**：upload-url 预签名地址仅约 **5 分钟**有效（脚本取址后立即上传）；绑定后包**异步解析约 2 分钟**（脚本 `-Submit` 时 sleep 120s 再提交）。
- **objectId 字段路径**：华为各版本文档对 upload-url 响应里 `objectId` 的位置有出入。脚本会**先打印整段响应**再多路径回退取值；若绑定失败，照打印的结构改 `upload-to-agc.ps1` 里的取值路径。
- **审核周期**：`ReleaseType=1` 正式发布需华为人工审核（约 1-7 工作日）；`ReleaseType=6` 测试通道更快，适合内测自检。
- **路径敏感**：鸿蒙工具链对路径含中文/超长（Windows 建议总路径 < 110 字符）敏感，构建失败优先排查。
- **密钥安全**：`AGC_CLIENT_SECRET` 等同发布权限，只放 `.env`（已忽略），勿提交、勿外泄。

---

## 参考（官方文档）

- uni-app 鸿蒙运行与发行：https://uniapp.dcloud.net.cn/tutorial/harmony/runbuild.html
- hvigorw 命令行构建：https://developer.huawei.com/consumer/cn/doc/harmonyos-guides/ide-command-line-building-app
- AGC 鉴权（client_credentials）：https://developer.huawei.com/consumer/cn/doc/app/agc-help-connect-api-obtain-server-auth-0000002271134661
- AGC 获取上传地址：https://developer.huawei.com/consumer/cn/doc/app/agc-help-upload-api-upload-url-0000002236201294
- AGC 绑定包信息(v3)：https://developer.huawei.com/consumer/cn/doc/app/agc-help-publish-api-app-package-info-update-0000002236201250
- AGC 提交发布(v3)：https://developer.huawei.com/consumer/cn/doc/app/agc-help-publish-api-app-submit-0000002271160585
