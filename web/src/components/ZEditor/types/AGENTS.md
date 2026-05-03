<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# types

## Purpose
ZEditor 的 TypeScript 类型集中文件。主要描述规则 (`RuleType`)、规则列表 (`RuleListType`) 与状态选项 (`MetricCovStatusOptionType`),供 `editorForm` 与 `ruleList/ruleForm` 共享。

## Key Files
| File | Description |
|------|-------------|
| `type.ts` | `RuleType`(operators / type / value / minValue / maxValue / status*),`MetricCovStatusOptionType`(id / name / color) |

## For AI Agents

### Working in this directory
- 字段名与后端 `metricCov` 接口契约对齐,新增字段前先核对 `/@/api/createModel/model` 的请求/响应结构。
- `id / covId / level / value / minValue / maxValue` 同时支持 `string | number`,序列化提交时统一转 string,避免后端类型校验失败。

### Common patterns
- 仅放纯类型,不放枚举(枚举在 `components/editorForm/const.ts`)。

## Dependencies
### Internal
- 被 `../components/editorForm/*` 引用

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
