<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# Item

## Purpose
节点 / 表单 JSON 内的「列表项」模型。被 `Properties/*` 与 `Conifg/*` 引用，描述子节点 / 候选项 / 条件项 / 字段规则 / 销售明细行等结构。

## Key Files
| File | Description |
|------|-------------|
| `AssignItem.cs` | 父子流程字段映射项：nodeId/title + ruleList（parentField↔childField）；含内部 `RuleItem` |
| `CandidateItem.cs` | 候选人项（候选节点编码 + 候选用户列表） |
| `ConditionsItem.cs` | 条件分支单条规则：fieldName/symbol/fieldValue/logic + 控件类型 + fieldType(1字段/3聚合) + fieldValueType(1字段/2自定义) |
| `EntryListItem.cs` | 销售订单明细行（商品/数量/单价） |
| `TemplateJsonItem.cs` | 流程模板 JSON 中节点的精简项（兼容旧版） |

## For AI Agents

### Working in this directory
- `ConditionsItem.fieldValue` 类型为 `dynamic`（前端可能是 string / number / array），后端解析时按 `fieldValuePoxiaoKey`（控件类型）做分支。
- 新增项目类型须保留 `[SuppressSniffer]`。

### Common patterns
- 命名以 `*Item` 结尾。
- 项目内可有内部辅助类（如 `RuleItem`），保持小颗粒度。

## Dependencies
### Internal
- `framework/Poxiao/DependencyInjection`

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
