<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# saleOrder

## Purpose
销售订单（Sale Order）演示页面，主从结构：上方订单主表，下方两张联动子表（订单商品 + 商品明细）。属于 `extend` 模块下复杂列表 + 子表联动的参考实现。

## Key Files
| File | Description |
|------|-------------|
| `index.vue` | 主页面：三张 `BasicTable` 通过 `row-click` 联动；`getOrderList` 拉主表，`getProductEntry` 按订单/商品 ID 取子表数据；状态列固定显示“未审核 / 未通知 / 未关闭”等 Tag。 |
| `Form.vue` | 订单新增 / 编辑大表单（~14KB），含商品明细子表、收款计划、客户选择等复合字段。 |

## Subdirectories
| Directory | Purpose |
|-----------|---------|
| `components/` | 订单内部使用的产品选择 Modal (see `components/AGENTS.md`) |

## For AI Agents

### Working in this directory
- 三表联动：`activeId` 跟随主表行；切换主表行时清空 `productList` 与 `productDetailList`，避免脏数据。
- API 全部走 `/@/api/extend/saleOrder`；删除前用 `useMessage().createConfirm`。
- 顶部按钮区遵循“新建 / 编辑 / 删除 / 更多”顺序，与 `extend` 其它模块保持一致。

### Common patterns
- `reactive({...}) + toRefs` 解构暴露字段；`useTable` 三次注册返回不同 register。
- 操作按钮使用 `icon-ym-btn-add / icon-ym-btn-edit / icon-ym-delete` 三个图标。

## Dependencies
### Internal
- `/@/api/extend/saleOrder`、`/@/components/Table`、`/@/components/Popup`、`/@/hooks/web/useI18n`、`/@/hooks/web/useMessage`
### External
- `@ant-design/icons-vue`、`ant-design-vue`、`vue`
