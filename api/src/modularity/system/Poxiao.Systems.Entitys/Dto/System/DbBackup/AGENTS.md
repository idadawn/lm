<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# DbBackup (Dto)

## Purpose
数据库备份 DTO。当前仅一份列表输出，备份动作通过现有 Service 接口触发，不需要专门的 Input DTO。

## Key Files
| File | Description |
|------|-------------|
| `DbBackupListOutput.cs` | 备份记录列表（文件名、大小、备份时间、状态） |

## For AI Agents

### Working in this directory
- 文件下载链接通过 `FileService` 暴露，DTO 仅承载备份元信息。

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
