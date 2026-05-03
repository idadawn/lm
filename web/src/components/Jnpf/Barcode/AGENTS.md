<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# Barcode

## Purpose
`JnpfBarcode` 条形码组件包装目录，导出经过 `withInstall` 注册的全局组件。用于检测室表单、报告中按业务字段生成 Code128 等格式条码。

## Key Files
| File | Description |
|------|-------------|
| `index.ts` | barrel：从 `./src/Barcode.vue` 导出 `JnpfBarcode` |

## Subdirectories
| Directory | Purpose |
|-----------|---------|
| `src/` | 组件实现（基于 `jsbarcode`）（见 `src/AGENTS.md`） |

## For AI Agents

### Working in this directory
- 仅维护 barrel 导出与目录结构；新增子组件时统一加 `withInstall` 包装并由本文件导出。
- 不要把 `jsbarcode` 等运行时依赖泄漏到 `index.ts`，保留在 `src/` 内。

### Common patterns
- 与 Jnpf 系列其它包装组件一致：`index.ts` 极简 + `src/` 内含 `.vue` 实现。

## Dependencies
### Internal
- `/@/utils` — `withInstall`
### External
- `jsbarcode`（在 `src/Barcode.vue` 中使用）

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
