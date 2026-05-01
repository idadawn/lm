<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# extend

## Purpose
非核心业务的扩展模块根目录。集中存放与 LIMS 主流程关系不强、但常作为周边能力出现的功能：大数据、文档/预览、邮件、职员、订单、产品/分类/客户/录入/商品、项目甘特、日程、表格示例、工作日志等。按"实现 / 实体 / 接口"分为三个 csproj。

## Subdirectories
| Directory | Purpose |
|-----------|---------|
| `Poxiao.Extend/` | 服务层 (各 *Service 动态 API 控制器，详见 `Poxiao.Extend/AGENTS.md`) |
| `Poxiao.Extend.Entitys/` | 实体 / Dto / Mapper / Model (详见 `Poxiao.Extend.Entitys/AGENTS.md`) |
| `Poxiao.Extend.Interfaces/` | 对外暴露的接口（当前为空 csproj，预留扩展） |

## For AI Agents

### Working in this directory
- 这里的功能多为 demo / 周边模块，**不要**把核心 LIMS 数据（检测样本、报告等）混进来。
- 实体表名前缀统一为 `EXT_*`（如 `EXT_EMPLOYEE`、`EXT_ORDER`），新增请保持一致。
- 字段命名遵循 `.cursorrules`：继承 `CLDEntityBase`，业务字段必须以 `[SugarColumn(ColumnName = "F_XXX")]` 显式映射。

### Common patterns
- 服务类：`IDynamicApiController` + `ITransient`，`[ApiDescriptionSettings(Tag = "Extend", Name = "...")]`，路由 `api/extend/[controller]`。
- Dto 命名：`<Feature><Action>Input/Output/Query`，如 `OrderListQuery`、`OrderCrInput`、`OrderInfoOutput`。

## Dependencies
### Internal
- `Poxiao.Common.Core`、`Poxiao.Systems.Interfaces`、`Poxiao.WorkFlow.Interfaces`

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
