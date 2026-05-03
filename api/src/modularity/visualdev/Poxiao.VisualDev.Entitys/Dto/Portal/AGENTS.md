<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# Portal

## Purpose
门户设计（`PortalEntity` + `PortalDataEntity`）API DTO：CRUD、列表（树形）、表单设计同步、权限授权、导出、外链、管理列表等。

## Key Files
| File | Description |
|------|-------------|
| `PortalCrInput.cs` / `PortalUpInput.cs` | 创建 / 更新门户 |
| `PortalSaveInput.cs` | 保存门户表单设计 JSON（PC/App） |
| `PortalSyncInput.cs` | 跨环境同步门户配置 |
| `PortalAuthInput.cs` | 授权用户/角色/部门 |
| `PortalInfoOutput.cs` / `PortalInfoAuthOutput.cs` | 详情、含授权详情 |
| `PortalListOutput.cs` | 列表行（继承 `TreeModel` 支持树形）；含 pcIsRelease/appIsRelease |
| `PortalListQueryInput.cs` | 列表查询入参 |
| `PortalManageInput.cs` / `PortalManageOutput.cs` | 管理面板的请求/响应 |
| `PortalSelectOutput.cs` | 选择器精简输出 |
| `PortalExportOutput.cs` | 导出 JSON 包结构 |

## For AI Agents

### Working in this directory
- `Type=0` 页面设计 / `Type=1` 自定义路径；`LinkType=0` 内部页面 / `LinkType=1` 外链。
- 表单 JSON（FormData）在 `PortalDataEntity` 按 `Platform`（web/app）+ `Type`（model/release）四个组合存储；保存接口需要把 4 条记录都更新。

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
