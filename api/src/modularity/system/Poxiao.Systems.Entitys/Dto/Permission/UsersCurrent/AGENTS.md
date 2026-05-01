<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# UsersCurrent (Dto)

## Purpose
当前登录用户上下文 DTO。覆盖个人信息、机构、岗位、签名图、主题、语言、密码修改、默认机构切换、个人系统日志等场景，被前端登录后大量调用。

## Key Files
| File | Description |
|------|-------------|
| `UsersCurrentInfoOutput.cs` | 当前用户基础信息 |
| `UsersCurrentInfoUpInput.cs` | 当前用户信息修改输入 |
| `UsersCurrentAuthorizeOutput.cs` | 授权聚合输出（菜单 + 按钮 + 列 + 数据权限） |
| `UsersCurrentDefaultOrganizeInput.cs` | 切换默认机构 |
| `UsersCurrentSubordinateOutput.cs` | 我的下属 |
| `UsersCurrentSignImgOutput.cs` | 签名图 |
| `UsersCurrentSysLanguage.cs` / `UsersCurrentSysTheme.cs` | 语言/主题保存 |
| `UsersCurrentActionsModifyPasswordInput.cs` | 修改密码 |
| `CurrentUserOrganizesOutput.cs` / `CurrentUserPositionsOutput.cs` | 当前用户机构/岗位列表 |
| `UsersCurrentSystemLogOutput.cs` / `UsersCurrentSystemLogQuery.cs` | 个人操作日志 |

## For AI Agents

### Working in this directory
- 这些 DTO 是登录态高频接口，结构改动需要同时更新前端 `useUserStore`、`permission` store 与菜单渲染逻辑。
- 不要在这里加密码相关 Output。

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
