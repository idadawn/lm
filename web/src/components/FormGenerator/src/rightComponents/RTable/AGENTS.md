<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# RTable

## Purpose
"子表格" (InputTable) 字段的属性配置面板。它不同于普通字段的单文件 `R*.vue`，因为子表格本身是一个嵌套设计器：父表单中嵌入一张表格，表格中每行又可承载多个字段配置。

## Key Files
| File | Description |
|------|-------------|
| `index.vue` | 子表格属性主面板：显示标题、动作文字、`addType` 行为模态切换、合计设置（`showSummary` + `summaryField`）以及"配置表单"按钮（打开嵌套设计弹窗） |
| `AddConfigModal.vue` | "配置表单"嵌套设计弹窗（~9KB）：内嵌迷你 FormGenerator 让用户编辑表格列字段（label、组件、属性） |

## For AI Agents

### Working in this directory
- 子表格字段元数据保存在父字段的 `__config__.children` 中，新增/删除列必须同步两处：本面板与 `helper/db.ts` 中的 drawing list。
- `AddConfigModal` 内的迷你设计器与外层共享 `useGeneratorStore`；提交前需 cloneDeep 隔离，避免污染父级。
- 合计字段 `summaryField` 选项来自 `childrenList`，仅数值类型字段允许参与合计。

### Common patterns
- 通过 `useModal` 注册 + `openModal(true, payload)` 打开嵌套设计弹窗。
- `editConf()` → `registerModal` → `updateConf(value)` 三段式回写。

## Dependencies
### Internal
- `/@/components/Modal`、`/@/components/Jnpf`、`../../helper/config`
### External
- `ant-design-vue`

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
