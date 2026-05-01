<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# Order

## Purpose
订单模块 DTO。包含订单 CRUD、列表查询、详情、上下条切换、收款计划与商品/客户行项 DTO。

## Key Files
| File | Description |
|------|-------------|
| `OrderCrInput.cs` / `OrderUpInput.cs` | 创建/更新订单（含商品行 + 收款计划） |
| `OrderListQuery.cs` | 列表查询（startTime/endTime/enabledMark + 分页） |
| `OrderListOutput.cs` / `OrderInfoOutput.cs` | 列表 / 详情输出 |
| `OrderActionsPrevOutput.cs` / `OrderActionsNextOutput.cs` | 上一条 / 下一条 |
| `OrderCollectionPlanOutput.cs` | 收款计划（基于 `OrderReceivableEntity`） |
| `OrderItemsOutput.cs` / `OrderGoodsOutput.cs` / `OrderCustomerOutput.cs` | 行项 / 商品 / 客户摘要 |

## For AI Agents

### Working in this directory
- `OrderCollectionPlanOutput` 通过 Mapper 把 `Abstract → fabstract`，金额类用 string 输出避免精度丢失。
- 与工作流挂钩，状态字段 `enabledMark`/`flowState` 须与 `WorkFlow.Entitys` 一致。

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
