# Android 打包与蒲公英上传指南

## 环境准备

### 1. 安装 HBuilderX

uni-app Android 打包依赖 **HBuilderX**。

- 下载地址：https://www.dcloud.io/hbuilderx.html
- 解压到任意目录即可（如 `C:\HBuilderX`）
- 在 HBuilderX 中登录 DCloud 账号（云打包需要）

### 2. 获取蒲公英 API Key

1. 前往 https://www.pgyer.com 注册/登录
2. 进入「账号设置」->「API 信息」
3. 复制 `API Key`

---

## 脚本说明

| 脚本 | 用途 |
|------|------|
| `build-android.ps1` | 打包向导：检查环境、打开项目、查找 APK、调用上传 |
| `upload-to-pgyer.ps1` | 上传 APK 到蒲公英 |

---

## 使用方式

### 方式一：全自动向导（推荐）

在 **PowerShell** 中执行：

```powershell
cd mobile/scripts
.\build-android.ps1 -PgyerApiKey "你的APIKey" -Description "v1.0.0 正式版"
```

脚本会：
1. 自动查找 HBuilderX
2. 在 HBuilderX 中打开项目
3. 提示你在 HBuilderX 中点击【发行】->【原生App-云打包】
4. 自动查找最新生成的 APK
5. 上传到蒲公英并输出下载链接

### 方式二：先手动打包，再单独上传

**步骤 1：手动打包**

1. 用 HBuilderX 打开 `mobile` 目录
2. 点击菜单【发行】->【原生App-云打包】
3. 选择 Android，勾选「使用公共测试证书」
4. 点击【打包】，等待完成
5. 记录 APK 路径（通常在 `mobile/unpackage/release/apk/*.apk`）

**步骤 2：上传蒲公英**

```powershell
cd mobile/scripts
.\upload-to-pgyer.ps1 `
  -ApkPath "..\unpackage\release\apk\__UNI__LM2026.apk" `
  -ApiKey "你的APIKey" `
  -Description "v1.0.0 正式版"
```

---

## 常见问题

### 1. 脚本提示未找到 HBuilderX

将 HBuilderX 解压到以下任一位置：
- `C:\HBuilderX`
- `D:\HBuilderX`
- `C:\Program Files\HBuilderX`

或把 `HBuilderX` 目录添加到系统环境变量 `PATH` 中。

### 2. 云打包提示未登录

打开 HBuilderX，点击右上角登录 DCloud 账号（支持微信扫码）。

### 3. 上传失败

- 检查 API Key 是否正确
- 检查 APK 文件是否存在且大小正常（应大于 1MB）
- 检查网络是否能访问 https://www.pgyer.com

### 4. 如何配置自己的签名证书（正式版）

在 HBuilderX 中：
1. 打开 `mobile/manifest.json`
2. 切换到「源码视图」
3. 在 `app-plus -> distribute -> android` 下添加：
   ```json
   {
     "keystore": "证书路径",
     "password": "证书密码",
     "aliasname": "证书别名"
   }
   ```
4. 云打包时选择「使用自由证书」
