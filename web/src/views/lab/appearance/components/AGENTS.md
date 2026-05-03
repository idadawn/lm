<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# components

## Purpose
`appearance` 页面专用子组件：大类管理抽屉、人工修正抽屉、特性卡片（含增强版）、特性匹配/选择对话框、特性等级编辑弹窗。

## Key Files
| File | Description |
|------|-------------|
| `CategoryDrawer.vue` | 大类侧边抽屉（替代 Modal，支持长列表）|
| `CorrectionDrawer.vue` | 人工修正抽屉版 |
| `FeatureCard.vue` | 特性展示卡片 |
| `FeatureCardEnhanced.vue` | 增强版特性卡片（含等级、统计）|
| `FeatureMatchDialog.vue` | 特性匹配测试对话框 |
| `FeatureModal.vue` | 特性新增/编辑弹窗（替代 `../Form.vue`）|
| `FeatureSelectDialog.vue` | 特性多选选择器 |
| `LevelModal.vue` | 等级编辑弹窗 |

## For AI Agents

### Working in this directory
- 卡片组件接收 `feature: AppearanceFeature` props 并 emit `edit`/`delete`/`select`。
- Drawer 与 Modal 二选一：长列表/侧栏配置用 Drawer，简单表单用 Modal。
- 不要在此目录直接调用接口；通过 emit 让父页统一处理 reload。

### Common patterns
- `useDrawer` / `useModal` + `useForm`。

## Dependencies
### Internal
- `/@/api/lab/appearance`, `/@/api/lab/severityLevel`
- `/@/components/Drawer`, `/@/components/Modal`, `/@/components/Form`
### External
- `ant-design-vue`, `@ant-design/icons-vue`

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
