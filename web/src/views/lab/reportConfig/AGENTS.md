<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# reportConfig

## Purpose
"报告配置"页面：维护月度报表/驾驶舱使用的统计列与字段映射（合格列、不合格列、班次分组等）。系统级条目（`isSystem`）不可删除。

## Key Files
| File | Description |
|------|-------------|
| `index.vue` | BasicTable + 新增按钮 + 编辑/删除（系统级隐藏删除）|

## Subdirectories
| Directory | Purpose |
|-----------|---------|
| `components/` | 配置抽屉表单 (see `components/AGENTS.md`) |

## For AI Agents

### Working in this directory
- `record.isSystem` 决定是否显示删除按钮（系统配置不允许）。
- API：`/@/api/lab/reportConfig`；变更后 `monthlyReport` / `monthly-dashboard` 需重新加载。

### Common patterns
- `useDrawer` 注册 `ReportConfigDrawer` + `success` 回调 reload。

## Dependencies
### Internal
- `/@/api/lab/reportConfig`, `/@/components/Drawer`, `/@/components/Table`
### External
- `ant-design-vue`

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
