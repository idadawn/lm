<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# settings

## Purpose
Static, build-time configuration that drives runtime defaults: the project shell config (theme, header, multi-tab, transitions, sessionTimeout), component defaults (table pagination, scrollbar), encryption salt/IV, locale defaults, design tokens (preset color lists), and site title.

## Key Files
| File | Description |
|------|-------------|
| `projectSetting.ts` | Default `ProjectConfig` — settingButton, sessionTimeout, themeColor, header/menu/multiTabs/transition setting blocks. Persisted into localStorage and merged in `logics/initAppConfig.ts`. |
| `componentSetting.ts` | Table fetch field mapping (`pageField`, `sizeField`, `listField`, `totalField`), default page sizes, scrollbar mode. |
| `designSetting.ts` | `SIDE_BAR_BG_COLOR_LIST` / `HEADER_PRESET_BG_COLOR_LIST` swatches for the Setting drawer. |
| `encryptionSetting.ts` | `DEFAULT_CACHE_TIME`, AES key/IV used by `utils/cipher.ts` + `utils/cache/persistent.ts`. |
| `localeSetting.ts` | Default locale, available locales, antd locale mapping. |
| `siteSetting.ts` | Site title constant. |

## For AI Agents

### Working in this directory
- Comment in `projectSetting.ts` warns: 改动后需要清空浏览器缓存 — persisted config can mask new defaults; `initAppConfig` therefore deep-merges code-defaults *over* the persisted blob.
- Backend pagination field names are hard-coded in `componentSetting.ts` (`currentPage` / `pageSize` / `list` / `pagination.total`) — do not change without aligning with `.NET` API DTO conventions.
- `encryptionSetting` keys are not secrets at rest — they only obfuscate localStorage; never use them for security-critical work.

### Common patterns
- All exports are plain const objects so they tree-shake; consumers use `/@/hooks/setting/*` wrappers.

## Dependencies
### Internal
- `/@/enums/menuEnum`, `/@/enums/appEnum`, `/@/enums/cacheEnum`, `../../build/config/themeConfig`.

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
