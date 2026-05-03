<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# Poxiao.Extend

## Purpose
扩展模块的服务实现项目。每个 *Service.cs 都是一个 `IDynamicApiController` 动态 API 控制器，通过 `IDynamicApiController` + `ITransient` 自动注册路由 `api/extend/[controller]`，对应一个业务领域。

## Key Files
| File | Description |
|------|-------------|
| `Poxiao.Extend.csproj` | 引用 Common.Core / Systems.Interfaces / WorkFlow.Interfaces / Extend.Interfaces |
| `BigDataService.cs` | 大数据列表/演示 |
| `DocumentService.cs` | 知识管理（文件夹/文件/分享/回收） |
| `DocumentPreview.cs` | 文档预览（基于 DocumentService 的预览端点） |
| `EmailService.cs` | 邮件收发与配置（IMAP/SMTP） |
| `EmployeeService.cs` | 职员管理（含 NPOI 导入导出） |
| `OrderService.cs` | 订单管理（订单 + 收款计划 + 商品行 + 工作流挂钩） |
| `ProductService.cs` / `ProductClassifyService.cs` / `ProductCustomerService.cs` / `ProductGoodsService.cs` | 产品体系（分类树 / 客户 / 商品） |
| `ProjectGanttService.cs` | 项目甘特（项目 + 任务树形结构） |
| `ScheduleService.cs` | 日程安排（含微信提醒标记） |
| `TableExampleService.cs` | 复杂表格示例（多行/批注/签字） |
| `WorkLogService.cs` | 工作日志（含分享/抄送） |

## For AI Agents

### Working in this directory
- 控制器 `[ApiDescriptionSettings(Tag = "Extend", Name = "<Feature>", Order = NNN)]` 中的 `Order` 决定 Swagger 排序，新增请避免与现有冲突。
- 通过构造函数注入 `ISqlSugarRepository<T>`、`IUserManager`、`ICacheManager`、`IFileManager` 等基础设施服务；遵循已有 readonly 字段命名（`_repository`、`_userManager`...）。
- 邮件/订单等服务包含工作流（`Poxiao.WorkFlow.Entitys.Entity`）联动，修改时确认 `IWorkFlow*` 接口契约。
- 导入导出基于 `Poxiao.Infrastructure.Models.NPOI` 与 `IFileManager`。

### Common patterns
- 控制器以 `#region GET/POST/PUT/DELETE` 区域分组方法。
- 实体 → Dto 通过 Mapster (`Adapt<T>()`) 完成；自定义映射写在 `../Poxiao.Extend.Entitys/Mapper/Mapper.cs`。

## Dependencies
### Internal
- `../Poxiao.Extend.Entitys`、`../Poxiao.Extend.Interfaces`、`Poxiao.Systems.Interfaces.Permission`、`Poxiao.WorkFlow.Entitys`

### External
- Mapster、SqlSugar、Yitter.IdGenerator、Aop.Api（部分服务）

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
