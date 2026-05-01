<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# Document

## Purpose
知识管理（文档/文件夹）相关 DTO。覆盖文件夹树、文件信息、分享、回收站、上传与各种动作型端点。

## Key Files
| File | Description |
|------|-------------|
| `DocumentCrInput.cs` / `DocumentUpInput.cs` | 文件/文件夹创建与更新入参 |
| `DocumentInfoOutput.cs` | 文件/文件夹详情（parentId / type / id / fullName / 路径...） |
| `DocumentListOutput.cs` | 文件列表项 |
| `DocumentFolderTreeOutput.cs` | 文件夹树形结构 |
| `DocumentTrashOutput.cs` | 回收站项 |
| `DocumentShareOutput.cs` / `DocumentShareTomeOutput.cs` / `DocumentShareUserOutput.cs` | 分享给我 / 我分享 / 分享对象 |
| `DocumentActionsShareInput.cs` | 触发分享的入参（用户/部门/角色） |
| `DocumentUploaderInput.cs` | 上传辅助入参 |

## For AI Agents

### Working in this directory
- 文档树形结构使用 `parentId` 自关联；`type` 区分文件 vs 文件夹。
- 分享对象抽象为 `users/depts/roles` 等多源；保持与 `DocumentService.cs` 的处理一致。

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
