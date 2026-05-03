<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# service

## Purpose
服务 (Service) 模块视图根：当前包含数据服务配置与服务观察两个子页。面向数据采集器/标签的运维管理。

## Subdirectories
| Directory | Purpose |
|-----------|---------|
| `dataService/` | 数据服务配置（采集器/标签的添加/导出） (see `dataService/AGENTS.md`) |
| `watch/` | 服务观察：采集器树勾选 + 观察列表的双栏移动 (see `watch/AGENTS.md`) |

## For AI Agents

### Working in this directory
- 两个子页面共用"采集器/标签"概念域（`tagInfos`/`tagId`/`tagName` 字段），但目前没有共享代码——存在抽公共组件的机会。

## Dependencies
### Internal
- `/@/components/Table` (dataService)、`ant-design-vue` 的 `a-tree`（watch）

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
