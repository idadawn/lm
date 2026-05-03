<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# src

## Purpose
`JnpfIconPicker` 的 SFC 实现。只读输入框 + "选择" 按钮触发模态框，模态框内 Tabs 分别渲染 `ymIcon` / `ymCustom` 图标网格，支持搜索 `keyword` 过滤；确认后回写选中类名。

## Key Files
| File | Description |
|------|-------------|
| `IconPicker.vue` | 组件实现：`a-input` + `a-modal`（宽度 1000）+ Tabs 网格、`ScrollContainer` 滚动 |

## For AI Agents

### Working in this directory
- 图标数据通过 `../data/ymIcon` 与 `../data/ymCustom` 引入，列表由 glyphs 生成；勿在组件内硬编码图标名。
- Modal 关闭按钮使用 `ModalClose`（不可全屏化），保留 `centered`、`maskClosable=false`、`keyboard=false`。
- 国际化文案：使用 `t('common.cleanText'/'cancelText'/'okText')`。

### Common patterns
- 弹窗内 `is-active` 状态比较：图标库 1 用 `active`，图标库 2 用 `innerValue`，与原模板保持一致以避免误清除。

## Dependencies
### Internal
- `../data/ymIcon`、`../data/ymCustom`、`/@/components/Modal`、`/@/components/Container`（ScrollContainer）
### External
- `ant-design-vue`

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
