<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# sysConfig

## Purpose
系统配置 — global app settings: branding (logo/icon/title), system name/version, and other base options. Tabbed form.

## Key Files
| File | Description |
|------|-------------|
| `index.vue` | Tabbed configuration form (基本设置 + additional tabs); uses `JnpfUploadImgSingle` for icons. |

## For AI Agents

### Working in this directory
- Single page, no list — submits the entire `baseForm` to the system-config endpoint.
- `sysVersion` is `readonly`; keep that. Do not surface it as editable.
- Image fields (loginIcon/navigationIcon/logoIcon/appIcon) are URL strings, not blobs.
