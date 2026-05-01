<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# intermediateDataFormula

## Purpose
"公式维护"页面：维护中间数据表中的计算公式，支持从多个数据源引用变量、按来源筛选、初始化列结构。是 `intermediateData` 计算引擎的元数据来源。

## Key Files
| File | Description |
|------|-------------|
| `index.vue` | 主页：标题/工具栏/筛选 + BasicTable，新增公式 + 更新列 |

## Subdirectories
| Directory | Purpose |
|-----------|---------|
| `components/` | 公式构建器、判定规则编辑、规则卡片等 (see `components/AGENTS.md`) |

## For AI Agents

### Working in this directory
- "更新列"按钮（`handleInitialize`）会触发后端按公式重建中间数据表列结构，**生产环境慎用**。
- API 命名空间 `/@/api/lab/intermediateDataFormula`。

### Common patterns
- Tailwind + 蓝色主题（`bg-blue-600`）。
- `sourceTypeFilter` 控制按 source 筛选；`a-select` 联动 reload 表格。

## Dependencies
### Internal
- `/@/api/lab/intermediateDataFormula`
- `/@/components/Table`, `/@/components/Modal`
### External
- `ant-design-vue`, `@ant-design/icons-vue`

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
