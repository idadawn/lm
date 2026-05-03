<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# conf

## Purpose
Nginx server configs used at runtime to serve the built SPA. Ships with the Docker image (`Dockerfile.build` copies `nginx.docker.conf` to `/etc/nginx/conf.d/default.conf`) and is also referenced as a `ConfigMap` for the Kubernetes deployment in `../deploy/`.

## Key Files
| File | Description |
|------|-------------|
| `default.conf` | Legacy/staging nginx config: `try_files` SPA fallback, `/collectServer/` proxy to data collector, `/api/` proxy to upstream `kpi-api-dev`. 100MB upload, 128k header buffers. |
| `nginx.docker.conf` | Production Docker config: gzip on for js/css/svg/fonts, immutable cache for hashed assets, `no-cache` on `*.html`, `/health` endpoint, security headers (`X-Frame-Options`, `X-Content-Type-Options`, `X-XSS-Protection`). API proxy block is commented out by default. |

## For AI Agents

### Working in this directory
- `nginx.docker.conf` is the active config baked into the image — `default.conf` is kept for legacy / k8s ConfigMap parity (`../deploy/cm.yaml` redefines it inline). Keep both in sync if adding a new route.
- SPA fallback `try_files $uri $uri/ /index.html` must remain — required by Vue Router history mode.
- `client_max_body_size 100m` is intentional (file/import uploads in lab modules).
- Health check at `/health` is consumed by Dockerfile `HEALTHCHECK` and k8s probes — do not remove.

### Common patterns
- All static asset locations point to `/usr/share/nginx/html` (the unzip target of `dist.zip`).
- Long cache (`expires 30d` / `immutable`) only on hashed assets; HTML always revalidates.

## Dependencies
### Internal
- Consumed by `../Dockerfile.build`, `../docker-compose.yaml`, `../deploy/deployment.yaml` (mounts ConfigMap).
### External
- nginx 1.27 alpine.

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
