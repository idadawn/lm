<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# Poxiao.WorkFlow.Entitys

## Purpose
工作流引擎的实体 / DTO / 枚举 / 节点属性模型集中工程。包含 SqlSugar 持久化实体（`FLOW_*`、`WFORM_*` 表）、HTTP 入参出参 DTO、状态/节点/经办类型枚举，以及流程节点 JSON 反序列化使用的 `Properties` / `Item` / `Conifg` 模型。

## Key Files
| File | Description |
|------|-------------|
| `Poxiao.WorkFlow.Entitys.csproj` | 项目工程文件 |

## Subdirectories
| Directory | Purpose |
|-----------|---------|
| `Dto/` | HTTP 入参/出参 DTO（按 FlowEngine/FlowTemplate/FlowTask 等子域分组） (see `Dto/AGENTS.md`) |
| `Entity/` | SqlSugar 实体（FLOW_TASK, FLOW_TEMPLATE, FLOW_ENGINE, …） (see `Entity/AGENTS.md`) |
| `Enum/` | 流程状态、节点类型、审批人类型枚举 (see `Enum/AGENTS.md`) |
| `Mapper/` | Mapster 映射注册（DTO ↔ Entity ↔ Properties） (see `Mapper/AGENTS.md`) |
| `Model/` | 节点属性、模板 JSON、参数容器、配置项与子项 (see `Model/AGENTS.md`) |

## For AI Agents

### Working in this directory
- 流程节点 JSON 解析模型集中在 `Model/Properties/`，禁止把节点属性散落到 Entity/DTO 里。
- 所有 DTO 类必须打 `[SuppressSniffer]`，避免被自动 Sniffer 注入产生重复参数。
- 实体一律继承 `CLDEntityBase`（除 `WorkFlowForm/*Entity` 继承 `OEntityBase<string>`），字段重命名遵循 `.cursorrules`。

### Common patterns
- DTO 命名：列表查询 `*ListQuery`、列表输出 `*ListOutput`、详情输出 `*InfoOutput`、创建 `*CrInput`、更新 `*UpInput`。
- 节点属性通过 Mapster 在 `Mapper/Mapper.cs` 集中映射；新增节点类型时同步注册映射。

## Dependencies
### Internal
- `framework/Poxiao/*`（`Infrastructure.Contracts`、`Infrastructure.Filter`、`DependencyInjection`、`Infrastructure.Const`）
### External
- SqlSugar、Mapster

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
