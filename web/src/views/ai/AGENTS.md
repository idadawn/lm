<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# ai

## Purpose
Single-page configuration UI for the in-app AI assistant ("小美"). Lets the operator persist a system prompt, system greeting, and an optional auto-sent user greeting. Used by the global Assistant component to seed conversations.

## Key Files
| File | Description |
|------|-------------|
| `config.vue` | `<script setup>` 表单页：systemPrompt / systemGreeting / userGreeting，保存到 `localStorage` 键 `XIAO_MEI_CONFIG`，提供"保存配置"与"重置默认"按钮 |

## For AI Agents

### Working in this directory
- This page persists to `localStorage` only (no backend call). When introducing real APIs, replace the `setTimeout` stub in `handleSave` and add an entry under `web/src/api/ai/`.
- Keep the storage key constant `XIAO_MEI_CONFIG` in sync with any consumer (search the repo before renaming).
- 中文标签 must remain — system prompts are authored in Chinese.

### Common patterns
- Local explicit imports of Ant Design Vue components (`Card as ACard`, `Form as AForm`, …) rather than relying on global registration.
- `reactive<ConfigState>({...defaultState})` for form, `ref(false)` for `saving` flag.

## Dependencies
### External
- `vue` (`reactive`, `ref`, `onMounted`)
- `ant-design-vue` (`message`, `Card`, `Form`, `Input`, `Textarea`, `Button`)

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
