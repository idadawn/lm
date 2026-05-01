<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# projectGantt

## Purpose
项目甘特图（Project Gantt）演示页面，提供项目列表 + 项目编辑表单 + 任务甘特视图三件套。属于 `extend` 模块的项目管理示例。

## Key Files
| File | Description |
|------|-------------|
| `index.vue` | 列表页：`BasicTable` 展示项目，`schedule` 列用 `<a-progress>` 进度条，`state` 1/2 → 进行中 / 已暂停；调 `getProjectList` / `delProject`。 |
| `Form.vue` | 项目新增 / 编辑 Modal 表单（基本信息、起止时间、负责人）。 |
| `Task.vue` | 任务甘特视图（~10KB），按时间轴展示任务条、依赖；通过 `usePopup` 打开。 |
| `TaskForm.vue` | 任务新增 / 编辑表单，关联到项目。 |

## For AI Agents

### Working in this directory
- API 路径 `/@/api/extend/projectGantt`；`schedule === 100` 时显示“已完成”，覆盖原 state。
- 头像使用 `globSetting.apiUrl + img.headIcon` 拼接；`useGlobSetting` 来自 `/@/hooks/setting`。
- 时间格式统一 `YYYY-MM-DD`，使用 `dayjs`。
- Form 与 TaskForm 不要互相导入，分别由 `index.vue` 与 `Task.vue` 管理。

### Common patterns
- `useModal` + `usePopup` 共存：Modal 用于 Form，Popup 用于 Task 全屏抽屉。
- 头像列用 `<a-avatar v-for>` + `key=i`。

## Dependencies
### Internal
- `/@/api/extend/projectGantt`、`/@/components/Table`、`/@/components/Modal`、`/@/components/Popup`、`/@/hooks/web/useI18n`、`/@/hooks/web/useMessage`、`/@/hooks/setting`
### External
- `dayjs`、`ant-design-vue`
