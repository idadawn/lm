<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# User (Dto)

## Purpose
用户 DTO。LIMS 中最核心的 DTO 集合之一，覆盖 CRUD、查询、选择器、IM 列表、导入/导出、密码重置、默认值获取等丰富场景。

## Key Files
| File | Description |
|------|-------------|
| `UserCrInput.cs` / `UserUpInput.cs` | 用户创建/更新（含账户、姓名、机构、角色、岗位等） |
| `UserInfoOutput.cs` | 用户详情 |
| `UserListOutput.cs` / `UserListAllOutput.cs` / `UserPageListOutput.cs` | 普通列表/全量/分页输出 |
| `UserListQuery.cs` / `UserConditionInput.cs` | 查询条件 |
| `UserSelectorOutput.cs` / `UserSelectedInput.cs` / `UserSelectedOutput.cs` | 选择器 |
| `UserExportDataInput.cs` / `UserImportDataInput.cs` / `UserImportDataOutput.cs` / `UserListImportDataInput.cs` | 导入导出 |
| `UserResetPasswordInput.cs` | 重置密码 |
| `IMUserListOutput.cs` | 站内 IM 用户列表 |
| `GetDefaultCurrentValueInput.cs` | 获取默认值（流程审批中使用） |

## For AI Agents

### Working in this directory
- 字段调整必须同步 `Mapper/PermissionMapper.cs`（HeadIcon 前缀、fullName 拼接等）。
- 导入导出 DTO 与 Excel 列顺序绑定，调整字段顺序会影响下载模板。
- 密码相关字段（`password`/`secretkey`）只在 Reset/Cr/Up Input 中出现，永远不要放到任何 Output。

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
