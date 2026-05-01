<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# flowForm

## Purpose
流程表单生成器：列表（`getVisualDevList` + `searchInfo.type=3`）+ 两步设计向导（基础设置 → 表单设计）。生成的工件可通过共享 `DownloadModal` 下载源码、`PreviewModal` 在线预览。

## Key Files
| File | Description |
|------|-------------|
| `index.vue` | 列表页：BasicTable + 新建/编辑/复制/删除/下载/预览操作列。 |
| `Form.vue` | 全屏 BasicModal 向导：步骤条 (基础设置 / 表单设计)；主表/从表关联字段；`FormGenerator` 渲染表单设计器。 |

## For AI Agents

### Working in this directory
- 仅有两步：基础设置（含数据源 + 主从表选择）→ 表单设计；不要把列表设计步骤合进来（那是 webForm 的职责）。
- 主从表通过 `record.typeId == '1'` 区分；切主表用 `changeTable(record)`。
- 命名 `defineOptions({ name: 'generator-flowForm' })`。

### Common patterns
- `useTable` 列表 + 多个 `useModal` 嵌套（Form / Download / Preview）
- 主从表字段联动 `tableField` / `relationField`
- `FormGenerator` 组件接收 `conf` / `formInfo` / `dbType`

## Dependencies
### Internal
- `/@/api/onlineDev/visualDev`, `/@/api/systemData/dataSource`, `/@/api/systemData/dataModel`, `/@/components/Modal`, `/@/components/Form`, `/@/components/Table`, `../DownloadModal.vue`, `../PreviewModal.vue`

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
