<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# Chat

## Purpose
指标对话接口 DTO。配合 `ChatController` 暴露给 AI 助手/自然语言问答，把指标值与线性图表数据序列化成对话能引用的扁平字符串。

## Key Files
| File | Description |
|------|-------------|
| `MetricInfoListForChatDto.cs` | 全部指标的精简列表项：`id` + `name`，用于 AI 选指标 |
| `MetricDataForChatDto.cs` | 单指标对话回包：`value` 文案、`data`（线性图分号串）、`x_axis`（x 轴分号串），承载 `$line_chart$` 调用所需参数 |

## For AI Agents

### Working in this directory
- 输出字段保持简单字符串，便于 LLM/前端模板字符串拼接，不要塞复杂对象。
- 字段命名风格混合（保留 `value` 小写）以兼容已对接的前端模板，新字段务必沿用既有 `[JsonProperty]` 命名。

### Common patterns
- 列表/x 轴数据用 `;` 分隔（在 `ChatController` 中通过 `.Join(';')` 生成）。
- 与 `Services/MetricInfo/IMetricInfoService.GetAll4ChatAsync()` / `GetByNameAsync()` 配合使用。

## Dependencies
### Internal
- `Services/MetricInfo`、`Services/MetricData`
### External
- Newtonsoft.Json

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
