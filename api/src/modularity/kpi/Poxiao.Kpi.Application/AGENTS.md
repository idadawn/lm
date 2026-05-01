<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# Poxiao.Kpi.Application

## Purpose
KPI 模块的应用层程序集 (`net10.0`)。集中放置 DTO、Mapster `IRegister` 映射器以及实现业务用例的服务类，依赖 `Poxiao.Kpi.Core` 与公共框架，向 `Poxiao.Kpi.Web.Core` 控制器暴露能力。

## Key Files
| File | Description |
|------|-------------|
| `Poxiao.Kpi.Application.csproj` | 引用 MailKit / MathNet.Numerics (alias `MathNetOfficial`) / NCalcSync，依赖 `Poxiao.Common.Core` 与 `Poxiao.Systems.Interfaces` |
| `GlobalUsings.cs` | 项目级全局 `using`：Mapster、NCalc、SqlSugar、`Poxiao.Infrastructure.*`、`Poxiao.Kpi.Core.Entitys/Enums` 等 |
| `GlobalSuppressions.cs` | StyleCop / 分析器抑制规则 |

## Subdirectories
| Directory | Purpose |
|-----------|---------|
| `Dtos/` | 入参 (`*CrInput/*UpInput/*QueryInput`) 与出参 (`*Output/*InfoOutput/*ListOutput`) 数据传输对象 (see `Dtos/AGENTS.md`) |
| `Mapper/` | Mapster `IRegister` 实现，处理实体↔DTO 的 JSON 字段转换 (see `Mapper/AGENTS.md`) |
| `Services/` | 业务服务实现及 `I*Service` 接口，按功能子领域拆分 (see `Services/AGENTS.md`) |

## For AI Agents

### Working in this directory
- 新建功能时按 `Dtos/<Domain>/`、`Services/<Domain>/I*Service.cs + *Service.cs` 平行落地。
- 服务类构造注入 `ISqlSugarRepository<TEntity>`，并实现 `ITransient` 以便 DI 容器自动发现。
- DTO 全部带 `[SuppressSniffer]` 和 `[JsonProperty("…")]`，前端字段名是 camelCase，C# 属性是 PascalCase。
- 与基础设施交互通过 `Poxiao.Infrastructure.*` 与 `Poxiao.Systems.Interfaces`（如 `IDbLinkService`、`IUserManager`、`IInfluxDBManager`）。

### Common patterns
- 实体到 DTO 用 `entity.Adapt<TOutput>()`，复杂 JSON 字段在 `Mapper/` 中显式映射。
- 树形列表先 `ToListAsync()` 再调用 `.ToTree("-1")` 扩展。
- 分页使用 `ToPagedListAsync(currentPage, pageSize)` + `PagedResultDto<T>.SqlSugarPageResult(...)`。
- 错误码统一 `Oops.Oh(ErrorCode.K100xx)`。

## Dependencies
### Internal
- `Poxiao.Kpi.Core`（实体、枚举、`DbSchemaOutput`）
- `Poxiao.Common.Core`、`Poxiao.Systems.Interfaces`、`Poxiao.Message.Entitys`

### External
- Mapster, SqlSugar, Newtonsoft.Json, NCalcSync, MathNet.Numerics, MailKit

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
