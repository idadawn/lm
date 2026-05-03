<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# unit

## Purpose
"单位管理"页面：维护物理单位维度（如长度/质量/温度）及其下属单位定义，支持单位换算与标准化。绿色主题，Tabs 切换"单位维度 / 单位"。

## Key Files
| File | Description |
|------|-------------|
| `index.vue` | Tabs（category/unit）+ 卡片网格 + 新增维度/单位 |

## Subdirectories
| Directory | Purpose |
|-----------|---------|
| `components/` | 单位维度与单位定义编辑弹窗 (see `components/AGENTS.md`) |

## For AI Agents

### Working in this directory
- 单位维度（category）= 大类；单位（unit）= 该大类下具体单位（含基准单位标记 + 换算系数）。
- 删除维度需先清空其下单位。
- API `/@/api/lab/unit`。

### Common patterns
- `a-tabs` 切换两类列表；卡片网格响应式。

## Dependencies
### Internal
- `/@/api/lab/unit`, `/@/components/Modal`
### External
- `ant-design-vue`

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
