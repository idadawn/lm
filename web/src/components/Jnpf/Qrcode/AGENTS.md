<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# Qrcode

## Purpose
`JnpfQrcode` 表单二维码组件入口。支持 `static`（静态文本）/`relation`（取表单字段）/动态模型三种数据来源，最后一种会序列化包含模型/流程/任务 id 的 JSON（用于扫码定位单据）。

## Key Files
| File | Description |
|------|-------------|
| `index.ts` | `withInstall(Qrcode)` 后导出 `JnpfQrcode` |

## Subdirectories
| Directory | Purpose |
|-----------|---------|
| `src/` | SFC（见 `src/AGENTS.md`） |

## For AI Agents

### Working in this directory
- 内部使用 `/@/components/Qrcode`（项目通用 QrCode），不要直接引第三方 qrcode 库。
- 动态模型 JSON schema（`t/id/mid/mt/fid/pid/ftid/opt`）属于跨端协议——修改前需同步 `views/common/dynamicModel`。

### Common patterns
- 通过 `withInstall` 全局注册；`useDesign('qrcode')` 命名空间。

## Dependencies
### Internal
- `/@/components/Qrcode`、`/@/store/modules/generator`、`/@/hooks/web/useDesign`
### External
- 无（依赖项目内 QrCode 组件）

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
