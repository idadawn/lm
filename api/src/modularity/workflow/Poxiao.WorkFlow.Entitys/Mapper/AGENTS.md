<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# Mapper

## Purpose
工作流模块的 Mapster 类型映射注册。集中声明 DTO ↔ Entity ↔ 节点属性 (`Properties`) 之间的字段重命名与字段复用规则。

## Key Files
| File | Description |
|------|-------------|
| `Mapper.cs` | 实现 `IRegister.Register(TypeAdapterConfig)`，含 FlowEngineEntity↔FlowEngineListAllOutput / FlowEngineInfoOutput / FlowEngineCrInput / FlowEngineUpInput / FlowBeforeListOutput、FlowTemplateJsonModel→TaskNodeModel、ChildTaskProperties→ApproversProperties、StartProperties→ApproversProperties 等映射 |

## For AI Agents

### Working in this directory
- 新增 DTO 字段重命名 / 跨类型字段复用必须在此处注册，不要散落到 Service 内 inline 配置。
- 节点属性投影（如 `StartProperties → ApproversProperties`）用于复用审批人解析逻辑，新增节点类型时同步注册。
- `FlowEngineInfoOutput.dbLinkId` 在源为空时被映射为 `"0"`，是历史约定（前端把 0 当作"未绑定数据源"），保留此分支。

### Common patterns
- 每条规则使用 `config.ForType<TSrc, TDest>().Map(dest => dest.X, src => src.Y)`。
- 类访问修饰符为 `internal`，由 Furion 自动扫描注册。

## Dependencies
### Internal
- `Poxiao.Infrastructure.Extension`（`IsEmpty()`）
### External
- Mapster

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
