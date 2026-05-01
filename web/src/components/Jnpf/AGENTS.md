<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# Jnpf

## Purpose
JNPF 业务表单控件库 — 把 ant-design-vue 原生输入控件（Input / Select / DatePicker 等）与检测室系统业务控件（OpenData、UserSelect、AreaSelect、PopupSelect、RelationForm、Calculate、Cron、Sign 等）统一封装。供 `BasicForm`、`FormGenerator/Parser`、各业务页面复用，是表单层的"组件中台"。

## Subdirectories
| Directory | Purpose |
|-----------|---------|
| `Alert/` | 信息提示条封装 (`JnpfAlert`)（见 `Alert/AGENTS.md`） |
| `AreaSelect/` | 省/市/区树形级联选择器 (`JnpfAreaSelect`)（见 `AreaSelect/AGENTS.md`） |
| `AutoComplete/` | 自动补全输入框 (`JnpfAutoComplete`) |
| `Barcode/` `Qrcode/` | 条形码/二维码生成显示控件 |
| `Button/` | 业务按钮（带权限/确认） |
| `Calculate/` | 公式计算字段（依赖其它字段值） |
| `Cascader/` `TreeSelect/` | 级联与树选择 |
| `Checkbox/` `Radio/` `Rate/` | 多选/单选/评分 |
| `ColorPicker/` `IconPicker/` | 颜色/图标选择 |
| `Cron/` | Cron 表达式生成器 |
| `DatePicker/` | 日期/日期范围/月/周选择 |
| `Divider/` `Link/` `GroupTitle/` | 分割线/链接/分组标题 |
| `Input/` `InputNumber/` `Textarea/` `InputGroup/` `InputSearch/` `InputTable/` | 文本/数值/子表格输入 |
| `NumberRange/` | 数值范围 |
| `OpenData/` | 自动渲染当前用户/部门/时间等内置字段 |
| `Organize/` `OrganizeSelect/` `DepSelect/` `PosSelect/` `RoleSelect/` `UserSelect/` `UsersSelect/` `GroupSelect/` | 组织/部门/岗位/角色/用户选择 |
| `PopupAttr/` `PopupSelect/` | 弹窗选择字段（关联实体） |
| `RelationForm/` `RelationFormAttr/` | 关联表单字段 |

## For AI Agents

### Working in this directory
- 命名约定：组件名 `JnpfXxx`，目录名 `Xxx`，文件结构 `Xxx/index.ts` + `Xxx/src/Xxx.vue`（可选 `props.ts`）。
- 所有控件通过 `withInstall` 包装，既支持 tree-shaking import 也支持 `app.use()` 全局注册。
- 业务控件 (`OpenData`、`UserSelect`、`PopupSelect`) 依赖后端 API，禁止把它们当成纯 UI 控件抽取到无后端环境；改用 `Input` 等基础控件回退。

### Common patterns
- props 多以独立 `props.ts` 导出，便于 `index.ts` 一并 re-export 类型。
- 内部使用 `useAttrs({ excludeDefaultKeys: false })` 透传 antdv 原生属性。

## Dependencies
### Internal
- `/@/utils` (`withInstall`)、`/@/components/BasicTree` 与 `/@/components/Modal`（弹窗类控件）、`/@/api/system/*`（人员组织相关）
### External
- `ant-design-vue`、`lodash-es`

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
