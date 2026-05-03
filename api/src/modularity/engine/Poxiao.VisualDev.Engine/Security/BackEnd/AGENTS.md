<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# BackEnd (Security)

## Purpose
后端代码生成帮助类集合。把控件属性 → C# 属性的转换、字段类型/必填判断、列表页按钮配置、表单方法（add/edit/delete/...) 配置等沉淀到静态 helper 中，被 `CodeGenWay` 与服务层调用以渲染 Service / Entity / Dto / Controller。

## Key Files
| File | Description |
|------|-------------|
| `CodeGenControlsAttributeHelper.cs` | 控件属性 → 实体属性转换（含 `ConversionStaticData` 静态数据递归扁平化） |
| `CodeGenExportFieldHelper.cs` | 导出字段（NPOI/Excel）配置帮助 |
| `CodeGenFieldJudgeHelper.cs` | 字段判定（是否字符串/数字/外键/枚举等） |
| `CodeGenFunctionHelper.cs` | 生成方法集合（纯表单 / 带流程 / 列表表单 等） |
| `GetCodeGenIndexButtonHelper.cs` | 列表页按钮（新增/导入/导出/删除...）配置 |

## For AI Agents

### Working in this directory
- 新增"方法预设"（如新增"批量审批"功能）需要在 `CodeGenFunctionHelper` 添加 `CodeGenFunctionModel{ FullName, IsInterface, orderBy }`，并在前端按钮设计器中暴露同名功能。
- `ConversionStaticData` 用 `ToJsonString()/ToObject<T>()` 递归——保留这套写法以兼容深层级数据字典。

## Dependencies
### Internal
- `../../Model/CodeGen/`、`Poxiao.Infrastructure.Models`

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
