<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# Radio

## Purpose
`JnpfRadio` 单选组合件入口。包装 `a-radio-group`，支持 `default`（默认 radio）/`button`（按钮组）两种 `optionType`，及 `horizontal`/`vertical` 两种排布。`fieldNames` 默认按 `id`/`fullName`/`disabled` 取选项字段。

## Key Files
| File | Description |
|------|-------------|
| `index.ts` | `withInstall(Radio)` 后导出 `JnpfRadio` |

## Subdirectories
| Directory | Purpose |
|-----------|---------|
| `src/` | SFC + props（见 `src/AGENTS.md`） |

## For AI Agents

### Working in this directory
- 选项字段命名遵循后端约定（`id`/`fullName`），与 `Select`、`Organize` 系列保持一致；自定义需通过 `fieldNames` 传入。
- 不直接渲染 ant 原生 radio——通过 `JnpfRadio` 走全局类名 `jnpf-vertical-radio` 等。

### Common patterns
- `withInstall` 全局注册；样式 `jnpf-{direction}-radio` 类名由组件运行时拼接。

## Dependencies
### Internal
- `/@/utils`、`/@/hooks/core/useAttrs`
### External
- `ant-design-vue`

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
