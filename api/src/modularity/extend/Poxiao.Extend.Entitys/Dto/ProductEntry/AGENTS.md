<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# ProductEntry

## Purpose
产品录入（入库/录入条目）DTO。和 `ProductGoods` 区别：这里是产品级别的"录入条目"，不是订单的商品行。

## Key Files
| File | Description |
|------|-------------|
| `ProductEntryCrInput.cs` | 录入创建入参 |
| `ProductEntryListOutput.cs` | 录入列表 |
| `ProductEntryInfoOutput.cs` | 录入详情 |

## For AI Agents

### Working in this directory
- 与 `Model/ProductEntryMdoel.cs`（注意原始拼写 Mdoel）配合做内嵌行项；保留拼写避免破坏既有反序列化。

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
