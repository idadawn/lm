<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# docker

## Purpose
Containerised deployment for the LM application tier (`api` + `web` + `nginx` reverse proxy). This stack is the Docker counterpart to the NSSM-based `../deploy/` toolkit. Build context is **`../apps`** (a sibling directory created by `scripts/rebuild.sh` that stages `apps/api/` and `apps/dist/` build outputs and `apps/volumes/` bind mounts) — there is a planned rename of `apps/` → `docker/` that is captured by the `VOLUMES_DIR` env var.

## Key Files
| File | Description |
|------|-------------|
| `docker-compose.yml` | Three-service compose: `api` (port `9530`, ASP.NET Core, `/health` endpoint), `web` (Nginx-served Vue bundle, internal port 80), `nginx` (reverse proxy on `${NGINX_PORT:-8923}` that template-renders `default.conf.template` via `docker-entrypoint.sh`). All services share `lm-apps-network`, log via `json-file` driver capped at 10MB×3, and use 30s/10s/3-retry healthchecks. |
| `.env.example` | Compose env template. Defaults: `IMAGE_TAG=1.1.0`, `API_PORT=9530`, `NGINX_PORT=8923`, `VOLUMES_DIR=../apps/volumes`, `NGINX_VERSION=1.27-alpine`. Copy to `.env` and adjust per environment. |

## Subdirectories
| Directory | Purpose |
|-----------|---------|
| `web/` | Frontend image build context — `Dockerfile` (Nginx 1.27-alpine + `dist/` static bundle) and `nginx.conf` (SPA fallback + asset cache). See `web/AGENTS.md`. |

## For AI Agents

### Working in this directory
- The `api` service builds from `context: ../apps` with `dockerfile: Dockerfile`, and the `web` service builds from `context: ../apps` with `dockerfile: web/Dockerfile`. Build will fail unless you first run `scripts/rebuild.sh` (or `scripts/build-api.sh` / `scripts/build-web-turbo.sh`) at the repo root to populate `../apps/`. Do not move the Dockerfiles into this directory unless you also update both `dockerfile:` paths.
- Bind mounts mount **into** `../apps/volumes/api/{logs,uploads,Configurations,wwwroot,resources,lib}` and `../apps/volumes/nginx/...`. Treat these paths as the runtime configuration surface; `Configurations/` is the same shape the API ships in `api/src/application/Poxiao.API.Entry/Configurations/`.
- The Nginx service uses an entrypoint template flow: `docker-entrypoint.sh` materialises `default.conf` from `default.conf.template` using `API_SERVICE_NAME`, `WEB_SERVICE_NAME`, `API_PORT`, `WEB_INTERNAL_PORT`. The runtime `/etc/nginx/conf.d` is a `tmpfs` so the templated file is recreated each boot.
- Healthchecks: API hits `http://localhost:9530/health` (matches the `/health` minimal endpoint configured in `Program.cs`), web hits `http://localhost/health` (matches `web/nginx.conf`'s `location /health` returning `200 "healthy\n"`). The standalone `../deploy/` flow probes `/` instead — keep that asymmetry in mind when reusing health logic.
- Do **not** commit a real `.env` here; only `.env.example` is tracked.

### Common patterns
- All env vars use `${NAME:-default}` so the compose file boots cleanly even without an `.env`.
- `nginx` `depends_on` both `api` and `web` with `condition: service_healthy`, so a flapping API healthcheck will block the proxy from starting — surface failures by checking the `api` healthcheck first when debugging.

## Dependencies
### Internal
- `../apps/` (generated) — Dockerfile contexts and bind-mount sources.
- `../api/` and `../web/` — source roots that `rebuild.sh` compiles into `../apps/`.

### External
- Docker Engine + Docker Compose v2.
- `nginx:1.27-alpine` image (configurable via `NGINX_VERSION`).

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
