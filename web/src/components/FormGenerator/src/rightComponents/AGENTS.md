<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# rightComponents

## Purpose
`FormGenerator` 右侧"字段属性"面板组件库 — 每个 `R*.vue` 对应一种字段控件的属性配置 UI（如 `RInput.vue` 配置 Input 字段的默认值/前后缀/图标/最大长度等）。`RightPanel.vue` 根据当前选中字段的 `tag` 动态加载这些组件。

## Key Files
| File | Description |
|------|-------------|
| `RInput.vue` | Input 字段配置：默认值、addonBefore/After、prefixIcon、maxlength、清空、长度校验 |
| `RSelect.vue`/`RRadio.vue`/`RCheckbox.vue` | 选项类控件配置：选项编辑、动态数据源、级联 |
| `RDatePicker.vue` | 日期类控件配置：format、起始/结束、默认值、禁用区间 |
| `RPopupSelect.vue`/`RPopupAttr.vue` | 弹窗选择字段配置：关联实体、显示字段、过滤条件 |
| `RRelationForm.vue`/`RRelationFormAttr.vue` | 关联表单字段配置：选择主表/字段映射 |
| `RCalculate.vue` | 计算字段公式编辑（基于其他字段） |
| `ROrgRight.vue` | 组织/部门/岗位/角色选择字段配置 |
| `RAlert.vue` `RButton.vue` `RDivider.vue` `RGroupTitle.vue` `RLink.vue` `RRow.vue` `RCard.vue` `RCollapse.vue` `RColorPicker.vue` `REditor.vue` `RBarcode.vue` `RQrcode.vue` `RRate.vue` `RInputNumber.vue` `RAreaSelect.vue` `RAutoComplete.vue` `RCascader.vue` | 对应控件的属性面板（一字段一文件） |

## Subdirectories
| Directory | Purpose |
|-----------|---------|
| `components/` | `BatchOperate`/`TreeBatchOperate`/`TreeNodeModal` — 选项批量编辑/树批量编辑模态（见 `components/AGENTS.md`） |
| `RTable/` | 内嵌"子表格"字段的复合配置（`index.vue` + `AddConfigModal`，见 `RTable/AGENTS.md`） |

## For AI Agents

### Working in this directory
- 命名约定：`R + 控件名.vue`，与 `helper/config.ts` 中的 `tag` 一一对应；`RightPanel.vue` 通过约定加载组件。
- 每个 `R*.vue` 通过 props 接收 `activeData`（响应式选中字段对象），双向修改字段属性，禁止用 emit 单点同步。
- 国际化原则：本目录文案保持中文，符合 JNPF 与检测室系统主语言。

### Common patterns
- `<a-form-item>` + `v-model:value="activeData.xxx"` 配置项，少量条件渲染基于 `showType === 'pc'`。
- 复杂面板（`RPopupSelect`、`RRelationForm`）使用 `useModal` 弹出"添加配置"对话框。

## Dependencies
### Internal
- `/@/components/Jnpf`、`/@/components/Modal`、`../helper/config`、`../hooks/useDynamic`
### External
- `ant-design-vue`、`lodash-es`

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
