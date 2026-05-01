<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# sendConfig

## Purpose
"发送配置"页面：将消息模板与发送策略组合，配置在何种业务事件下使用哪个模板、推送哪些用户、走哪个渠道。支持测试发送。

## Key Files
| File | Description |
|------|-------------|
| `index.vue` | BasicTable + 新建/编辑/详情 + 测试发送弹窗 + 多颜色 messageType 标签 |
| `Form.vue` | 发送配置表单 |
| `Detail.vue` | 详情查看 |

## Subdirectories
| Directory | Purpose |
|-----------|---------|
| `components/` | 模板表单/弹窗、测试发送、发送结果 (see `components/AGENTS.md`) |

## For AI Agents

### Working in this directory
- `messageType` 是数组（多类型混合），按类型显示不同 `colorList[type]` 色块标签。
- 测试发送独立 Modal，复用 `TestSend.vue`。

### Common patterns
- `useModal` 注册 Form/Detail/TestSend 三个弹窗。

## Dependencies
### Internal
- `/@/api/msgCenter/sendConfig`, `/@/api/msgCenter/msgTemplate`
- `/@/components/Table`, `/@/components/Modal`
### External
- `ant-design-vue`

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
