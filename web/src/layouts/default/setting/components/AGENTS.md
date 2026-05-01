<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# components

## Purpose
SettingDrawer 内部使用的小颗粒控件。封装一行设置项的统一布局（label + 控件），支持类型/主题色/开关/下拉/数值多种交互形式。

## Key Files
| File | Description |
|------|-------------|
| `index.ts` | 通过 `createAsyncComponent` 导出 `TypePicker`/`ThemeColorPicker`/`SettingFooter`/`SwitchItem`/`SelectItem`/`InputNumberItem` |
| `TypePicker.vue` | 菜单类型/导航布局可视化卡片选择器（带预览缩略图） |
| `ThemeColorPicker.vue` | 预设色块选择器（用于主题色、头部背景、侧栏背景） |
| `SwitchItem.vue` | 开关项：label + a-switch + 可选 disabled |
| `SelectItem.vue` | 下拉项：label + a-select，options 来自 `enum.ts` |
| `InputNumberItem.vue` | 数值项：label + a-input-number，用于菜单宽度等 |
| `SettingFooter.vue` | 底部按钮区：复制配置 / 清空缓存重置 / 重置 |

## For AI Agents

### Working in this directory
- 所有控件 emit `change(value)`，由父级 `SettingDrawer` 统一交给 `baseHandler(event, value)`。
- 不要在控件内直接读写 store，保持纯展示+受控；这样可以让 `baseHandler` 集中决定副作用。
- 颜色/选项数据应来自 `enum.ts` 或 `/@/settings/designSetting`，禁止本地硬编码色值。

### Common patterns
- Vue SFC + Composition API，props 含 `def`（默认值）、`event`（HandlerEnum）、`title`/`disabled`。
- `createAsyncComponent` barrel 让 SettingDrawer 按需 chunk。

## Dependencies
### Internal
- `/@/utils/factory/createAsyncComponent`、`/@/settings/designSetting`、父级 `enum.ts`/`handler.ts`。
### External
- `ant-design-vue` (`Switch`/`Select`/`InputNumber`/`Tooltip`)。

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
