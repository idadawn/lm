<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# list

## Purpose
列表短链运行时页。匿名访问发布的在线列表（含搜索表单、子表展开、行点击查看详情），用于将动态模型列表对外分享。

## Key Files
| File | Description |
|------|-------------|
| `index.vue` | 列表主页：密码门 + `BasicForm` 搜索 + `BasicTable` 渲染，行点击调起 `detail/index.vue` 查看详情 |

## Subdirectories
| Directory | Purpose |
|-----------|---------|
| `detail/` | 短链列表只读详情弹层（见 `detail/AGENTS.md`） |

## For AI Agents

### Working in this directory
- 列与子表配置来源于服务端 `columnData`，前端不写死字段；`childColumnList` 按 `getChildTableStyle` 选择 tabs 或行内 `ChildTableColumn` 风格。
- 单元格渲染复用 `jnpf-*` 控件 detail 模式（如 `jnpf-input-number`、`jnpf-calculate`），与 `dynamicModel/list` 行为一致但禁写。
- 数据接口必须携带 `state.encryption` 进行短链鉴权。

### Common patterns
- 与 `dynamicModel/list/index.vue` 同构但去掉了登录、按钮权限与编辑/流程相关分支。
- 子表展开通过 `record[`${item.prop}Expand`]` 状态字段控制，避免改写原始 record。

## Dependencies
### Internal
- `/@/api/onlineDev/shortLink`
- `/@/components/{Form,Table,Qrcode}`
- `../../dynamicModel/list/{ChildTableColumn.vue,detail/Parser.vue}`（共享子表与详情解析器）
### External
- `ant-design-vue`

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
