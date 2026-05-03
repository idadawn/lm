<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# System

## Purpose
系统级业务实现。包含菜单/按钮/列/表单权限、字典、数据库与数据接口管理、库链接、打印、消息/短信模板、任务调度、日志、监控、省份地图、单据规则、门户管理、系统配置等核心后台能力。

## Key Files
| File | Description |
|------|-------------|
| `ModuleService.cs` | 菜单/功能（BASE_MODULE）管理；动态 API `Tag="System", Name="Menu"` |
| `ModuleButtonService.cs` | 菜单按钮权限定义 |
| `ModuleColumnService.cs` | 列字段权限定义 |
| `ModuleFormService.cs` | 表单字段权限定义 |
| `ModuleDataAuthorize{,Link,Scheme}Service.cs` | 数据级权限规则、关联与方案 |
| `DictionaryTypeService.cs` / `DictionaryDataService.cs` | 数据字典分类/明细 |
| `DataInterfaceService.cs` / `DataInterfaceNewService.cs` / `DataInterfaceLogService.cs` | 数据接口（HTTP/SQL）配置、调用、日志 |
| `DbLinkService.cs` / `DataBaseService.cs` / `DataSyncService.cs` / `DbBackupService.cs` | 多库连接、表结构、数据同步、备份 |
| `MessageTemplateService.cs` / `SmsTemplateService.cs` | 站内信/短信模板 |
| `ScheduleService.cs` | 业务排班（区别于 Common.ScheduleTaskService 系统调度） |
| `SysLogService.cs` / `MonitorService.cs` / `OnlineUserService.cs` | 操作/异常/登录日志、服务器监控、在线用户 |
| `PrintDevService.cs` / `PrintLogService.cs` | 打印模板设计与打印日志 |
| `ProvinceService.cs` / `ProvinceAtlasService.cs` | 省市区数据与地图 GeoJSON |
| `BillRuleService.cs` / `ComFieldsService.cs` / `CommonWordsService.cs` | 单据编号规则、公共字段、常用语 |
| `SysConfigService.cs` / `SystemService.cs` / `VersionService.cs` | 系统全局配置、子系统、版本号 |
| `AdvancedQueryService.cs` | 高级查询方案保存 |
| `InterfaceOauthService.cs` / `SynThirdInfoService.cs` | OAuth 接入凭据、第三方同步 |
| `SysCacheService.cs` / `PortalManageService.cs` | 缓存查看、门户首页配置 |

## For AI Agents

### Working in this directory
- Module/ModuleButton/ModuleColumn/ModuleForm 四类配合 `ModuleDataAuthorize*` 实现完整 RBAC + 数据权限；前端权限指令依赖此处接口稳定。
- DataInterface 同时存在新旧两套实现（`DataInterfaceService` + `DataInterfaceNewService`），新需求请走 New 版本。
- 涉及多库连接（`DbLink`、`DataBase`、`DataSync`）时注意 SQL Server / MySQL / Oracle 兼容（参见根 CLAUDE.md）。

### Common patterns
- `Order` 段在 200-220。
- 字典缓存：`DictionaryDataService` 使用 `IMemoryCache` 减少高频查询。
- 日志服务读自 `BASE_SYSLOG`/`BASE_LOGEXCEPTION`/`BASE_LOGLOGIN` 等多张分类表。

## Dependencies
### Internal
- `Poxiao.Systems.Interfaces.System`、`Poxiao.Systems.Entitys.System` 与对应 Dto/Model
- `Poxiao.VisualDev.Engine`（可视化开发引擎驱动菜单/表单）
- `workflow/Poxiao.WorkFlow.Interfaces`、`message/Poxiao.Message.Interfaces`、`taskschedule/Poxiao.TaskScheduler.Interfaces`

### External
- SqlSugar、Mapster、SharpZipLib（备份压缩）

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
