<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# lib

## Purpose
`JnpfColorPicker` 的纯 TS 工具集：手写颜色模型、HSV↔HSL/RGB/HEX 转换、拖拽事件绑定、尺寸校验。

## Key Files
| File | Description |
|------|-------------|
| `color.ts` | `Color` 类与 `hsv2hsl` 等工具：解析输入、保存 hue/saturation/value/alpha、按 `format` 输出字符串 |
| `draggable.ts` | 通用 `draggable(element, options)` 工具，按 mousedown/move/up 触发回调 |
| `validators.ts` | `isValidComponentSize`：校验 `'' / large / default / small` |

## For AI Agents

### Working in this directory
- `color.ts` 不要替换为第三方库（参见父级 ColorPicker 说明）；扩展格式时新增分支并保留兼容字段。
- `draggable.ts` 没有 cleanup hook，调用方需在 `onUnmounted` 时自行解绑（沿用 antd 风格）。
- `validators.ts` 仅做尺寸校验，避免承担其它业务校验。

### Common patterns
- 使用 `@vue/shared` 的 `hasOwn`，不要手写 `Object.prototype.hasOwnProperty.call`。

## Dependencies
### External
- `@vue/shared`

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
