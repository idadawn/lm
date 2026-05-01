<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# appearance

## Purpose
"外观特性"维护页面：左侧大类树 + 中间特性表格 + 右侧"语义匹配测试"面板（规则引擎 + AI 模型联合匹配，输入炉号后缀/备注，返回匹配特性）。还包含人工修正列表入口。

## Key Files
| File | Description |
|------|-------------|
| `index.vue` | 三栏主页：BasicLeftTree 大类、特性 BasicTable、紫色 AI 测试面板 |
| `Form.vue` | 特性新增/编辑弹窗（category/name/description/severityLevel）|
| `correction.vue` | 人工修正列表弹窗（导入 Excel 生成修正项 + 确认/删除）|

## Subdirectories
| Directory | Purpose |
|-----------|---------|
| `components/` | 类目抽屉、修正抽屉、特性卡片/选择器/匹配对话框等 (see `components/AGENTS.md`) |

## For AI Agents

### Working in this directory
- 接口 `/@/api/lab/appearance`：`createAppearanceFeature` / `updateAppearanceFeature` / `getAppearanceFeatureInfo` 等。
- AI 匹配按"规则 → AI → 文本模糊"优先级返回 `searchMethod`，UI 中需展示。
- "管理特性等级"按钮跳转到 `severityLevel` 页面。

### Common patterns
- `BasicLeftTree` 字段映射 `{ key: 'id', title: 'title', children: 'children' }`。
- `a-tag` 颜色：category 紫色、severityLevel 橙色。

## Dependencies
### Internal
- `/@/api/lab/appearance`
- `BasicLeftTree`, `/@/components/Table`, `/@/components/Modal`, `/@/components/Form`
### External
- `ant-design-vue`, `@ant-design/icons-vue`

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
