<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# order

## Purpose
订单管理演示页面，集成工作流（FlowParser）发起审批：列表 + 行展开（订单商品 / 收款计划子表）+ 审核状态 Tag + 流程发起。是 `extend` 模块中工作流接入的参考实现。

## Key Files
| File | Description |
|------|-------------|
| `index.vue` | 主页面：`BasicTable` + `expandedRowRender` 展示嵌套子表，`currentState` 1-6 映射成不同颜色的审核 Tag；通过 `getFlowIdByCode`、`getFlowList` 选流程后调用 `FlowParser` 发起审批。 |

## For AI Agents

### Working in this directory
- 调用三个 API：`getOrderList`、`getOrderEntryList`、`getOrderPlanList`（来自 `/@/api/extend/order`），子表懒加载在 `expand` 时触发，避免列表初始过慢。
- 状态码 1-6 的语义请保持一致：1 等待审核 / 2 通过 / 3 退回 / 4 撤回 / 5 终止 / 6 挂起 / 其他 = 等待提交。
- 与工作流模块耦合：`FlowParser` 来自 `/@/views/workFlow/components/FlowParser.vue`；不要复制其逻辑到本目录。

### Common patterns
- 双 `BasicTable`（嵌入展开行）+ `useModal` 选流程弹窗 + `usePopup` 表单弹窗；汇总行使用 `<a-table-summary>`。

## Dependencies
### Internal
- `/@/api/extend/order`、`/@/api/workFlow/flowEngine`、`/@/components/Table`、`/@/components/Modal`、`/@/components/Popup`、`/@/components/Container`、`/@/views/workFlow/components/FlowParser.vue`
### External
- `ant-design-vue`、`vue`
