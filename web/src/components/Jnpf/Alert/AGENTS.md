<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# Alert

## Purpose
`JnpfAlert` — JNPF 风格的提示条控件，对 `ant-design-vue` 的 `<Alert>` 进行轻量包装。统一 `title` 字段名（vs antdv 的 `message`）、统一默认值 (`type='warning'`、`showIcon=false`、`closable=false`)，便于在 `BasicForm` 与 `FormGenerator` schema 中以一致 API 配置。

## Key Files
| File | Description |
|------|-------------|
| `index.ts` | 桶文件：通过 `withInstall` 导出 `JnpfAlert` |

## Subdirectories
| Directory | Purpose |
|-----------|---------|
| `src/` | `Alert.vue` 实现（见 `src/AGENTS.md`） |

## For AI Agents

### Working in this directory
- `title` 是该组件的"业务字段名"，但底层映射到 antdv 的 `message`（见 `src/Alert.vue` 中 `:message="title"`）；schema 配置时使用 `title`。
- 通过 `omit({ ...attrs, ...props }, ['title'])` 透传所有 attrs，但避免重复绑 message。
- 组件无 v-model — 它仅用于显示，不持有值。

### Common patterns
- vben-admin `withInstall` 模式 + `defineOptions({ name: 'JnpfAlert', inheritAttrs: false })`。

## Dependencies
### Internal
- `/@/utils` (`withInstall`)、`/@/hooks/core/useAttrs`
### External
- `ant-design-vue`、`lodash-es`

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
