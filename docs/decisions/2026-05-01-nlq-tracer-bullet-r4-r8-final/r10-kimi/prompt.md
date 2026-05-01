# 你是 round-10 KIMI 工人 — F3 .NET 事件总线接 nlq-agent

## 工作区（严格）
- worktree：**`/data/project/lm-team/kimi/`**（分支 `omc-team/r10/kimi-net-eventbus`，基于 main `0bfcc64`）
- **第一件事**：`pwd` + `git rev-parse --abbrev-ref HEAD`。错则 BLOCKER（写 `/data/project/lm/.omc/team-nlq-r10/kimi/BLOCKER.md`）。
- **绝对禁止** 在 `/data/project/lm` 主仓库执行任何 git 操作。
- **绝对禁止** `cd` 出 `/data/project/lm-team/kimi/` 范围。
- 主仓库与 GLM worktree 禁止触碰。

## 背景（必读）
设计稿在 `docs/decisions/2026-05-01-nlq-tracer-bullet-r4-r8-final/F3_NET_EVENT_DESIGN.md`。
**已决定 hybrid（事件总线实时 + cron 兜底，**不**上 outbox）**。本轮你做"实时"那一半。

## 范围 — F3 .NET 端实时同步

### 改动 #1 — `Poxiao.Lab/EventBus/RuleChangedEventSource.cs`（新）
```csharp
namespace Poxiao.Lab.EventBus;
public sealed class RuleChangedEventSource : ChannelEventSource
{
    public RuleChangedEventSource(string ruleId, RuleChangeKind kind, object? rule)
        : base("Rule:Changed") { RuleId = ruleId; Kind = kind; Rule = rule; }
    public string RuleId { get; }
    public RuleChangeKind Kind { get; }
    public object? Rule { get; } // null for Deleted
}
public enum RuleChangeKind { Created, Updated, Deleted }
```
+ `SpecChangedEventSource.cs` 同结构，topic `Spec:Changed`。

### 改动 #2 — 业务服务发布事件
找出 IntermediateDataFormulaService（或类似的判定规则 CRUD 服务），在 SaveAsync / DeleteAsync 末尾发布事件：
```csharp
await _eventPublisher.PublishAsync(new RuleChangedEventSource(rule.F_Id, RuleChangeKind.Updated, rule));
```
同样找 ProductSpec 的 service。两处 publish。

**调研步骤先做**：
- `grep -rln "IIntermediateDataFormulaService\|IProductSpecService" api/src/modularity/lab/`
- 读两个 service 的 SaveAsync/DeleteAsync 找合适的 publish 点（事务提交后、return 前）
- 注入 `IEventPublisher`（参考 `RawDataImportSessionService.cs:90` 写法）

### 改动 #3 — `Poxiao.Lab/EventBus/NlqAgentSyncSubscriber.cs`（新，~80 LOC）
按设计稿样板（subscriber sketch 段）实现：
- `[EventSubscribe("Rule:Changed")]` HandleRuleChanged → HttpClient POST `/api/v1/sync/rules`
- `[EventSubscribe("Spec:Changed")]` HandleSpecChanged → HttpClient POST `/api/v1/sync/specs`
- payload: `{ rule_id, change_kind, rule }`（rule 在 Deleted 时为 null）
- catch 任何异常：log error + **不抛出**（best-effort；nightly cron 兜底）
- HttpClient 通过 `IHttpClientFactory.CreateClient("nlq-agent")` 获取

### 改动 #4 — Program.cs HttpClient 注册
找到 `api/src/application/Poxiao.API.Entry/Program.cs`（或 Startup.cs），加：
```csharp
services.AddHttpClient("nlq-agent", c => {
    c.BaseAddress = new Uri(configuration["NlqAgent:BaseUrl"] ?? "http://nlq-agent:18100");
    c.Timeout = TimeSpan.FromSeconds(configuration.GetValue("NlqAgent:TimeoutSeconds", 5));
});
```
appsettings.json 加 `"NlqAgent": { "BaseUrl": "http://localhost:18100", "TimeoutSeconds": 5 }`。

### 改动 #5 — 测试
- `api/tests/.../NlqAgentSyncSubscriberTests.cs`（如有 xUnit project）：mock IHttpClientFactory + HttpMessageHandler，断言 POST 路径 + body shape。
- 如无 xUnit project：跳过，REPORT 中说明。

## 执行约束
- **不准** spawn 子代理
- **不准**改 `nlq-agent/` 下任何代码（GLM 的领域）
- **不准**改 `web/` 下任何代码（lead 的领域）
- **不准**新建 outbox 表 / 加 MySQL migration（hybrid 决定不上 outbox）
- 跑外部服务 — 不准（用 mock HttpMessageHandler）
- 现有测试必须仍全绿（`dotnet test api/tests/`，如本机有 dotnet）

## 提交规范
≥2 atomic commits：
- `feat(api): publish Rule/Spec change events from lab services`
- `feat(api): NlqAgentSyncSubscriber + HttpClient registration`

每条 commit body 写 1-3 句 why（引用 F3_NET_EVENT_DESIGN.md 的决策）。
`Co-Authored-By: Claude Opus 4.7` attribution。`git status` 干净。

## 输出
1. `git log --oneline 0bfcc64..HEAD` ≥2 commits
2. `dotnet build api/` 0 错（如本机有 dotnet；没就标 TODO）
3. 测试输出（如跑了）
4. `/data/project/lm/.omc/team-nlq-r10/kimi/REPORT.md`：
   - 调研发现（找到的 service 类 + publish 点）
   - 改动文件 + LOC
   - build 结果
   - 已知限制（"未实测真发到 nlq-agent，依赖 r10 GLM 完成 admin endpoint"）

阻塞写到 `/data/project/lm/.omc/team-nlq-r10/kimi/BLOCKER.md`。

## 时间约束
~30 min。优先调研 + 改动 #1-4；测试看时间。

开始 — 先 pwd！
