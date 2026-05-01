<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# Model

## Purpose
引擎使用的纯数据 POCO 集合，承载来自前端 visual-design JSON 的反序列化结构：表单（`FormDataModel`）、控件（`FieldsModel/ConfigModel/PropsModel`）、列表配置（`ColumnDesignModel/ColumnOptionsModel`）、子表（`TableModel`）、数据库表关系（`DbTableRelationModel`）等。`CodeGen/` 子目录另存代码生成专用模型。

## Key Files
| File | Description |
|------|-------------|
| `FormDataModel.cs` | 表单顶层模型（areasName / className / serviceDirectory / module / formRef 等） |
| `FieldsModel.cs` | 单个控件模型（Config + Slot + 各类样式/校验） |
| `ConfigModel.cs` | 控件 config 节点（label / tag / poxiaoKey / children 等） |
| `ColumnDesignModel.cs` | 列表列 + 搜索字段设计 |
| `ColumnOptionsModel.cs` | 列表分页/排序/工具栏选项 |
| `TableModel.cs` | 子表配置 |
| `DbTableRelationModel.cs` | 主子表关系/外键 |
| `EntityFieldModel.cs` | 实体字段（用于代码生成 entity 渲染） |
| `IndexGridFieldModel.cs` / `IndexSearchFieldModel.cs` | 列表网格/搜索条件 |
| `OptionsModel.cs` / `PropsModel.cs` / `PropsBeanModel.cs` | 控件选项与属性集合 |
| `RegListModel.cs` / `SlotModel.cs` / `UploaderTemplateJsonModel.cs` | 校验、插槽、上传模板 |

## Subdirectories
| Directory | Purpose |
|-----------|---------|
| `CodeGen/` | 代码生成专用模型（前端配置、按钮、字段映射、表关系等） |

## For AI Agents

### Working in this directory
- 全部属性使用 camelCase（与前端 JSON 一致）。`[SuppressSniffer]` 已用于关闭 StyleCop 命名警告。
- 新字段先在前端表单设计器 schema 中确认，否则反序列化得到 null 不会报错但下游 CodeGen 会静默缺失。
- 不要在此处加业务逻辑/校验——这里只放 POCO 与极简 computed property（如 `LowerPrimaryKey` 之类）。

## Dependencies
### Internal
- 所有调用方在 `../Core/` 与 `../Security/`

### External
- `Poxiao.DependencyInjection`（仅为 `[SuppressSniffer]`）

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
