<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# msgTemplate

## Purpose
"消息模板"页面：维护消息模板内容（标题/正文/参数占位），支持启用/禁用、复制、编辑、查看详情。模板被 `sendConfig` 引用。

## Key Files
| File | Description |
|------|-------------|
| `index.vue` | BasicTable + 新建/编辑/复制 + Detail 弹窗 |
| `Form.vue` | 模板编辑表单（含正文模板占位语法）|
| `Detail.vue` | 模板详情查看 |

## For AI Agents

### Working in this directory
- 模板正文支持占位符（如 `${name}`），后端渲染时替换。
- `getMsgTypeList` 用于初始化消息类型 select。

### Common patterns
- BasicTable + Form + Detail 三件套。

## Dependencies
### Internal
- `/@/api/msgCenter/msgTemplate`, `/@/store/modules/base`
- `/@/components/Popup`, `/@/components/Modal`, `/@/components/Table`
### External
- `ant-design-vue`

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
