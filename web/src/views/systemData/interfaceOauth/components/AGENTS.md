<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# components

## Purpose
Sub-modals for interface-OAuth: assigning interfaces to a client and showing credential info.

## Key Files
| File | Description |
|------|-------------|
| `Empower.vue` | 授权 modal — pick which DataInterfaces a client can call. |
| `Info.vue` | Read-only credential info modal (AppId/AppSecret display, copy). |

## For AI Agents

### Working in this directory
- `Info.vue` shows secrets — never log values. Provide copy-to-clipboard rather than console output.
