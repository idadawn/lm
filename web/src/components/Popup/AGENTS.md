<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# Popup

## Purpose
全屏覆盖型的「Popup」组件，定位绝对、覆盖父容器（常用于侧边抽屉式页面、详情面板替代 modal 的全屏场景）。提供与 `BasicModal` 一致的命令式 API（`usePopup` / `usePopupInner`）。

## Key Files
| File | Description |
|------|-------------|
| `index.ts` | 通过 `withInstall` 暴露 `BasicPopup`，并导出 `usePopup` / `usePopupInner` 与 typing。 |

## Subdirectories
| Directory | Purpose |
|-----------|---------|
| `src/` | 主体、props、hooks (see `src/AGENTS.md`) |

## For AI Agents

### Working in this directory
- `BasicPopup` 与 `BasicModal` 共用类似的注册/调用范式，新增的能力（如 `closeFunc`、`changeOkLoading`）须在 `usePopup` 与 `usePopupInner` 同步。
- 与 Modal 不同，Popup 不是独立 portal，而是嵌入业务父容器，因此请勿手工 `getContainer` 到 body，避免破坏布局。

## Dependencies
### Internal
- `/@/utils`、`./src/typing`、`./src/usePopup`
### External
- `ant-design-vue`

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
