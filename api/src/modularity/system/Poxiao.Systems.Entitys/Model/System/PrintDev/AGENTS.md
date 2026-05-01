<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# PrintDev (Model)

## Purpose
打印模板设计相关 Model。承载模板字段定义、SQL 数据源、最终渲染数据三类对象，配合 `PrintDevService` 提供模板预览/打印接口。

## Key Files
| File | Description |
|------|-------------|
| `PrintDevFieldModel.cs` | 模板字段定义（名称、类型、绑定来源） |
| `PrintDevSqlModel.cs` | 模板数据源 SQL（含库链接、参数、结果列） |
| `PrintDevDataModel.cs` | 渲染前组装的数据对象（字段 + 行数据） |

## For AI Agents

### Working in this directory
- SQL 在打印模板里被白名单化执行，必须经 `DbLink` 校验，禁止在 Model 层做 SQL 拼接。
- 新增字段类型（图片/二维码/条码）需要同时扩展前端 `print-dev` 设计器组件。

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
