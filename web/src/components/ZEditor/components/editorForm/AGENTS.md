<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# editorForm

## Purpose
ZEditor 节点详情抽屉表单。提供"指标数据 / 分级 / 通知 / 规则"四类配置 Tab,负责拉取指标列表、消息模板,处理父节点选择、规则增删改、分级创建等业务逻辑。

## Key Files
| File | Description |
|------|-------------|
| `index.vue` | 主表单容器,Tab 切换 + a-form 字段(父节点 / 名称 / 指标 / 通知模板/列表) |
| `Grading.vue` | 分级面板,创建/编辑数值/百分比阈值与状态色 |
| `ruleList.vue` | 规则列表展示("大于/区间/小于"语义化文案 + 编辑/通知/删除操作) |
| `ruleForm.vue` | 规则编辑模态框,基于 BasicModal,三种 `AddRuleTypeEnum` 操作符 |
| `previewWarning.vue` | 规则预警预览组件 |
| `const.ts` | `NodeTabsEnum / AddRuleTypeEnum / GradingTypeEnum` 等枚举 |
| `props.ts` | 表单组件 props |

## For AI Agents

### Working in this directory
- 父节点选择仅在 `parentId === '-1'` 时显示,通过 `gotParentId` 单独绑定避免污染原表单。
- 规则操作符共三种 (`GreaterThan / Between / LessThan`),新增类型需同步 `ruleForm` 与 `ruleList` 的渲染分支。
- 字段变化触发 `updateItem`(blur 提交),不要改成 input 实时提交,避免高频请求。

### Common patterns
- 表单 `colon=false`、`labelCol style.width=40px`,与项目其他抽屉表单风格一致。
- 选项编辑统一使用 `jnpf-select` 包装,而非裸 a-select。

## Dependencies
### Internal
- `/@/components/Modal` (`BasicModal`)、`jnpf-*` 全局组件
- `../../types/type.ts`、`./const.ts`
### External
- Ant Design Vue,`@ant-design/icons-vue`

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
