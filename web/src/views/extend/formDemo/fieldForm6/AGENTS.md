<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# fieldForm6

## Purpose
联动选择字段示例：演示客户选择、商品选择器（弹层选择 + 自动回填编码 / 规格 / 单位 / 单价等关联字段），常用于销售单、出入库单等业务表单。

## Key Files
| File | Description |
|------|-------------|
| `index.vue` | 客户与商品选择联动表单：`jnpf-select` 客户名称 + 自定义 `GoodsModal` 商品弹层 |

## For AI Agents

### Working in this directory
- 商品选择走 `GoodsModal` 弹层 + `onGoodsSelect(record)` 回填，多个只读字段通过 `record` 同步。
- 客户名称改为接口加载选项时，改用 `request` 模式或 `useRequest` 拉取，避免静态 mock。
- 演示中 `dataForm.CustomerCode` 与 `客户名称` 共享同一字段名属于示例疏漏，业务化时分别命名 `customerId` / `customerName`。

### Common patterns
- `addonAfter` slot 触发选择弹层并把选中数据 emit 回主页面。
- 所有联动字段在主表单中保持 `readonly` 防止用户改写。

## Dependencies
### Internal
- `/@/components/Container`
### External
- `ant-design-vue`

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
