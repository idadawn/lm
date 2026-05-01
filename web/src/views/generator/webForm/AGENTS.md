<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# webForm

## Purpose
普通/流程通用表单生成器：列表 + 三步设计向导（基础设置 → 表单设计 → 列表设计）。`enableFlow` 字段决定渲染类型徽标（流程表单/普通表单）。共享 `DownloadModal` / `PreviewModal` 完成代码下载与在线预览。

## Key Files
| File | Description |
|------|-------------|
| `index.vue` | 列表页：BasicTable + `enableFlow` 渲染 + 操作列；嵌入两个共享弹窗。 |
| `Form.vue` | 全屏 BasicModal 向导：步骤条三步、可切换是否开启列表设计 (`webType=2`)，`FormGenerator` + `BasicColumnDesign`。 |

## For AI Agents

### Working in this directory
- 三步向导第二步「列表设计」可被 `closeList` 按钮关闭并退回，状态由 `webType`/`activeStep` 控制；改流程时同步 `maxStep`。
- 命名 `defineOptions({ name: 'generator-webForm' })`。
- 与 flowForm 的差异主要是步骤数与是否包含列表设计；公共逻辑（主从表配置、保存）保持一致。

### Common patterns
- `BasicColumnDesign` 同时维护 web/app 两套列定义（`columnData` / `appColumnData`）
- `@toggleWebType` 在子组件向父级回传步骤切换

## Dependencies
### Internal
- `/@/api/onlineDev/visualDev`, `/@/api/systemData/dataSource`, `/@/api/systemData/dataModel`, `/@/components/Modal`, `/@/components/Form`, `/@/components/Table`, `../DownloadModal.vue`, `../PreviewModal.vue`

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
