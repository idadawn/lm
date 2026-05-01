<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# views

## Purpose
Page-level Vue 3 SFCs for the Laboratory Data Analysis System frontend. Each subdirectory is a feature area, generally mirroring a backend module under `api/src/modularity/`. Pages are wired into routes in `web/src/router/` and import shared widgets from `web/src/components/`.

## Subdirectories
| Directory | Purpose |
|-----------|---------|
| `ai/` | AI 助手配置 page (system prompt, greetings) (see `ai/AGENTS.md`) |
| `basic/` | Framework basics: login, lock, home/dashboard, profile, error log, exception, redirect, iframe, message records (see `basic/AGENTS.md`) |
| `collectorManagement/` | 采集器与标签管理 — collector list and tag (普通/逻辑) editors with history charts (see `collectorManagement/AGENTS.md`) |
| `common/` | Reusable runtime hosts: dynamic data report, dynamic dictionary, dynamic model (form/list), portals, external links, short-link forms |
| `extend/` | Demo / extended showcases (charts, components, examples) |
| `generator/` | Code generator pages (front-end / back-end / app) |
| `kpi/` | KPI / 关键指标 management |
| `lab/` | 实验室核心业务 — mirrors `api/src/modularity/lab` (sample, test, report, etc.) |
| `model/` | 模型管理 |
| `msgCenter/` | 消息中心 |
| `onlineDev/` | 在线开发 (visualDev / portal / app design) |
| `permission/` | RBAC: user, role, organization, position, group, userRelation |
| `prediction/`, `predictionManagement/` | 预测相关 |
| `service/` | 服务/接口/任务 |
| `system/` | 系统管理 — mirrors `api/src/modularity/system` |
| `systemData/` | 字典 / 数据接口 / 区域 / 元数据 |
| `template/`, `warning/`, `workFlow/` | 模板、预警、工作流模块 |

## For AI Agents

### Working in this directory
- Each feature dir typically contains `index.vue` (entry, used by router meta/`relationId`), optional `*.data.ts` for column/form schemas, and `components/` for page-local widgets.
- Use `<script setup lang="ts">`, import shared bits via `/@/components`, `/@/api`, `/@/hooks`, `/@/store`.
- Keep pages thin — heavy logic belongs in `/@/hooks/` or store modules. UI is Ant Design Vue.
- Folder casing is mixed (camelCase like `collectorManagement` and lower like `basic`); preserve existing names.

### Common patterns
- `useTable` + `BasicTable` + modal `Form.vue` CRUD trio.
- Dynamic pages (`common/dynamicModel`, `common/dynamicDictionary`) read `route.meta.relationId` to load configurations from `onlineDev` APIs.

## Dependencies
### Internal
- `web/src/api/`, `web/src/components/`, `web/src/hooks/`, `web/src/store/`, `web/src/router/`
### External
- `vue`, `vue-router`, `ant-design-vue`, `@vueuse/core`, `dayjs`, `echarts` (via `/@/components/Chart`)

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
