<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# System (Entity)

## Purpose
系统级 SqlSugar 实体。映射菜单/按钮/列/表单权限、字典、数据接口、库连接、调度、日志、打印、省份、消息模板等 BASE_* 表。

## Key Files
| File | Description |
|------|-------------|
| `ModuleEntity.cs` | `BASE_MODULE` 菜单/功能（目录、页面、功能、字典、报表、大屏、门户、外链） |
| `ModuleButtonEntity.cs` / `ModuleColumnEntity.cs` / `ModuleFormEntity.cs` | 按钮、列、表单字段权限定义 |
| `ModuleDataAuthorizeEntity.cs` / `ModuleDataAuthorizeLinkEntity.cs` / `ModuleDataAuthorizeSchemeEntity.cs` | 数据级权限规则、关联表与方案 |
| `DictionaryTypeEntity.cs` / `DictionaryDataEntity.cs` | 字典分类与字典明细 |
| `DataInterfaceEntity.cs` / `DataInterfaceLogEntity.cs` | 数据接口配置与调用日志 |
| `DbLinkEntity.cs` / `DbBackupEntity.cs` | 多库连接、备份记录 |
| `ScheduleEntity.cs` / `ScheduleLogEntity.cs` / `ScheduleUserEntity.cs` | 任务调度、执行日志、人员关系 |
| `MessageTemplateEntity.cs` / `SmsTemplateEntity.cs` | 站内信/短信模板 |
| `BillRuleEntity.cs` / `ComFieldsEntity.cs` / `CommonWordsEntity.cs` | 单据规则、公共字段、常用语 |
| `PrintDevEntity.cs` / `PrintLogEntity.cs` | 打印模板与打印日志 |
| `ProvinceEntity.cs` / `ProvinceAtlasEntity.cs` | 省市区与地图 GeoJSON |
| `SysLogEntity.cs` / `SysConfigEntity.cs` / `SystemEntity.cs` | 操作日志、系统配置、子系统 |
| `AdvancedQuerySchemeEntity.cs` | 高级查询方案保存 |
| `InterfaceOauthEntity.cs` / `SynThirdInfoEntity.cs` | OAuth 凭据、第三方数据同步 |
| `PortalManageEntity.cs` | 门户首页配置 |

## For AI Agents

### Working in this directory
- `ModuleEntity.Type` 枚举定义见 `../../Enum/MenuType.cs`，新增类型需同步该枚举。
- ModuleDataAuthorize 三件套用法：Scheme（方案） -> Link（与具体表/列） -> Authorize（绑定到角色/用户）。
- 调度任务走 `taskschedule` 模块，本目录仅持久化业务排班。

### Common patterns
- 大量带 `[Tenant(ClaimConst.TENANTID)]` 多租户。
- `SysConfigEntity` 是 K-V 配置承载，配合枚举 `SysConfig` 使用。

## Dependencies
### Internal
- `Poxiao.Infrastructure.Const`、`Poxiao.Infrastructure.Contracts`

### External
- SqlSugar

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
