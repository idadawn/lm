<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# FlowForm

## Purpose
流程表单元数据查询输出 DTO。被 `FlowFormService` 用于返回流程表单元信息（formData / flowTemplateJson / tableJson / dbLinkId / urlAddress / appUrlAddress 等），供前端表单设计器或表单渲染器加载。

## Key Files
| File | Description |
|------|-------------|
| `FlowFormListOutput.cs` | 流程表单完整元数据：基本信息 + 表单/流程/草稿 JSON + 关联表 + 数据库连接 + Web/APP 地址 |

## For AI Agents

### Working in this directory
- 该目录仅有输出 DTO；输入参数复用 `FlowEngine/*` 的查询结构。
- 字段是 `FlowEngineEntity` 的扁平投影，更新前端字段时同步检查 Entity 与 Mapper。

### Common patterns
- 字段顺序与 FlowEngineCrInput 保持一致，便于代码互相映射。

## Dependencies
### Internal
- `DependencyInjection`（`SuppressSniffer`）

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
