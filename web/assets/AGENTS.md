<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# assets

## Purpose
Pre-built static asset bundles emitted alongside the Vite dist output and committed for offline install scenarios. Contains compiled Ant Design theme stylesheets and Monaco editor web workers (raw and pre-gzipped variants). These files are referenced from production HTML / Monaco runtime, not from `src/`.

## For AI Agents

### Working in this directory
- This directory holds **generated/build artifacts only**. Do not edit by hand — they are produced by Monaco Editor build, the antd theme plugin (`@rys-fe/vite-plugin-theme`), or the postBuild script under `web/build/`.
- The `e3b0c442` / `13a2f36d` style hashes are content hashes: replacing a file requires regenerating the matching `.gz` and updating any HTML/JS reference.
- Files include `app-antd-dark-theme-style.<hash>.css`, `app-theme-style.<hash>.css`, and Monaco workers `css.worker`, `editor.worker`, `html.worker`, `json.worker`, `ts.worker` (each with a `.gz` companion).
- If regenerating: clear the directory before rebuilding to avoid stale hashes.

### Common patterns
- Pair pattern: every `.js`/`.css` has a sibling `.gz` for nginx `gzip_static`-style serving (see `conf/nginx.docker.conf`).

## Dependencies
### Internal
- Consumed by `web/index.html` and Monaco runtime in `src/components/` editor wrappers; theme files loaded at runtime when dark mode toggles.
### External
- `monaco-editor@0.38`, `@rys-fe/vite-plugin-theme@0.8.6` (patched).

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
