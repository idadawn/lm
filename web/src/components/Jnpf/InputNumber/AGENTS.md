<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# InputNumber

## Purpose
`JnpfInputNumber` 数值输入包装目录。在 `a-input-number` 基础上叠加 LIMS 业务能力：千分位 (`thousands`)、金额大写 (`isAmountChinese`)、只读详情态 (`detailed`)。

## Key Files
| File | Description |
|------|-------------|
| `index.ts` | barrel：导出 `JnpfInputNumber` |

## Subdirectories
| Directory | Purpose |
|-----------|---------|
| `src/` | SFC 实现（见 `src/AGENTS.md`） |

## For AI Agents

### Working in this directory
- 仅维护导出；金额格式化逻辑放在 `/@/utils/jnpf` 内供组件调用。

## Dependencies
### Internal
- `/@/utils` — `withInstall`

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
