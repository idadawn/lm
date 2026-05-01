<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# locales

## Purpose
i18n 国际化基础设施。基于 `vue-i18n` 提供运行时语言切换、动态 import 语言包、设置 antd locale 与 html lang。默认中文 (zh-CN)，目前支持 `zh-CN` / `en` / `zh-TW` 三语。

## Key Files
| File | Description |
|------|-------------|
| `setupI18n.ts` | `setupI18n(app)`：从 `./lang/${locale}.ts` 动态 import 默认语言，构造 `createI18n` 选项（legacy:false、fallback、`silentFallbackWarn`），暴露全局 `i18n` 实例 |
| `useLocale.ts` | `useLocale()`：响应式 `getLocale`/`getShowLocalePicker`/`getAntdLocale`，`changeLocale(locale)` 动态加载语言包并写入 `localeStore` 与 html lang |
| `helper.ts` | `genMessage(modules, prefix)` 把 `import.meta.glob` 拿到的子文件合并为单层对象；`setHtmlPageLang`/`setLoadLocalePool` 维护已加载语言池 |

## Subdirectories
| Directory | Purpose |
|-----------|---------|
| `lang/` | 三语包入口与按命名空间拆分的 ts 文件 (see `lang/AGENTS.md`) |

## For AI Agents

### Working in this directory
- 新增语言：①在 `lang/` 下建同名子目录 + 入口 `xx_XX.ts`；②`/@/settings/localeSetting` 注册 availableLocales；③确保有匹配的 antd locale。
- `setupI18n` 是 async；启动顺序在 `main.ts` 中需 `await setupI18n(app)` 再 `app.mount`。
- 不要在业务代码直接 `import { i18n }`，统一通过 `useI18n()` 钩子，保证 setup 阶段响应式。

### Common patterns
- `genMessage(import.meta.glob('./zh-CN/**/*.ts', { eager: true }), 'zh-CN')` 自动生成嵌套 message 树，键名取自文件名。
- `loadLocalePool` 防止重复 import 同一语言。

## Dependencies
### Internal
- `/@/settings/localeSetting`、`/@/store/modules/locale`、`/#/config` 类型。
### External
- `vue-i18n`、`ant-design-vue/es/locale/*`。

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
