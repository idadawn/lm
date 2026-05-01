<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# Screen

## Purpose
大屏主体（`VisualEntity` → `BLADE_VISUAL`）的输入/输出 DTO，配套 `ScreenService`。

## Key Files
| File | Description |
|------|-------------|
| `ScreenCrInput.cs` | 创建大屏（标题/背景/分类/发布密码） |
| `ScreenUpInput.cs` | 更新大屏 |
| `ScreenInfoOutput.cs` | 详情输出 |
| `ScreenListOutput.cs` | 列表行（backgroundUrl/category/createDept/createTime/createUser/isDeleted/password/status/title/updateTime/updateUser） |
| `ScreenListQueryInput.cs` | 列表查询参数 |
| `ScreenSelectorOuput.cs` | 选择器精简输出（注意原文件名拼写 `Selector**Ouput**`） |
| `ScreenImgFileOutput.cs` | 图片资源列表输出（按 `ScreenImgEnum` 分类） |

## For AI Agents

### Working in this directory
- `password` 是大屏发布密码（明文展示给运维），与系统登录密码无关；严禁混入 `MD5Encryption.Encrypt`。
- `backgroundUrl` 缺省 `/api/file/VisusalImg/bg/bg1.png`（拼写 `Visusal` 沿用，详见 `VisualEntity.Create()`）。

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
