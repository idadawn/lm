<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# Product

## Purpose
产品主体 DTO。负责产品本身（不含分类、客户、商品行）的 CRUD 与查询。

## Key Files
| File | Description |
|------|-------------|
| `ProductCrInput.cs` / `ProductUpInput.cs` | 产品创建/更新入参 |
| `ProductListQueryInput.cs` | 列表查询（关键字 + 分类 + 分页） |
| `ProductListOutput.cs` | 列表项 |
| `ProductInfoOutput.cs` | 详情 |

## For AI Agents

### Working in this directory
- 产品分类、客户、商品行分别在同级 `ProductClassify/`、`ProductCustomer/`、`ProductGoods/` 目录下，不要把它们的字段塞回 `ProductInfoOutput`。

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
