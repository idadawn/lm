<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# Entity

## Purpose
SqlSugar ORM 实体定义。所有类继承 `CLDEntityBase`，对应 BASE_* 数据表（SQL Server / MySQL / Oracle 兼容）。

## Subdirectories
| Directory | Purpose |
|-----------|---------|
| `Permission/` | 权限/组织域实体：用户、机构、角色、岗位、分组等 (see `Permission/AGENTS.md`) |
| `System/` | 系统域实体：菜单、按钮、字典、日志、调度、打印等 (see `System/AGENTS.md`) |

## For AI Agents

### Working in this directory
- 严格遵守 `/data/project/lm/.cursorrules` 字段命名规则：从 `OEntityBase` 继承的是 `F_Id`/`F_TenantId`，从 `CLDEntityBase` 大写部分是 `F_CREATORTIME`/`F_CREATORUSERID`/`F_ENABLEDMARK`，混合大小写部分是 `F_LastModifyTime`/`F_LastModifyUserId`/`F_DeleteMark`/`F_DeleteTime`/`F_DeleteUserId`。
- 老表常用 `[SugarColumn(ColumnName = "F_XXX")]` 显式覆盖；缺失字段加 `[SugarColumn(IsIgnore = true)]`。
- 多租户表加 `[Tenant(ClaimConst.TENANTID)]`。

### Common patterns
- 类注释中文摘要必填，方便自动文档生成。
- 类名后缀统一 `Entity`，文件名同类名。

## Dependencies
### Internal
- `Poxiao.Infrastructure.Const`、`Poxiao.Infrastructure.Contracts`（CLDEntityBase、ClaimConst）

### External
- SqlSugar

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
