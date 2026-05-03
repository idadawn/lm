<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# WorkFlowForm (Entity)

## Purpose
内置业务表单的 SqlSugar 实体目录。与 `Poxiao.WorkFlow.Entitys/Dto/WorkFlowForm/*` 配对，存储独立业务数据（不混入 FLOW_* 流程表）。所有实体继承 `OEntityBase<string>`，并标注 `[Tenant(ClaimConst.TENANTID)]` 实现多租户。

## Key Files
| File | Description |
|------|-------------|
| `LeaveApplyEntity.cs` | `WFORM_LEAVEAPPLY`：请假申请主表（申请人 / 类型 / 起止时间 / 天数 / 小时 / 附件） |
| `SalesOrderEntity.cs` | `WFORM_SALESORDER`：销售订单主表（客户 / 业务员 / 发票 / 付款 / 销售日期） |
| `SalesOrderEntryEntity.cs` | `WFORM_SALESORDER_ENTRY`：销售订单明细行（商品 / 数量 / 单价） |

## For AI Agents

### Working in this directory
- 实体使用 `OEntityBase<string>`（包含 Id / TenantId / 审计字段），不同于 FLOW_* 实体的 `CLDEntityBase`。
- 表名前缀 `WFORM_` 用于区分流程引擎自身的表与业务表单表。
- 添加新表单：复制现有实体，调整 `[SugarTable]` + 列名，并保留版本/作者/日期注释（保持 Poxiao 原始 header）。

### Common patterns
- 所有金额字段使用 `decimal?`，时间使用 `DateTime?`。
- 流程关联字段固定：`F_FLOWID / F_FLOWTITLE / F_FLOWURGENT / F_BILLNO`。

## Dependencies
### Internal
- `framework/Poxiao/Infrastructure.Contracts`、`Infrastructure.Const`
### External
- SqlSugar

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
