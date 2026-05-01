<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# frontend

## Purpose
Schedule 看板前端的预编译产物（React SPA），作为嵌入资源由 `ScheduleUIMiddleware` / `EmbeddedFileProvider` 服务出去。整个目录下的文件均由外部构建链生成，运行时被原样回包。

## Key Files
| File | Description |
|------|-------------|
| `index.html` | SPA 入口，包含 `/__schedule__` 占位符，中间件运行时替换为 `VirtualPath + RequestPath`。 |
| `apiconfig.js` | 通过 `%(RequestPath)` 占位符拼接 API 入口地址，供 SPA 在加载时读取。 |
| `asset-manifest.json` | CRA 生成的资源清单，便于手动校验内嵌资源完整性。 |
| `favicon.ico` | 看板图标。 |

## Subdirectories
| Directory | Purpose |
|-----------|---------|
| `static/` | 哈希命名的 JS / CSS 静态资源 (see `static/AGENTS.md`) |

## For AI Agents

### Working in this directory
- 不要手动编辑 hash 文件名（`main.<hash>.css/js`）；前端升级以整套替换为主，并保持文件名形如 `main.<hash>.<ext>` 以匹配 `index.html` 的引用。
- `index.html`/`apiconfig.js` 内的占位符 `STATIC_FILES_PATH = "/__schedule__"` 与 `%(RequestPath)` 必须保留——中间件会在响应时做字符串替换。
- 修改后需确认 csproj 已将 `frontend/**` 设置为 `EmbeddedResource`。

### Common patterns
- 资源名映射规则：`Poxiao.Schedule.Dashboard.frontend.<relative.path>`（`.` 代替 `/`）。

## Dependencies
### External
- React 构建产物；运行时仅依赖现代浏览器。

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
