<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# AutoComplete

## Purpose
`JnpfAutoComplete` — JNPF 自动补全输入框，对 `ant-design-vue` `<AutoComplete>` 的封装。提供统一 props 类型导出 (`AutoCompleteProps`) 与 `withInstall` 全局注册支持。常用于业务表单中"输入即过滤"的字段（如客户名、设备型号搜索建议）。

## Key Files
| File | Description |
|------|-------------|
| `index.ts` | 桶文件：`withInstall(AutoComplete)` 导出 + `AutoCompleteProps` 类型导出（基于 `ExtractPropTypes<typeof autoCompleteProps>`） |

## Subdirectories
| Directory | Purpose |
|-----------|---------|
| `src/` | `AutoComplete.vue` 主体 + `props.ts` props 定义（无单独 AGENTS.md，结构同 AreaSelect） |

## For AI Agents

### Working in this directory
- 类型导出使用 `Partial<ExtractPropTypes<typeof autoCompleteProps>>` 模式，所有 props 自动可选；如要必填字段需在 props.ts 中显式声明 `required: true` 并补类型。
- 选项数据 (`options`/`dataSource`) 由调用方提供，本组件不内置远程数据请求 — 复杂场景请用 `JnpfAutoComplete` + 父组件 `watchEffect` 拉接口。
- 不要给该组件加 `popupSlot` / 额外业务逻辑；保持轻量包装即可。

### Common patterns
- vben-admin 标准 wrapper：`index.ts (桶 + 类型) → src/Component.vue (主体) → src/props.ts (props)`。

## Dependencies
### Internal
- `/@/utils` (`withInstall`)
### External
- `ant-design-vue` (`AutoComplete`)、`vue` (`ExtractPropTypes`)

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
