<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# Enum

## Purpose
代码生成模式枚举，用于 `VisualDevService` 决定下载/创建的表结构形式（主表、主带子、主带副、主带副子）。

## Key Files
| File | Description |
|------|-------------|
| `GeneratePatterns.cs` | PrimaryTable / MainBelt / MainBeltVice / PrimarySecondary 四种 |

## For AI Agents

### Working in this directory
- 模式选择会决定 `Engine` 端生成多少张子表 DDL；新增模式需同步 Engine 模板。

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
