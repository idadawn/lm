<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# webhookConfig

## Purpose
Webhook 账号配置页：维护 HTTP 回调地址、Method、Header、Body 模板等，支持启用/禁用、复制、编辑。

## Key Files
| File | Description |
|------|-------------|
| `index.vue` | BasicTable 列表 + 操作列 |
| `Form.vue` | Webhook 表单（url/method/headers/body）|

## For AI Agents

### Working in this directory
- Header/Body 多为 JSON 文本，建议使用 JsonEditor/Codemirror 输入。
- URL 校验为 https 推荐；http 给警告。

### Common patterns
- 与同级其他 accountConfig 子目录一致。

## Dependencies
### Internal
- `/@/api/msgCenter/accountConfig`
### External
- `ant-design-vue`

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
