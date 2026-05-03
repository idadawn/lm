# F3 .NET Event Bus → nlq-agent 实时同步 — 完成报告

## 提交记录

```
418409d feat(api): NlqAgentSyncSubscriber + HttpClient registration
cc68227 feat(api): publish Rule/Spec change events from lab services
```

## 调研发现

| 服务类 | 文件路径 | Publish 点 |
|---|---|---|
| `IntermediateDataFormulaService` | `api/src/modularity/lab/Poxiao.Lab/Service/IntermediateDataFormulaService.cs` | `CreateAsync` → Created; `UpdateAsync` / `UpdateFormulaAsync` → Updated; `DeleteAsync` → Deleted |
| `ProductSpecService` | `api/src/modularity/lab/Poxiao.Lab/Service/ProductSpecService.cs` | `Create` → Created; `Update` → Updated; `Delete` → Deleted |

`IEventPublisher` 注入方式参考 `RawDataImportSessionService.cs:90`（构造函数注入）。

## 改动文件 + 新增 LOC

| 文件 | 动作 | 说明 |
|---|---|---|
| `Poxiao.Lab/EventBus/RuleChangedEventSource.cs` | 新增 | topic `Rule:Changed`，含 `RuleChangeKind` enum |
| `Poxiao.Lab/EventBus/SpecChangedEventSource.cs` | 新增 | topic `Spec:Changed`，含 `SpecChangeKind` enum |
| `Poxiao.Lab/EventBus/NlqAgentSyncSubscriber.cs` | 新增 | ~60 LOC，两个 handler + 通用 POST 辅助方法 |
| `Poxiao.Lab/Service/IntermediateDataFormulaService.cs` | 修改 | 注入 `IEventPublisher`，4 处 publish |
| `Poxiao.Lab/Service/ProductSpecService.cs` | 修改 | 注入 `IEventPublisher`，3 处 publish |
| `Poxiao.API.Entry/Program.cs` | 修改 | 注册 `AddHttpClient("nlq-agent")` |
| `Poxiao.API.Entry/Configurations/AppSetting.json` | 修改 | 添加 `"NlqAgent"` 配置节 |
| `tests/Poxiao.UnitTests/Lab/NlqAgentSyncSubscriberTests.cs` | 新增 | 4 个 xUnit case，mock `HttpMessageHandler` |

## Build 结果

- **TODO**：本机未安装 `dotnet SDK`，未执行 `dotnet build` 与 `dotnet test`。
- 代码已按现有事件总线模式（`ChannelEventSource`、`IEventSubscriber + ISingleton`、`[EventSubscribe]`）编写，语义与 `IntermediateDataCalcEventSubscriber.cs` 一致。
- `PostAsJsonAsync` 来自 `System.Net.Http.Json`（随 `Microsoft.AspNetCore.App` FrameworkReference 提供，项目已引用）。

## 已知限制

1. **未实测真发到 nlq-agent**：依赖 r10 GLM 完成 `/api/v1/sync/rules` 与 `/api/v1/sync/specs` admin endpoint。
2. **无 outbox 表**：按 hybrid 决策不上 outbox，nlq-agent 宕机期间的丢失同步由 nightly cron 兜底。
3. **未执行编译**：建议在 GLM 端点就绪后，于有 dotnet 环境的主机执行 `dotnet build api/` 与 `dotnet test api/tests/Poxiao.UnitTests/`。
