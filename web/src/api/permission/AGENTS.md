<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# permission

## Purpose
RBAC + organization API surface. Manages users, roles, positions, organisation tree, groups (user collections), authorization grants, online-user sessions, social-bound users, grade/level configuration, user relations, and per-user app/dashboard settings.

## Key Files
| File | Description |
|------|-------------|
| `user.ts` | User CRUD + selectors (`/All`, `/Selector`); base `/api/permission/Users`. Used by user management page + login. |
| `role.ts` | Role CRUD (`/api/permission/Role`). |
| `position.ts` | 岗位 CRUD. |
| `group.ts` | 用户分组 CRUD. |
| `organize.ts` | 组织架构 CRUD + tree fetch. |
| `authorize.ts` | Authorization grants (role <-> resource). |
| `gradeManage.ts` | Grade/level config. |
| `onlineUser.ts` | Active session listing + force-logout. |
| `socialsUser.ts` | Third-party-account binding management. |
| `userRelation.ts` | User-to-user relations (manager / subordinate). |
| `userSetting.ts` | Per-user dashboard / preference settings. |

## For AI Agents

### Working in this directory
- Backend prefixes are PascalCase (`/api/permission/Users`, `/api/permission/Role`) — match exactly.
- `getUserSelector` returns combined company+department+user tree; do not bypass it for per-user search.
- Granular CRUD is offered for every entity — prefer composing here over duplicating logic in views.

### Common patterns
- Standard `getXxxList`, `createXxx`, `updateXxx`, `getInfo`, `delXxx` quintet per resource.

## Dependencies
### Internal
- `/@/utils/http/axios`.

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
