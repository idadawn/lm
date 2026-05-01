<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# IconPicker

## Purpose
`JnpfIconPicker` 图标选择器包装目录。弹窗形式展示 `ymIcon`/`ymCustom` 两套字体图标列表，支持搜索关键字过滤；选中后写回字体类名（如 `icon-ym-xxx`）。

## Key Files
| File | Description |
|------|-------------|
| `index.ts` | barrel：导出 `JnpfIconPicker` |

## Subdirectories
| Directory | Purpose |
|-----------|---------|
| `data/` | 图标数据 JSON（见 `data/AGENTS.md`） |
| `src/` | SFC 实现（见 `src/AGENTS.md`） |

## For AI Agents

### Working in this directory
- 图标数据来自 iconfont 项目（`JNPF-ym`、`JNPF-Custom`）；切换图标库时需同步替换 `data/` 与样式 `font-family`。
- 不要在 barrel 中加 i18n/工具逻辑。

## Dependencies
### Internal
- `/@/utils` — `withInstall`

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
