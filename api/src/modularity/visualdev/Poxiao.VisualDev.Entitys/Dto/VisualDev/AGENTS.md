<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# VisualDev

## Purpose
低代码功能模板（`VisualDevEntity`）的 API DTO：CRUD、列表（TreeModel）、字段查询、导入/导出、转菜单、外链填单/查询、Selector。

## Key Files
| File | Description |
|------|-------------|
| `VisualDevCrInput.cs` / `VisualDevUpInput.cs` | 创建 / 更新功能 |
| `VisualDevInfoOutput.cs` | 详情（含 formData/columnData/flowTemplateJson） |
| `VisualDevListOutput.cs` | 列表行（继承 TreeModel；type/webType/pcIsRelease/appIsRelease/enableFlow/接口元信息） |
| `VisualDevListQueryInput.cs` | 列表查询参数 |
| `VisualDevSelectorInput.cs` / `VisualDevSelectorOutput.cs` | 选择器（按分类筛选） |
| `VisualDevDataFieldDataListInput.cs` | 数据字段联动查询 |
| `VisualDevFormDataFieldsOutput.cs` | 表单字段集合输出 |
| `VisualDevImportDataInput.cs` / `VisualDevImportDataOutput.cs` | 导入导出 |
| `VisualDevToMenuInput.cs` | 一键发布到系统菜单 |
| `VisualDevShortLinkInput.cs` / `VisualDevShortLinkInfoOutput.cs` / `VisualDevShortLinkPwdInput.cs` / `VisualdevShortLinkFormInput.cs` | 外链填单/查询配置与公开访问 |

## For AI Agents

### Working in this directory
- `type`：1-Web 设计、3-流程表单、4-Web 表单（与 release 实体对齐，App 类型 2/5 在 release 中存在）。`webType`：1 纯表单/2 表单加列表/3 系统表单/4 数据视图。
- `interfaceId`/`interfaceParam` 用于「数据视图」类型，从外部接口拉数据；运行时由 `RunService` 调用。
- 外链 DTO 的密码字段需配合 `VisualDevShortLinkEntity.FormPassword/ColumnPassword`，前端传明文，后端落库前用 `MD5Encryption.Encrypt`。

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
