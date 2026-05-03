<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# components

## Purpose
顶部栏内可复用的功能小部件集合。包含用户下拉、面包屑、错误日志按钮、全屏、消息抽屉（站内消息/系统/流程/公告/日程）、通知、关于/声明 Modal、重置密码表单。

## Key Files
| File | Description |
|------|-------------|
| `index.ts` | 导出 `UserDropDown`/`LayoutBreadcrumb`/`ErrorAction` (createAsyncComponent) 与同步导出的 `FullScreen`/`Notify` |
| `MessageDrawer.vue` | 站内消息抽屉：分类筛选、未读切换、滚动加载、跳转消息中心 |
| `Breadcrumb.vue` | 基于路由匹配的面包屑导航 |
| `FullScreen.vue` | 切换浏览器全屏 |
| `Notify.vue` | 简易通知图标按钮 |
| `ErrorAction.vue` | 错误日志计数 + 跳转错误页 |
| `ResetPwdForm.vue` | 强制初始密码修改表单（与 `LayoutHeader.initData` 配合） |
| `AboutModal.vue` / `StatementModal.vue` | 关于本系统 / 法律声明弹窗 |

## Subdirectories
| Directory | Purpose |
|-----------|---------|
| `chat/` | 即时聊天 (IM) 抽屉与表情数据 (see `chat/AGENTS.md`) |
| `user-dropdown/` | 头像下拉菜单（个人/系统切换/退出） (see `user-dropdown/AGENTS.md`) |

## For AI Agents

### Working in this directory
- 异步小组件统一在 `index.ts` 通过 `createAsyncComponent` 暴露，新增组件请同样懒加载并写入 barrel。
- `MessageDrawer` 与后端 IM/通知接口耦合，调整字段时同步 `web/src/api/oa/message`。
- 各 Modal 控制权放在父级 (`LayoutHeader` / UserDropDown)，组件本身仅 emit register/visible。

### Common patterns
- 文件命名 PascalCase；图标使用 `icon-ym-*` 类（项目自有 iconfont）。
- 使用 `BasicDrawer` / `BasicModal` 二次封装的容器，搭配 `useDrawer`/`useModal` register 模式。

## Dependencies
### Internal
- `/@/components/{Drawer,Modal,Application}`、`/@/api/oa/message`、`/@/store/modules/user`。
### External
- `ant-design-vue` (`Avatar`/`Dropdown`/`Menu`/`Badge`/`Switch`)、`@ant-design/icons-vue`。

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
