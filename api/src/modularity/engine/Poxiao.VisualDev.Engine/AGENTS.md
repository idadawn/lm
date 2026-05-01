<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# Poxiao.VisualDev.Engine

## Purpose
可视化开发引擎主项目。包含模板解析（`Core/`）、控件/表单数据模型（`Model/`）、代码生成枚举（`Enum/`）、Mapster 映射（`Mapper/`）、以及代码生成核心算法（`Security/`、`CodeGen/`）。被 `Poxiao.VisualDev` 服务层调用以解析在线开发表单 JSON 并产出 Vue 页面、C# Service/Entity/Dto 等代码。

## Key Files
| File | Description |
|------|-------------|
| `Poxiao.VisualDev.Engine.csproj` | 工程文件，引用 Common.Core / Systems.Interfaces / VisualDev.Interfaces / WorkFlow.Entitys |

## Subdirectories
| Directory | Purpose |
|-----------|---------|
| `CodeGen/` | 代码生成入口枚举/编排（`CodeGenWay`） |
| `Core/` | 模板解析核心（FormDataParsing、TemplateAnalysis、TemplateParsingBase） |
| `Enum/` | 引擎枚举（`VisualDevModelData/vModelType` 等） |
| `Mapper/` | Mapster 类型映射注册（`VisualDev.cs`） |
| `Model/` | 表单/列/控件配置 POCO（含 `CodeGen/` 子集） |
| `Security/` | 代码生成各类 Helper（路径、控件属性、字段判断、前/后端） |

## For AI Agents

### Working in this directory
- `Model/` 内全部 POCO 使用 camelCase 属性名（与前端 JSON 一致），不要改成 PascalCase，会破坏 `JObject.ToObject<T>` 反序列化。
- 添加新控件类型时需要同步：`PoxiaoKeyConst`（在 Common 层）、`Core/TemplateAnalysis.cs` 的 switch 分支、`Security/FrontEnd/CodeGenQueryControlClassificationHelper.cs` 的归类。
- 不要在 `Engine` 内直接持久化数据；仅做内存中模板/代码字符串处理。

### Common patterns
- 大量 `switch (config.poxiaoKey)` 按控件标识派发逻辑。
- 通过 `Mapster` 把前端 JSON（`CodeGenFieldsModel`）映射成强类型 `FieldsModel`。
- 生成的目标路径都基于 `KeyVariable.SystemPath/CodeGenerate/{fileName}/Net`。

## Dependencies
### Internal
- `Poxiao.Common.Core`、`Poxiao.Systems.Interfaces`、`Poxiao.VisualDev.Interfaces`、`Poxiao.WorkFlow.Entitys`

### External
- Mapster、Newtonsoft.Json、SqlSugar

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
