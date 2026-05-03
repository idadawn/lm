<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# statusManagement

## Purpose
KPI 模块"状态管理"页：维护指标/任务的自定义状态（名称、颜色），通过树形表格展示，支持新增、编辑、删除、成员授权。

## Key Files
| File | Description |
|------|-------------|
| `index.vue` | BasicTable + 颜色色块单元格 + 操作列；树形表格 |
| `DepForm.vue` | 新建/编辑状态表单（含颜色选择）|
| `Member.vue` | 状态成员/授权弹窗 |

## For AI Agents

### Working in this directory
- API 走 `/@/api/status/model`（`getStatusList`/`deleteStatus`）。
- 表格 `isTreeTable: true` + `useSearchForm: true`，搜索栏从 `getFormConfig()` 返回。
- `defineOptions({ name: 'permission-organize' })` —— 注意此 name 与组织模块共享，用于路由 keep-alive 缓存。

### Common patterns
- 颜色单元格：`<div :style="{ background: record.color }" class="w-[70px] h-[20px]"></div>`。

## Dependencies
### Internal
- `/@/api/status/model`
- `/@/components/Table`, `/@/components/Modal`, `/@/hooks/web/useMessage`
### External
- `ant-design-vue`

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
