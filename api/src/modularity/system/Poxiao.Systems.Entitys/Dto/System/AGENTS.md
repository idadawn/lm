<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# System (Dto)

## Purpose
系统级 API DTO 集合。按功能划分子目录（菜单/字典/数据接口/数据库/调度/日志/打印/省份/模板/单据规则等），与 `Poxiao.Systems/System/` 服务一一对应。

## Subdirectories
| Directory | Purpose |
|-----------|---------|
| `AdvancedQuery/` | 高级查询方案 (see `AdvancedQuery/AGENTS.md`) |
| `BillRule/` | 单据编号规则 (see `BillRule/AGENTS.md`) |
| `ComFields/` | 公共字段定义 (see `ComFields/AGENTS.md`) |
| `CommonWords/` | 常用语 (see `CommonWords/AGENTS.md`) |
| `DataBase/` | 数据库表/字段元信息 (see `DataBase/AGENTS.md`) |
| `DataInterFace/` | 数据接口配置 (see `DataInterFace/AGENTS.md`) |
| `DataInterfaceLog/` | 数据接口调用日志 (see `DataInterfaceLog/AGENTS.md`) |
| `DataSync/` | 数据同步 (see `DataSync/AGENTS.md`) |
| `DbBackup/` | 数据库备份 (see `DbBackup/AGENTS.md`) |
| `DbLink/` | 数据库链接 (see `DbLink/AGENTS.md`) |
| `DictionaryData/` | 字典数据 (see `DictionaryData/AGENTS.md`) |
| `DictionaryType/` | 字典分类 (see `DictionaryType/AGENTS.md`) |
| `InterfaceOauth/` | OAuth 凭据 (see `InterfaceOauth/AGENTS.md`) |
| `MessageTemplate/` | 站内信模板 (see `MessageTemplate/AGENTS.md`) |
| `Module/` | 菜单/功能 (see `Module/AGENTS.md`) |
| `ModuleButton/` | 菜单按钮权限 (see `ModuleButton/AGENTS.md`) |
| `ModuleColumn/` | 菜单列权限 (see `ModuleColumn/AGENTS.md`) |
| `ModuleDataAuthorize/` | 数据权限规则 (see `ModuleDataAuthorize/AGENTS.md`) |
| `ModuleDataAuthorizeLink/` | 数据权限关联 (see `ModuleDataAuthorizeLink/AGENTS.md`) |
| `ModuleDataAuthorizeScheme/` | 数据权限方案 (see `ModuleDataAuthorizeScheme/AGENTS.md`) |
| `ModuleForm/` | 表单字段权限 (see `ModuleForm/AGENTS.md`) |
| `Monitor/` | 服务器监控 (see `Monitor/AGENTS.md`) |
| `OnlineUser/` | 在线用户 (see `OnlineUser/AGENTS.md`) |
| `PortalManage/` | 门户管理 (see `PortalManage/AGENTS.md`) |
| `PrintDev/` | 打印模板设计 (see `PrintDev/AGENTS.md`) |
| `PrintLog/` | 打印日志 (see `PrintLog/AGENTS.md`) |
| `Province/` | 省市区 (see `Province/AGENTS.md`) |
| `ProvinceAtlas/` | 省份地图 GeoJSON (see `ProvinceAtlas/AGENTS.md`) |
| `Schedule/` | 业务排班 (see `Schedule/AGENTS.md`) |
| `SmsTemplate/` | 短信模板 (see `SmsTemplate/AGENTS.md`) |
| `SynThirdInfo/` | 第三方信息同步 (see `SynThirdInfo/AGENTS.md`) |
| `SysCache/` | 系统缓存查看 (see `SysCache/AGENTS.md`) |
| `SysConfig/` | 系统配置 (see `SysConfig/AGENTS.md`) |
| `SysLog/` | 系统日志 (see `SysLog/AGENTS.md`) |
| `System/` | 子系统配置 (see `System/AGENTS.md`) |

## For AI Agents

### Working in this directory
- 命名空间统一 `Poxiao.Systems.Entitys.Dto.{FeatureName}`（不带 System. 段）。
- 新增功能时，**先建子目录**（一个功能一个目录），保持与 Service 一一对应。

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
