<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# ProductCustomer

## Purpose
产品客户 DTO。客户管理只暴露列表（其他读写复用基础接口）。

## Key Files
| File | Description |
|------|-------------|
| `ProductCustomerListOutput.cs` | 客户列表项（编号/名称/联系人/电话） |

## For AI Agents

### Working in this directory
- 客户实体 `ProductCustomerEntity` 是订单里 `CustomerId/CustomerName` 的来源，新增展示字段记得同步订单详情。

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
