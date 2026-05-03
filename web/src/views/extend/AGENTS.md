<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# extend

## Purpose
扩展功能演示与工具集合（“扩展”菜单根目录）。聚合与核心 LIMS 业务无强耦合的辅助页面：条码/二维码、海量数据列表、文档管理、邮件、表单/图表 Demo、电子签名签章、地图、甘特图、排程、销售订单等示例与扩展模块，多数提供给最终用户作为参考实现或可选附加功能。

## Subdirectories
| Directory | Purpose |
|-----------|---------|
| `barCode/` | 二维码 / 条形码生成 demo（见 `barCode/AGENTS.md`） |
| `bigData/` | 海量数据列表（虚拟滚动 / 分批加载示例） |
| `document/` | 企业网盘式文档管理（文件夹 + 共享 + 上传） |
| `documentPreview/` | 文档在线预览（本地预览 + 永中云预览） |
| `email/` | 邮箱模块（收发件、星标、草稿、配置） |
| `formDemo/` | 表单组件示例集合（字段、验证、动态字段） |
| `graphDemo/` | 图表示例（ECharts + Highcharts） |
| `importAndExport/`, `map/`, `order/`, `printData/`, `projectGantt/`, `saleOrder/`, `schedule/`, `signature/`, `signet/`, `tableDemo/` | 其他扩展演示子模块 |

## For AI Agents

### Working in this directory
- 该目录主要承载演示/扩展功能；新增 LIMS 核心业务页面应放到对应业务目录而非这里。
- 子模块约定：每个子目录通常以 `index.vue` 为入口，复杂模块再拆 `Form.vue`、`Detail.vue` 等同级组件。
- API 统一在 `/@/api/extend/<feature>` 下，配置/枚举走 `/@/api/system`、`/@/api/systemData`。

### Common patterns
- `defineOptions({ name: 'extend-<feature>' })` 命名约定便于 keep-alive。
- 页面外壳沿用 `page-content-wrapper` / `page-content-wrapper-center` 三段式布局。

## Dependencies
### Internal
- `/@/api/extend/*`, `/@/components/{Form,Table,Modal,Popup,Drawer,Chart}`
### External
- `ant-design-vue`, `echarts`, `highcharts`, `jsbarcode`, `qrcode`, `dayjs`

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
