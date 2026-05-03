<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# en

## Purpose
English (`en`) translation bundles for vue-i18n. Mirrors the `zh-CN` namespace shape so that locale switching works seamlessly across UI strings, layout chrome, route titles, and system messages.

## Key Files
| File | Description |
|------|-------------|
| `common.ts` | Generic shared strings (buttons, prompts, status). |
| `component.ts` | Component-level labels (forms, tables, modals, uploaders). |
| `layout.ts` | Layout chrome: header, sidebar, multi-tab, settings drawer. |
| `routes.ts` | Route/menu titles keyed by route name (matches `router/routes/modules/*`). |
| `sys.ts` | System-level messages (api errors, login, exceptions, error log). |

## For AI Agents

### Working in this directory
- Keep keys structurally identical to `../zh-CN/*.ts`; missing keys fall back to source language.
- Translations are imported by `web/src/locales/index.ts`; do not add new files without registering them.
- Default UI language for this LIMS project is Chinese — English is secondary and may lag features.

### Common patterns
- Each file `export default { ... }` an object literal of nested string keys.
- Use the same key path used in Chinese bundle (e.g. `routes.lab.lab`).

## Dependencies
### Internal
- Consumed by `web/src/locales/` and `/@/hooks/web/useI18n`.

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
