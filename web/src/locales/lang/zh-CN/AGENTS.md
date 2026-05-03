<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# zh-CN

## Purpose
简体中文 (zh-CN) 翻译包 — 项目主语言。所有 UI 文案、菜单标题、系统提示均以此为权威来源，其他语言以此为蓝本对齐键结构。

## Key Files
| File | Description |
|------|-------------|
| `common.ts` | 通用文案（按钮、占位符、操作结果）。 |
| `component.ts` | 组件级文案（表格、表单、上传、模态等）。 |
| `layout.ts` | 布局相关：顶栏、侧边栏、多标签、设置抽屉。 |
| `routes.ts` | 路由/菜单标题，键名对齐 `router/routes/modules/*`，包含 `lab`/`onlineDev`/`system` 等模块。 |
| `sys.ts` | 系统消息：API 错误、登录提示、异常错误日志列表表头等。 |

## For AI Agents

### Working in this directory
- 新增功能模块时优先在此添加键值，再同步到 `../en` 与 `../zh-TW`。
- 错误码相关文案集中在 `sys.api.errMsg{code}`；与 axios 拦截器（`utils/http/axios/checkStatus.ts`）成对维护。
- 路由 i18n 键由 `t('routes.xxx')` 在路由 meta 内消费；新增路由请同步本目录。

### Common patterns
- 嵌套对象按业务域划分 (api / app / errorLog / login / common 等)。
- 占位变量遵循 vue-i18n 语法（`{count}` 等）。

## Dependencies
### Internal
- `web/src/locales/index.ts`、`/@/hooks/web/useI18n`。

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
