<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# DbLink (Dto)

## Purpose
数据库链接（外部库连接配置）DTO。提供 CRUD、连接测试、详情、列表、选择器。

## Key Files
| File | Description |
|------|-------------|
| `DbLinkCrInput.cs` / `DbLinkUpInput.cs` | 创建/更新（数据库类型、连接串、用户名、密码） |
| `DbLinkInfoOutput.cs` | 详情 |
| `DbLinkListInput.cs` / `DbLinkListOutput.cs` | 列表查询/输出 |
| `DbLinkSelectorOutput.cs` | 选择器 |
| `DbLinkActionsTestInput.cs` | 测试连接（前端"测试"按钮入参） |

## For AI Agents

### Working in this directory
- 密码字段在保存时由 Service 加密；Output 中不暴露明文密码（只显示 ****）。
- 数据库类型枚举值（SqlServer/MySql/Oracle）必须与 SqlSugar 支持类型匹配。

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
