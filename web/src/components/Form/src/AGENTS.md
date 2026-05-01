<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# src

## Purpose
`BasicForm` 表单组件的核心实现层。包含主组件、组件注册映射 (`componentMap`)、props 定义、辅助工具与子组件/hooks/types 三个子目录。基于 ant-design-vue `<Form>` + `<Row>` 布局，schema 驱动渲染。

## Key Files
| File | Description |
|------|-------------|
| `BasicForm.vue` | 表单主组件，处理 schema 渲染、模型、提交、重置、高级折叠、插槽透传、`createFormContext` 注入子项 |
| `componentMap.ts` | `ComponentType -> Vue Component` 注册表（StrengthMeter、CountdownInput、几乎所有 JNPF 控件、CreateUser/CreateTime 等业务别名） |
| `props.ts` | 表单 props：schemas、layout、labelWidth、submitButtonOptions、autoFocus、disabled 等 |
| `helper.ts` | `createPlaceholderMessage`、`setComponentRuleType`、`dateItemType` 等基于组件类型推断校验规则与默认值的工具 |

## Subdirectories
| Directory | Purpose |
|-----------|---------|
| `components/` | 单字段渲染 `FormItem.vue` 与底部 `FormAction.vue`（见 `components/AGENTS.md`） |
| `hooks/` | `useForm`、`useFormEvents`、`useFormValues`、`useAdvanced` 等 composables（见 `hooks/AGENTS.md`） |
| `types/` | 表单与 FormItem 的 TypeScript 类型契约（见 `types/AGENTS.md`） |

## For AI Agents

### Working in this directory
- 新增可用组件类型：① 在 `types/index.ts` 的 `ComponentType` 联合类型加 key；② 在 `componentMap.ts` 调用 `componentMap.set('XXX', JnpfXXX)`；③ 必要时在 `helper.ts` 加 placeholder/校验推断分支。
- `BasicForm` 自身不持有数据，`formModel` 通过 `setFormModel` 双向绑定；不要在子组件里直接 mutate `formModel` 之外的字段。
- 高级搜索折叠依赖 `useAdvanced` 计算列宽 (`BASIC_COL_LEN = 24`) — 修改栅格逻辑请同步该 hook。

### Common patterns
- TSX/SFC 混合：`FormItem.vue` 使用 tsx 以便动态创建 `componentMap[type]` 渲染节点。
- 所有 emit 事件命名使用 kebab-case：`advanced-change`、`field-value-change` 等。

## Dependencies
### Internal
- `/@/components/Jnpf` (业务控件)
- `/@/components/Modal` (`useModalContext`)
- `/@/hooks/web/useDesign`、`useI18n`、`usePermission`
### External
- `ant-design-vue`、`@vueuse/core`、`lodash-es`

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
