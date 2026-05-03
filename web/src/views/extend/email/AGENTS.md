<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# email

## Purpose
邮箱扩展模块。提供收件箱 / 星标件 / 草稿箱 / 已发送多 tab 列表，支持写邮件、查看详情（弹层 / 独立页两种）、邮箱账户配置以及收发件操作，对接后端 `/@/api/extend/email`。

## Key Files
| File | Description |
|------|-------------|
| `index.vue` | 邮箱主页：左侧 tab 分类 + 右侧 `BasicTable` + 写邮件 / 收邮件入口 |
| `Form.vue` | 写邮件弹层：收件人 / 抄送 / 密送（标签输入）+ 富文本正文 + 附件上传 |
| `Detail.vue` | 邮件详情弹层（`BasicPopup` + `DetailMain`） |
| `DetailMain.vue` | 邮件详情主体（主题、收发件人、附件、富文本正文） |
| `DetailPage.vue` | 独立页形式的邮件详情（路由直达） |
| `Config.vue` | 邮箱账户配置弹层（SMTP/IMAP 等） |

## For AI Agents

### Working in this directory
- 状态字段 `isRead` / `starred` 切换调用单独接口（`handleSetRead/Unread/Star/UnStar`），不要本地静默改 record。
- 写邮件 / 草稿同源逻辑通过 `handleSubmit(true)` 区分发送 vs 草稿；保存草稿走 `saveDraft`，发送走 `saveSent`。
- 附件统一交由 `jnpf-upload-file type="mail"` 处理，富文本编辑器使用 `jnpf-editor`。

### Common patterns
- 标签输入 (`mode="tags"`) + `token-separators=[',']` 适配多收件人粘贴场景。
- 详情存在弹层与页面两种入口（`Detail.vue` / `DetailPage.vue`），共享 `DetailMain.vue` 渲染主体。

## Dependencies
### Internal
- `/@/api/extend/email`
- `/@/components/{Popup,Form,Table}`
- `/@/hooks/web/useMessage`
### External
- `ant-design-vue`

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
