<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# workFlowDetail

## Purpose
独立的流程详情页 — opened via route with a base64-encoded `config` query param. Mounts `FlowParser` directly so external links can deep-link into a flow detail view.

## Key Files
| File | Description |
|------|-------------|
| `index.vue` | Decodes `route.query.config` (`decodeByBase64`) and opens `FlowParser`; closes the tab on parser reload. |

## For AI Agents

### Working in this directory
- This page is a thin shell — all interaction logic belongs in `components/FlowParser.vue`.
- `useTabs().closeCurrent()` is called on `@reload` so completed approvals close the deep-link tab automatically.

## Dependencies
### Internal
- `/@/utils/cipher`, `/@/api/workFlow/flowBefore`, `/@/views/workFlow/components/FlowParser.vue`
