<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# web

## Purpose
Front-end SPA for the Laboratory Data Analysis System (实验室数据分析系统, package name `venus-web`). Vue 3.3 + TypeScript + Ant Design Vue 3.2 admin shell built with Vite 4.4, themed via Less + WindiCSS, packaged for Nginx (Docker / k8s) deployment. Targets `pnpm@9.5+`, Node `>=16.15`.

## Key Files
| File | Description |
|------|-------------|
| `package.json` | Scripts (`dev`, `build`, `build:fast`, `build:test`, `lint:*`, `type:check`) and full Vue 3 / antd-vue / echarts / monaco / tinymce dependency set. |
| `index.html` | Vite entry; injects `<%= title %>`, preloads `/config.js` for runtime overrides, mounts `#app`. |
| `vite.config.ts` | Vite config (lives at root with `tsconfig.json`). |
| `.env.development` / `.env.production` / `.env.fastbuild` / `.env.test` | `VITE_*` proxy and API base URL settings; default backend `http://127.0.0.1:10089`. |
| `Dockerfile.build` | Builds an `nginx:1.27-alpine` image from a pre-built `dist.zip` using `conf/nginx.docker.conf`. |
| `docker-compose.yaml` | Local container stack for the web image. |
| `Jenkinsfile` | CI pipeline. |
| `_app.config.js` / `config.js` | Runtime app config exposed on `window`. |
| `tailwind.config.js`, `postcss.config.js`, `windi.config.ts` | Style toolchain configs. |
| `.eslintrc.js`, `.prettierrc.js`, `stylelint.config.js`, `.editorconfig` | Lint/format configs. |

## Subdirectories
| Directory | Purpose |
|-----------|---------|
| `src/` | Application source (entry `main.ts`, `App.vue`, all features) (see `src/AGENTS.md`). |
| `mock/` | Standalone Koa mock server on port 19003 for frontend-only dev (see `mock/AGENTS.md`). |
| `conf/` | Nginx server configs for local + Docker deploy (see `conf/AGENTS.md`). |
| `deploy/` | Kubernetes manifests for `jnpf-staging` namespace (see `deploy/AGENTS.md`). |
| `public/` | Static assets copied verbatim to dist root (see `public/AGENTS.md`). |
| `resource/` | Legacy mirror of `public/resource/` (emoji, img, tinymce) (see `resource/AGENTS.md`). |
| `assets/` | Pre-built theme/worker bundles emitted by build scripts (see `assets/AGENTS.md`). |
| `build/` | Vite plugin wrappers, postBuild script, icon generators. |
| `cdn/` | Local CDN-style mirror of vendored libs. |
| `patches/` | `pnpm patch` files (e.g. `@rys-fe/vite-plugin-theme`). |

## For AI Agents

### Working in this directory
- Use `pnpm` only (lockfile + `packageManager` pinned). Run `pnpm install`, then `pnpm dev`.
- Build entrypoint: `pnpm build` runs `vite build` followed by `build/script/postBuild.ts`. Use `build:fast` to skip imagemin.
- API base is `VITE_GLOB_API_URL=/dev` proxied via `VITE_PROXY` to `127.0.0.1:10089`; KPI v1 and collect server proxies are separate.
- Production runtime config is patched by overwriting `public/config.js` (loaded before `main.ts`); never bake URLs into the bundle.
- Do not edit `dist.zip` or `assets/*.worker-*.js` by hand.

### Common patterns
- Path alias `/@/` → `src/` (used everywhere instead of relative imports).
- Theme switching via `localStorage['__APP__DARK__MODE__']` set on `<html data-theme>`.
- Chinese for UI strings, comments, descriptions; English for code identifiers.

## Dependencies
### Internal
- Backend at `../api/` exposes the routes proxied through `/dev`, `/dev/kpiV1`, `/dev/collectServer`.
### External
- Vue 3.3, Vue Router 4, Pinia 2.1, Ant Design Vue 3.2, VueUse 10, ECharts 5.4, Highcharts 11, Monaco 0.38, TinyMCE 5, AntV G6, LogicFlow, FullCalendar, Vditor, mqtt, mathjs, xlsx, sortablejs, vue-grid-layout.

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
