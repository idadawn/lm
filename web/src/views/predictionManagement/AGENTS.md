<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# predictionManagement

## Purpose
预测管理 (Prediction Management) 模块根：聚合预测看板与开发测试入口。`prediction/` 与 `views/prediction/overview/` 视图近似，定位为"管理侧"重复展示 + 临时调试占位。

## Subdirectories
| Directory | Purpose |
|-----------|---------|
| `prediction/` | 预测仪表盘视图（与 `views/prediction/overview` 复制粘贴） (see `prediction/AGENTS.md`) |
| `test/` | 占位/调试页（仅 search box 与 content 占位） (see `test/AGENTS.md`) |

## For AI Agents

### Working in this directory
- 重复内容警告：`prediction/` 与 `views/prediction/overview/` 文件几乎一致，是技术债；新增功能请优先抽公共组件而非继续复制。
- `test/index.vue` 为脚手架占位，正式上线前应替换或删除。

## Dependencies
### Internal
- `/@/hooks/web/useECharts`

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
