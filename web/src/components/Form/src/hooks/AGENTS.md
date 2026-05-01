<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# hooks

## Purpose
`BasicForm` 的 Composition API 业务逻辑层，每个 hook 负责一类正交关注点：实例注册、字段事件、值处理、高级折叠、自动聚焦、上下文注入、标签宽度计算。`BasicForm.vue` 通过组合这些 hook 完成主要功能。

## Key Files
| File | Description |
|------|-------------|
| `useForm.ts` | 对外暴露的命令式 API hook；维护 `formRef`、提供 `register/setProps/getFieldsValue/...` 委托到内部 `FormActionType` |
| `useFormEvents.ts` | 字段相关动作集合：`setFieldsValue`、`resetFields`、`updateSchema`、`appendSchemaByField`、`removeSchemaByField`、`validate` 等 |
| `useFormValues.ts` | 默认值/初始值计算、空值处理、time/dateRange 字段映射、`handleFormValues` 提交前转换 |
| `useAdvanced.ts` | 高级搜索折叠：根据屏幕断点计算 `colSpan`，决定哪些字段在折叠状态隐藏（基于 `BASIC_COL_LEN=24`） |
| `useFormContext.ts` | `provide/inject` 表单上下文，子 `FormItem` 用其访问父表单状态 |
| `useFormEvents.ts` | (见上) — 实际操作 `formModel` 的事务性逻辑入口 |
| `useAutoFocus.ts` | 第一个可聚焦字段的自动聚焦 |
| `useLabelWidth.ts` | 计算单个 FormItem 的 label 宽度（合并全局与 schema 局部） |
| `useComponentRegister.ts` | 运行期向 `componentMap` 注册自定义组件 |

## For AI Agents

### Working in this directory
- 严守"hook 不耦合渲染"约定：hook 只接收 `Ref/ComputedRef` 与回调，不直接调用组件实例方法。
- 修改 `useFormEvents` 中的方法签名时，须同步更新 `types/form.ts` 的 `FormActionType` 接口。
- 新增 hook 必须在 `BasicForm.vue` 的 setup 中注入并解构暴露给 `formActionType`，否则 `useForm` 调用方拿不到。

### Common patterns
- Hooks 使用 `unref()`/`toRaw()` 防止响应式污染；mutations 通过 `await nextTick()` 保证 DOM 同步。
- 用 `error()` (来自 `/@/utils/log`) 输出契约违规警告，而不是 `console.error`。

## Dependencies
### Internal
- `../types/form`、`../types/hooks`、`../helper`、`/@/utils/is`、`/@/utils/log`
- `/@/hooks/event/useBreakpoint`
### External
- `vue`、`@vueuse/core`、`lodash-es`、`ant-design-vue`

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
