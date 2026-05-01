<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# Input

## Purpose
文本输入控件包装目录，导出 `JnpfInput`（含密码态 `Input.Password`）与 `JnpfTextarea`，是 LIMS 表单生成器中最常用的基础控件。

## Key Files
| File | Description |
|------|-------------|
| `index.ts` | barrel：导出 `JnpfInput` 与 `JnpfTextarea` |

## Subdirectories
| Directory | Purpose |
|-----------|---------|
| `src/` | SFC 实现 + props（见 `src/AGENTS.md`） |

## For AI Agents

### Working in this directory
- 保留两个组件名导出，FormGenerator 配置项 (`JnpfInput`/`JnpfTextarea`) 与之绑定。
- 与 antd 升级保持兼容，prefixIcon/suffixIcon 通过插槽实现，避免破坏。

## Dependencies
### Internal
- `/@/utils` — `withInstall`

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
