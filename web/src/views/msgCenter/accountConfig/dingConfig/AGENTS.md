<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# dingConfig

## Purpose
钉钉账号配置页：维护钉钉应用的 AppKey/AppSecret/AgentId 等信息，支持启用/禁用、复制配置、编辑、删除。

## Key Files
| File | Description |
|------|-------------|
| `index.vue` | BasicTable 列表（名称/编码/创建人/修改时间/启用状态）+ 新建按钮 |
| `Form.vue` | 钉钉账号新增/编辑表单 |

## For AI Agents

### Working in this directory
- 路由名 `msgCenter-accountConfig-ding`。
- API 共享 `getConfigList` / `delConfig` / `copy`，调用时按钉钉类型 type 区分。

### Common patterns
- enabledMark 标签 + TableAction 操作列 + dropDown actions（复制等）。

## Dependencies
### Internal
- `/@/api/msgCenter/accountConfig`, `/@/components/Modal`, `/@/components/Table`
### External
- `ant-design-vue`

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
