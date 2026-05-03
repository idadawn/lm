<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# system

## Purpose
系统管理模块视图根：聚合区域、单据规则、缓存、常用语、图标、标签、日志、菜单、监控、通知、打印开发、系统配置、定时任务等所有系统级运维页面。所有子目录均为页面级 SFC，配 `/@/api/system/*` 接口。

## Subdirectories
| Directory | Purpose |
|-----------|---------|
| `area/` | 区域 (省市区) 维护 (see `area/AGENTS.md`) |
| `billRule/` | 单据规则配置（含 .bb 导入） (see `billRule/AGENTS.md`) |
| `cache/` | 系统缓存查看与清理 (see `cache/AGENTS.md`) |
| `commonWords/` | 常用语 / 系统级提示词 (see `commonWords/AGENTS.md`) |
| `icons/`, `label/`, `log/`, `menu/`, `monitor/`, `notice/`, `printDev/`, `sysConfig/`, `task/` | 其他系统功能（不在本批 deepinit 范围） |

## For AI Agents

### Working in this directory
- 所有列表页统一布局：`page-content-wrapper-center > page-content-wrapper-content > BasicTable`。
- 启用/禁用字段统一为 `enabledMark`，`a-tag` 颜色 `success/error`。
- `defineOptions({ name: 'system-xxx' })` 用于 keep-alive；多个文件复用同一 `name='system-task'` 是历史遗留。

### Common patterns
- 绝大多数子目录都是 `index.vue` + `Form.vue` 两文件结构，CRUD 模板高度一致。

## Dependencies
### Internal
- `/@/api/system/*`、`/@/components/Table`、`/@/components/Modal`、`/@/components/Popup`

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
