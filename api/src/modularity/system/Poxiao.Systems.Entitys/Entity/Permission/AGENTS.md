<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# Permission (Entity)

## Purpose
权限/组织域 SqlSugar 实体。映射 BASE_USER、BASE_ORGANIZE、BASE_ROLE 等基础人员/组织/授权表。

## Key Files
| File | Description |
|------|-------------|
| `UserEntity.cs` | `BASE_USER` 用户主表：账户、姓名、密码、机构、签名等 |
| `OrganizeEntity.cs` | `BASE_ORGANIZE` 机构/部门树 |
| `OrganizeRelationEntity.cs` | 机构与用户/角色/岗位关联表 |
| `OrganizeAdministratorEntity.cs` | 机构管理员配置 |
| `RoleEntity.cs` | 角色 |
| `PositionEntity.cs` | 岗位 |
| `GroupEntity.cs` | 用户分组 |
| `AuthorizeEntity.cs` | 角色/用户与菜单/按钮/列权限挂接 |
| `ColumnsPurviewEntity.cs` | 列字段权限明细 |
| `UserRelationEntity.cs` | 用户关系（上下级/关注） |
| `UserOldPasswordEntity.cs` | 用户历史密码（防复用） |
| `SocialsUsersEntity.cs` / `TenantSocialsEntity.cs` | 第三方社交账号绑定及租户社交配置 |
| `SignImgEntity.cs` | 用户签名图片 |

## For AI Agents

### Working in this directory
- 用户密码字段名为 `F_PASSWORD`/`F_SECRETKEY`，加密通过 `Poxiao.DataEncryption`，不要直接写明文。
- `UserEntity` 同时映射在 `Mapper/PermissionMapper.cs` 中，向 DTO/`UserInfoModel` 映射时 HeadIcon 会被前缀 `/api/File/Image/userAvatar/`，新增字段时考虑映射器。
- 多数表是多租户（带 `[Tenant(ClaimConst.TENANTID)]`），新增字段时不要破坏租户隔离查询。

### Common patterns
- `[SugarTable("BASE_XXX")]`、`[SugarColumn(ColumnName="F_XXX")]` 大写命名。
- 中文 XML 注释统一描述业务含义。

## Dependencies
### Internal
- `Poxiao.Infrastructure.Const`、`Poxiao.Infrastructure.Contracts`

### External
- SqlSugar

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
