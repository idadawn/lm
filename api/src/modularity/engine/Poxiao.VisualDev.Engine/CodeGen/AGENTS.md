<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# CodeGen

## Purpose
代码生成入口枚举/编排目录。当前主要存放生成方式（前端、后端、整体打包等）相关的核心类型，被 `Core/` 中的解析器与 `Security/` 中的帮助类一起调用以决定生成哪些目标文件。

## Key Files
| File | Description |
|------|-------------|
| `CodeGenWay.cs` | 代码生成主流程类（按 web 类型驱动 Vue / Service / Entity / Dto 等文件的生成） |

## For AI Agents

### Working in this directory
- 任何对生成顺序（解析 → 路径 → 写文件）的修改都需要同时验证 `Security/CodeGenTargetPathHelper.cs` 中输出路径数组与本目录调用一致。
- 不要在此处直接 IO 操作业务数据库；本类被注入到 `FormDataParsing` 等解析器中协同执行。

### Common patterns
- 与 `Model/CodeGen/` 中各 *Model 类强耦合，通常作为入参驱动模板字符串拼装。

## Dependencies
### Internal
- `../Core/`、`../Model/CodeGen/`、`../Security/`

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
