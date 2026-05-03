<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# propPanel

## Purpose
工作流流程设计器 (`FlowProcess`) 的右侧节点属性配置抽屉面板。`index.vue` 是顶层 `BasicDrawer`，根据当前选中的节点类型 (start/timer/condition/approver/subFlow) 动态渲染对应配置子面板，承担所有流程节点的可视化属性编辑职责。

## Key Files
| File | Description |
|------|-------------|
| `index.vue` | 节点属性面板主容器，按节点类型分发到子面板，处理 ok/cancel 与表单提交 |
| `StartNode.vue` | 发起节点配置：基本信息、表单设置、发起人范围、字段权限、按钮设置 |
| `ApproverNode.vue` | 审批节点配置：审批人、表单权限、节点事件、节点通知、超时处理（最大文件） |
| `ConditionNode.vue` | 条件节点配置：分支条件字段、比较运算符、数据值、与/或逻辑 |
| `SubFlowNode.vue` | 子流程节点配置：选择子流程及参数传递规则 |
| `TimerNode.vue` | 定时器节点配置（轻量） |
| `FlowFormModal.vue` | 选择关联流程表单的模态框 |
| `FlowModal.vue` | 选择/嵌套流程的模态框 |
| `MsgModal.vue` / `MsgTemplateDetail.vue` | 节点消息通知模板配置 |
| `FormulaModal.vue` | 公式编辑模态框，用于条件/赋值表达式 |

## For AI Agents

### Working in this directory
- 所有面板均使用 `<a-tabs>` 切换"基础设置/高级设置/表单权限/节点事件/节点通知/超时处理"标签页，新增标签需保持一致 UI 模式。
- 表单字段绑定 `formConf` 响应式对象，由父级通过 `v-bind="getBindValue"` 注入，禁止直接修改父级状态，使用 emit/事件回调。
- 流程节点判定逻辑（`isStartNode`、`isApproverNode` 等）来自 `../helper/util.ts`（`NodeUtils`），节点默认配置来自 `../helper/config.ts`。
- Chinese-only labels in templates; keep wording aligned with existing JNPF workflow terminology.

### Common patterns
- `<script setup>` + Composition API + `useDrawer`/`useMessage` hooks
- `cloneDeep` from `lodash-es` 在打开抽屉时深拷贝节点数据，避免脏写。

## Dependencies
### Internal
- `/@/components/Drawer` — `BasicDrawer` 容器
- `/@/components/Jnpf` — `jnpf-select` 等表单控件
- `/@/api/workFlow/formDesign` — 表单元数据
- `../helper/util`、`../helper/config` — 节点工具与默认配置
### External
- `ant-design-vue`, `lodash-es`, `vue`

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
