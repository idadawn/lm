<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# CommonWords (Dto)

## Purpose
常用语 DTO。在文本输入框（如评审意见、备注）提供快捷短语。当前结构精简，仅保留单进/单出。

## Key Files
| File | Description |
|------|-------------|
| `CommonWordsInput.cs` | 创建/更新常用语输入 |
| `CommonWordsOutput.cs` | 输出（单条/列表） |

## For AI Agents

### Working in this directory
- 输入复用同一类做创建与更新，依靠 `Id` 是否传值区分。

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
