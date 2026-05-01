<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# Mapper

## Purpose
Mapster 映射注册（`IRegister` 实现）。集中维护 Entity ↔ DTO/Model 的字段映射规则，避免在各 Service 中重复 `.Adapt<T>` 自定义。

## Key Files
| File | Description |
|------|-------------|
| `PermissionMapper.cs` | 权限域映射：UserEntity → UserInfoModel/UserInfoOutput/UserSelectorOutput；机构/部门/角色等 |
| `SystemMapper.cs` | 系统域映射：菜单/字典/数据接口/打印模板等 Entity 与 DTO/Model 互转 |

## For AI Agents

### Working in this directory
- 框架启动会自动扫描 `IRegister` 并注册映射，无需手工 `TypeAdapterConfig.GlobalSettings`。
- 头像 HeadIcon 字段在 `UserEntity → UserInfoModel/UserInfoOutput` 映射时被前缀 `/api/File/Image/userAvatar/`，新增任何头像/文件路径字段请保持同样规则。
- 新增 Entity/Dto 字段后若两边名称不同，必须在此处补 `.Map(dest => ..., src => ...)`，否则数据不会出现在 API 输出。

### Common patterns
- `config.ForType<TSource, TDest>().Map(...)` 链式注册；多个映射可以在同一 `Register` 中串接。
- 计算字段（如 `fullName = RealName + "/" + Account`）也放在这里。

## Dependencies
### Internal
- `Poxiao.Infrastructure.Models.User`（UserInfoModel）
- 同项目 Entity 与 Dto 命名空间

### External
- Mapster

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
