<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# Module (Dto)

## Purpose
菜单/功能 DTO。承载菜单树渲染、菜单 CRUD、按钮/列/表单授权基类、菜单选择器、节点输出、导出等丰富场景。

## Key Files
| File | Description |
|------|-------------|
| `ModuleAuthorizeBase.cs` | 菜单授权基类（被 Button/Column/Form/DataAuthorize DTO 共用） |
| `ModuleCrInput.cs` / `ModuleUpInput.cs` | 菜单创建/更新 |
| `ModuleInfoOutput.cs` | 详情 |
| `ModuleListOutput.cs` / `ModuleListQuery.cs` | 列表/查询条件 |
| `ModuleNodeOutput.cs` | 树节点（子节点列表） |
| `ModuleOutput.cs` | 通用菜单输出 |
| `ModuleSelectorOutput.cs` / `ModuleSelectorAllOutput.cs` | 选择器（按系统/全部） |
| `ModuleExportInput.cs` | 导出菜单配置（备份/迁移） |

## For AI Agents

### Working in this directory
- `ModuleAuthorizeBase` 是其他权限 DTO 的基类，调整字段会传染到 ModuleButton/ModuleColumn/ModuleForm/ModuleDataAuthorize。
- 菜单类型枚举见 `Enum/MenuType.cs`。

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
