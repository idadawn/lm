<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# System (Interfaces)

## Purpose
系统级服务接口。声明菜单/按钮/列/表单/数据权限、字典、数据接口、库连接、消息/短信模板、调度、版本、单据规则、系统配置等服务契约。

## Key Files
| File | Description |
|------|-------------|
| `IModuleService.cs` | 菜单/功能管理（核心，被 Permission 与多数业务模块依赖） |
| `IModuleButtonService.cs` / `IModuleColumnService.cs` / `IModuleFormService.cs` | 按钮/列/表单字段权限契约 |
| `IModuleDataAuthorizeService.cs` / `IModuleDataAuthorizeSchemeService.cs` | 数据权限规则与方案 |
| `IDictionaryTypeService.cs` / `IDictionaryDataService.cs` | 数据字典分类与明细 |
| `IDataInterfaceService.cs` | 数据接口配置与运行 |
| `IDbLinkService.cs` | 多库连接管理 |
| `IMessageTemplateService.cs` / `ISmsTemplateService.cs` | 站内信/短信模板 |
| `IScheduleService.cs` | 业务排班 |
| `ISynThirdInfoService.cs` | 第三方数据同步 |
| `ISysConfigService.cs` | 系统配置 K-V 读写 |
| `IBillRullService.cs` | 单据编号规则（注意拼写为 "Rull"，历史保留） |
| `IVersionService.cs` | 系统版本号 |

## For AI Agents

### Working in this directory
- 文件名 `IBillRullService.cs` 拼写错误（应为 `IBillRule`），但已被多模块引用，保持现状不要重命名。
- 跨模块取字典请使用 `IDictionaryDataService`，已带缓存逻辑；不要绕开它直接查 `BASE_DICTIONARYDATA`。
- 数据接口在底层支持 HTTP 与多库 SQL 两种方式，新增接口实现请保持二者兼容。

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
