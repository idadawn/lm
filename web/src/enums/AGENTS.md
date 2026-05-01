<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# enums

## Purpose
全局共享的枚举与常量定义。覆盖应用主题、缓存 key、HTTP 状态、菜单/侧栏类型、断点、页面路由别名、异常类型、尺寸等。stores、hooks、layouts 等多处复用以保证字符串常量统一。

## Key Files
| File | Description |
|------|-------------|
| `appEnum.ts` | `ContentEnum`/`ThemeEnum`/`SettingButtonPositionEnum`/`SessionTimeoutProcessingEnum`/`RouterTransitionEnum`，及 `SIDE_BAR_MINI_WIDTH`/`SIDE_BAR_SHOW_TIT_MINI_WIDTH` 常量 |
| `cacheEnum.ts` | localStorage/sessionStorage 缓存键名（`TOKEN_KEY`、`USER_INFO_KEY`、`PERMISSIONS_KEY`、`PROJ_CFG_KEY`、`MULTIPLE_TABS_KEY` 等）和 `CacheTypeEnum` |
| `menuEnum.ts` | 菜单类型 (`SIDEBAR`/`MIX`/`MIX_SIDEBAR`/`TOP_MENU`)、模式、触发器位置、对齐方式、混合侧栏触发方式 |
| `breakpointEnum.ts` | 屏幕断点枚举与 `screenMap`，与 `design/var/breakpoint.less` 数值同步 |
| `httpEnum.ts` | HTTP 业务状态码与 `RequestEnum`/`ContentTypeEnum`/`ResultEnum` |
| `pageEnum.ts` | 内置页面路径常量（登录、首页、404 等） |
| `exceptionEnum.ts` | 异常页类型（404/403/500/网络/无数据等） |
| `sizeEnum.ts` | 通用 size 枚举（large/middle/small）等 |
| `chartEnum.ts` | 图表相关常量 |
| `publicEnum.ts` | 通用业务公开枚举 |

## For AI Agents

### Working in this directory
- 缓存键禁止重复或随意更改：变更 `TOKEN_KEY`/`USER_INFO_KEY` 等会导致登录态丢失；旧版本兼容需走迁移逻辑。
- 新增枚举优先放在最贴近语义的文件，命名遵循 `XxxEnum`；纯字符串常量用 `export const` 而非 enum。
- 修改 `breakpointEnum.ts` 时务必同步 `design/var/breakpoint.less`，否则 LESS 与 JS 断点错位。

### Common patterns
- 文件以 1–3 个枚举为粒度，避免巨文件；同主题集中以利 import。
- 字符串型枚举使用 `=` 显式赋值，便于序列化进 store / URL。

## Dependencies
### Internal
- 被 `/@/store/**`、`/@/hooks/setting/**`、`/@/layouts/**`、`/@/router/**` 等广泛 import。

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
