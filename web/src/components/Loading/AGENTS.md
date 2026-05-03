<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# Loading

## Purpose
全局加载遮罩组件。在 `ant-design-vue` `Spin` 之上提供 fixed/absolute 切换、暗黑/亮色主题、以及命令式调用 API（`useLoading` / `createLoading`），主要服务于路由切换、表格加载、模态框中的局部 loading 等场景。

## Key Files
| File | Description |
|------|-------------|
| `index.ts` | 同时导出 `Loading` 组件、`useLoading` Hook 与 `createLoading` 工厂函数。 |

## Subdirectories
| Directory | Purpose |
|-----------|---------|
| `src/` | 组件、工厂、hook、类型定义 (see `src/AGENTS.md`) |

## For AI Agents

### Working in this directory
- 命令式 API 与组件式用法共存，新增能力时务必同时更新 `Loading.vue` 的 props 与 `LoadingProps` 类型。
- 不要直接 `withInstall`，本模块按需 import 不注册全局组件。

### Common patterns
- `createLoading(props, target?, wait?)` 创建独立 VNode，`useLoading` 是其 Composition API 包装，返回 `[open, close, setTip]`。

## Dependencies
### Internal
- `/@/enums/sizeEnum`
### External
- `ant-design-vue`（`Spin`）

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
