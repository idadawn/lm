<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# Model

## Purpose
扩展模块跨 DTO 共用的内嵌 Model。常作为订单 / 表格演示 / 产品录入中的"行项"出现，通过 JSON 序列化保存到主表的字段或独立子表。

## Key Files
| File | Description |
|------|-------------|
| `CollectionPlanModel.cs` | 收款计划行（receivableDate/Rate/Money/Mode/state/sortCode...） |
| `GoodsModel.cs` | 商品行（goodsId/goodsName/specifications/unit/qty/price/amount/cess/actualPrice/actualAmount...） |
| `PostilModel.cs` | 表格演示批注模型 |
| `ProductEntryMdoel.cs` | 产品录入条目（注意拼写 `Mdoel`，保持兼容） |

## For AI Agents

### Working in this directory
- 所有类标 `[SuppressSniffer]` 关闭命名警告，属性 camelCase。
- `ProductEntryMdoel.cs` 是历史拼写错误的文件名/类名 —— 不要批量改名，会破坏序列化与已部署数据。
- 金额相关字段使用 `decimal?`；计量字段（qty/cess/discount）也保持 nullable decimal，避免 0 与"未填"的二义性。

## Dependencies
### External
- `Poxiao.DependencyInjection`（仅 `[SuppressSniffer]`）

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
