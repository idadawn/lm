<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# Security

## Purpose
代码生成"安全/规范"帮助类层。这里的 *Helper 不是真正的鉴权安全，而是把生成过程中重复的字段规范化、控件归类、目标路径、统一处理逻辑沉淀到静态类中，供 `Core/`、`CodeGen/` 调用。下分 `BackEnd/`（C# 服务/实体相关）与 `FrontEnd/`（Vue 控件相关）。

## Key Files
| File | Description |
|------|-------------|
| `CodeGenTargetPathHelper.cs` | 计算前/后端各类生成文件的目标路径（基于 `KeyVariable.SystemPath/CodeGenerate/<fileName>/Net`） |
| `CodeGenUnifiedHandlerHelper.cs` | 统一处理表单内控件，识别 PC/App 查询字段、子表递归 |

## Subdirectories
| Directory | Purpose |
|-----------|---------|
| `BackEnd/` | 后端代码生成相关帮助类（控件属性、字段判断、列表按钮、方法配置） |
| `FrontEnd/` | 前端代码生成相关帮助类（表单控件设计、查询控件归类） |

## For AI Agents

### Working in this directory
- 所有 helper 都是 `static` 类，禁止持有可变全局状态（`CodeGenFormControlDesignHelper.active` 已存在但仅在主循环开启时复位，注意线程安全）。
- 修改路径生成时务必同步前后端 zip 打包逻辑（`CodeGenWay`）。

### Common patterns
- 大量按 `webType`（1 纯表单 / 2 表单+列表 / 3 表单+列表+流程）的 switch。
- 输入：`FieldsModel`、`ColumnDesignModel`；输出：`*DesignModel` 或路径字符串。

## Dependencies
### Internal
- `../Model/`、`../Model/CodeGen/`、`Poxiao.Infrastructure.Configuration`

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
