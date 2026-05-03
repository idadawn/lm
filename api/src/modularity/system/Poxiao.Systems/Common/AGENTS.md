<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# Common

## Purpose
通用基础服务实现。提供跨模块复用的文件管理、调度任务桥接、与开发期测试入口；属于权限以外、不分 System/Permission 的中性能力。

## Key Files
| File | Description |
|------|-------------|
| `FileService.cs` | `[Tag="Common", Name="File"]` 文件上传/下载、缩略图、压缩、验证码图片、数据加解密；`[AllowAnonymous][IgnoreLog]` 公共入口 |
| `ScheduleTaskService.cs` | 定时任务调度桥接 Service（与 `taskschedule` 模块对接） |
| `TestService.cs` | 调试用测试接口（开发期使用） |

## For AI Agents

### Working in this directory
- `FileService` 是 LIMS 前端唯一文件入口；新增上传业务尽量以参数方式扩展现有方法，不新建并行入口。
- `[IgnoreLog]` 表示该路由不入操作日志，避免污染 SysLog；新增匿名静态资源接口请保持。
- `ScheduleTaskService` 仅做编排转发，真实任务执行落在 `taskschedule` 模块。

### Common patterns
- `IDynamicApiController, ITransient` + `[Route("api/[controller]")]`。
- 通过 `OptionsManager`/`App.GetConfig` 读取 AppOptions、Oauth 等配置。

## Dependencies
### Internal
- `Poxiao.Systems.Interfaces.Common`（IFileService）
- `Poxiao.Infrastructure.Core.Manager.Files`、`Poxiao.Infrastructure.Captcha.General`
- `taskschedule/Poxiao.TaskScheduler.Interfaces`

### External
- System.IO.Compression、System.Security.Cryptography、SkiaSharp（图片处理）

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
