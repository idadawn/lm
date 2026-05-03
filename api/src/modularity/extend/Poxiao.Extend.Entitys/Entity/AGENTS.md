<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# Entity

## Purpose
扩展模块的 SqlSugar 实体。所有实体继承 `CLDEntityBase`，表名使用 `EXT_*` 前缀，字段以 `[SugarColumn(ColumnName = "F_XXX")]` 大写下划线显式映射。

## Key Files
| File | Description |
|------|-------------|
| `BigDataEntity.cs` | 大数据演示实体 |
| `DocumentEntity.cs` / `DocumentShareEntity.cs` | 文档与文档分享 |
| `EmailConfigEntity.cs` / `EmailReceiveEntity.cs` / `EmailSendEntity.cs` | 邮箱配置 / 收件 / 发件 |
| `EmployeeEntity.cs` | 职员（EXT_EMPLOYEE：F_ENCODE/F_FULLNAME/F_GENDER...） |
| `OrderEntity.cs` / `OrderEntryEntity.cs` / `OrderReceivableEntity.cs` | 订单主表 / 商品行 / 收款计划 |
| `ProductEntity.cs` / `ProductClassifyEntity.cs` / `ProductCustomerEntity.cs` / `ProductEntryEntity.cs` / `ProductgoodsEntity.cs` | 产品体系（注意 `Productgoods` 类名小写 g） |
| `ProjectGanttEntity.cs` | 项目+任务统一表 |
| `ScheduleEntity.cs` | 日程 |
| `TableExampleEntity.cs` | 表格演示 |
| `WorkLogEntity.cs` / `WorkLogShareEntity.cs` | 工作日志 + 抄送 |

## For AI Agents

### Working in this directory
- 字段命名严格遵循 `.cursorrules`：基类字段名混用（`F_Id`、`F_TenantId` 来自 `OEntityBase`；`F_CREATORTIME/F_CREATORUSERID/F_ENABLEDMARK` 来自 `CLDEntityBase` 全大写；`F_LastModifyTime/F_LastModifyUserId/F_DeleteMark/F_DeleteTime/F_DeleteUserId` 大小写混用）。新表使用标准基类字段，遗留表用 `[SugarColumn(ColumnName="...")]` 覆盖；缺失字段加 `[SugarColumn(IsIgnore=true)]`。
- 业务字段一律 `string?/long?/decimal?/DateTime?` 这种可空类型。
- 不要在实体里写业务方法，所有逻辑放到 `Poxiao.Extend/<Feature>Service.cs`。

## Dependencies
### Internal
- `Poxiao.Infrastructure.Contracts.CLDEntityBase`

### External
- SqlSugar

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
