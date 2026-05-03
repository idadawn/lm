<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# scripts

## Purpose
mobile 端构建与发布的 PowerShell 自动化脚本。封装 HBuilderX CLI 云打包 Android APK 与蒲公英（Pgyer）分发上传，把"打包→等待→上传→发版本说明"串成一条命令。

## Key Files
| File | Description |
|------|-------------|
| `build-android.ps1` | 一键云打包：检查 HBuilderX → CLI 触发原生云打包 → 轮询等待 APK → 调用 `upload-to-pgyer.ps1` |
| `upload-to-pgyer.ps1` | 单独上传脚本：通过 Pgyer API v2 上传 APK，支持 `InstallType`（公开/密码/邀请）、密码、版本号 |

## For AI Agents

### Working in this directory
- 必需 PowerShell 5.1+；`#Requires -Version 5.1` 已声明，不要降级。
- API Key 通过参数 `-PgyerApiKey` 传入或读取同目录 `.pgyer-config.ps1`（`.gitignore`），切勿硬编码到脚本中。
- 默认云打包等待 8 分钟，可通过 `-MaxWaitMinutes` 调整；调试 CI 流水线时建议设为 12+。
- APK 输出路径与 HBuilderX 约定一致：`<ProjectRoot>/unpackage/release/android/`。
- 与 `mobile/utils/update-pgyer.js`、`update.js` 的 `PGYER_CONFIG` 共用同一应用 Key，发版后客户端自动检测更新。

### Common patterns
- `$ErrorActionPreference = "Stop"` + `Write-Host -ForegroundColor` 彩色阶段日志。
- `param(...)` 强校验输入，`[ValidateSet(...)]` 限定枚举字段。

## Dependencies
### Internal
- `mobile/manifest.json`、`mobile/unpackage/`（HBuilderX 产物）
- `mobile/utils/update-pgyer.js`（运行时检测同一渠道）

### External
- HBuilderX CLI、Pgyer Open API v2

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
