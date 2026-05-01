<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# Poxiao.Message.Entitys

## Purpose
消息模块共享实体工程：SqlSugar 实体、DTO、枚举、Mapster 配置、内部 Model。被 `Poxiao.Message`（实现）和 `Poxiao.Message.Interfaces` 共同引用。

## Key Files
| File | Description |
|------|-------------|
| `Poxiao.Message.Entitys.csproj` | 实体工程文件 |

## Subdirectories
| Directory | Purpose |
|-----------|---------|
| `Dto/` | 输入/输出 DTO，按业务域分子目录 (see `Dto/AGENTS.md`) |
| `Entity/` | SqlSugar 实体（`BASE_MESSAGE*` / `BASE_IM*` / `BASE_USER_DEVICE`）(see `Entity/AGENTS.md`) |
| `Enums/` | WebSocket 消息收发类型与方法枚举 (see `Enums/AGENTS.md`) |
| `Mapper/` | Mapster `IRegister` 配置 (see `Mapper/AGENTS.md`) |
| `Model/` | 跨层共享视图 Model（IM 未读、模板参数等）(see `Model/AGENTS.md`) |

## For AI Agents

### Working in this directory
- 实体继承 `CLDEntityBase`（旧表用 `[SugarColumn(ColumnName=...)]` 全大写），`OEntityBase<string>` 用于纯业务子表（IMContent、ImReply、TimeTaskLog 风格）。
- DTO 字段统一 camelCase；前端直接消费，不要重命名。
- 新增渠道凭据字段时，请同步更新 `MessageAccountEntity` 与 DTO，保持租户隔离 (`F_TENANTID`)。

### Common patterns
- 表名前缀 `BASE_MESSAGE_*`、`BASE_IM*`、`BASE_USER_DEVICE`，字段使用全大写下划线（旧约定）。

## Dependencies
### Internal
- `Poxiao.Common.Core`（基类、特性、Filter）
### External
- SqlSugar、Mapster

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
