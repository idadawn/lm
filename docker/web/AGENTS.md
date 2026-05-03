<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# docker/web

## Purpose
Build context for the `web` service container — packages the Vue 3 production bundle (`dist/`) into an Nginx 1.27-alpine image that serves the SPA at internal port 80, behind the upstream `nginx` reverse proxy declared in `../docker-compose.yml`.

## Key Files
| File | Description |
|------|-------------|
| `Dockerfile` | `FROM nginx:1.27-alpine`; `apk add curl` (for the healthcheck), copies `web/nginx.conf` to `/etc/nginx/conf.d/default.conf`, copies `dist` to `/usr/share/nginx/html`. Exposes 80, declares a 30s/10s/3-retry HEALTHCHECK against `http://localhost/health`. The `COPY` paths are relative to the compose build context `../apps`, so the file references `web/nginx.conf` and `dist` at the apps root. |
| `nginx.conf` | Single `server { listen 80; }` block with: gzip for text/JS/CSS/font/SVG (min length 1024), 1-year immutable cache for static assets, SPA fallback `try_files $uri $uri/ /index.html`, `/health` returning `200 "healthy\n"`, and a deny rule for dotfiles. |

## For AI Agents

### Working in this directory
- The Dockerfile runs in the `../apps` build context (set by `../docker-compose.yml`), **not** in this directory. That's why it references `web/nginx.conf` and `dist/` instead of `./nginx.conf` — both are siblings inside `../apps`. Do not "fix" these paths to `./` without simultaneously moving the Dockerfile and changing the compose `dockerfile:` key.
- The `/health` endpoint is the contract used by the compose healthcheck. Keep it as-is: the upstream `nginx` proxy depends on `condition: service_healthy` and will not start if you remove or rename it.
- Static-asset caching uses `Cache-Control: public, immutable` for one year. Vite already emits hashed filenames, so this is safe — but do not extend the regex to cover `index.html`, or you will break SPA refresh-cycle deploys.
- The `apk add --no-cache curl` line exists solely so the HEALTHCHECK can use `curl -f`. Do not strip it during image-size optimisation.
- This image carries no app config; runtime tweaks happen at the upstream proxy level (`../apps/volumes/nginx/`), not here.

### Common patterns
- SPA wiring is the standard `try_files $uri $uri/ /index.html` pattern that Vue Router needs in `history` mode. Static-first, fallback-last ordering is intentional — flipping it serves `index.html` for assets and breaks caching.

## Dependencies
### Internal
- `../docker-compose.yml` — references this Dockerfile via `dockerfile: web/Dockerfile`.
- `../../web/` — source of `dist/` after `pnpm build` (or via `scripts/build-web-turbo.sh`).

### External
- `nginx:1.27-alpine` base image.

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
