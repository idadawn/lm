<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# CodeGen (Model)

## Purpose
代码生成专用 POCO。这些模型只在生成 Vue 页面 / C# Service / Entity / Dto 的过程中使用，不会出现在普通业务接口的请求/响应中。包含整体生成配置、前后端控件设计、按钮、列、搜索字段、特殊日期格式、表关系等模型。

## Key Files
| File | Description |
|------|-------------|
| `CodeGenConfigModel.cs` | 顶层配置（FullName/BusName/NameSpace/ClassName/PrimaryKey 等） |
| `FrontEndGenConfigModel.cs` / `CodeGenFrontendConfigModel.cs` | 前端生成配置 |
| `FormControlDesignModel.cs` / `DefaultFormControlModel.cs` | 表单控件设计模型（用于渲染 Form.vue） |
| `FormScriptDesignModel.cs` | 表单脚本设计 |
| `IndexButtonDesign.cs` / `IndexColumnDesign.cs` / `IndexSearchFieldDesignModel.cs` | 列表页按钮/列/查询设计 |
| `CodeGenFunctionModel.cs` | 生成方法描述（add/edit/delete...） |
| `CodeGenTableRelationsModel.cs` | 表关系（一对一/一对多） |
| `TableColumnConfigModel.cs` | 表列配置（DB 字段层） |
| `CodeGenSpecifyDateFormatSetModel.cs` | 日期格式特殊处理 |
| `CodeGenExportPropertyJsonModel.cs` | 导出属性的 JSON 描述 |
| `CodeGenConvFormPropsControlDesign.cs` / `CodeGenConvIndexListControlOptionDesign.cs` | 形态转换中间模型 |
| `CodeGenFormRealControlModel.cs` | 真实控件（已剥离布局） |

## For AI Agents

### Working in this directory
- 命名风格：以 `CodeGen` 开头通常是配置/中间结构；不带前缀的（`FormControlDesignModel`、`IndexButtonDesign` 等）是直接喂给模板渲染的"最终视图模型"。
- `LowerPrimaryKey` 这类 computed 属性需要保持兼容旧模板（生成器里直接以名字插值）。

## Dependencies
### Internal
- 与 `../` 的 `FieldsModel/ConfigModel` 配合使用

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
