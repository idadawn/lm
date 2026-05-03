<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# lang

## Purpose
分语种的 i18n 消息包。每个语种一个入口 ts 文件 + 同名子目录，子目录下按命名空间 (`common` / `component` / `layout` / `routes` / `sys`) 拆分文件，由入口通过 `import.meta.glob` 聚合后挂在 `message` 上，并附带 antd `dateLocale` / `antdLocale`。

## Key Files
| File | Description |
|------|-------------|
| `zh_CN.ts` | 简体中文入口：`genMessage(import.meta.glob('./zh-CN/**/*.ts', { eager: true }), 'zh-CN')` + `ant-design-vue/es/locale/zh_CN` |
| `en.ts` | 英文入口（同结构，glob `./en/**/*.ts`） |
| `zh_TW.ts` | 繁体中文入口（glob `./zh-TW/**/*.ts`） |
| `zh-CN/` | 简中分类文件：`common.ts` `component.ts` `layout.ts` `routes.ts` `sys.ts` |
| `en/` | 英文同结构 |
| `zh-TW/` | 繁中同结构 |

## For AI Agents

### Working in this directory
- 主语言为 zh-CN（项目 UI/注释中文优先），新增 key 时三个语种同步补齐，避免回退英文导致界面混杂。
- 入口文件名形如 `xx_XX.ts`（下划线），子目录形如 `xx-XX`（连字符）：与 `useLocale.changeLocale()` 的动态 import 路径以及 `genMessage` 的前缀匹配保持一致。
- `routes.ts` 用于路由 meta.title 的多语种映射，文案需与 `web/src/router/routes` 中 `title` 一一对应。

### Common patterns
- 每个分类文件 `export default { ... }`；`genMessage` 自动以文件名作为顶层命名空间（如 `t('common.okText')`）。
- antd locale 与日期 locale 通过 spread 合入入口对象，无需各分类文件管理。

## Dependencies
### Internal
- 父级 `helper.ts`（`genMessage`）。
### External
- `ant-design-vue` 自带的 `es/locale/{zh_CN,en_US,zh_TW}`。

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
