<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# SocialsUser (Model)

## Purpose
第三方社交账号绑定信息聚合 Model。包含 OpenId、UnionId、平台类型、账号、昵称、绑定状态等字段。

## Key Files
| File | Description |
|------|-------------|
| `SocialsUserModel.cs` | 三方账号 + 系统用户的绑定关系视图 |

## For AI Agents

### Working in this directory
- 与 `Entity/Permission/SocialsUsersEntity` 一一对应，但增加了平台、是否绑定等派生字段；通过 Mapster 在 `PermissionMapper` 注册。
- 用于钉钉 / 企业微信 / OAuth 绑定流程，配合 `Poxiao.Extras.CollectiveOAuth`。

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
