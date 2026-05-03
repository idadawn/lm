<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# engine

## Purpose
代码生成与可视化开发引擎模块根目录。当前仅承载 `Poxiao.VisualDev.Engine` 项目，是低代码 / 在线开发功能的核心，负责模板解析、表单/列表元数据加工以及前后端代码生成的所有底层算法。该模块被 `visualdev`（可视化开发服务层）调用以产出可部署到 `web/` 与 `api/src/modularity/*` 的实际页面与服务代码。

## Subdirectories
| Directory | Purpose |
|-----------|---------|
| `Poxiao.VisualDev.Engine/` | 引擎主项目（模板解析、CodeGen、Mapper、Security 帮助类等）。详见 `Poxiao.VisualDev.Engine/AGENTS.md` |

## For AI Agents

### Working in this directory
- 引擎与 `visualdev/Poxiao.VisualDev.Interfaces` 紧耦合，新增依赖务必经过 Interfaces 层而非直接引用 `Poxiao.VisualDev`。
- `engine` 输出的产物路径由 `KeyVariable.SystemPath/CodeGenerate` 决定（见 `Security/CodeGenTargetPathHelper.cs`），调试时不要直接修改生成目录。
- 引擎不持有自己的 EFCore/SqlSugar 表，所有元数据来源都是 `VisualDevEntity` 等已存在的实体。

### Common patterns
- 解析阶段（`Core/`）→ 模型组装（`Model/`）→ 安全/控件分类（`Security/`）→ 输出代码或 JSON。
- 大量基于 `PoxiaoKeyConst.*` 控件标识的 `switch` 派发逻辑。

## Dependencies
### Internal
- `api/src/modularity/visualdev/Poxiao.VisualDev.Interfaces`
- `api/src/modularity/system/Poxiao.Systems.Interfaces`
- `api/src/modularity/workflow/Poxiao.WorkFlow.Entitys`

### External
- Mapster, Newtonsoft.Json, SqlSugar

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
