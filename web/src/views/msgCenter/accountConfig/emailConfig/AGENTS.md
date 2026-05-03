<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# emailConfig

## Purpose
邮件账号配置页：维护 SMTP 服务器、端口、账号、密码等；除 Form 外多了一个 `Test.vue` 用于发送测试邮件验证配置可用。

## Key Files
| File | Description |
|------|-------------|
| `index.vue` | BasicTable 列表 + 操作列 |
| `Form.vue` | 邮件账号表单（host/port/username/password/SSL）|
| `Test.vue` | 测试发送弹窗（输入收件人 + 内容 → 调用测试发送 API）|

## For AI Agents

### Working in this directory
- 密码字段需 password 类型 + 编辑时不回填明文。
- 测试发送独立于正式发送链路，API 通常 `accountConfig/test`。

### Common patterns
- Form/Test 两个 Modal 各自 `useModal` 注册。

## Dependencies
### Internal
- `/@/api/msgCenter/accountConfig`, `/@/components/Modal`
### External
- `ant-design-vue`

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
