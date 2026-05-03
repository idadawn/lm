<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# SalesOrder

## Purpose
销售订单表单的入参 / 输出 DTO，对应 `WFORM_SALESORDER` + `WFORM_SALESORDER_ENTRY`（明细）表与 `SalesOrderService` 控制器。

## Key Files
| File | Description |
|------|-------------|
| `SalesOrderInput.cs` | 销售订单入参：customerName/contacts/contactPhone/customerAddres/ticketNum/ticketDate/invoiceType/paymentMethod/paymentMoney/salesman/salesDate/billNo/...，含 entryList 商品明细 |
| `SalesOrderInfoOutput.cs` | 详情输出（主表 + 明细） |

## For AI Agents

### Working in this directory
- `entryList: List<EntryListItem>` 来自 `Model/Item/EntryListItem.cs`，新增明细字段需要同步 Item 和 `SalesOrderEntryEntity`。
- `SalesOrderInput` 继承 `FlowTaskOtherModel`（候选 / 委托 / 抄送公共参数）。

### Common patterns
- 货币字段使用 `decimal?`；日期字段使用 `DateTime?`。

## Dependencies
### Internal
- `Poxiao.Infrastructure.Models.WorkFlow`、`Poxiao.WorkFlow.Entitys/Model/Item`（`EntryListItem`）

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
