<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# types

## Purpose
`BasicForm` 的 TypeScript 类型契约。定义表单整体 props (`FormProps`)、命令式 API 接口 (`FormActionType`)、字段 schema (`FormSchema`)、以及 `ComponentType` 联合常量。组件实现与 `useForm` 调用方共享这份契约。

## Key Files
| File | Description |
|------|-------------|
| `form.ts` | `FormProps`、`FormSchema`、`FormActionType`、`Rule`、`RenderCallbackParams`、`UseFormReturnType` 等核心类型 |
| `formItem.ts` | `FormItem`/校验规则相关辅助类型，对接 ant-design-vue 的 `RuleObject` |
| `index.ts` | `ColEx`（栅格扩展）与 `ComponentType` 字符串联合（'Input' \| 'Select' \| 'JnpfUserSelect' …） |
| `hooks.ts` | `AdvanceState` 等 hook 内部状态类型（轻量） |

## For AI Agents

### Working in this directory
- 新增 `componentMap` key 必须先在 `index.ts` 的 `ComponentType` 联合中加入对应字符串字面量，否则 `BasicForm.vue` 的 schema 类型校验失败。
- 修改 `FormActionType` 方法签名是 breaking change：所有 `useForm()` 消费者会受影响，请检查 `web/src` 全量引用。
- 不要把运行时函数放在此目录，仅留类型与 `interface`。

### Common patterns
- 使用 `Recordable<T>`、`Nullable<T>` 等全局工具类型（来自 `/@/types/global.d.ts`）。
- `Rule` 类型扩展了 antdv 的 `RuleObject`，加入 `trigger` 取值约束。

## Dependencies
### Internal
- `/@/components/Button`、`/@/components/Table/src/types/table` （类型借用）
### External
- `ant-design-vue/lib/form/interface`、`ant-design-vue/lib/grid/Row`、`vue` (类型)

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
