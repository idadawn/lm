<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# Constants

## Purpose
事件总线运行期常量与枚举。当前仅承载动态订阅器的操作类型（新增/删除）。

## Key Files
| File | Description |
|------|-------------|
| `EventSubscribeOperates.cs` | `internal enum` —— `Append` / `Remove`；由 `EventBusFactory.Subscribe / Unsubscribe` 写入 `EventSubscribeOperateSource`，`EventBusHostedService.ManageEventSubscribers` 读取后增删 `_eventHandlers` 字典。 |

## For AI Agents

### Working in this directory
- 该枚举为 `internal`，仅供框架内部使用，业务代码不要直接引用。
- 新增运行期常量请保持 `namespace Poxiao.EventBus`，文件命名沿用复数 + `Operates`/`States` 风格。

### Common patterns
- 动态订阅链路：`IEventBusFactory` → 写入 `IEventSourceStorer`（特殊类型 `EventSubscribeOperateSource`）→ `EventBusHostedService` 在 `BackgroundProcessing` 中分支处理而非作为普通事件。

## Dependencies
### Internal
- 配套类型 `EventSubscribeOperateSource`（`../Sources/`）、`EventBusFactory`（`../Factories/`）。

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
