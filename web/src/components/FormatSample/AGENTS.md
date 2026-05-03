<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# FormatSample

## Purpose
"编辑指标格式"模态组件 — 用于检测室数据/指标的数值显示格式编辑器。支持 `None / Number / Currency / Percentage` 四类格式，可设置小数位数、单位（万/亿）、货币符号 (¥ / $)、千分位分隔符。供 BI/指标编辑场景调用。

## Key Files
| File | Description |
|------|-------------|
| `index.ts` | 通过 `withInstall` 包装 `src/index.vue`，导出 `FormatSample` |

## Subdirectories
| Directory | Purpose |
|-----------|---------|
| `src/` | 模态组件实现与 props 定义（见 `src/AGENTS.md`） |

## For AI Agents

### Working in this directory
- 该组件是检测室业务专用的展示格式编辑器；UI 文案为中文（"编辑指标格式"、"小数位数"、"使用千分位分隔符"），新增字段需保持术语一致。
- 通过 `withInstall` 包装后既可作为单独组件 import，也可全局注册；不要绕过该工具直接导出 `.vue`。

### Common patterns
- vben-admin 风格：`index.ts` 桶文件 + `src/index.vue` 主体 + `src/props.ts` props。

## Dependencies
### Internal
- `/@/utils` (`withInstall`)
### External
- `ant-design-vue` (`Modal`)

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
