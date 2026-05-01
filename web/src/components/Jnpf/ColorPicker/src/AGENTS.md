<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# src

## Purpose
`JnpfColorPicker` 的核心实现：基于 `a-popover` 的颜色面板，组合 `SvPanel`/`HueSlider`/`AlphaSlider`/`PreDefine` 共同操作一个 `Color` 实例，输出 hex/rgb/hsv/hsl 字符串。

## Key Files
| File | Description |
|------|-------------|
| `ColorPicker.vue` | 主面板：弹层、确定/清空按钮、`customInput` 文本输入、禁用态展示 |
| `useOptions.ts` | 通过 `Symbol` 提供 `currentColor` 的 `provide/inject` 钩子 `useOptions` |

## Subdirectories
| Directory | Purpose |
|-----------|---------|
| `components/` | 拖拽面板与色相/透明度滑块、预定义色板（见 `components/AGENTS.md`） |
| `lib/` | `Color` 类、`draggable` 工具、尺寸校验（见 `lib/AGENTS.md`） |
| `type/` | TypeScript 类型定义（见 `type/AGENTS.md`） |

## For AI Agents

### Working in this directory
- 子面板通过共享 `color: Color` 实例联动，禁止在子组件内 `new Color()`，应由主面板创建。
- `format` 取值参考 `type/types.ts` 的 `Options.format`，新增格式需在 `lib/color.ts` 内补实现。
- `colorDisabled` 与 `colorSize` 控制禁用态/尺寸，对应 less class `.jnpf-color-picker--{size}` / `.is-disabled`。

## Dependencies
### External
- `ant-design-vue`、`@ant-design/icons-vue`

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
