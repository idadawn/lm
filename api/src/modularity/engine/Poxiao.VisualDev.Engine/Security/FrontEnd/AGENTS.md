<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# FrontEnd (Security)

## Purpose
前端代码生成帮助类集合。负责把控件 schema 转成 Vue 表单设计模型（行/列/栅格 gutter/labelWidth），以及把不同控件按"输入/日期/选择..."类别归类，供 `index.vue / Form.vue / superQueryJson.js` 等模板拼装。

## Key Files
| File | Description |
|------|-------------|
| `CodeGenFormControlDesignHelper.cs` | `FormControlDesign` 主入口：递归把 `FieldsModel` 转 `FormControlDesignModel`，处理 popup/autocomplete/日期等特殊控件 |
| `CodeGenQueryControlClassificationHelper.cs` | 列表查询控件归类（input/date/select/...），按 `type` 1-5 区分 Web 设计 / App 设计 / 流程表单 / Web 表单 / App 表单 |

## For AI Agents

### Working in this directory
- `CodeGenFormControlDesignHelper.active` 是模块级静态计数器，仅在 `isMain=true` 时复位为 1；在多线程场景下需要外部串行化。
- 新增控件类别时同步 `ListQueryControl(int type)` 中所有相关分支，否则前端高级搜索会缺字段。

## Dependencies
### Internal
- `../../Model/CodeGen/`、`Poxiao.Infrastructure.Const.PoxiaoKeyConst`

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
