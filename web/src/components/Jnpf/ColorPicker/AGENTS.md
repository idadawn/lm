<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# ColorPicker

## Purpose
`JnpfColorPicker` 颜色选择器包装目录。提供基于 `a-popover` 的 HSV/Alpha 调节面板与预定义色板，独立实现 `Color` 类与拖拽逻辑（不依赖第三方颜色库）。

## Key Files
| File | Description |
|------|-------------|
| `index.ts` | barrel：导出 `JnpfColorPicker` |

## Subdirectories
| Directory | Purpose |
|-----------|---------|
| `src/` | 主组件、子面板、`lib/` 颜色算法、`type/` 类型定义（见 `src/AGENTS.md`） |

## For AI Agents

### Working in this directory
- 内部 `lib/color.ts` 是手写实现，请勿替换为 `tinycolor`/`color` 等库以避免破坏 `predefine`/`alpha` 行为。
- 改动子面板（svPanel/hueSlider/alphaSlider）需保持通过 `provide/inject` 共享 `Color` 实例的契约。

## Dependencies
### Internal
- `/@/utils` — `withInstall`
### External
- `ant-design-vue`、`@ant-design/icons-vue`（在 src 中）

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
