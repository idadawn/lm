<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# data

## Purpose
`JnpfIconPicker` 使用的字体图标元数据（来自 iconfont 项目导出），分为业务图标 `ymIcon` 与自定义图标 `ymCustom`。运行时按 `glyphs[].name` 拼装 CSS 类。

## Key Files
| File | Description |
|------|-------------|
| `ymIcon.ts` | 导出 `ymIconJson`：`font_family: 'icon-ym'`、`css_prefix_text: 'icon-ym-'` 与 glyphs 列表 |
| `ymCustom.ts` | 自定义图标集（同结构，前缀不同），约 270KB |

## For AI Agents

### Working in this directory
- 这些是从 iconfont 项目自动导出的大型常量文件，**不要手工编辑**；如需更新，请重新从 iconfont 站下载替换整个 JSON。
- 文件命名/导出名 (`ymIconJson` 等) 被 `src/IconPicker.vue` 直接引用，请保持一致。

### Common patterns
- 字体类拼装：`${css_prefix_text}${glyph.name}`（如 `icon-ym-view`）。

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
