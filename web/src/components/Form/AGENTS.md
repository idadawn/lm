<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# Form

## Purpose
`BasicForm` 通用表单组件 — 基于 vben-admin 的 schema-driven 表单封装，支持动态字段注册、JNPF 业务控件集成、高级搜索折叠、`useForm` 命令式 API。是检测室系统几乎所有列表筛选与编辑表单的基础设施。

## Key Files
| File | Description |
|------|-------------|
| `index.ts` | 桶文件：导出 `BasicForm`、`useForm`、`useComponentRegister` 与表单类型 |

## Subdirectories
| Directory | Purpose |
|-----------|---------|
| `src/` | 组件实现、componentMap、props 与 helper（见 `src/AGENTS.md`） |

## For AI Agents

### Working in this directory
- 该目录是组件的对外公共 API；新增导出在 `index.ts` 集中维护，类型从 `src/types/form` 与 `src/types/formItem` re-export。
- 使用 `useForm` 命令式 API 时务必通过 `register(formActionType)` 拿到实例，禁止直接 ref 子组件内部状态。
- 业务方使用模式：传入 `schemas: FormSchema[]` 配置 + 使用插槽 `formHeader/formFooter/resetBefore/submitBefore` 自定义行为。

### Common patterns
- Schema field 的 `component` 字段引用 `componentMap` key，例如 `'Input'`、`'JnpfUserSelect'`、`'PopupSelect'`。
- 通过 `useComponentRegister` 在运行期注册自定义组件。

## Dependencies
### Internal
- `/@/components/Jnpf` — 表单控件主体
- `/@/components/StrengthMeter`、`/@/components/CountDown` — 内置扩展控件
### External
- `ant-design-vue`、`@vueuse/core`、`lodash-es`

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
