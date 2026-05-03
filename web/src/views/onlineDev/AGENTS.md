<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# onlineDev

## Purpose
"在线开发"模块容器：低代码/可视化平台入口，包含数据报表（dataReport）、可视化门户（visualPortal）、Web 设计器（webDesign）三块。当前目录本身仅为分组，无页面文件。

## Subdirectories
| Directory | Purpose |
|-----------|---------|
| `dataReport/` | 数据报表设计 |
| `visualPortal/` | 可视化门户/大屏配置 |
| `webDesign/` | Web 表单/页面设计器 |

## For AI Agents

### Working in this directory
- 此目录无页面，仅作命名空间；新增子模块按现有命名规范放在同级。
- 子模块通常依赖 `/@/api/onlineDev/*` 与设计器组件库（如 form-create、jflow-design 等）。

### Common patterns
- 各子模块独立页面 `index.vue` + 设计器 IDE 风格容器。

## Dependencies
### Internal
- 各子目录 API
### External
- 设计器/可视化第三方库

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
