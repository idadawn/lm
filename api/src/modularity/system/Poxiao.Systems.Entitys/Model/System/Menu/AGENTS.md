<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# Menu (Model)

## Purpose
菜单/功能聚合 Model 集合。在 ModuleService、UsersCurrentService、AuthorizeService 输出菜单树时使用，将 BASE_MODULE / BASE_MODULE_BUTTON / BASE_MODULE_COLUMN / BASE_MODULE_FORM 整合为前端可消费的功能视图。

## Key Files
| File | Description |
|------|-------------|
| `FunctionalBase.cs` | 菜单/功能基类（共有字段：Id、ParentId、FullName、EnCode、Sort 等） |
| `FunctionalModel.cs` | 功能完整 Model：Type、UrlAddress、Icon、systemId 等 |
| `FunctionalButtonModel.cs` | 按钮权限 Model |
| `FunctionalFormModel.cs` | 表单字段权限 Model |
| `FunctionalResourceModel.cs` | 资源（菜单+按钮+列）聚合 |
| `FunctionalViewModel.cs` | 前端菜单树视图（含子节点递归） |

## For AI Agents

### Working in this directory
- 所有 Functional* 都以 `FunctionalBase` 为根基，新增字段优先放基类避免重复。
- 字段命名混用 PascalCase 与 camelCase（如 `Type` vs `systemId`），与前端约定保持一致，不要统一改写。
- `FunctionalViewModel` 是渲染左侧菜单的最终结构，结构变更需联调前端 `web/src/router` 与 `permission` store。

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
