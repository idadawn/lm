<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# Page

## Purpose
页面级布局组件。`PageWrapper` 提供带头部 / 内容 / 底部三段结构，自动计算内容区高度（用于固定高度表格）；`PageFooter` 是固定在底部的操作栏。是项目大多数业务页面的根容器。

## Key Files
| File | Description |
|------|-------------|
| `index.ts` | 通过 `withInstall` 暴露 `PageWrapper` 与 `PageFooter`。 |

## Subdirectories
| Directory | Purpose |
|-----------|---------|
| `src/` | 两个组件实现 (see `src/AGENTS.md`) |

## For AI Agents

### Working in this directory
- 业务页应优先使用 `PageWrapper` 而不是直接拼 div，以便统一头部 / 高度计算 / 间距规范。
- 不要把 `PageFooter` 作为独立组件嵌套使用，约定通过 `PageWrapper` 的 `leftFooter` / `rightFooter` 插槽渲染。

## Dependencies
### Internal
- `/@/utils`
### External
- `ant-design-vue`

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
