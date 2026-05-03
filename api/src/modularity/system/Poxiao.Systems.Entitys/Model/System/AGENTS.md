<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# System (Model)

## Purpose
系统域复合 Model 集合。表示数据库表/字段抽象、数据接口请求结构、菜单功能聚合、打印数据、系统配置等内部对象。

## Subdirectories
| Directory | Purpose |
|-----------|---------|
| `DataBase/` | 动态库/表/字段对象 (see `DataBase/AGENTS.md`) |
| `DataInterFace/` | 数据接口属性 JSON 与请求参数 Model (see `DataInterFace/AGENTS.md`) |
| `Menu/` | 菜单/功能/按钮/表单/视图聚合 Model（继承 FunctionalBase） (see `Menu/AGENTS.md`) |
| `PrintDev/` | 打印模板字段/SQL/数据 Model (see `PrintDev/AGENTS.md`) |
| `SysConfig/` | 系统配置 Model (see `SysConfig/AGENTS.md`) |

## For AI Agents

### Working in this directory
- Menu 子目录的 `FunctionalBase` 是 ModuleService 树形输出的核心父类，新增功能类型应优先扩展现有 Model。

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
