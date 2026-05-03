<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# publicDimension

## Purpose
"公共维度管理"页面：维护跨产品规格复用的维度（如温度、湿度、批次属性），用于公式引用与计算精度控制。卡片网格展示，支持版本管理。

## Key Files
| File | Description |
|------|-------------|
| `index.vue` | 主页：标题/新增 + 卡片网格（紫色主题）|

## Subdirectories
| Directory | Purpose |
|-----------|---------|
| `components/` | 公共维度表单与版本管理弹窗 (see `components/AGENTS.md`) |

## For AI Agents

### Working in this directory
- API `/@/api/lab/publicDimension`；与 `intermediateDataFormula` 联动（公式从此引用变量）。
- 修改维度版本会影响下游公式计算结果，必须有版本号 + 弹窗确认。

### Common patterns
- Tailwind 紫色 `purple-600`/`purple-100` 主题。
- 网格响应式 1/2/3 列；卡片首字母作为头像。

## Dependencies
### Internal
- `/@/api/lab/publicDimension`
### External
- `ant-design-vue`

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
