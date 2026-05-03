<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# document

## Purpose
企业文档管理（类网盘）扩展模块。提供「全部文档 / 我的共享 / 收到的共享 / 回收站」多 tab，支持文件夹层级浏览（面包屑导航）、文件上传、共享、重命名、预览等操作。

## Key Files
| File | Description |
|------|-------------|
| `index.vue` | 文档主页：左侧 tab 区分类，右侧 `BasicTable` + 面包屑 + 上传/新建按钮 |
| `Form.vue` | 文件夹新建 / 文件重命名表单弹层（`BasicModal` + `BasicForm`） |
| `FolderTree.vue` | 移动文件时使用的目标文件夹树选择器 |
| `FileUploader.vue` | 文件上传弹层组件 |
| `Preview.vue` | 文件预览组件（按扩展名分发到内置 viewer） |
| `UserBox.vue` | 共享给用户/部门的人员选择弹层 |

## For AI Agents

### Working in this directory
- 文件类型图标统一通过 `toFileExt(record.fileExtension)` 得到 `icon-ym icon-ym-extend-*` 类名；新增格式同步更新工具方法。
- 文件大小展示走 `toFileSize(record.fileSize)`；勿在模板里直接处理字节数。
- 共享标记 `record.isShare`、`record.type`（0=文件夹 / 1=文件）决定列渲染分支。

### Common patterns
- `levelList` 维护当前路径栈，面包屑回退通过 `handleJump(item, i)` 截断；`openFolder` 入栈。
- 表格行操作通过 `getTableActions` / `getDropDownActions` 返回 `TableAction` 配置数组。

## Dependencies
### Internal
- `/@/api/extend/document`
- `/@/components/{Form,Modal,Table}`
### External
- `ant-design-vue`

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
