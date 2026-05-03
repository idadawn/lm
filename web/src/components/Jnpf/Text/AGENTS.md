<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# Text

## Purpose
Jnpf 表单引擎中的纯文本展示组件。封装一个仅渲染段落的轻量控件，常用于在低代码表单里嵌入说明文字、标签或只读内容。

## Key Files
| File | Description |
|------|-------------|
| `index.ts` | 通过 `withInstall` 暴露 `JnpfText`；这是模块的唯一对外入口。 |

## Subdirectories
| Directory | Purpose |
|-----------|---------|
| `src/` | 组件实现（`Text.vue`） (see `src/AGENTS.md`) |

## For AI Agents

### Working in this directory
- 入口必须保持 `JnpfText` 命名以与 Jnpf 表单引擎的 `jnpfKey` 映射对应。
- 不要在 `index.ts` 内引入样式或副作用代码，所有样式封装在 `src/Text.vue`。

### Common patterns
- `withInstall(Component)` 让组件既可全局 `app.use` 也可局部按需引入。

## Dependencies
### Internal
- `/@/utils`（`withInstall`）

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
