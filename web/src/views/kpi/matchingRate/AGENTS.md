<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# matchingRate

## Purpose
KPI 模块"匹配率"演示页：以工程/订单/合同/MRP 系统四个维度的演示数据展示进度阶段（a-steps + 彩色按钮分组）。当前为静态/演示性页面，数据来自本地 JS 文件。

## Key Files
| File | Description |
|------|-------------|
| `index.vue` | 主页：四个维度按钮 + a-steps 时间轴 + 卡片分组 |

## Subdirectories
| Directory | Purpose |
|-----------|---------|
| `data/` | 演示用静态数据集（JS）(see `data/AGENTS.md`) |

## For AI Agents

### Working in this directory
- 该页是占位/演示，将来对接真实接口时应把 `./data/*` 替换为 API 调用。
- `defineOptions({ name: 'matchingRate' })` 用于 `keep-alive` 路由匹配，勿改名。

### Common patterns
- `reactive(state)` + `data: ref<any[]>` + `currentIndex` 切换步骤。

## Dependencies
### Internal
- `./data/*.js`
### External
- `ant-design-vue`, `@ant-design/icons-vue`

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
