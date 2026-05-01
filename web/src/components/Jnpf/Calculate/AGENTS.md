<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# Calculate

## Purpose
`JnpfCalculate` 计算控件包装目录。运行时根据 `expression`（来自 FormGenerator 配置）动态求值并渲染为只读 `Input` 或纯文本，支持金额大写与千分位。

## Key Files
| File | Description |
|------|-------------|
| `index.ts` | barrel：导出 `JnpfCalculate` |

## Subdirectories
| Directory | Purpose |
|-----------|---------|
| `src/` | SFC 实现，集成 RPN 表达式求值（见 `src/AGENTS.md`） |

## For AI Agents

### Working in this directory
- 表达式求值入口位于 `FormGenerator/src/helper/utils`（`mergeNumberOfExps`/`toRPN`/`calcRPN`）；新增运算符要在那里同步。
- 仅维护导出，避免在此文件 import Vue 组件实例。

## Dependencies
### Internal
- `/@/utils` — `withInstall`
- `/@/components/FormGenerator/src/helper/utils`（由子组件使用）

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
