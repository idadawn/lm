<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# Poxiao.Systems.Entitys

## Purpose
系统模块的实体定义层。集中存放数据库实体（SqlSugar）、API 输入/输出 DTO、视图/聚合 Model、Mapster 映射规则与枚举。被 Interfaces 与实现项目共享。

## Key Files
| File | Description |
|------|-------------|
| `Poxiao.Systems.Entitys.csproj` | 仅引用 Poxiao.Common（Poxiao.Infrastructure），保持轻量纯定义 |

## Subdirectories
| Directory | Purpose |
|-----------|---------|
| `Dto/` | API 输入输出 DTO，按 Permission/System 分组 (see `Dto/AGENTS.md`) |
| `Entity/` | SqlSugar 实体（继承 CLDEntityBase） (see `Entity/AGENTS.md`) |
| `Enum/` | 枚举：MenuType、MenuCategory、SysConfig、ErrorStrategy (see `Enum/AGENTS.md`) |
| `Mapper/` | Mapster 映射注册（PermissionMapper、SystemMapper） (see `Mapper/AGENTS.md`) |
| `Model/` | 复合视图 Model（不直接落库），如菜单聚合、SSO 用户、动态表 (see `Model/AGENTS.md`) |

## For AI Agents

### Working in this directory
- 实体严格按 `.cursorrules`：继承 `CLDEntityBase`，遗留表用 `[SugarColumn(ColumnName="F_XXX")]` 大写映射；不存在的基类字段加 `[SugarColumn(IsIgnore = true)]`。
- DTO 字段命名以 camelCase 居多（前端直接消费），属于历史约定，新增 DTO 沿用风格保持一致性。
- 不要在此项目引用 `Poxiao.Systems.Interfaces` 或 `Poxiao.Systems`（避免循环依赖）。

### Common patterns
- 实体类 `[SugarTable("BASE_XXX")]` + 多租户 `[Tenant(ClaimConst.TENANTID)]`。
- DTO 类标 `[SuppressSniffer]` 阻止动态 API 自动暴露。

## Dependencies
### Internal
- `api/src/modularity/common/Poxiao.Common`

### External
- SqlSugar、Mapster

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
